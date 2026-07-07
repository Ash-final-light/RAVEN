using System.Collections.Generic;

namespace Tools.CustomObject.Data
{
    /// <summary>
    /// 二进制文件中的物品信息，通用类型
    /// </summary>
    public class ObjectInfo
    {
        public int Id; // 物品Id，同一种物品预设Id相同
        public string Type;
        public string Name;
        public string Introduction;
        public int Price;
        public string Thumbnail;
        public string QuantitativeRestriction;
        public int GroupMaximum;

        public string WeaponPrefabId;
        public string WeaponType;
        public int WeaponAddDamage;
        public int WeaponAddDefense;
        public int WeaponAddSpeed;
        public int WeaponAddCrit;
        public string Skills;
        
        public string ItemPrefabId;
        public int RecoverHp;
        public string BuffId;
        public int BuffStack;
        public float BuffDuration;
        
        public string MaterialPrefabId;
        
        public string GearPrefabId;
        public string GearType;
        public int GearAddDamage;
        public int GearAddDefense;
        public int GearAddSpeed;
        public int GearAddCrit;
    }
}