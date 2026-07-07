using Tools.Battle.Data;
using Tools.Damage.SO;

namespace Tools.Damage
{
    public struct DamageInfo
    {
        public string AttackerId;
        public string TargetId;

        public CharacterProperty AttackerProperty;
        public CharacterProperty TargetProperty;

        public string SerialNumber;

        public DamageType Type;
        public DamageValue Value;
        public DamageResource Resource;

        public int AttackerMpGainOnHit;
        public bool GainMpOnlyWhenHpDamageApplied;
        public int AttackerSkillBallEnergyGainOnHit;
        public bool GainSkillBallEnergyOnlyWhenHpDamageApplied;
        public bool AttackerSkillBallEnergyGained;
        public DamageHitSourceType HitSourceType;
        public int SkillBallConsumeCount;
        public int AttackerMpBeforeGain;
        public int AttackerMpAfterGain;
        public bool AttackerMpChanged;

        public float CriticalRate;
        public bool IsCritical;

        public int TriggerFlags;

        public static DamageResource CalculateResource(DamageValue damageValue)
        {
            var hp = (int)damageValue.Total();
            var breakValue = (int)damageValue.Total();
            return new DamageResource
            {
                hp = hp,
                @break = breakValue,
            };
        }

        public DamageInfo SetTriggerFlags(int triggerFlags)
        {
            TriggerFlags = triggerFlags;
            return this;
        }
    }
}
