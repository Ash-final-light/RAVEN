using Tools.Battle.Data;
using UnityEngine;

namespace Tools.Damage.SO
{
    public abstract class BaseDamageAttackConvertSO : ScriptableObject
    {
        public abstract DamageValue ConvertToDamageValue(
            DamageValue originFixedDamage,
            DamageType damageType,
            CharacterProperty attacker
        );
    }
}