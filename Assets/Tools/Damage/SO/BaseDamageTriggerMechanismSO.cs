using UnityEngine;

namespace Tools.Damage.SO
{
    public abstract class BaseDamageTriggerMechanismSO: ScriptableObject
    {
        public abstract int GetTriggerFlags(DamageInfo damageInfo);
    }
}