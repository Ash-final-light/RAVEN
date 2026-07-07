using System.Collections.Generic;
using UnityEngine;

namespace Tools.AI
{
    [CreateAssetMenu(fileName = "AgentAbilityConfig", menuName = "Action/AgentAbility")]
    public class AgentAbilityConfig : ScriptableObject
    {
        [System.Serializable]
        public struct AbilityData
        {
            public string actionName; // 动作标识符
            public float cooldown;    // 冷却时间（秒）
        }

        public List<AbilityData> abilities;
        
    }
}