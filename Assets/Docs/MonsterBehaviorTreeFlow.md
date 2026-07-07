# 怪物行为树与 ActionStateController 流程图

本文说明 Beast01 怪物当前的行为树如何和 `ActionStateController`、状态机、`ActionPlayer` 混合工作。

![怪物行为树流程图](MonsterBehaviorTreeFlow.svg)

## 核心流程

```text
Beast01BehaviourRunnner.Update
-> Beast01Status.UpdateBlackboard
-> BehaviourTree.Tick
-> 条件节点读取 playerDistance
-> Beast01 专用行为节点决定移动、转向、攻击
-> Beast01Status 作为桥接层调用状态机
-> TestBeast01StateMachine 检查 CD、状态优先级、动作资源
-> BaseStateMachine / ActionPlayer 播放 ActionClip
-> ActionClip 驱动动画、碰撞、事件、音效、特效
-> 命中后进入伤害系统
-> 受击目标 OnCharacterHit，再由状态机尝试切受击动作
```

## Mermaid 版本

```mermaid
flowchart TD
    A["Beast01BehaviourRunnner.Update"] --> B["Beast01Status.UpdateBlackboard"]
    B --> C["写入黑板: playerDistance"]
    C --> D["runningTree.Tick(deltaTime * behaviourTreeScale)"]
    D --> E["行为树组合节点: Selector / Sequence"]
    E --> F["黑板条件节点判断距离等条件"]
    F --> G{"选择 Beast01 行为节点"}
    G --> H["MoveToTarget / OrbitKeepDistance"]
    G --> I["TurnToTargetJump"]
    G --> J["PlayAction"]
    H --> K["Beast01Status.MoveDirection / FaceTarget"]
    I --> K
    J --> L["Beast01Status.TryToPlayAction"]
    K --> L
    L --> M["TestBeast01StateMachine.TryPlayAction"]
    M --> N{"AgentCDController.CanExecute"}
    N -- "否" --> O["返回 Failure / 保持当前动作"]
    N -- "是" --> P{"ActionClip 是否存在"}
    P -- "否" --> O
    P -- "是" --> Q{"CanTransitionTo 状态优先级"}
    Q -- "否" --> O
    Q -- "是" --> R["BaseStateMachine.TryPlayAction"]
    R --> S["ActionPlayer.SetActionClipAndPlay"]
    S --> T["ActionClip Tick"]
    T --> U["动画 / 位移 / 碰撞 / 事件 / 音效 / 特效"]
    U --> V["SelfCollideManager -> BattleManager -> DamageManager"]
    V --> W["目标 ActionStateController.OnCharacterHit"]
    W --> X["StateMachine.TriggerHurt(hit01)"]
```

## 关键边界

- 行为树负责决策，不直接执行底层动画、碰撞和伤害。
- `Beast01Status` 是行为树和角色控制器之间的桥。
- `TestBeast01StateMachine` 负责怪物动作 CD、状态切换合法性和自动回 idle。
- `ActionStateController` 和 `ActionPlayer` 负责真正执行 ActionClip。
- 受击不由行为树主动处理，而是伤害系统回调 `OnCharacterHit` 后，由状态机尝试切到 `hit01`。

