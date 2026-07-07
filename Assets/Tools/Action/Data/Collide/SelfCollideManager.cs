using System.Collections.Generic;
using System.Linq;
using TimeScale;
using TimeScale.Command;
using Tools.Action.Data;
using Tools.Action.Data.Slide;
using Tools.Battle;
using Tools.Camera;
using Tools.Damage.SO;
using Tools.InputHandle;
using Tools.TimeScale.Command;
using UnityEngine;

namespace Tools.Action.Data.Collide
{
    public class SelfCollideManager
    {
        public readonly Dictionary<string, DamageHitConfigSO> _comboConfigs = new();

        private readonly Dictionary<string, ActionCollideDetectionClipData> _collideClipConfigs = new();

        private readonly Dictionary<string, List<(GameObject gameObject, float countdown)>> _detectedObjectRecords =
            new();

        private readonly ActionStateController _character;

        public SelfCollideManager(ActionStateController character)
        {
            _character = character;

            foreach (var actionClip in character.actionClipContainerSo.ActionClips.Values)
            {
                foreach (var collide in actionClip.collideDetection.collideDetectionClips)
                {
                    _collideClipConfigs[collide.sliceId] = collide;

                    if (collide.collideHitConfig is DamageHitConfigSO damageHitConfig)
                    {
                        _comboConfigs[collide.sliceId] = damageHitConfig;
                        ////Debug.Log($"[Collide_Config] Slice {collide.sliceId} damage config registered.");
                    }
                }
            }
        }

        public void Tick(float deltaTime)
        {
            TickDetectedObjectRecords(deltaTime);
        }

        public void HandleCollide(string sliceId, Collider collider)
        {
            if (collider == null || collider.gameObject == _character.gameObject)
            {
                return;
            }

            if (!TryResolveDamageConfig(sliceId, out var comboConfig))
            {
                //Debug.LogError($"[Collide_Error] Missing damage config. slice={sliceId}");
                return;
            }

            switch (collider.tag)
            {
                case CollideConstants.Enemy:
                case CollideConstants.Character:
                    HandleDamageCollide(sliceId, collider, comboConfig);
                    break;

                case CollideConstants.Slide:
                    HandleSlideCollide(sliceId, collider, comboConfig);
                    break;

                case CollideConstants.Parryable:
                    HandleParryableCollide(sliceId, collider, comboConfig);
                    break;

                default:
                    //Debug.Log($"[Collide_Ignore] Undefined tag ignored. tag={collider.tag}");
                    break;
            }
        }

        private bool TryResolveDamageConfig(string sliceId, out DamageHitConfigSO damageConfig)
        {
            damageConfig = null;

            if (!_collideClipConfigs.TryGetValue(sliceId, out var clip))
            {
                return false;
            }

            int consumedCount = _character.ActionPlayer?.RuntimeContext?.ConsumedSignalCount ?? 0;
            if (consumedCount > 0)
            {
                DamageHitConfigSO signalConfig = clip.GetSignalBallDamageConfig(consumedCount);
                if (signalConfig != null)
                {
                    damageConfig = signalConfig;
                    return true;
                }
            }

            if (clip.collideHitConfig is DamageHitConfigSO defaultConfig)
            {
                damageConfig = defaultConfig;
                return true;
            }

            return false;
        }

        private void HandleDamageCollide(string sliceId, Collider collider, DamageHitConfigSO comboConfig)
        {
            if (!collider.TryGetComponent(out ActionStateController targetCharacter))
            {
                targetCharacter = collider.GetComponentInParent<ActionStateController>();
            }

            if (targetCharacter == null)
            {
                return;
            }

            TryTriggerPerfectDodgeFromTarget(sliceId, targetCharacter);

            if (!CheckMultiHitValid(sliceId, collider, comboConfig))
            {
                return;
            }

            //Debug.Log($"[Collide_Damage] Target={targetCharacter.name}, slice={sliceId}");

            PlayHitEffects(targetCharacter, comboConfig);

            BattleManager.Instance.AddDamage(_character.characterId, targetCharacter.characterId, comboConfig);
        }

        private void HandleSlideCollide(string sliceId, Collider collider, DamageHitConfigSO comboConfig)
        {
            if (!IsCauseDamageSlice(sliceId))
            {
                //Debug.Log($"[PerfectDodge] Ignored non-damage slice. slice={sliceId}");
                return;
            }

            SlideObjMono slideObjMono = collider.GetComponent<SlideObjMono>();
            if (slideObjMono == null)
            {
                Debug.LogError($"[Collide_Error] SlideObjMono missing. object={collider.gameObject.name}");
                return;
            }

            var targetCharacter = slideObjMono.GetComponentInParent<ActionStateController>();
            if (!CanTargetPerfectDodge(targetCharacter))
            {
                return;
            }

            if (!slideObjMono.IsJudging)
            {
                //Debug.Log($"[PerfectDodge] Ignored inactive slide judge. target={targetCharacter.characterId}");
                return;
            }

            if (!slideObjMono.HandleSuccessSlide())
            {
                return;
            }

            //Debug.Log($"[PerfectDodge] Success from slide collider. slice={sliceId}, target={targetCharacter.characterId}");
        }

        private void HandleParryableCollide(string sliceId, Collider collider, DamageHitConfigSO comboConfig)
        {
            //Debug.Log($"[Collide_Parry] Parryable hit. slice={sliceId}, target={collider.name}");
        }

        private bool TryTriggerPerfectDodgeFromTarget(
            string sliceId,
            ActionStateController targetCharacter)
        {
            if (!IsCauseDamageSlice(sliceId) || !CanTargetPerfectDodge(targetCharacter))
            {
                return false;
            }

            var slideObjMono = targetCharacter.GetComponentInChildren<SlideObjMono>(true);
            if (slideObjMono == null || !slideObjMono.IsJudging)
            {
                return false;
            }

            if (!slideObjMono.HandleSuccessSlide())
            {
                return false;
            }

            //Debug.Log($"[PerfectDodge] Success from target body. slice={sliceId}, target={targetCharacter.characterId}");
            return true;
        }

        private bool IsCauseDamageSlice(string sliceId)
        {
            if (!_collideClipConfigs.TryGetValue(sliceId, out var clip))
            {
                Debug.LogWarning($"[PerfectDodge] Missing collide clip config. slice={sliceId}");
                return false;
            }

            return clip.collideEventType == ActionCollideEventType.CauseDamage;
        }

        private bool CanTargetPerfectDodge(ActionStateController targetCharacter)
        {
            if (targetCharacter == null || targetCharacter == _character)
            {
                return false;
            }

            if (!BattleManager.Instance.Data.TryGetValue(targetCharacter.characterId, out var targetProperty))
            {
                Debug.LogWarning($"[PerfectDodge] Target is not in battle data. target={targetCharacter.characterId}");
                return false;
            }

            if (!targetProperty.isPlayer)
            {
                return false;
            }

            return true;
        }

        private bool CheckMultiHitValid(string sliceId, Collider collider, DamageHitConfigSO config)
        {
            if (!_detectedObjectRecords.TryGetValue(sliceId, out var records))
            {
                _detectedObjectRecords.Add(sliceId,
                    new List<(GameObject, float)> { (collider.gameObject, config.colliderDetectionInterval) });
                return true;
            }

            if (records.Count >= config.colliderDetectionMaximum)
            {
                return false;
            }

            if (records.Any(t => t.gameObject == collider.gameObject))
            {
                return false;
            }

            records.Add((collider.gameObject, config.colliderDetectionInterval));
            return true;
        }

        private void PlayHitEffects(ActionStateController target, DamageHitConfigSO config)
        {
            config.hitAudioSettings.ForEach(s => _character.AudioAbility?.PlaySound(s.AudioClip, false, s.volume));

            ApplyHitFreeze(target, config);
            ApplyHitShake(target, config);
        }

        private void ApplyHitFreeze(ActionStateController target, DamageHitConfigSO config)
        {
            if (config == null || !config.openHitFreeze)
            {
                return;
            }

            if (config.hitFreezeDuration <= 0f)
            {
                return;
            }

            float scale = Mathf.Clamp(config.hitFreezeTimeScale, 0.01f, 1f);
            string commandId =
                $"HitFreeze_{_character.characterId}_{target.characterId}_{config.hitFreezeCommandType}_{Time.frameCount}";

            AddHitFreezeCommand(commandId, target, config, scale);

            /*Debug.Log(
                $"[HitFreeze] Triggered. attacker={_character.characterId}, " +
                $"target={target.characterId}, duration={config.hitFreezeDuration:0.000}, " +
                $"scale={scale:0.00}, type={config.hitFreezeAffectedType}, " +
                $"command={config.hitFreezeCommandType}");
            */
        }

        private void AddHitFreezeCommand(
            string commandId,
            ActionStateController target,
            DamageHitConfigSO config,
            float scale)
        {
            switch (config.hitFreezeCommandType)
            {
                case HitFreezeTimeScaleCommandType.Global:
                    TimeScaleManager.Instance.AddCommand(new TimeScaleGlobalCommand(
                        commandId,
                        config.hitFreezeDuration,
                        scale,
                        config.hitFreezeAffectedType));
                    break;

                case HitFreezeTimeScaleCommandType.Attacker:
                    TimeScaleManager.Instance.AddCommand(new TimeScaleSingleCommand(
                        commandId,
                        config.hitFreezeDuration,
                        scale,
                        _character.characterId,
                        config.hitFreezeAffectedType));
                    break;

                case HitFreezeTimeScaleCommandType.Target:
                    TimeScaleManager.Instance.AddCommand(new TimeScaleSingleCommand(
                        commandId,
                        config.hitFreezeDuration,
                        scale,
                        target.characterId,
                        config.hitFreezeAffectedType));
                    break;

                case HitFreezeTimeScaleCommandType.AttackerAndTarget:
                    TimeScaleManager.Instance.AddCommand(new TimeScaleSingleCommand(
                        commandId + "_Attacker",
                        config.hitFreezeDuration,
                        scale,
                        _character.characterId,
                        config.hitFreezeAffectedType));
                    TimeScaleManager.Instance.AddCommand(new TimeScaleSingleCommand(
                        commandId + "_Target",
                        config.hitFreezeDuration,
                        scale,
                        target.characterId,
                        config.hitFreezeAffectedType));
                    break;

                case HitFreezeTimeScaleCommandType.Witch:
                    TimeScaleManager.Instance.AddCommand(new TimeScaleWitchCommand(
                        commandId,
                        config.hitFreezeDuration,
                        scale,
                        Mathf.Clamp(config.hitFreezeSelfTimeScale, 0.01f, 2f),
                        _character.characterId,
                        config.hitFreezeAffectedType));
                    break;
            }
        }

        private void ApplyHitShake(ActionStateController target, DamageHitConfigSO config)
        {
            if (config == null || config.hitShake == null)
            {
                return;
            }

            config.hitShake.GenerateShake(_character.transform.position);
            //Debug.Log($"[HitShake] Cinemachine impulse triggered. attacker={_character.characterId}, target={target.characterId}");
        }

        private void TickDetectedObjectRecords(float deltaTime)
        {
            if (_detectedObjectRecords.Count == 0)
            {
                return;
            }

            var sliceIds = _detectedObjectRecords.Keys.ToList();
            foreach (var sliceId in sliceIds)
            {
                var records = _detectedObjectRecords[sliceId];

                for (int i = records.Count - 1; i >= 0; i--)
                {
                    var record = records[i];
                    float newCountdown = record.countdown - deltaTime;

                    if (newCountdown <= 0)
                    {
                        records.RemoveAt(i);
                        //Debug.Log($"[Collide_Refresh] Slice={sliceId}, object={record.gameObject.name}");
                    }
                    else
                    {
                        records[i] = (record.gameObject, newCountdown);
                    }
                }

                if (records.Count == 0)
                {
                    _detectedObjectRecords.Remove(sliceId);
                }
            }
        }

    }
}
