# 战双战斗界面 UI 与逻辑分析

> 分析对象：截图中的《战双帕弥什》战斗 HUD。  
> 目标：按照当前项目的 UI 设计框架，整理界面结构、模块职责、状态逻辑与 Unity 实现思路。

## 1. UI 总体框架

当前战斗 UI 可以拆成三层主框架：

```text
BattleHUDCanvas
├── TopLayer
│   ├── PlayerStatus
│   ├── EnemyStatus
│   └── TimerAndExit
├── MiddleLayer
│   ├── CharacterSwitchGroup
│   └── LockTargetButton
├── BottomLayer
│   ├── MoveJoystick
│   └── CombatActionGroup
```

三层职责如下：

| 层级 | 主要职责 | 对应区域 |
| --- | --- | --- |
| `TopLayer` | 展示战斗状态、敌人血量、时间、退出入口 | 屏幕顶部 |
| `MiddleLayer` | 展示切人、锁定等中频操作 | 屏幕右侧中部 |
| `BottomLayer` | 展示移动、攻击、闪避、技能等高频操作 | 屏幕底部两侧 |

这种结构的核心是“中央战斗区留空”。角色、敌人、攻击范围和受击反馈都发生在画面中心，因此 HUD 主要贴边布局，避免遮挡战斗判断。

## 2. TopLayer 顶部状态层

### 2.1 PlayerStatus 玩家状态

位置：左上角。  
功能：显示玩家角色当前生命、能量积攒与性能状态。

截图中左上角包含：

- `6854/6854`：当前生命值 / 最大生命值。
- 红白色上层条：生命或生命相关状态条。
- 蓝色下层条：角色能量积攒条。
- `FPS 30`：当前帧率显示。
- 绿色竖条：帧率或性能状态的视觉辅助。

#### 蓝色能量条逻辑

蓝色条不是装饰，也不是敌人信息，而是玩家角色的能量积攒表现。它的长度或颜色比例会根据角色当前能量变化。

建议逻辑字段：

```text
currentEnergy 当前能量
maxEnergy     最大能量
energyRatio   当前能量比例 = currentEnergy / maxEnergy
```

表现规则：

- `energyRatio = 0`：蓝色条为空或只显示暗色底条。
- `0 < energyRatio < 1`：蓝色条按比例增长。
- `energyRatio = 1`：蓝色条满格，可增加发光、闪烁或边缘高亮。
- 能量变化时不要瞬间跳变，建议用插值动画平滑过渡。

Unity 实现建议：

```csharp
energyFill.fillAmount = currentEnergy / maxEnergy;
```

如果蓝条是斜向 HUD 条，可以使用以下方式实现：

- 使用 `Image.type = Filled` 表示填充比例。
- 使用带斜切透明区域的 Sprite。
- 或使用 `Mask` / `RectMask2D` 裁切能量条宽度。
- 能量满时启用 `Glow` 子节点或播放短动画。

#### PlayerStatus 组件职责

建议拆分为：

```text
PlayerStatus
├── HpText
├── HpBar
├── EnergyBar
├── EnergyGlow
└── FpsGroup
```

逻辑职责：

- 监听玩家生命变化，更新 `HpText` 与 `HpBar`。
- 监听玩家能量变化，更新 `EnergyBar` 的比例和颜色状态。
- 当能量满时触发可释放提示。
- 按需更新 FPS 文本。

### 2.2 EnemyStatus 敌人状态

位置：顶部中央。  
功能：显示当前锁定或当前战斗目标的名称、血量管与剩余血条数量。

截图中顶部中央包含：

- 敌人名称：`园丁（基础型）`。
- 横向白色长条：敌人当前血量管的剩余血量。
- `x5`：敌人剩余血量管数量。
- 淡色图标：敌人类型、阵营或目标标记装饰。

#### `x5` 血量管逻辑

`x5` 表示敌人当前还剩多少管血，不是连击倍率，也不是分数倍率。

可以理解为：

```text
敌人总血量被拆成多管。
顶部白色长条显示当前这一管的剩余比例。
x5 表示当前还剩 5 管血。
```

示例：

```text
enemyHpPerBar      每管血量
enemyCurrentHp     敌人当前总血量
enemyTotalBars     敌人总血条管数
enemyRemainBars    当前剩余血量管数
currentBarRatio    当前血量管内比例
```

推荐计算方式：

```text
enemyRemainBars = ceil(enemyCurrentHp / enemyHpPerBar)
currentBarHp = enemyCurrentHp % enemyHpPerBar
currentBarRatio = currentBarHp / enemyHpPerBar
```

需要注意：

- 如果 `enemyCurrentHp` 正好是整管血，`currentBarRatio` 应显示为 `1`，而不是 `0`。
- 当敌人从一管血扣到下一管时，顶部血条应重新从满条开始扣。
- `x5` 应随剩余血管数减少，例如 `x5 -> x4 -> x3`。

更稳妥的伪代码：

```csharp
int remainBars = Mathf.CeilToInt(enemyCurrentHp / enemyHpPerBar);
float hpInsideCurrentBar = enemyCurrentHp - (remainBars - 1) * enemyHpPerBar;
float currentBarRatio = hpInsideCurrentBar / enemyHpPerBar;

barFill.fillAmount = currentBarRatio;
barCountText.text = "x" + remainBars;
```

#### EnemyStatus 组件职责

建议拆分为：

```text
EnemyStatus
├── EnemyNameText
├── EnemyHpBarBack
├── EnemyHpBarFill
├── EnemyBarCountText
└── EnemyTypeIcon
```

逻辑职责：

- 当前目标变化时，刷新敌人名称和血量数据。
- 敌人受击时，更新当前血量管比例。
- 血量跨管时，播放扣管动画并更新 `xN`。
- 敌人死亡时隐藏或切换到下一个目标。

### 2.3 TimerAndExit 时间与系统入口

位置：右上角。  
功能：显示关卡计时与退出/暂停按钮。

截图中包含：

- `TIME 00:17`：关卡计时。
- `Esc`：退出或暂停入口。

逻辑职责：

- 战斗开始后计时递增。
- 暂停时停止计时显示或保持当前时间。
- 按下 `Esc` 时打开暂停菜单、退出确认或系统面板。

建议结构：

```text
TimerAndExit
├── TimeLabel
├── TimeText
└── ExitButton
```

## 3. MiddleLayer 中部操作层

### 3.1 CharacterSwitchGroup 角色切换组

位置：右侧中部。  
功能：展示队伍角色头像、切换按键与当前可用状态。

截图中包含两个圆形头像：

- 上方头像绑定 `Q`。
- 下方头像绑定 `E`。

建议结构：

```text
CharacterSwitchGroup
├── CharacterSwitchButton_Q
│   ├── Avatar
│   ├── CooldownMask
│   ├── HighlightRing
│   └── KeyHint
└── CharacterSwitchButton_E
    ├── Avatar
    ├── CooldownMask
    ├── HighlightRing
    └── KeyHint
```

逻辑职责：

- 根据队伍角色数据更新头像。
- 根据切换冷却显示遮罩。
- 可切换时显示普通高亮。
- QTE 或特殊可触发时使用更强的黄色/金色发光。
- 按下 `Q` / `E` 时触发角色切换或 QTE 技能。

### 3.2 LockTargetButton 锁定按钮

位置：右侧中部偏下。  
功能：目标锁定、切换锁定目标或取消锁定。

截图中表现为准星按钮，按键提示为 `Tab`。

建议结构：

```text
LockTargetButton
├── AimIcon
├── LockStateRing
└── KeyHint_Tab
```

逻辑职责：

- 未锁定目标时：显示普通准星。
- 已锁定目标时：准星高亮或显示锁定环。
- 按下 `Tab`：锁定最近敌人、切换目标或取消锁定。
- 目标死亡/离开范围时自动解除或重新选择目标。

## 4. BottomLayer 底部操作层

### 4.1 MoveJoystick 移动摇杆

位置：左下角。  
功能：展示移动输入区域与 `W/A/S/D` 键盘提示。

截图中摇杆为大圆盘 + 中心点，周围有 `W/A/S/D` 键帽。

建议结构：

```text
MoveJoystick
├── JoystickBack
├── JoystickHandle
├── KeyHint_W
├── KeyHint_A
├── KeyHint_S
└── KeyHint_D
```

逻辑职责：

- PC 端：主要作为移动输入提示。
- 移动端：可扩展为虚拟摇杆。
- 移动输入发生时，可轻微偏移 `JoystickHandle`。
- 无输入时，中心点回到原位。

### 4.2 CombatActionGroup 战斗操作组

位置：右下角。  
功能：承载普攻、闪避、技能、资源数量等高频战斗操作。

截图中包含：

- 普攻按钮：右下角大圆按钮，键位 `J`。
- 闪避按钮：偏中下大圆按钮，键位 `K`。
- 技能按钮：数字键 `1/2/3/4`。
- 技能资源或弹药数量：如 `5/16`。
- 技能冷却/禁用状态：通过灰暗、红色遮罩、发光区分。

建议结构：

```text
CombatActionGroup
├── DodgeButton
├── AttackButton
├── SkillButton_1
├── SkillButton_2
├── SkillButton_3
├── SkillButton_4
└── ResourceText
```

#### 技能按钮通用逻辑

每个技能按钮建议统一字段：

```text
skillId
icon
keyCode
cooldown
cooldownRemain
cost
isUnlocked
isUsable
count
```

显示规则：

- 可用：图标清晰，外圈正常或轻微发光。
- 冷却中：显示径向冷却遮罩和倒计时。
- 资源不足：图标变暗，必要时显示红色提示。
- 未解锁/禁用：灰色遮罩或红色禁用标识。
- 可强化/可连携：外圈高亮或播放脉冲动画。

Unity 实现建议：

```text
BattleSkillButton
├── Icon
├── CooldownMask
├── CooldownText
├── KeyHint
├── CountText
├── DisableOverlay
└── Glow
```

按钮刷新流程：

```text
1. 根据技能数据设置图标和键位。
2. 根据 cooldownRemain 更新冷却遮罩。
3. 根据 isUsable 设置亮度和禁用层。
4. 根据 count 或资源数量更新角标。
5. 状态从不可用变为可用时播放一次高亮动画。
```

## 5. 数据驱动建议

为了避免 UI 写死，推荐 HUD 只负责显示，战斗系统负责提供数据。

建议数据来源：

```text
PlayerCombatState
├── CurrentHp
├── MaxHp
├── CurrentEnergy
├── MaxEnergy
└── Fps

EnemyCombatState
├── EnemyName
├── CurrentHp
├── MaxHp
├── HpPerBar
└── EnemyType

TeamCombatState
├── CharacterSlots
├── CurrentCharacter
├── SwitchCooldown
└── QteAvailable

SkillCombatState
├── SkillId
├── CooldownRemain
├── CooldownTotal
├── Cost
├── Count
└── IsUsable
```

HUD 通过事件刷新：

```text
OnPlayerHpChanged
OnPlayerEnergyChanged
OnEnemyTargetChanged
OnEnemyHpChanged
OnSkillStateChanged
OnCharacterSwitchStateChanged
OnBattleTimerChanged
```

这样做的好处：

- UI 不直接计算战斗规则，只消费战斗数据。
- 以后替换美术资源时不影响战斗逻辑。
- 同一套 HUD 组件可以复用到不同角色、不同敌人、不同关卡。

## 6. 视觉与反馈重点

整体视觉关键词：

- 冷灰背景。
- 白色 HUD 文字。
- 蓝色表示玩家能量积攒。
- 黄色/金色表示可触发、当前激活或高价值状态。
- 红色表示危险、禁用、攻击性反馈。
- 半透明圆形按钮用于操作。
- 斜切线条用于顶部科技感状态区。

关键反馈：

- 玩家能量增长时，蓝条平滑填充。
- 玩家能量满时，蓝条或边缘出现短促高亮。
- 敌人掉血时，顶部白色血条减少。
- 敌人掉一整管血时，`xN` 数字减少，并可播放轻微跳动动画。
- 技能冷却结束时，按钮闪一下，提示可使用。
- 角色可切换或 QTE 可用时，头像外圈发光。
- 锁定目标成功时，准星按钮状态改变。

## 7. 当前项目落地总结

如果按照你的 UI 设计框架实现，推荐优先搭建以下模块：

```text
TopLayer
├── PlayerStatus：生命、蓝色能量积攒、FPS
├── EnemyStatus：敌人名、当前血量管、剩余血量管 xN
└── TimerAndExit：计时、Esc

MiddleLayer
├── CharacterSwitchGroup：Q/E 角色头像与切换状态
└── LockTargetButton：Tab 锁定

BottomLayer
├── MoveJoystick：WASD 移动提示
└── CombatActionGroup：普攻、闪避、技能、资源数量
```

核心逻辑修正：

- 左上角蓝色条是玩家角色能量积攒条，颜色/填充比例随角色能量变化。
- 顶部中央 `x5` 是敌人剩余血量管数量，和当前白色血条共同表现敌人总血量。
- UI 层级应服务于战斗操作频率：顶部看状态，中部管目标和切人，底部负责高频输入。

