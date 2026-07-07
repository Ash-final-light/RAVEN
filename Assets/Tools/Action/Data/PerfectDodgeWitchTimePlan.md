# 完美闪避触发魔女时间方案

## 当前项目现状

当前闪避代码集中在 `Assets/Tools/Action/Data/Slide`：

- `SlideAction`：由角色状态机驱动，负责开启/关闭闪避判定窗口。
- `SlideObjMono`：挂在子碰撞体上，负责成功闪避后的冷却与单次触发过滤。
- `SlideConfigSo`：已有普通闪避限制参数：最大连续闪避次数、统计窗口、锁定时长、判定时间。
- `PlayerStateMachine.HandleSlide()`：播放 `SlideBehind`，并调用 `_slideAction.StartJudge(Owner.slideConfigSo._judgeTime)` 开启完美闪避判定窗口。

时间缩放系统已经存在：

- `TimeScaleManager.Instance.AddCommand(...)`
- `TimeScaleWitchCommand`
- `CharacterTimeScaleDispatcher`
- `ActionStateController.characterId`

`TimeScaleWitchCommand` 的语义正好适合魔女时间：指定一个发动者 ID，发动者保持正常速度或加速，其他注册到时间系统的对象减速。

## 目标效果

玩家在闪避判定窗口内被敌方攻击范围检测扫到时：

1. 只触发一次完美闪避。
2. 进入魔女时间。
3. 玩家保持 `1.0` 速度，敌人/子弹/特效/逻辑按配置变慢。
4. 魔女时间有独立冷却，避免连续触发。
5. 触发点与具体动作解耦，未来可以把闪避成功接到音效、特效、UI、震屏、Buff 等系统。

## 推荐最小落地方案

### 1. 扩展 SlideConfigSo

在 `SlideConfigSo` 中新增魔女时间配置：

```csharp
[Header("Perfect Dodge / Witch Time")]
public bool enablePerfectDodgeWitchTime = true;
public float witchTimeDuration = 1.5f;
public float witchSlowScale = 0.1f;
public float witchSelfScale = 1.0f;
public float witchCooldown = 3.0f;
public TimeScaleType witchAffectedType = TimeScaleType.All;
```

含义：

- `enablePerfectDodgeWitchTime`：是否开启完美闪避触发魔女时间。
- `witchTimeDuration`：魔女时间持续时间。
- `witchSlowScale`：非发动者的时间倍率。
- `witchSelfScale`：发动者自己的时间倍率。
- `witchCooldown`：完美闪避触发冷却。
- `witchAffectedType`：影响哪些系统。当前可先用 `TimeScaleType.All`，后续可改成 `Logic | Bullet | Effect | Audio`。

### 2. 让 SlideAction 持有发动者信息

`SlideAction` 当前已经拿到：

```csharp
SlideObjMono slideObjMono
Collider characterCollider
Transform ownerTransform
SlideConfigSo slideConfigSo
```

但触发魔女时间需要 `ActionStateController.characterId`，所以建议把构造函数改成接收 `ActionStateController owner`：

```csharp
private readonly ActionStateController _owner;
private readonly SlideConfigSo _slideConfigSo;

public SlideAction(
    SlideObjMono slideObjMono,
    Collider characterCollider,
    Transform ownerTransform,
    SlideConfigSo slideConfigSo,
    ActionStateController owner)
```

`TestStateController.InitSlide()` 中改为：

```csharp
_slideAction = new SlideAction(
    _slideObjMono,
    _characterCollider,
    transform,
    slideConfigSo,
    this);
```

### 3. SlideObjMono 只负责判定，不直接依赖时间系统

当前 `SlideObjMono.HandleSuccessSlide()` 已经有两个关键能力：

- `_isSuccessTriggered`：同一轮闪避只触发一次。
- `specialTimer/specialDuration`：成功闪避冷却。

建议把它改成事件出口：

```csharp
public event Action OnSuccessSlide;

public void HandleSuccessSlide()
{
    if (_isSuccessTriggered)
    {
        return;
    }

    if (specialTimer <= 0)
    {
        _isSuccessTriggered = true;
        specialTimer = specialDuration;
        Debug.Log("[PerfectDodge] Success slide triggered.");
        OnSuccessSlide?.Invoke();
    }
}
```

这样 `SlideObjMono` 只表达“成功闪避发生了”，不关心魔女时间、特效、音效、UI。

### 4. SlideAction 订阅成功闪避事件

在 `SlideAction` 构造函数里订阅：

```csharp
_slideObjMono.OnSuccessSlide += HandlePerfectDodge;
```

新增处理函数：

```csharp
private void HandlePerfectDodge()
{
    if (_owner == null || _slideConfigSo == null)
    {
        return;
    }

    if (!_slideConfigSo.enablePerfectDodgeWitchTime)
    {
        return;
    }

    var command = new TimeScaleWitchCommand(
        $"PerfectDodge_{_owner.characterId}",
        _slideConfigSo.witchTimeDuration,
        _slideConfigSo.witchSlowScale,
        _slideConfigSo.witchSelfScale,
        _owner.characterId,
        _slideConfigSo.witchAffectedType);

    TimeScaleManager.Instance.AddCommand(command);
    Debug.Log($"[PerfectDodge] Witch time triggered by {_owner.characterId}.");
}
```

注意需要引用：

```csharp
using TimeScale;
using TimeScale.Command;
```

### 5. 避免重复订阅

`SlideAction` 目前不是 `MonoBehaviour`，没有 `OnDestroy()`。可以加一个 `Dispose()`：

```csharp
public void Dispose()
{
    if (_slideObjMono != null)
    {
        _slideObjMono.OnSuccessSlide -= HandlePerfectDodge;
    }
}
```

然后在 `ActionStateController.OnDestroy()` 或具体角色控制器 `OnDestroy()` 中调用 `_slideAction?.Dispose()`。

## 碰撞触发链路

推荐链路如下：

```text
玩家按闪避
PlayerStateMachine.HandleSlide()
SlideAction.StartJudge(judgeTime)
SlideObjMono.StartCollideJudge()
闪避判定碰撞体开启
敌方 ActionClip 的攻击范围检测生效
ActionPlayer.CheckCollide()
CollideDetectionDelegate.Tick(...)
范围检测扫到玩家 SlideObjMono 的 Collider
SelfCollideManager.HandleCollide(sliceId, collider)
SelfCollideManager.HandleSlideCollide(...)
SlideObjMono.HandleSuccessSlide()
SlideAction.HandlePerfectDodge()
TimeScaleManager.AddCommand(TimeScaleWitchCommand)
CharacterTimeScaleDispatcher 下发缩放
ActionPlayer / CollideAbility / BulletAbility / Effect / Audio 响应缩放
```

## 如何判断一次碰撞是完美闪避

当前项目的攻击伤害模板主要是范围检测，不是“敌方攻击碰撞体真正撞到玩家闪避碰撞体”。所以完美闪避不要理解成独立物理碰撞事件，而应该理解成：

```text
敌方攻击范围检测结果中扫到了玩家当前开启的 SlideObjMono 判定体。
```

最小验证阶段可以简单认为：

- 攻击 ActionClip 的范围检测扫到 `SlideObjMono`，才是完美闪避。
- `SlideObjMono` 必须处于 `StartCollideJudge()` 打开的判定窗口中。
- `SlideObjMono` 自己用 `_isSuccessTriggered` 防止单次闪避重复触发。
- `specialTimer` 或新的 `witchCooldown` 防止短时间反复触发魔女时间。

后续更严谨时，过滤应该加在攻击范围检测命中回调里，也就是 `SelfCollideManager.HandleCollide(sliceId, collider)` 附近，而不是只依赖物理碰撞体本身：

- 当前 `sliceId` 对应的 `ActionCollideDetectionClipData.collideEventType` 必须是 `CauseDamage`。
- 当前 `SelfCollideManager` 的 `_character` 必须是攻击发起者，且不能和被闪避角色是同一方或同一个角色。
- 范围检测扫到的 `collider` 必须能拿到 `SlideObjMono`，而不是环境碰撞、普通角色身体碰撞、友方碰撞。
- 如果同一轮范围检测同时扫到玩家本体 Collider 和玩家 Slide Collider，应该让 Slide 判定优先，并跳过本次对该玩家的伤害结算。

为支持这个判断，`SelfCollideManager` 不应该只保存：

```csharp
Dictionary<string, DamageHitConfigSO> _comboConfigs
```

还应该保存当前 slice 的完整碰撞片段配置，例如：

```csharp
private readonly Dictionary<string, ActionCollideDetectionClipData> _collideClipConfigs = new();
```

初始化时记录：

```csharp
_collideClipConfigs[collide.sliceId] = collide;
```

然后在 `HandleSlideCollide()` 前判断：

```csharp
if (!_collideClipConfigs.TryGetValue(sliceId, out var clip))
{
    return;
}

if (clip.collideEventType != ActionCollideEventType.CauseDamage)
{
    return;
}
```

这样完美闪避触发点就和当前项目的范围检测模板对齐：它发生在“攻击范围检测命中结果”阶段，并且早于 `BattleManager.Instance.AddDamage(...)`。

## 范围检测下的伤害抑制

当前 `SelfCollideManager` 会按 collider tag 分流：

```text
Character / Enemy -> HandleDamageCollide -> BattleManager.Instance.AddDamage(...)
Slide             -> HandleSlideCollide  -> SlideObjMono.HandleSuccessSlide()
```

如果敌方攻击范围同一帧同时扫到玩家本体和 `SlideObjMono`，可能出现“既完美闪避又受伤”。为避免这个问题，建议增加一次攻击 slice 内的完美闪避记录：

```csharp
private readonly Dictionary<string, HashSet<string>> _perfectDodgedTargets = new();
```

当 `HandleSlideCollide()` 成功触发时，记录目标角色 ID：

```csharp
_perfectDodgedTargets[sliceId].Add(targetCharacterId);
```

在 `HandleDamageCollide()` 伤害结算前检查：

```csharp
if (HasPerfectDodged(sliceId, targetCharacter.characterId))
{
    Debug.Log($"[PerfectDodge] Damage skipped. slice={sliceId}, target={targetCharacter.characterId}");
    return;
}
```

如果暂时拿不到 `targetCharacterId`，可以先用 `SlideObjMono` 所属父级上的 `ActionStateController`：

```csharp
var target = slideObjMono.GetComponentInParent<ActionStateController>();
```

这样完美闪避就不是单纯“触发魔女时间”，而是完整地覆盖了本次攻击命中：触发成功闪避、进入魔女时间、跳过本次伤害。

## 和当前 TimeScale 系统的配合点

`TimeScaleWitchCommand` 需要正确的发动者 ID：

```csharp
_owner.characterId
```

`CharacterTimeScaleDispatcher.Initialize(host)` 会把：

```csharp
_cachedInstanceId = hostCharacter.characterId;
```

注册进 `TimeScaleManager`。所以玩家、敌人都必须满足：

1. 挂了 `CharacterTimeScaleDispatcher`。
2. `ActionStateController.joinTimeScale == true`。
3. 初始化时调用了 `InitTimeScale()`。
4. `characterId` 不重复。

否则魔女时间命令无法区分“发动者”和“被减速对象”。

## 配置建议

初始推荐：

```text
_judgeTime = 0.2
witchTimeDuration = 1.2
witchSlowScale = 0.1
witchSelfScale = 1.0
witchCooldown = 3.0
witchAffectedType = All
```

如果感觉太强：

```text
witchTimeDuration = 0.8
witchSlowScale = 0.2
```

如果希望玩家也有轻微慢动作手感：

```text
witchSelfScale = 0.8
```

## 推荐日志

建议统一英文日志，方便筛选：

```text
[PerfectDodge] Judge started. duration=0.20
[PerfectDodge] Success slide triggered.
[PerfectDodge] Witch time triggered. caster=player, duration=1.20, slow=0.10, self=1.00
[PerfectDodge] Ignored by cooldown. remaining=2.40
```

## 最小验证步骤

1. 在当前测试场景确认玩家和敌人都挂了 `CharacterTimeScaleDispatcher`。
2. 玩家 `slideConfigSo` 配置：
   - `_judgeTime = 0.2`
   - `enablePerfectDodgeWitchTime = true`
   - `witchTimeDuration = 1.2`
   - `witchSlowScale = 0.1`
   - `witchSelfScale = 1.0`
3. 让敌人攻击碰撞体在玩家闪避窗口内碰到 `SlideObjMono`。
4. 观察日志是否出现：
   - `[PerfectDodge] Success slide triggered.`
   - `[PerfectDodge] Witch time triggered...`
   - `[TimeScale Test: ...] scale=0.10`
5. 验证玩家动作正常，敌人行为树/动作/子弹变慢。

## 后续事件化扩展

如果未来 ActionClip 也要触发更多组件，建议不要让 `SlideObjMono` 直接调用所有业务，而是建立 `PerfectDodgeEvent`：

```csharp
public struct PerfectDodgeEvent
{
    public ActionStateController Owner;
    public Collider AttackCollider;
    public Vector3 HitPoint;
}
```

然后由一个 `PerfectDodgeDispatcher` 或现有 `ActionEventDispatcher` 派发：

```text
SlideObjMono.HandleSuccessSlide()
PerfectDodgeDispatcher.Dispatch(context)
WitchTimeHandler
ParticleHandler
AudioHandler
BuffHandler
UIHandler
```

这样完美闪避只负责“成功事实”，魔女时间只是其中一个 handler。后续加特效、音效、奖励资源、反击窗口，不需要改闪避判定本体。

## 结论

最小改法：

- `SlideObjMono` 增加 `OnSuccessSlide` 事件。
- `SlideAction` 订阅事件并创建 `TimeScaleWitchCommand`。
- `SlideConfigSo` 增加魔女时间参数。
- `TestStateController.InitSlide()` 把 `this` 传给 `SlideAction`。

这个方案改动范围小，符合当前项目结构，并且能自然扩展到事件系统。
