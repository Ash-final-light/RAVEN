using System.Collections.Generic;

namespace Tools.CustomObject.Data
{
    public class WeaponData: ObjectData
    {
        public string WeaponPrefabId;
        public string WeaponType;
        public int WeaponAddDamage;
        public int WeaponAddDefense;
        public int WeaponAddSpeed;
        public int WeaponAddCrit;
        public List<string> skills;
    }
}