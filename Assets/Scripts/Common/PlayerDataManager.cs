using Sirenix.OdinInspector;
using UnityEngine;

namespace Common
{
    public class PlayerDataManager:MonoBehaviour
    {
        [ShowInInspector] private int currentLevel = 2;
        [ShowInInspector] private int currentExp = 0;
        
        public int CurrentLevel{get => currentLevel;}
        [Button("添加等级")]
        public void AddLevel(int level)
        {
            currentLevel += level;
        }
        [Button("添加经验")]
        public void AddExp(int exp)
        {
            currentExp += exp;
        }
    }
}