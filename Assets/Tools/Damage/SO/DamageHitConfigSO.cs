using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Audio;
using Tools.Camera;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Tools.Damage.SO
{
    public abstract class CollideHitConfigSO : SerializedScriptableObject
    {
    }

    public enum HitFreezeTimeScaleCommandType
    {
        Global,
        Attacker,
        Target,
        AttackerAndTarget,
        Witch,
    }

    public enum DamageHitSourceType
    {
        None = 0,
        BasicAttack = 10,
        SkillBallRed = 20,
        SkillBallYellow = 21,
        SkillBallBlue = 22,
        SkillBallWhite = 23,
        Ultimate = 30,
    }

    [CreateAssetMenu(menuName = "Action/DamageHitConfig")]
    public class DamageHitConfigSO : CollideHitConfigSO
    {
        [Title("Collider Config")]
        [InfoBox("Detection interval for hitting the same object inside one collide track, in seconds.")]
        public float colliderDetectionInterval = 1f;

        [InfoBox("Maximum hit count for a collide track. Usually adjusted for single-target attacks.")]
        public int colliderDetectionMaximum = 100;

        [Title("Hit Audio Config")]
        public List<ComboHitAudioSetting> hitAudioSettings = new();

        [Title("Hit Freeze Config")]
        public bool openHitFreeze = true;
        [ShowIf("openHitFreeze")] public float hitFreezeDuration = 0.1f;
        [ShowIf("openHitFreeze")] public float hitFreezeTimeScale = 0.2f;
        [ShowIf("openHitFreeze")] public TimeScaleType hitFreezeAffectedType = TimeScaleType.All;
        [ShowIf("openHitFreeze")] public HitFreezeTimeScaleCommandType hitFreezeCommandType =
            HitFreezeTimeScaleCommandType.Global;
        [ShowIf("@openHitFreeze && hitFreezeCommandType == HitFreezeTimeScaleCommandType.Witch")]
        public float hitFreezeSelfTimeScale = 1f;

        [Title("Hit Shake Config")]
        [SerializeReference]
        public BaseCameraShakeData hitShake = null;

        [Title("Damage Config")]
        public DamageType hitDamageType = DamageType.DirectDamage;
        public DamageValue hitDamage = DamageValue.Zero;

        [Title("Hit Source Config")]
        public DamageHitSourceType hitSourceType = DamageHitSourceType.None;
        [ShowIf("@hitSourceType == DamageHitSourceType.SkillBallRed || hitSourceType == DamageHitSourceType.SkillBallYellow || hitSourceType == DamageHitSourceType.SkillBallBlue || hitSourceType == DamageHitSourceType.SkillBallWhite")]
        [Range(1, 3)]
        public int skillBallConsumeCount = 1;

        [Title("Hit Energy Config")]
        public int attackerMpGainOnHit = 0;
        public bool gainMpOnlyWhenHpDamageApplied = true;

        [Title("Skill Ball Energy Config")]
        public int attackerSkillBallEnergyGainOnHit = 0;
        public bool gainSkillBallEnergyOnlyWhenHpDamageApplied = true;
    }

    public enum ComboHitAudioType
    {
        Specified,
        Random,
    }

    [Serializable]
    public class ComboHitAudioSetting
    {
        public ComboHitAudioType type = ComboHitAudioType.Specified;

        [ShowIf("type", ComboHitAudioType.Specified)]
        public AudioClip specifiedAudioClip;

        [ShowIf("type", ComboHitAudioType.Random)]
        public AudioClipRandomizer randomAudioClip;

        public float volume = 1f;
        public bool useOwnLengthAsDuration = true;
        [HideIf("useOwnLengthAsDuration")] public float duration = 0f;

        public AudioClip AudioClip => type switch
        {
            ComboHitAudioType.Specified => specifiedAudioClip,
            ComboHitAudioType.Random => randomAudioClip.Random(),
            _ => specifiedAudioClip,
        };
    }
}
