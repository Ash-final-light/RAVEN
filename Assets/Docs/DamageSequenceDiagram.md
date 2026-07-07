# 伤害流程时序图

本文件引用已渲染的 SVG 图，描述从 `SelfCollideManager.HandleCollide` 到 `ActionStateController.OnCharacterHit` 的当前伤害链路。

![伤害流程时序图](DamageSequenceDiagram_Render.svg)

## 说明

- 命中入口：`SelfCollideManager.HandleCollide(sliceId, collider)`
- 是否生成伤害：`SelfCollideManager.HandleDamageCollide(...)`
- 伤害入队：`BattleManager.AddDamage(...)` -> `DamageManager.AddDamage(...)`
- 实际结算：`DamageManager.HandleDamage(...)`
- 最终扣资源：`TargetProperty.Modify(Resource.Cost())`
- 受击通知：`BattleManager.ApplyDamage(...)` -> `ActionStateController.OnCharacterHit(damageInfo)`

## 关键时机

- 攻击者 Buff：`OnHitTriggerBuff(ref DamageInfo)`，发生在最终结算前。
- 受击者 Buff：`OnBeHurtTriggerBuff(ref DamageInfo)`，发生在最终结算前。
- 击杀/死亡 Buff：资源修改后，如果目标死亡，触发 `OnKillTriggerBuff` 和 `OnDieTriggerBuff`。
- 存活判断：当前主要通过 `BattleManager.Data` 是否包含目标 `targetId` 判断。
- `TriggerFlags`：当前只由 `GetTriggerFlags` 计算后写入 `DamageInfo`，后续没有实际消费逻辑，属于 TODO/预留状态。
- 弱点/免疫：`SimpleDamageSettlementCalculateSO` 中已有弱点和免疫倍率计算代码，但角色 Excel 数据当前没有接入对应字段，运行时属性默认不会从表里带出弱点/免疫配置。
- 注意：`unbreakable` 字段目前存在，但 `DamageManager.HandleDamage` 里的破防处理仍是 TODO。
