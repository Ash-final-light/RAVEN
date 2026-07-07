# 当前造成伤害链路

## 总览

当前项目的攻击伤害不是靠“攻击碰撞体物理撞到目标”直接结算，而是由 `ActionClip` 中的碰撞检测片段驱动范围检测，再把命中的 `Collider` 交给 `SelfCollideManager` 分流。

主链路如下：

```text
ActionClip.collideDetection
ActionPlayer.CheckCollide()
CollideAbility.GetCollideDetectionDelegate(...)
CollideDetectionDelegate.Tick(...)
范围检测扫到目标 Collider
SelfCollideManager.HandleCollide(sliceId, collider)
SelfCollideManager.HandleDamageCollide(...)
BattleManager.Instance.AddDamage(...)
DamageManager.AddDamage(...)
BattleManager.FixedUpdate()
DamageManager.FixedTick()
DamageManager.HandleDamage(...)
CharacterProperty.Modify(...)
ActionStateController.OnCharacterHit(...)
```

## 1. ActionClip 配置碰撞检测

入口数据在 `Assets/Tools/Action/Data/ActionClip.cs`。

`ActionClip` 中有：

```csharp
public ActionCollideDetection collideDetection;
```

每个碰撞片段是：

```csharp
public class ActionCollideDetectionClipData
{
    public string sliceId;
    public ActionCollideDetectionType type;
    public ActionCollideEventType collideEventType = ActionCollideEventType.CauseDamage;
    public CollideHitConfigSO collideHitConfig;
    public int startTick;
    public int durationTicks;
    public int colliderLayerMask;
    public ActionCollideDetectionBaseData data;
}
```

当前造成伤害主要依赖：

- `sliceId`：一次攻击碰撞片段的唯一 ID。
- `collideEventType`：语义上表示碰撞事件类型，默认是 `CauseDamage`。
- `collideHitConfig`：命中后的配置。造成伤害时通常是 `DamageHitConfigSO`。
- `startTick / durationTicks`：攻击范围检测生效帧段。
- `colliderLayerMask`：范围检测会扫哪些层。
- `data`：范围形状数据，例如 Box、Sphere、Sector。

## 2. ActionPlayer 驱动范围检测

运行时入口在 `Assets/Tools/Action/Running/ActionPlayer.cs` 的 `CheckCollide()`。

当动作 Tick 进入某个碰撞片段的生效区间：

```text
_tick >= startTick && _tick <= endTick
```

`ActionPlayer` 会创建或复用一个 `CollideDetectionDelegate`：

```csharp
_stateController.CollideAbility?.GetCollideDetectionDelegate(
    _stateController.transform,
    owner,
    collideDetectionClip.colliderLayerMask,
    collideDetectionClip.data,
    collideDelegateEvent);
```

这里传入的关键信息是：

- 发起者 `_stateController.transform`
- 检测形状 `collideDetectionClip.data`
- 检测层 `collideDetectionClip.colliderLayerMask`
- 命中回调 `collideDelegateEvent`

然后每个有效 Tick 调用：

```csharp
collideDetectionDelegate?.Tick(_collideDelegateEvent[collideDetectionClip.sliceId]);
```

也就是说，真正的命中不是物理碰撞自动触发，而是每帧主动范围检测。

## 3. CollideDetectionDelegate 执行检测

代码在 `Assets/Tools/Action/Data/Collide/CollideDetectionDelegate.cs`。

`CollideDetectionDelegate` 根据 `ActionCollideDetectionBaseData` 的实际类型创建检测对象：

```text
ActionCollideDetectionSelfData   -> BindSelfCollideMono
ActionCollideDetectionBoxData    -> BindBoxCollideMono
ActionCollideDetectionSphereData -> BindSphereCollideMono
ActionCollideDetectionSectorData -> BindSectorCollideMono
```

之后 `Tick()` 会调用：

```csharp
collideObj.Detect(onCollideEvent, _layerMask);
```

例如扇形检测会按 `layerMask` 扫描场景中的 Collider，检测到目标后调用传入的 `onCollideEvent(Collider)`。

## 4. SelfCollideManager 接管命中结果

命中回调是在 `ActionStateController.InitCollide()` 中注册的：

```csharp
_actionPlayer.RegisterCollideEvent(currentId, collider =>
{
    CollideAbility._selfCollideManager.HandleCollide(currentId, collider);
});
```

也就是说，范围检测扫到任意 Collider 后，会进入：

```csharp
SelfCollideManager.HandleCollide(sliceId, collider)
```

代码位置：

`Assets/Tools/Action/Data/Collide/SelfCollideManager.cs`

### 初始化阶段

`SelfCollideManager` 构造时会遍历角色动作容器中所有 `ActionClip`：

```csharp
foreach (var actionClip in character.actionClipContainerSo.ActionClips.Values)
{
    foreach (var collide in actionClip.collideDetection.collideDetectionClips)
    {
        if (collide.collideHitConfig is DamageHitConfigSO damageHitConfig)
        {
            _comboConfigs.Add(collide.sliceId, damageHitConfig);
        }
    }
}
```

当前它只保存：

```csharp
Dictionary<string, DamageHitConfigSO> _comboConfigs
```

所以后续 `HandleCollide()` 能通过 `sliceId` 找到伤害配置，但暂时拿不到完整 `ActionCollideDetectionClipData`。

### 分流阶段

`HandleCollide()` 先做基础校验：

```csharp
if (collider.gameObject == _character.gameObject) return;
if (!_comboConfigs.TryGetValue(sliceId, out var comboConfig)) return;
```

然后根据 `collider.tag` 分流：

```text
Enemy / Character -> HandleDamageCollide(...)
Slide             -> HandleSlideCollide(...)
Parryable         -> HandleParryableCollide(...)
default           -> ignore
```

当前真正造成伤害的是：

```csharp
HandleDamageCollide(sliceId, collider, comboConfig);
```

## 5. HandleDamageCollide 执行伤害前置逻辑

`HandleDamageCollide()` 做三件事。

第一步，确认目标 Collider 上有 `ActionStateController`：

```csharp
if (!collider.TryGetComponent(out ActionStateController targetCharacter)) return;
```

这意味着当前伤害目标必须直接在被扫到的 Collider 所在 GameObject 上挂 `ActionStateController`。如果角色控制器在父级，而 Collider 在子级，这里会拿不到，需要后续改成 `GetComponentInParent<ActionStateController>()`。

第二步，做同一攻击片段的重复命中过滤：

```csharp
if (!CheckMultiHitValid(sliceId, collider, comboConfig)) return;
```

过滤数据结构是：

```csharp
Dictionary<string, List<(GameObject gameObject, float countdown)>> _detectedObjectRecords
```

含义：

- key 是 `sliceId`
- value 是本 slice 已命中的目标列表
- 每个目标有倒计时，倒计时没结束时不会重复命中

倒计时来自：

```csharp
DamageHitConfigSO.colliderDetectionInterval
```

最多命中数量来自：

```csharp
DamageHitConfigSO.colliderDetectionMaximum
```

第三步，播放命中特效/音效/顿帧配置，并把伤害送进战斗系统：

```csharp
PlayHitEffects(targetCharacter, comboConfig);
BattleManager.Instance.AddDamage(
    _character.characterId,
    targetCharacter.characterId,
    comboConfig);
```

这里 `_character` 是攻击发起者，`targetCharacter` 是被命中的目标。

## 6. BattleManager 将伤害入队

代码位置：

`Assets/Tools/Battle/BattleManager.cs`

调用入口：

```csharp
public void AddDamage(
    string attackerObjId,
    string targetObjId,
    DamageHitConfigSO damageHitConfig,
    float critRate = 0f)
```

它会先从 `BattleManager.Data` 里找到双方运行时属性：

```csharp
Data.TryGetValue(attackerObjId, out var characterProperty)
Data.TryGetValue(targetObjId, out var targetProperty)
```

找不到任意一方，就不会继续。

找到后，转交给 `DamageManager.AddDamage()`：

```csharp
damageManager.AddDamage(
    attackerObjId,
    targetObjId,
    characterProperty,
    targetProperty,
    damageHitConfig.hitDamageType,
    damageHitConfig.hitDamage,
    critRate);
```

这里使用的是 `DamageHitConfigSO` 中配置的：

- `hitDamageType`
- `hitDamage`

## 7. DamageManager 创建 DamageInfo

代码位置：

`Assets/Tools/Damage/DamageManager.cs`

`DamageManager.AddDamage()` 不会立刻扣血，而是创建一条 `DamageInfo` 放进队列：

```csharp
_damageInfos.Add(damageInfo);
```

`DamageInfo` 包含：

- `AttackerId`
- `TargetId`
- `AttackerProperty`
- `TargetProperty`
- `Type`
- `Value`
- `CriticalRate`
- `SerialNumber`

注意：这里的伤害还是“期望伤害”，还没有经过 Buff、弱点、免疫、防御、暴击、无敌等结算。

## 8. FixedUpdate 中处理伤害队列

`BattleManager.FixedUpdate()` 每个物理帧调用：

```csharp
damageManager.FixedTick();
```

`DamageManager.FixedTick()` 会处理队列中的所有伤害：

```csharp
while (_damageInfos.Count > 0)
{
    var damageInfo = _damageInfos[0];
    HandleDamage(ref damageInfo);
    _damageInfos.RemoveAt(0);
}
```

## 9. DamageManager.HandleDamage 结算最终资源

`HandleDamage()` 是最终伤害结算核心。

流程如下：

### 9.1 校验双方属性

如果攻击者或目标属性为空，直接返回。

如果目标已死亡，并且当前不是治疗，也直接返回。

### 9.2 触发 Buff

先触发攻击者命中 Buff：

```csharp
_buffManager.OnHitTriggerBuff(ref originDamageInfo);
```

再触发受击者受伤 Buff：

```csharp
_buffManager.OnBeHurtTriggerBuff(ref originDamageInfo);
```

这两个阶段可以修改 `DamageInfo`。

### 9.3 计算最终资源

如果 `DamageType.IgnoreSettlement()`，例如真实伤害，则直接：

```csharp
DamageInfo.CalculateResource(originDamageInfo.Value);
```

否则走算法：

```csharp
_algorithmManager.DamageSettlementCalculateSO.CalculateDamageSettlement(...)
```

当前简单结算逻辑在：

`Assets/Tools/Damage/SO/SimpleDamageSettlementCalculateSO.cs`

它会处理：

- 治疗正负值约束
- 目标弱点加成
- 目标免疫衰减
- 防御减免
- 暴击倍率

最后把最终 `DamageValue` 转成 `DamageResource`：

```csharp
DamageInfo.CalculateResource(finalDamageTuple.damageValue);
```

当前 `CalculateResource()` 的逻辑是：

```csharp
hp = damageValue.Total()
break = damageValue.Total()
```

也就是总伤害同时转换为生命损失和破防积累。

### 9.4 处理无敌和不可破防

如果目标 `immune == true`，且伤害类型不能无视无敌：

```csharp
hp = 0
mp = 0
break = 原 break
```

如果目标 `unbreakable == true`：

```csharp
break = 0
```

### 9.5 计算触发标记

调用：

```csharp
_algorithmManager.DamageTriggerMechanismSO.GetTriggerFlags(originDamageInfo)
```

当前简单机制在：

`Assets/Tools/Damage/SO/SimpleDamageTriggerMechanismSO.cs`

它会设置：

- `BrokenFlag`
- `DefenceFlag`
- `PerfectAttackFlag`
- `PerfectEvadeFlag`

注意：这里的 `PerfectEvadeFlag` 是伤害结算阶段的标记判断，不等同于前面 `SelfCollideManager.HandleSlideCollide()` 的闪避判定入口。后续如果完美闪避要彻底跳过伤害，应该在 `BattleManager.AddDamage(...)` 之前拦截；如果只是想在受伤结算里标记，则可以用这里的 flag。

### 9.6 修改目标资源

最终真正改变血量/破防/mp 的位置是：

```csharp
originDamageInfo.TargetProperty.Modify(originDamageInfo.Resource.Cost());
```

`DamageResource.Cost()` 会把伤害资源转换为角色资源变化：

```csharp
hp = -damageHp
mp = -damageMp
break = +damageBreak
```

然后 `CharacterPropertyExtensions.Modify()` 会调用：

```text
hp > 0     -> AddHealth
hp <= 0    -> DecreaseHealth
break > 0  -> AddBreak
break <= 0 -> RemoveBreak
mp > 0     -> AddMp
mp <= 0    -> RemoveMp
```

所以实际扣血发生在：

```text
DamageManager.HandleDamage()
-> CharacterProperty.Modify(...)
-> DecreaseHealth(...)
```

### 9.7 死亡 Buff 与伤害完成回调

扣资源后，如果目标死亡：

```csharp
_buffManager.OnDieTriggerBuff(ref originDamageInfo);
```

最后调用：

```csharp
AfterDamageHandled?.Invoke(originDamageInfo);
```

这个委托在 `BattleManager.Start()` 里绑定到：

```csharp
afterDamageHandled += ApplyDamage;
```

## 10. BattleManager.ApplyDamage 通知角色

`BattleManager.ApplyDamage(DamageInfo damageInfo)` 会找到目标角色控制器：

```csharp
Controllers.TryGetValue(damageInfo.TargetId, out var actionStateController)
```

然后调用：

```csharp
actionStateController.OnCharacterHit(damageInfo);
```

当前 `TestStateController.OnCharacterHit()` 会：

```csharp
_stateMachine?.TriggerHurt();
```

也就是伤害结算完成后，目标角色进入受击表现。

## 11. 死亡和破防回调

角色进入战斗时通过：

```csharp
BattleManager.JoinBattle(...)
```

创建 `CharacterProperty`，并绑定：

```csharp
OnCharacterDie
OnCharacterBreak
```

如果属性变化导致死亡或破防，最终会回到 `BattleManager`：

```text
ApplyDeath(characterObjId) -> actionStateController.OnCharacterDie()
ApplyBreak(characterObjId) -> actionStateController.OnCharacterBreak()
```

## 关键结论

当前“造成伤害”的真正分界点有两个：

### 命中判定阶段

```text
SelfCollideManager.HandleDamageCollide(...)
```

这里决定“这次范围检测命中是否应该产生一条伤害”。

适合处理：

- 完美闪避提前拦截
- 格挡/弹刀提前拦截
- 同一攻击片段重复命中过滤
- 阵营/目标合法性过滤

### 伤害结算阶段

```text
DamageManager.HandleDamage(...)
```

这里决定“这条伤害最终扣多少资源”。

适合处理：

- Buff 加减伤
- 弱点/免疫
- 防御
- 暴击
- 无敌
- 不可破防
- 最终扣血/破防/mp

如果目标是“完美闪避不受伤并触发魔女时间”，更推荐在 `SelfCollideManager.HandleDamageCollide()` 之前或内部拦截，而不是等到 `DamageManager.HandleDamage()` 后再处理。
