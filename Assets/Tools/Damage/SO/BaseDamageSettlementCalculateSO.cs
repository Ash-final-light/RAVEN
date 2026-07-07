using Tools.Battle.Data;
using UnityEngine;

namespace Tools.Damage.SO
{
    public abstract class BaseDamageSettlementCalculateSO : ScriptableObject
    {
        public abstract (DamageValue damageValue, bool isCritical) CalculateDamageSettlement(
            DamageType type,
            DamageValue value,
            float criticalRate,
            CharacterProperty attacker,
            CharacterProperty defender
        );
    }
}