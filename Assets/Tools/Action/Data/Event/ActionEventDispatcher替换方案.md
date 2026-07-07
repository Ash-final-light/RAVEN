# ActionEventDispatcher 替换方案

## 目标

当前 ActionClip 事件系统使用 `ActionPlayer.RegisterEvent(eventName, callback)` 按事件名注册回调。这个方式可以跑简单事件，但后续如果 ActionClip 要调用子弹、相机、召唤物、武器显隐、角色变速等更多组件，`ActionPlayer.CheckEvent()` 和 `EventAbility` 会持续膨胀。

建议替换为：

```text
ActionPlayer.CheckEvent()
-> ActionEventDispatcher.Dispatch(context, eventClip)
-> IActionEventHandler.Handle(context, eventClip)
-> 具体组件能力执行
```

核心原则：

- `ActionPlayer` 只负责动作播放、tick 推进、发现事件触发时机。
- `ActionEventDispatcher` 只负责按事件类型找到对应 handler。
- `IActionEventHandler` 负责执行具体逻辑，例如 AddBuff、Blackboard、Debug、FireBullet。
- ActionClip 只配置“第几帧触发什么事件和参数”，不直接依赖某个组件实现。

## 当前问题

### 1. 事件参数协议不统一

当前 `ActionPlayer.CheckEvent()` 对不同事件传了不同 payload：

```csharp
case ActionEventEnum.AddBuff:
    listener?.Invoke(eventClip.name, eventClip.type, eventClip.buffId);
    break;
case ActionEventEnum.Blackboard:
    listener?.Invoke(eventClip.name, eventClip.type, eventClip.blackboardEvent);
    break;
case ActionEventEnum.Debug:
    listener?.Invoke(eventClip.name, eventClip.type, eventClip.message);
    break;
```

但 `EventAbility.AddBuffCallback()` 期望收到的是完整 `ActionEventClipData`：

```csharp
ActionEventClipData data = message as ActionEventClipData;
```

这会导致 AddBuff 回调拿到 `int` 后转换失败。

建议统一为：事件系统永远传完整 `ActionEventClipData`。

```csharp
listener?.Invoke(eventClip.name, eventClip.type, eventClip);
```

如果采用 Dispatcher，则更进一步：不再使用 `object message` 作为主要协议，而是直接传 `ActionEventClipData`。

### 2. Blackboard 回调判断错误

当前 `BlackboardActionCallback()` 中判断写成：

```csharp
if (data == null || eventName != ActionEventEnum.AddBuff)
```

这里应该是：

```csharp
if (data == null || eventName != ActionEventEnum.Blackboard)
```

这是复制 AddBuff 回调时遗留的问题。

### 3. EventAbility 初始化依赖外部主动访问

`ActionStateController` 中有懒加载属性：

```csharp
public EventAbility EventAbility
{
    get
    {
        if (_eventAbility == null)
        {
            _eventAbility = new EventAbility(this, buffManager);
        }
        return _eventAbility;
    }
}
```

但如果没有任何地方访问 `EventAbility`，`EventAbility.InitEvent()` 不会执行，事件监听不会注册。

`TestStateController.Start()` 里原本有：

```csharp
// _eventAbility = new EventAbility(this,buffManager);
```

这句被注释后，如果没有其他地方访问 `EventAbility`，事件系统不会真正生效。

## 推荐的新结构

### ActionEventContext

用于给 handler 提供执行上下文。

```csharp
public readonly struct ActionEventContext
{
    public readonly ActionStateController Controller;
    public readonly ActionPlayer Player;
    public readonly ActionClip Clip;
    public readonly int Tick;

    public ActionEventContext(
        ActionStateController controller,
        ActionPlayer player,
        ActionClip clip,
        int tick)
    {
        Controller = controller;
        Player = player;
        Clip = clip;
        Tick = tick;
    }
}
```

### IActionEventHandler

每种事件一个 handler。

```csharp
public interface IActionEventHandler
{
    ActionEventEnum EventType { get; }
    void Handle(ActionEventContext context, ActionEventClipData data);
}
```

### ActionEventDispatcher

按 `ActionEventEnum` 找 handler。

```csharp
public class ActionEventDispatcher
{
    private readonly Dictionary<ActionEventEnum, IActionEventHandler> handlers = new();

    public ActionEventDispatcher(ActionStateController controller)
    {
        Register(new DebugEventHandler());
        Register(new AddBuffEventHandler());
        Register(new BlackboardEventHandler());
        // 后续新增：
        // Register(new FireBulletEventHandler());
    }

    public void Register(IActionEventHandler handler)
    {
        handlers[handler.EventType] = handler;
    }

    public void Dispatch(ActionEventContext context, ActionEventClipData data)
    {
        if (!handlers.TryGetValue(data.type, out var handler))
        {
            Debug.LogWarning($"[ActionEventDispatcher] Missing handler. type={data.type}, event={data.name}");
            return;
        }

        handler.Handle(context, data);
    }
}
```

## ActionPlayer 修改方向

当前 `CheckEvent()` 中的 switch 可以替换为：

```csharp
public void CheckEvent()
{
    foreach (var eventClip in _actionClip.events.eventClips.Where(eventClip => _tick == eventClip.tick))
    {
        var context = new ActionEventContext(_stateController, this, _actionClip, _tick);
        _stateController.EventAbility.Dispatch(context, eventClip);
    }
}
```

这里 `EventAbility` 可以保留名称，但职责改成持有 `ActionEventDispatcher`：

```csharp
public class EventAbility
{
    private readonly ActionEventDispatcher dispatcher;

    public EventAbility(ActionStateController stateController, BuffManager buffManager)
    {
        dispatcher = new ActionEventDispatcher(stateController, buffManager);
    }

    public void Dispatch(ActionEventContext context, ActionEventClipData data)
    {
        dispatcher.Dispatch(context, data);
    }
}
```

这样能减少对现有 `ActionStateController.EventAbility` 属性的破坏。

## Handler 示例

### DebugEventHandler

```csharp
public class DebugEventHandler : IActionEventHandler
{
    public ActionEventEnum EventType => ActionEventEnum.Debug;

    public void Handle(ActionEventContext context, ActionEventClipData data)
    {
        Debug.Log($"[ActionDebug] clip={context.Clip.name}, event={data.name}, message={data.message}");
    }
}
```

### AddBuffEventHandler

```csharp
public class AddBuffEventHandler : IActionEventHandler
{
    private readonly BuffManager buffManager;

    public ActionEventEnum EventType => ActionEventEnum.AddBuff;

    public AddBuffEventHandler(BuffManager buffManager)
    {
        this.buffManager = buffManager;
    }

    public void Handle(ActionEventContext context, ActionEventClipData data)
    {
        if (buffManager == null)
        {
            Debug.LogError("[AddBuffEventHandler] BuffManager is null.");
            return;
        }

        buffManager.CreateBuff(
            context.Controller.characterId,
            context.Controller.characterId,
            data.buffId,
            data.buffStack);
    }
}
```

### BlackboardEventHandler

```csharp
public class BlackboardEventHandler : IActionEventHandler
{
    public ActionEventEnum EventType => ActionEventEnum.Blackboard;

    public void Handle(ActionEventContext context, ActionEventClipData data)
    {
        var blackboard = context.Controller.RunningBlackboard;
        var blackboardEvent = data.blackboardEvent;

        if (blackboard == null || blackboardEvent == null)
        {
            Debug.LogError($"[BlackboardEventHandler] Invalid blackboard event. event={data.name}");
            return;
        }

        // 将现有 BlackboardActionCallback 的 switch 迁移到这里。
    }
}
```

## Bullet 接入方案

### 新增事件类型

在 `ActionEventEnum` 中新增：

```csharp
FireBullet,
```

### 新增 BulletEventData

建议不要直接把 `TestStateController.Fire()` 的测试代码搬进 ActionClip。应该把子弹配置放入事件数据。

```csharp
[Serializable]
public class BulletEventData
{
    public BulletModel model;
    public string fireSocketName;
    public Vector3 localOffset;
    public float fireDegreeOffset;
    public float duration = 1f;

    public BulletLauncher CreateLauncher(ActionStateController controller)
    {
        Transform fireSocket = controller.transform;

        if (!string.IsNullOrEmpty(fireSocketName))
        {
            var found = controller.transform.Find(fireSocketName);
            if (found != null)
            {
                fireSocket = found;
            }
        }

        return new BulletLauncher
        {
            model = model,
            firePosition = fireSocket.TransformPoint(localOffset),
            fireDegree = fireSocket.eulerAngles.y + fireDegreeOffset,
            duration = duration
        };
    }
}
```

在 `ActionEventClipData` 中增加：

```csharp
[ShowIf("type", ActionEventEnum.FireBullet)]
public BulletEventData bulletEvent;
```

### FireBulletEventHandler

```csharp
public class FireBulletEventHandler : IActionEventHandler
{
    public ActionEventEnum EventType => ActionEventEnum.FireBullet;

    public void Handle(ActionEventContext context, ActionEventClipData data)
    {
        if (data.bulletEvent == null)
        {
            Debug.LogError($"[FireBulletEventHandler] Bullet event is null. event={data.name}");
            return;
        }

        var launcher = data.bulletEvent.CreateLauncher(context.Controller);
        if (launcher?.model == null)
        {
            Debug.LogError($"[FireBulletEventHandler] Bullet launcher or model is null. event={data.name}");
            return;
        }

        context.Controller.BulletAbility.SetBulletModel(
            launcher.model.abName,
            launcher.model.assetName);

        context.Controller.BulletAbility.Fire(launcher);
    }
}
```

### Bullet Tick

`BulletAbility.Tick(deltaTime)` 不应该继续依赖 `TestStateController.UpdateBulletTime(float deltaTime)`。

正式运行中应放入角色更新链路，例如：

```csharp
BulletAbility.Tick(Time.deltaTime);
```

后续更好的做法是让所有能力实现统一 tick 接口：

```csharp
public interface IActionAbilityTick
{
    void Tick(float deltaTime);
}
```

然后 `ActionStateController` 统一维护能力列表，避免每个 Controller 手写：

```csharp
CollideAbility.Tick(Time.deltaTime);
ParticleAbility.Tick(Time.deltaTime);
BulletAbility.Tick(Time.deltaTime);
```

## 迁移步骤

### 第一步：低风险修复现有事件

先不引入 Dispatcher，只修当前 bug：

- `ActionPlayer.CheckEvent()` 统一传 `eventClip`。
- `BlackboardActionCallback()` 判断改为 `ActionEventEnum.Blackboard`。
- 角色初始化时确保访问一次 `EventAbility`，例如 `_ = EventAbility;`。

这样能先验证 AddBuff、Blackboard、Debug 都能跑。

### 第二步：引入 Dispatcher，但保留 EventAbility 入口

- 新增 `ActionEventContext`。
- 新增 `IActionEventHandler`。
- 新增 `ActionEventDispatcher`。
- 将 `EventAbility` 改为持有 dispatcher。
- `ActionPlayer.CheckEvent()` 改成调用 `EventAbility.Dispatch(...)`。

这一步可以保持 `ActionStateController.EventAbility` 这个外部入口不变。

### 第三步：把现有事件迁移成 Handler

- `DebugActionCallback` -> `DebugEventHandler`
- `AddBuffCallback` -> `AddBuffEventHandler`
- `BlackboardActionCallback` -> `BlackboardEventHandler`

迁移完成后，可以删除 `_eventListeners`、`RegisterEvent()`、`UnRegisterEvent()` 这套按事件名注册 callback 的逻辑。

### 第四步：接入 FireBullet

- `ActionEventEnum` 增加 `FireBullet`。
- `ActionEventClipData` 增加 `BulletEventData bulletEvent`。
- 新增 `FireBulletEventHandler`。
- `ActionStateController` 或具体角色 Controller 中正式 tick `BulletAbility`。

### 第五步：为更多组件扩展事件

后续新增能力不再修改 `ActionPlayer`，只新增：

- 一个 `ActionEventEnum` 枚举值。
- 一个事件数据结构。
- 一个 `IActionEventHandler` 实现。

例如：

- `CameraShakeEventHandler`
- `ShowWeaponEventHandler`
- `SpawnObjectEventHandler`
- `SetTimeScaleEventHandler`
- `LockTargetEventHandler`

## 注意事项

- 不建议让 ActionClip 直接保存任意组件方法名并用反射调用。反射虽然看起来通用，但参数校验、编辑器提示、重构安全性都会变差。
- 不建议把 Bullet 逻辑直接写进 `ActionPlayer.CheckEvent()`。这样会让 `ActionPlayer` 变成所有组件的耦合中心。
- `EventAbility` 可以保留为入口，但内部职责应从“遍历 ActionClip 注册 callback”改成“持有 dispatcher 并执行 Dispatch”。
- 子弹配置应来自 ActionClip 的事件数据，而不是 `TestStateController.Fire()` 里的临时测试数据。

## 最终形态

```text
ActionClip
  events
    eventClips
      tick = 12
      type = FireBullet
      bulletEvent = ...

ActionPlayer
  Tick()
    CheckEvent()
      EventAbility.Dispatch(context, eventClip)

EventAbility
  ActionEventDispatcher.Dispatch(context, eventClip)

ActionEventDispatcher
  FireBulletEventHandler.Handle(context, eventClip)

FireBulletEventHandler
  BulletAbility.SetBulletModel(...)
  BulletAbility.Fire(launcher)

ActionStateController Update
  BulletAbility.Tick(deltaTime)
```

这个结构能让 ActionClip 继续作为动作时间轴配置入口，同时避免 `ActionPlayer` 持续膨胀。
