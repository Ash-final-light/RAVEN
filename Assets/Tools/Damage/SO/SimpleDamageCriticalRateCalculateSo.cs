using Tools.Battle.Data;
using UnityEngine;

namespace Tools.Damage.SO
{
    [CreateAssetMenu(menuName = "Damage/Critical Rate Calculate/Simple")]
    public class SimpleDamageCriticalRateCalculateSo : BaseDamageCriticalRateCalculateSO
    {
        public override float CalculateCriticalRate(DamageType damageType, CharacterProperty property)
        {
            if (!damageType.AllowCritical())
            {
                return 0f;
            }
            
            return 1f * property.CurrentCritRate / (property.CurrentCritRate + 100);
        }
    }
}