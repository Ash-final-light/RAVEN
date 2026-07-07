using System;
using UnityEngine;

namespace Tools.Battle.Data
{
    public static class ExtensionMethod
    {
        public static CharacterProperty ToCharacterProperty(this CharacterInfoData baseInfo,
            string gameObjId,Action<string> onCharacterDie,Action<string> onCharacterBreak,Transform transform)
        {
            return new CharacterProperty
            {
                Info = baseInfo,
                gameObjId = gameObjId,
                CurrentHealth = baseInfo.Health,
                CurrentDamage = baseInfo.Damage,
                CurrentCritRate = baseInfo.CritRate,
                CurrentDefense = baseInfo.Defense,
                CurrentSpeed = baseInfo.Speed,
                CurrentShield = 0,
                CurrentMp = 0,
                CurrentMaxMp = Mathf.Max(1, baseInfo.MaxMp),
                dead = false,
                isActive = true,
                
                OnCharacterDie = onCharacterDie,
                OnCharacterBreak = onCharacterBreak,
                targetTransform = transform
            };
        }
    }
}
