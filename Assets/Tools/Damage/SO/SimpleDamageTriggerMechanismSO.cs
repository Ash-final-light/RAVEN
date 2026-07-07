using UnityEngine;

namespace Tools.Damage.SO
{
    [CreateAssetMenu(menuName = "Damage/Trigger Mechanism/Simple")]
    public class SimpleDamageTriggerMechanismSO : BaseDamageTriggerMechanismSO
    {
        public override int GetTriggerFlags(DamageInfo damageInfo)
        {
            return -1;
        }
    }
}