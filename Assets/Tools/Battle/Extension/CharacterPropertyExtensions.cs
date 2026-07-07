using Tools.Battle.Data;
using Tools.Damage;
using UnityEngine;

namespace Tools.Battle.Extension
{
    public static class CharacterPropertyExtensions
    {
        #region Health API
        public static void DecreaseHealth(this CharacterProperty characterProperty, int damage)
        {
            if(characterProperty.CurrentShield >= damage)
            {
                characterProperty.CurrentShield -= damage;
            }
            else
            {
                characterProperty.CurrentHealth -= damage - characterProperty.CurrentShield;
                characterProperty.CurrentShield = 0;
            }
            if(characterProperty.CurrentHealth <= 0)
            {
                // 通知BattleManager角色死亡
                characterProperty.OnCharacterDie?.Invoke(characterProperty.gameObjId);
            }
        }
        public static void AddHealth(this CharacterProperty characterProperty, int health)
        {
            characterProperty.CurrentHealth += health;
            if(characterProperty.CurrentHealth > characterProperty.Info.Health)
            {
                characterProperty.CurrentHealth = characterProperty.Info.Health;
            }
        }
        public static void AddHealthByMaxPercent(this CharacterProperty characterProperty, float maxPercent)
        {
            characterProperty.CurrentHealth += (int)(maxPercent * characterProperty.Info.Health);
            if(characterProperty.CurrentHealth > characterProperty.Info.Health)
            {
                characterProperty.CurrentHealth = characterProperty.Info.Health;
            }
        }
        public static void AddHealthByCurrentPercent(this CharacterProperty characterProperty, float percent)
        {
            characterProperty.CurrentHealth += (int)(percent * characterProperty.CurrentHealth);
            if(characterProperty.CurrentHealth > characterProperty.Info.Health)
            {
                characterProperty.CurrentHealth = characterProperty.Info.Health;
            }   
        }
        #endregion
        #region Break API
        public static void AddBreak(this CharacterProperty characterProperty, float breakValue)
        {
            characterProperty.CurrentBreak += breakValue;
            //TODO:增加范围检查，不能超过Excel中的配置值
        }
        public static void RemoveBreak(this CharacterProperty characterProperty, float breakValue)
        {
            characterProperty.CurrentBreak -= breakValue;
        }
        public static void ResetBreak(this CharacterProperty characterProperty)
        {
            characterProperty.CurrentBreak = 0;
        }
        #endregion
        #region Mp API
        public static void AddMp(this CharacterProperty characterProperty, int mp)
        {
            int maxMp = GetCurrentMaxMp(characterProperty);
            characterProperty.CurrentMp = Mathf.Clamp(characterProperty.CurrentMp + Mathf.Max(0, mp), 0, maxMp);
        }
        public static void RemoveMp(this CharacterProperty characterProperty, int mp)
        {
            int maxMp = GetCurrentMaxMp(characterProperty);
            characterProperty.CurrentMp = Mathf.Clamp(characterProperty.CurrentMp - Mathf.Max(0, mp), 0, maxMp);
        }
        public static void ResetMp(this CharacterProperty characterProperty)
        {
            characterProperty.CurrentMp = 0;
        }

        public static int GetCurrentMaxMp(this CharacterProperty characterProperty)
        {
            if (characterProperty == null)
            {
                return 1;
            }

            if (characterProperty.CurrentMaxMp > 0)
            {
                return characterProperty.CurrentMaxMp;
            }

            return Mathf.Max(1, characterProperty.Info?.MaxMp ?? 1);
        }
        #endregion
        public static void Modify(this CharacterProperty characterProperty, CharacterResource resource)
        {
            Debug.Log($"[CharacterPropertyExtensions] Modify {characterProperty.Info.Name} hp:{resource.hp} break:{resource.@break} mp:{resource.mp}");
            if(resource.hp > 0) 
                characterProperty.AddHealth(resource.hp);
            else
                characterProperty.DecreaseHealth(-resource.hp);
            
            if(resource.@break > 0)
                characterProperty.AddBreak(resource.@break);
            else
                characterProperty.RemoveBreak(-resource.@break);
            
            if(resource.mp > 0)
                characterProperty.AddMp(resource.mp);
            else
                characterProperty.RemoveMp(-resource.mp);
        }
    }
}
