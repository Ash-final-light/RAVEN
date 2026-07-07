# 技能球 UI 代码草案

本文记录当前技能球 UI 的实现方案。当前结论是：`SignalBallSlot_0` 本身作为技能球实体模板使用，运行时根据技能球池动态复制实体。

## 当前规则

- `SignalBallSlot_0` 是实体 prefab/template。
- 技能球实体动态生成，不固定占用场景里已有的 `SignalBallSlot_0 ~ SignalBallSlot_5`。
- 实体里的数字表示当前可按的第几个技能球。
- 最右侧是第 1 个技能球，显示数字 `1`。
- 往左依次是第 2、3、4 个技能球。
- 新增技能球时，从左侧向目标位置推入。
- 技能球减少时，消除实体播放动画并销毁，剩余实体按右对齐位置补位。

## 建议层级

```text
SignalBallBar_DynamicQueue
├── BallEntityRoot
└── SignalBallSlot_0
    ├── Aura_Ring
    ├── Ball_Background
    ├── Ball_Icon
    ├── KeyHint_Bg
    └── KeyHint_Text
```

说明：

- `SignalBallSlot_0` 作为 `ActionUISkillBallEntity entityPrefab` 绑定。
- `SignalBallSlot_0` 也可以作为 `rightMostAnchor`，代表第 1 个球的最终位置。
- `BallEntityRoot` 用于承载动态生成的实体。
- 动态实体和 `rightMostAnchor` 最好在同一个 UI 坐标空间下，避免坐标转换。

## 核心脚本

```text
Assets/Scripts/UI/UIAction/SkillBallUI/ActionUISkillBallBar.cs
Assets/Scripts/UI/UIAction/SkillBallUI/ActionUISkillBallEntity.cs
```

`ActionUISkillBallBar` 挂在 `SignalBallBar_DynamicQueue`。

`ActionUISkillBallEntity` 挂在 `SignalBallSlot_0`。

## ActionUISkillBallBar 字段

```csharp
[SerializeField] private RectTransform rightMostAnchor;
[SerializeField] private RectTransform ballEntityRoot;
[SerializeField] private ActionUISkillBallEntity entityPrefab;
[SerializeField, Min(1)] private int maxVisibleCount = 8;

[SerializeField] private float slotSpacing = 52f;
[SerializeField] private float pushFromLeftOffset = 72f;
[SerializeField] private float pushDuration = 0.22f;
[SerializeField] private float moveDuration = 0.18f;
[SerializeField] private float consumeStagger = 0.035f;
```

位置计算：

```csharp
Vector2 rightMostPosition = rightMostAnchor.anchoredPosition;
Vector2 slotPosition = rightMostPosition + Vector2.left * (slotSpacing * visibleIndex);
```

因此：

```text
visibleIndex = 0 -> 最右侧 -> 数字 1
visibleIndex = 1 -> 左侧第二个 -> 数字 2
visibleIndex = 2 -> 左侧第三个 -> 数字 3
```

推入起点：

```csharp
Vector2 pushStart = slotPosition + Vector2.left * pushFromLeftOffset;
```

也就是新球从目标位置左侧向右推入。

## 动态实体流程

dispatcher 发来当前可见技能球全量列表：

```text
SkillBallManager.BuildVisibleViewData()
-> BaseSkillDispatcher.SkillBallsAdded
-> ActionUISkillBallBar.OnSkillBallsChanged
```

`ActionUISkillBallBar` 按 `RuntimeId` 做 diff：

```text
旧实体字典没有 RuntimeId
-> Instantiate(entityPrefab, ballEntityRoot)
-> 放在目标位置左侧
-> PlayPushIn(targetPosition)

旧实体字典已有 RuntimeId
-> 复用实体
-> MoveTo(targetPosition)

旧实体不在新列表中
-> PlayDisappearTween()
-> Destroy
```

## 数量增加

例如当前只有一个球：

```text
[1]
```

新增一个球后：

```text
[2] [1]
```

表现：

- 原本的 `1` 保持在最右侧。
- 新增的 `2` 在它左侧生成。
- `2` 从左侧推入到自己的目标位置。

## 数量减少

例如当前：

```text
[3] [2] [1]
```

如果消除 `1`：

```text
[2] [1]
```

表现：

- 原来的 `1` 播放消除并销毁。
- 原来的 `2 / 3` 根据新列表重新显示数字并移动补位。

注意：数字永远代表当前可见队列中的按键序号，不代表 RuntimeId。

## 消除动画

`SkillBallConsumeViewData.RuntimeIds` 是消除动画的核心。

流程：

```text
SkillBallsConsumed(RuntimeIds)
-> 按 RuntimeId 找实体
-> PlayConsumeTween()
-> 动画完成 Destroy
-> 后续 SkillBallsAdded 全量刷新负责剩余实体补位
```

UI 不按下标猜测要消除哪个球，优先按 RuntimeId 找实体。

## 超算空间光圈

进入超算空间：

```text
NotifyMatrixModeChanged(true)
-> 所有现有实体 SetAuraActive(true)
```

超算空间中新增球：

```text
新实体创建
-> SetAuraActive(true)
-> 推入动画和光圈同时存在
```

退出超算空间：

```text
NotifyMatrixModeChanged(false)
-> 所有实体 SetAuraActive(false)
```

光圈动画下一步会用 DOTween 循环实现：

```csharp
DOTween.Sequence()
    .SetUpdate(true)
    .SetLoops(-1, LoopType.Restart);
```

`SetUpdate(true)` 使用 unscaled time，避免战斗时间缩放影响 UI 呼吸速度。

当前实体动画细节：

```text
推入：
从目标位置左侧进入，缩放从 spawnScale 到 1，CanvasGroup 淡入。

补位移动：
已有实体按 RuntimeId 复用，移动到新 visibleIndex 对应的位置。

消除：
先放大到 consumePopScale，再缩小到 consumeEndScale，同时淡出。

普通消失：
缩小、淡出，并轻微向左偏移，用于非消球的队列移除。

超算光圈：
Aura_Ring 在 auraMinScale 和 auraMaxScale 之间循环缩放；
AuraGroup 在 auraMinAlpha 和 auraMaxAlpha 之间循环变化。
```

## UI 边界

UI 不做：

- 不判断三消。
- 不判断有没有球。
- 不决定播放哪个技能。
- 不修改 `SkillBallManager` 的球池。

UI 只做：

- 按 RuntimeId 创建实体。
- 按 visibleIndex 计算右到左布局。
- 播放推入、移动、消除、消失、光圈动画。
- 根据 dispatcher 的全量刷新结果保持显示同步。
