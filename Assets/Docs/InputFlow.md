# 输入流程说明

本文档说明当前项目中玩家输入从 Unity Input System 到角色状态机的处理流程。

![输入流程图](InputFlow.svg)

## 核心链路

```text
Unity PlayerInput
-> PlayerInputManager
-> InputBuffer
-> PlayerCharacterBrain
-> PlayerParameters
-> PlayerStateMachine
```

## PlayerInputManager

`PlayerInputManager` 是全局输入管理入口，封装 Unity `PlayerInput`。

主要职责：

- 持有当前 `PlayerInput`。
- 切换 ActionMap。
- 提供 `WasPerformedThisFrame`、`IsPressed`、`GetInputAction`。
- 管理 started / performed / canceled 回调。
- 切换 ActionMap 后重新绑定同名 Action 的事件，避免回调丢失。

ActionMap 切换由 `GameManager.SetGameStatus()` 控制：

```text
Playing -> Player
其他状态 -> UI
```

## InputBuffer

`InputBuffer` 负责预输入缓存。

当前注册输入：

```text
PlayerInteract
PlayerAttack
PlayerSlide
PlayerUltimate
SkillBall0 ~ SkillBall7
```

工作方式：

- `performed` 时记录输入。
- `canceled` 时给输入设置缓存时间。
- 每帧 `Tick(deltaTime)` 递减缓存时间。
- `WasPerformedThisFrame(inputName)` 用于读取缓存输入。
- `ConsumeAction(inputName)` 在输入被使用后清除缓存。

当前 `onlyLastInputPerformed = true`，表示只保留最后一次有效输入。

## PlayerCharacterBrain

`PlayerCharacterBrain` 每个渲染帧执行输入处理。

核心流程：

```text
UpdateParameters()
-> 清空上一帧输入标记
-> InputBuffer.Tick(deltaTime)
-> GetPlayerInput()
-> HandlePlayerInteract()
-> CalculatePlayerInput()
```

它不会直接播放动作，而是写入 `PlayerParameters`。

写入内容包括：

- `isAttackInFrame`
- `isSlideInFrame`
- `isUltimateInFrame`
- `isSkillBallInFrame`
- `skillBallInputIndex`
- `playerInputRawValueInFrame`
- `playerInputMovementInFrame`
- `playerInputCharacterMovementInFrame`

技能球输入会先调用：

```text
SkillBallManager.TryPrepareUseBall(visibleIndex)
```

成功后才设置技能球本帧输入。

## 动作阶段对输入的影响

`PlayerCharacterBrain` 会监听 `ActionPlayer` 阶段事件：

```text
Start / Stop
-> 禁止输入

Anticipation
-> 允许预输入

Judgment
-> 允许输入
-> 忽略状态优先级
-> 打开 combo_window

Recovery
-> 允许输入
-> 忽略状态优先级
-> 关闭 combo_window

End
-> 允许输入
```

`combo_window` 写入角色 `RunningBlackboard`，供连招表条件判断使用。

## 状态机消费输入

`PlayerStateMachine` 读取 `PlayerParameters`，按优先级处理：

```text
大招
-> 技能球
-> 普攻
-> 闪避
-> 移动
```

输入本身不直接执行动作。最终是否播放动作，由状态机、资源条件、连招表和当前状态共同决定。

