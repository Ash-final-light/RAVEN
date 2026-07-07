using System;
using System.Collections.Generic;
using Common;
using Common.Constant;
using Game.TimeScale;
using Sirenix.OdinInspector;
using Tools.Battle.Data;
using Tools.Battle.Extension;
using Tools.Buff;
using Tools.Damage;
using Tools.Damage.SO;
using Tools.DesignMode;
using Tools.InputHandle;
using Tools.Skill;
using UnityEngine;
using VContainer;

namespace Tools.Battle
{
    /// <summary>
    /// 全局战斗管理器(所有数据都在这里存储）
    /// </summary>
    public class BattleManager : MonoSingleton<BattleManager>
    {
        private Action<DamageInfo> afterDamageHandled;
        
        private Action<string> OnCharacterDie;
        private Action<string> OnCharacterBreak;
        public event Action<CharacterProperty> OnCharacterAdded;
        public event Action<CharacterProperty, DamageInfo> OnCharacterResourceChanged;
        public event Action<string> OnCharacterRemoved;
        
        /// <summary>
        /// 所有存活角色的数据,key-场景中物体id
        /// </summary>
        [ShowInInspector][ReadOnly]public Dictionary<string,CharacterProperty> Data = new();
        [ShowInInspector][ReadOnly]public Dictionary<string,ActionStateController> Controllers = new();
        /// <summary>
        /// 所有物体基础信息字典（即可以存在多个）
        /// </summary>
        private CharacterInfoDataContainer characterInfoDataContainer =>
            GameApplication.Instance.ExcelBinaryManager.GetContainer<CharacterInfoDataContainer>();
        [Inject,ShowInInspector][ReadOnly] private BuffManager buffManager;
        [Inject] private AlgorithmManager algorithmManager;
        [ShowInInspector][ReadOnly]private DamageManager damageManager;
        [ShowInInspector][ReadOnly]private bool isWitchTimeActive;
        private void Start()
        {
            afterDamageHandled += ApplyDamage;
            OnCharacterDie += ApplyDeath;
            OnCharacterBreak += ApplyBreak;
            damageManager = new DamageManager(this,algorithmManager,buffManager,afterDamageHandled);
            RegisterWitchTimeEvents();
            InvokeRepeating(nameof(UpdateBuff),0,1f);
        }

        private void OnDestroy()
        {
            UnregisterWitchTimeEvents();
        }
        private void UpdateBuff()
        {
            if(buffManager == null || buffManager.IsDisposing)
            {
                return;
            }
            buffManager?.UpdateBuff(1f);
        }

        private void FixedUpdate()
        {
            damageManager.FixedTick();
        }

        /// <summary>
        /// 加入战斗
        /// </summary>
        /// <param name="attackerObjId"></param>
        /// <param name="infoId"></param>
        public CharacterProperty JoinBattle(bool isPlayer = false,string attackerObjId = "test1",int infoId = 3001,ActionStateController actionStateController = null)
        {
            if(Data.ContainsKey(attackerObjId))
            {
                Debug.LogError($"角色{attackerObjId}已经在战斗中");
                return null;
            }
            else
            {
                // 从excel中获取角色基础信息
                var characterInfoData = characterInfoDataContainer.Data[infoId];
                if(characterInfoData == null)
                {
                    Debug.LogError($"角色{infoId}不在excel中");
                    return null;
                }
                // 创建角色属性
                var characterProperty = characterInfoData.ToCharacterProperty
                    (attackerObjId,OnCharacterDie,OnCharacterBreak,actionStateController?.transform);
                characterProperty.isPlayer = isPlayer;
                Data.Add(attackerObjId,characterProperty);
                Controllers.Add(attackerObjId,actionStateController);
                OnCharacterAdded?.Invoke(characterProperty);
                return characterProperty;
            }
        }
        [Button("添加buff")]
        public void AddBuff(string attackerObjId = "test",string targetObjId = "test",int buffId = 2003,int stack = 1)
        {
            if(!Data.TryGetValue(attackerObjId,out var characterProperty) || !Data.TryGetValue(targetObjId,out var targetProperty))
            {
                Debug.LogError($"角色{attackerObjId}或{targetObjId}不在战斗中");
                return;
            }
            buffManager.CreateBuff(attackerObjId,targetObjId,buffId,stack);
        }
        public CharacterProperty GetCharacterProperty(string characterObjId)
        {
            return Data[characterObjId];
        }

        public bool TryConsumeAllMpForUltimate(string characterObjId)
        {
            if (string.IsNullOrEmpty(characterObjId) ||
                !Data.TryGetValue(characterObjId, out CharacterProperty characterProperty) ||
                characterProperty == null)
            {
                return false;
            }

            int maxMp = characterProperty.GetCurrentMaxMp();
            if (characterProperty.CurrentMp < maxMp)
            {
                return false;
            }

            characterProperty.ResetMp();
            OnCharacterResourceChanged?.Invoke(characterProperty, default);
            return true;
        }
        
        [Button("处理伤害")]
        private void TestTickDamage()
        {
            damageManager.FixedTick();
        }
        public void AddDamage(string attackerObjId,string targetObjId,DamageHitConfigSO damageHitConfig,float critRate = 0f)
        {
            if(!Data.TryGetValue(attackerObjId,out var characterProperty) || 
               !Data.TryGetValue(targetObjId,out var targetProperty))
            {
                Debug.LogError($"角色{attackerObjId}或{targetObjId}不在战斗中");
                return;
            }
            if (ShouldIgnoreDamageDuringWitchTime(characterProperty))
            {
                Debug.Log($"[WitchTime] Non-player damage ignored. attacker={attackerObjId}, target={targetObjId}");
                return;
            }

            damageManager.AddDamage(attackerObjId,targetObjId,characterProperty,targetProperty,
                damageHitConfig.hitDamageType,damageHitConfig.hitDamage,
                critRate,
                damageHitConfig.attackerMpGainOnHit,
                damageHitConfig.gainMpOnlyWhenHpDamageApplied,
                damageHitConfig.attackerSkillBallEnergyGainOnHit,
                damageHitConfig.gainSkillBallEnergyOnlyWhenHpDamageApplied,
                damageHitConfig.hitSourceType,
                damageHitConfig.skillBallConsumeCount
                );
        }

        private bool ShouldIgnoreDamageDuringWitchTime(CharacterProperty attackerProperty)
        {
            return isWitchTimeActive &&
                   attackerProperty != null &&
                   !attackerProperty.isPlayer;
        }

        private void RegisterWitchTimeEvents()
        {
            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null)
            {
                return;
            }

            eventCenter.AddEventListener<WitchTimeRuntimeInfo>(GlobalEvents.OnWitchTimeStarted, HandleWitchTimeStarted);
            eventCenter.AddEventListener<WitchTimeRuntimeInfo>(GlobalEvents.OnWitchTimeStopped, HandleWitchTimeStopped);
        }

        private void UnregisterWitchTimeEvents()
        {
            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null)
            {
                return;
            }

            eventCenter.RemoveEventListener<WitchTimeRuntimeInfo>(GlobalEvents.OnWitchTimeStarted, HandleWitchTimeStarted);
            eventCenter.RemoveEventListener<WitchTimeRuntimeInfo>(GlobalEvents.OnWitchTimeStopped, HandleWitchTimeStopped);
        }

        private void HandleWitchTimeStarted(WitchTimeRuntimeInfo info)
        {
            isWitchTimeActive = true;
        }

        private void HandleWitchTimeStopped(WitchTimeRuntimeInfo info)
        {
            isWitchTimeActive = false;
        }

        private void ApplyDamage(DamageInfo damageInfo)
        {
            // 伤害处理完毕，这里进行视觉操作
            OnCharacterResourceChanged?.Invoke(damageInfo.TargetProperty, damageInfo);
            if (damageInfo.AttackerMpChanged && damageInfo.AttackerProperty != null)
            {
                OnCharacterResourceChanged?.Invoke(damageInfo.AttackerProperty, damageInfo);
            }

            ApplySkillBallEnergyGain(damageInfo);

            if(Controllers.TryGetValue(damageInfo.TargetId,out var actionStateController))
            {
                actionStateController.OnCharacterHit(damageInfo);
            }
        }

        private void ApplySkillBallEnergyGain(DamageInfo damageInfo)
        {
            if (!damageInfo.AttackerSkillBallEnergyGained ||
                damageInfo.AttackerSkillBallEnergyGainOnHit <= 0)
            {
                return;
            }

            if (!Controllers.TryGetValue(damageInfo.AttackerId, out var attackerController) ||
                attackerController == null)
            {
                return;
            }

            attackerController.SkillBallPitcher.AddEnergy(
                damageInfo.AttackerSkillBallEnergyGainOnHit,
                new SkillBallEnergySourceContext(damageInfo));
        }
        private void ApplyDeath(string characterObjId)
        {
            if(Controllers.TryGetValue(characterObjId,out var actionStateController))
            {
                Data.Remove(characterObjId);
                Controllers.Remove(characterObjId);
                OnCharacterRemoved?.Invoke(characterObjId);
                actionStateController.OnCharacterDie();
            }
        }
        private void ApplyBreak(string characterObjId)
        {
            if(Controllers.TryGetValue(characterObjId,out var actionStateController))
            {
                actionStateController.OnCharacterBreak();
            }
        }
    }
}
