using Sirenix.OdinInspector;
using Tools.Damage.SO;
using UnityEngine;

namespace Common
{
    public class AlgorithmManager:MonoBehaviour
    {
        [SerializeField, LabelText("攻击力转伤害算法")]
        private BaseDamageAttackConvertSO damageAttackConvertSO;

        public BaseDamageAttackConvertSO DamageAttackConvertSO => damageAttackConvertSO;

        [SerializeField, LabelText("暴击率算法")] private BaseDamageCriticalRateCalculateSO damageCriticalRateCalculateSO;
        public BaseDamageCriticalRateCalculateSO DamageCriticalRateCalculateSO => damageCriticalRateCalculateSO;

        [SerializeField, LabelText("伤害结算算法")] private BaseDamageSettlementCalculateSO damageSettlementCalculateSO;
        public BaseDamageSettlementCalculateSO DamageSettlementCalculateSO => damageSettlementCalculateSO;
        
        [SerializeField, LabelText("伤害触发机制算法")] private BaseDamageTriggerMechanismSO damageTriggerMechanismSO;
        public BaseDamageTriggerMechanismSO DamageTriggerMechanismSO => damageTriggerMechanismSO;
    }
}