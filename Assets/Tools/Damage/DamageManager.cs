using System;
using System.Collections.Generic;
using Common;
using Framework.Common.Util;
using Sirenix.OdinInspector;
using Tools.Battle.Data;
using Tools.Buff;
using Tools.Buff.Data;
using UnityEngine;
using VContainer;
using Tools.Battle.Extension;
using Tools.Damage.SO;

namespace Tools.Damage
{
    public class DamageManager
    {
        private BuffManager _buffManager;
        private Tools.Battle.BattleManager _battleManager;
        private AlgorithmManager _algorithmManager;
        
        [ShowInInspector]private readonly List<DamageInfo> _damageInfos = new();

        public event System.Action<DamageInfo> AfterDamageHandled; // 伤害处理回调
        
        public DamageManager(Tools.Battle.BattleManager battleManager,AlgorithmManager algorithmManager,
            BuffManager buffManager,Action<DamageInfo> afterDamageHandled)
        {
            _battleManager = battleManager;
            _algorithmManager = algorithmManager;
            _buffManager = buffManager;
            AfterDamageHandled = afterDamageHandled;
        }
       
        /// <summary>
        /// 请在逻辑帧调用
        /// </summary>
        public void FixedTick()
        {
            while (_damageInfos.Count > 0)
            {
                var damageInfo = _damageInfos[0];
                HandleDamage(ref damageInfo);
                _damageInfos.RemoveAt(0);
            }
        }

        private void OnDestroy()
        {
            _damageInfos.Clear();
        }
        
        /// <summary>
        /// 添加伤害到伤害管理器，交由其处理后续伤害判定
        /// 注意，在添加时，伤害仅是攻击者期望伤害，在后续会经过双方Buff计算->双方属性、弱点和免疫计算—>资源转换，才会对受击者进行资源调整
        /// </summary>
        public void AddDamage(
            string attackerId,
            string targetId,
            CharacterProperty attackerProperty,
            CharacterProperty targetProperty,
            DamageType type,
            DamageValue value,
            float criticalRate,
            int attackerMpGainOnHit = 0,
            bool gainMpOnlyWhenHpDamageApplied = true,
            int attackerSkillBallEnergyGainOnHit = 0,
            bool gainSkillBallEnergyOnlyWhenHpDamageApplied = true,
            DamageHitSourceType hitSourceType = DamageHitSourceType.None,
            int skillBallConsumeCount = 1,
            bool ignoreSideLimit = false
        )
        {
            value = value.Check(type.IsHeal());

            var damageInfo = new DamageInfo
            {
                SerialNumber = Guid.NewGuid().ToString(),
                AttackerId = attackerId,
                TargetId = targetId,
                AttackerProperty = attackerProperty,
                TargetProperty = targetProperty,
                Type = type,
                Value = value,
                AttackerMpGainOnHit = Mathf.Max(0, attackerMpGainOnHit),
                GainMpOnlyWhenHpDamageApplied = gainMpOnlyWhenHpDamageApplied,
                AttackerSkillBallEnergyGainOnHit = Mathf.Max(0, attackerSkillBallEnergyGainOnHit),
                GainSkillBallEnergyOnlyWhenHpDamageApplied = gainSkillBallEnergyOnlyWhenHpDamageApplied,
                HitSourceType = hitSourceType,
                SkillBallConsumeCount = Mathf.Clamp(skillBallConsumeCount, 1, 3),
                CriticalRate = criticalRate,
                IsCritical = false, // 默认为不暴击，等待后续算法判断是否暴击
                //TODO: 流水线后期补齐暴击流程
            };

            _damageInfos.Add(damageInfo);
        }

        /// <summary>
        /// 处理伤害，每帧都会调用
        /// </summary>
        /// <param name="originDamageInfo"></param>
        private void HandleDamage(ref DamageInfo originDamageInfo)
        {
            // 如果不存在来源或目标，就直接返回
            if (originDamageInfo.AttackerProperty == null || originDamageInfo.TargetProperty == null)
            {
                Debug.LogError("AddDamage: attackerProperty or targetProperty is null");
                return;
            }

            // 如果是伤害而目标死亡，也直接返回
            if (!originDamageInfo.Type.IsHeal() && !_battleManager.Data.ContainsKey(originDamageInfo.TargetId))
            {
                Debug.LogError("AddDamage: target is dead");
                return;
            }
            
            // 遍历攻击者Buff列表
            _buffManager.OnHitTriggerBuff(ref originDamageInfo);

            // 遍历受击者Buff列表
            _buffManager.OnBeHurtTriggerBuff(ref originDamageInfo);

            // 在遍历Buff后再根据伤害信息计算最终伤害结算
            if (originDamageInfo.Type.IgnoreSettlement()) // 如果是真实伤害则无视结算
            {
                var finalResource =
                    DamageInfo.CalculateResource(originDamageInfo.Value);
                originDamageInfo.Resource = finalResource;
                
            }
            else // 否则就要经过结算得出最终伤害
            {
                var finalDamageTuple = _algorithmManager.DamageSettlementCalculateSO.CalculateDamageSettlement(
                    originDamageInfo.Type,
                    originDamageInfo.Value,
                    originDamageInfo.CriticalRate,
                    originDamageInfo.AttackerProperty,
                    originDamageInfo.TargetProperty
                );
                // 这里根据最终伤害结算最终资源
                var finalResource =
                    DamageInfo.CalculateResource(finalDamageTuple.damageValue);
                originDamageInfo.Resource = finalResource;
            }

            // 如果受击者处于无敌且攻击不能穿透无敌，则将本次伤害的hp和mp值置空
            if (originDamageInfo.TargetProperty.immune && !originDamageInfo.Type.IgnoreImmune())
            {
                Debug.Log($"immune: {originDamageInfo.TargetProperty.immune}");
                Debug.Log($"ignoreImmune: {originDamageInfo.Type.IgnoreImmune()}");
                originDamageInfo.Resource = new DamageResource
                {
                    hp = 0,
                    mp = 0,
                    @break = 0,
                };
            }

            // TODO：对破防值进行处理

            // 最终调整受击者伤害资源前，进行伤害的机制触发鉴定
            originDamageInfo = originDamageInfo.SetTriggerFlags(
                _algorithmManager.DamageTriggerMechanismSO.GetTriggerFlags(originDamageInfo));

            // 根据最终资源调整受击者的资源，这里是伤害的本质体现
            originDamageInfo.TargetProperty.Modify(originDamageInfo.Resource.Cost());
            ApplyAttackerHitMpGain(ref originDamageInfo);
            MarkAttackerSkillBallEnergyGain(ref originDamageInfo);

            // TODO:显示UI伤害数字

            // 在伤害结算后再判断目标角色是否死亡，是则遍历双方Buff执行击杀相关函数
            if (!originDamageInfo.Type.IsHeal() && originDamageInfo.TargetProperty.dead)
            {
               _buffManager.OnKillTriggerBuff(ref originDamageInfo);
               _buffManager.OnDieTriggerBuff(ref originDamageInfo);
            }
            
            // 最终执行伤害处理的事件委托
            AfterDamageHandled?.Invoke(originDamageInfo);
        }

        private static void ApplyAttackerHitMpGain(ref DamageInfo damageInfo)
        {
            if (damageInfo.AttackerMpGainOnHit <= 0)
            {
                return;
            }

            if (damageInfo.Type.IsHeal() ||
                damageInfo.AttackerProperty == null ||
                damageInfo.TargetProperty == null ||
                damageInfo.AttackerId == damageInfo.TargetId)
            {
                return;
            }

            bool hasEffectiveHit = !damageInfo.GainMpOnlyWhenHpDamageApplied ||
                                   damageInfo.Resource.hp > 0 ||
                                   damageInfo.Resource.@break > 0;
            if (!hasEffectiveHit)
            {
                return;
            }

            damageInfo.AttackerMpBeforeGain = damageInfo.AttackerProperty.CurrentMp;
            damageInfo.AttackerProperty.Modify(new CharacterResource
            {
                mp = damageInfo.AttackerMpGainOnHit
            });
            damageInfo.AttackerMpAfterGain = damageInfo.AttackerProperty.CurrentMp;
            damageInfo.AttackerMpChanged = damageInfo.AttackerMpAfterGain != damageInfo.AttackerMpBeforeGain;
        }

        private static void MarkAttackerSkillBallEnergyGain(ref DamageInfo damageInfo)
        {
            if (damageInfo.AttackerSkillBallEnergyGainOnHit <= 0)
            {
                return;
            }

            if (damageInfo.Type.IsHeal() ||
                damageInfo.AttackerProperty == null ||
                damageInfo.TargetProperty == null ||
                damageInfo.AttackerId == damageInfo.TargetId)
            {
                return;
            }

            bool hasEffectiveHit = !damageInfo.GainSkillBallEnergyOnlyWhenHpDamageApplied ||
                                   damageInfo.Resource.hp > 0 ||
                                   damageInfo.Resource.@break > 0;
            damageInfo.AttackerSkillBallEnergyGained = hasEffectiveHit;
        }
    }
}
