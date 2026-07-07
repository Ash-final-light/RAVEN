using Tools.Battle.Data;
using UnityEngine;

namespace Tools.Damage.SO
{
    [CreateAssetMenu(menuName = "Damage/Attack Convert/Simple")]
    public class SimpleDamageAttackConvertSO : BaseDamageAttackConvertSO
    {
        public override DamageValue ConvertToDamageValue(
            DamageValue originFixedDamage,
            DamageType damageType,
            CharacterProperty attacker
        )
        {
            originFixedDamage = originFixedDamage.Check(damageType.IsHeal());
           
            // 计算单纯攻击力增幅后的伤害，当前默认所有属性增伤
            var attackDamage = originFixedDamage + new DamageValue
            {
                noType = (int)(attacker.CurrentDamage),
                physics = (int)(attacker.CurrentDamage),
                fire = (int)(attacker.CurrentDamage),
                ice = (int)(attacker.CurrentDamage),
                wind = (int)(attacker.CurrentDamage),
                lightning = (int)(attacker.CurrentDamage),
            };
            return attackDamage.Check(damageType.IsHeal());
        }
    }
}