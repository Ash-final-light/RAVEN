using Tools.Battle.Data;
using UnityEngine;

namespace Tools.Damage.SO
{
    public abstract class BaseDamageCriticalRateCalculateSO : ScriptableObject
    {
        public abstract float CalculateCriticalRate(
            DamageType damageType,
            CharacterProperty property
        );
    }
}