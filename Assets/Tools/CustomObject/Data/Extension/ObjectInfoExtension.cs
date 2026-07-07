
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.CustomObject.Data.Extension
{
    public static class ObjectInfoExtension
    {
        private static Dictionary<string, ObjectType> m_ObjectTypes = new Dictionary<string, ObjectType>(StringComparer.OrdinalIgnoreCase)
        {
            { "weapon", ObjectType.Weapon },
            { "gear", ObjectType.Gear },
            { "item", ObjectType.Item },
            { "material", ObjectType.Material },
        };
        private static Dictionary<string ,PackageQuantityRestriction> m_QuantityRestrictions = new Dictionary<string ,PackageQuantityRestriction>(StringComparer.OrdinalIgnoreCase)
        {
            { "only one group", PackageQuantityRestriction.OnlyOneGroup },
            { "no restriction", PackageQuantityRestriction.NoRestriction },
        };
        public static ObjectData ToObjectData(this ObjectInfo objectInfo)
        {
            ObjectType objectType = objectInfo.GetObjectType();
            switch (objectType)
            {
                case ObjectType.Weapon:
                    WeaponData weaponData = new WeaponData()
                    {
                        WeaponPrefabId = objectInfo.WeaponPrefabId,
                        WeaponType = objectInfo.WeaponType,
                        WeaponAddDamage = objectInfo.WeaponAddDamage,
                        WeaponAddDefense = objectInfo.WeaponAddDefense,
                        WeaponAddSpeed = objectInfo.WeaponAddSpeed,
                        WeaponAddCrit = objectInfo.WeaponAddCrit,   
                        skills = ToStringList(objectInfo.Skills)
                    };
                    SetObjectData(weaponData, objectInfo);
                    return weaponData;
                case ObjectType.Gear:
                    GearData gearData = new GearData()
                    {
                        GearPrefabId = objectInfo.GearPrefabId,
                        GearType = objectInfo.GearType,
                        GearAddDamage = objectInfo.GearAddDamage,
                        GearAddDefense = objectInfo.GearAddDefense,
                        GearAddSpeed = objectInfo.GearAddSpeed,
                        GearAddCrit = objectInfo.GearAddCrit,
                    };
                    SetObjectData(gearData, objectInfo);
                    return gearData;
                case ObjectType.Item:
                    ItemData itemData = new ItemData()
                    {
                        ItemPrefabId = objectInfo.ItemPrefabId,
                        RecoverHp =  objectInfo.RecoverHp,
                        BuffId = objectInfo.BuffId,
                        BuffStack = objectInfo.BuffStack,
                        BuffDuration = objectInfo.BuffDuration,
                    };
                    SetObjectData(itemData, objectInfo);
                    return itemData;
                case ObjectType.Material:
                    MaterialData materialData = new MaterialData()
                    {
                        MaterialPrefabId = objectInfo.MaterialPrefabId,
                    };
                    SetObjectData(materialData, objectInfo);
                    return materialData;
                default:
                    Debug.LogError($"Unknown object type {objectType}");
                    return null;
            }

            void SetObjectData(ObjectData objectData, ObjectInfo objectInfo)
            {
                objectData.Id = objectInfo.Id;
                objectData.Name = objectInfo.Name;
                objectData.Introduction = objectInfo.Introduction;
                objectData.Price = objectInfo.Price;
                List<string> temp = ToStringList(objectInfo.Thumbnail);
                objectData.ThumbnailAtlas = temp.Count > 0 ? temp[0] : string.Empty;
                objectData.ThumbnailName = temp.Count > 1 ? temp[1] : string.Empty;
                objectData.QuantitativeRestriction = GetPackageQuantityRestriction(objectInfo);
                objectData.GroupMaximum = objectInfo.GroupMaximum > 0 ? objectInfo.GroupMaximum : 1;
            }
        }

        public static ObjectInfo ToObjectInfo(this ObjectData objectData)
        {
            var objectInfo = new ObjectInfo()
            {
                Id = objectData.Id,
                Name = objectData.Name,
                Introduction = objectData.Introduction,
                Price = objectData.Price,
                Thumbnail = $"{objectData.ThumbnailAtlas},{objectData.ThumbnailName}",
                QuantitativeRestriction = objectData.QuantitativeRestriction.GetString(),
                GroupMaximum = objectData.GroupMaximum,
            };
            switch (objectData.GetObjectType())
            {
                case ObjectType.Weapon:
                    var weaponData = objectData as WeaponData;
                    objectInfo.WeaponPrefabId = weaponData.WeaponPrefabId;
                    objectInfo.WeaponType = weaponData.WeaponType;
                    objectInfo.WeaponAddDamage = weaponData.WeaponAddDamage;
                    objectInfo.WeaponAddDefense = weaponData.WeaponAddDefense;
                    objectInfo.WeaponAddSpeed = weaponData.WeaponAddSpeed;
                    objectInfo.WeaponAddCrit = weaponData.WeaponAddCrit;
                    objectInfo.Skills = string.Join(",", weaponData.skills);
                    break;
                case ObjectType.Gear:
                    var gearData = objectData as GearData;
                    objectInfo.GearPrefabId = gearData.GearPrefabId;
                    objectInfo.GearType = gearData.GearType;
                    objectInfo.GearAddDamage = gearData.GearAddDamage;
                    objectInfo.GearAddDefense = gearData.GearAddDefense;
                    objectInfo.GearAddSpeed = gearData.GearAddSpeed;
                    objectInfo.GearAddCrit = gearData.GearAddCrit;
                    break;
                case ObjectType.Item:
                    var itemData = objectData as ItemData;
                    objectInfo.ItemPrefabId = itemData.ItemPrefabId;
                    objectInfo.RecoverHp = itemData.RecoverHp;
                    objectInfo.BuffId = itemData.BuffId;
                    objectInfo.BuffStack = itemData.BuffStack;
                    objectInfo.BuffDuration = itemData.BuffDuration;
                    break;
                case ObjectType.Material:
                    var materialData = objectData as MaterialData;
                    objectInfo.MaterialPrefabId = materialData.MaterialPrefabId;
                    break;
                default:
                    Debug.LogError($"Unknown object type {objectData.GetObjectType()}");
                    break;
            }
            objectInfo.Type = objectData.GetObjectType().GetString();
            return objectInfo;
        }
        public static ObjectInfo ToObjectInfo(this ObjectGroup objectGroup)
        {
            return objectGroup.Data.ToObjectInfo();
        }
        private static List<string> ToStringList(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            return input
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        public static ObjectType GetObjectType(this ObjectInfo objectInfo)
        {
            if (objectInfo == null || string.IsNullOrWhiteSpace(objectInfo.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(objectInfo), "ObjectInfo type is empty.");
            }

            if (m_ObjectTypes.TryGetValue(objectInfo.Type.Trim(), out ObjectType objectType))
            {
                return objectType;
            }

            throw new ArgumentOutOfRangeException(nameof(objectInfo), objectInfo.Type, null);
        }
        public static ObjectType GetObjectType(this ObjectData objectData)
        {
            return objectData switch
            {
                MaterialData => ObjectType.Material,
                WeaponData => ObjectType.Weapon,
                GearData => ObjectType.Gear,
                ItemData => ObjectType.Item,
                _ => throw new ArgumentOutOfRangeException(nameof(objectData), objectData, null),
            };
        }

        public static bool TryParseObjectType(string type, out ObjectType objectType)
        {
            objectType = default;
            return !string.IsNullOrWhiteSpace(type) &&
                   m_ObjectTypes.TryGetValue(type.Trim(), out objectType);
        }

        public static string GetString(this ObjectType objectType)
        {
            return objectType switch
            {
                ObjectType.Weapon => "weapon",
                ObjectType.Gear => "gear",
                ObjectType.Item => "item",
                ObjectType.Material => "material",
                _ => throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null),
            };
        }

        public static PackageQuantityRestriction GetPackageQuantityRestriction(this ObjectInfo objectInfo)
        {
            if (objectInfo == null || string.IsNullOrWhiteSpace(objectInfo.QuantitativeRestriction))
            {
                return PackageQuantityRestriction.NoRestriction;
            }

            if (m_QuantityRestrictions.TryGetValue(objectInfo.QuantitativeRestriction.Trim(), out PackageQuantityRestriction restriction))
            {
                return restriction;
            }

            throw new ArgumentOutOfRangeException(
                nameof(objectInfo),
                objectInfo.QuantitativeRestriction,
                null);
        }

        public static string GetString(this PackageQuantityRestriction restriction)
        {
            return restriction switch
            {
                PackageQuantityRestriction.OnlyOneGroup => "only one group",
                PackageQuantityRestriction.NoRestriction => "no restriction",
                _ => throw new ArgumentOutOfRangeException(nameof(restriction), restriction, null),
            };
        }
    }
}
