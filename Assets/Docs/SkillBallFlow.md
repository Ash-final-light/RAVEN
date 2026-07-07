# 技能球流程说明

本文档说明当前项目中技能球从生成、输入、消除、动作执行到伤害配置生效的流程。

![技能球流程图](SkillBallFlow.svg)

## 核心流程

```text
命中获得技能球能量
-> SkillBallPitcher 累积能量
-> ISkillBallSpawnProvider 决定生成颜色和消除模式
-> SkillBallManager 存入球池并刷新 UI
-> 玩家按下可见槽位
-> SkillBallManager 预解析一消/二消/三消
-> PlayerStateMachine 查询连招表
-> 动作成功进入 Start 阶段
-> 正式删除技能球并写入 ActionRuntimeContext
-> 命中时 SelfCollideManager 按消除数选择 DamageHitConfigSO
```

## 生成配置

技能球由 `SkillBallPitcher` 根据能量生成。能量达到 `skillBallEnergyPerBall` 后，会调用角色身上的 `ISkillBallSpawnProvider`。

当前主要规则：

- `DefaultSkillBallSpawnProvider`：默认随机生成红、黄、蓝；可配置是否包含白球。
- `AlphaSkillBallSpawnProvider`：普通状态生成红、黄、蓝；剑意状态只生成白球。
- 红、黄、蓝默认使用 `AlwaysTripleCheck`。
- 白球使用 `AlwaysSingle`。

## 消除规则

消除规则由 `SkillBallConsumeMode` 决定：

- `AlwaysTripleCheck`：检查左右连续同色，最多三消。
- `TripleCheckOnlyOutsideMatrix`：非矩阵模式检查三消，矩阵模式单消。
- `AlwaysSingle`：永远单消。

矩阵模式下，非白球的 `SignalCount` 会按 3 处理，用于连招和伤害配置。

## 输入和连招

玩家按下 `SkillBall0` 到 `SkillBall7`，或点击 UI 可见槽位后，会调用：

```text
SkillBallManager.TryPrepareUseBall(visibleIndex)
```

这一步只是预消除，不会立刻删球。预消除结果会生成连招输入名：

```text
SkillBallRed1 / SkillBallRed2 / SkillBallRed3
SkillBallYellow1 / SkillBallYellow2 / SkillBallYellow3
SkillBallBlue1 / SkillBallBlue2 / SkillBallBlue3
SkillBallWhite1
```

随后状态机用：

```text
当前动作名 + 技能球输入名
```

去 `ComboInfoContainer` 的 `Input2ActionTable` 中查询要播放的技能动作。

## 正式消除

技能球不会在按键瞬间删除。只有状态机找到技能动作，并且动作成功进入 `ActionPlayer.OnStartStage` 后，才会执行：

```text
SkillBallManager.CommitPreparedConsume()
```

正式消除时会：

- 删除对应技能球。
- 通知 UI 播放消除动画。
- 刷新可见球列表。
- 写入 `ActionPlayer.RuntimeContext`：
  - `ConsumedSignalColor`
  - `ConsumedSignalInputName`
  - `ConsumedSignalCount`
  - `ConsumedSkillActionName`
  - `ConsumedSignalRuntimeIds`

## 伤害配置生效

技能动作的 `ActionClip` 碰撞片段可以配置：

```text
collideHitConfig
signalBallDamageConfigs
```

命中时：

```text
SelfCollideManager.TryResolveDamageConfig(sliceId)
-> 读取 RuntimeContext.ConsumedSignalCount
-> 优先查 signalBallDamageConfigs
-> 找不到则回退 collideHitConfig
```

因此技能球配置分两层：

- 连招表：决定几消输入播放哪个技能动作。
- 碰撞伤害配置：决定该动作在一消、二消、三消时使用哪个 `DamageHitConfigSO`。

