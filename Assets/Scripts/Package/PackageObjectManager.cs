using System;
using System.Collections.Generic;
using Common;
using Events;
using Events.Data;
using Tools.CustomObject.Data;
using Tools.CustomObject.Data.Extension;
using Tools.CustomObject.Interface;
using UnityEngine;

namespace Package
{
    /// <summary>
    /// 玩家物品包对象管理器，负责管理物品包对象的加载、获取、添加、移除等操作
    /// </summary>
    public class PackageObjectManager:IPackageObjectManager,IPackageObjectProvider
    {
        //TODO:后续更改为游戏开始前先加载存档资源
        
        private readonly List<ObjectGroup> _itemGroups = new();
        private readonly List<ObjectGroup> _materialGroups = new();
        private readonly List<ObjectGroup> _weaponGroups = new();
        private readonly List<ObjectGroup> _gearGroups = new();
        
        public event System.Action<string,ObjectGroup> OnObjectGroupAdded;
        public event System.Action<string,ObjectGroup> OnObjectGroupRemoved;
        public event System.Action<string,ObjectGroup> OnObjectGroupChanged;

        public readonly List<BagData> typeList = new()
        {
            new("weapon", "武器", "ItemIconAtlas,Icon_Weapons", 2000, 620,"weapon123"),
            new("gear", "装备", "ItemIconAtlas,Icon_Relics", 1800, 1796,"gear123"),
            new("material", "材料", "ItemIconAtlas,Icon_Materials", 2000, 1184,"material123"),
            new("item", "道具", "ItemIconAtlas,Icon_Props", 2000, 1184,"item123"),
        };
        /// <summary>
        /// 获取全局可以出现的物品信息字典
        /// </summary>
        private ObjectInfocontainer ObjectInfocontainer=>
            GameApplication.Instance.ExcelBinaryManager.GetContainer<ObjectInfocontainer>();
        
        public ObjectInfo GetObjectInfoById(int id)
        {
            Debug.Log($"GetObjectInfoById:{id}");
            return ObjectInfocontainer.Data.GetValueOrDefault(id);
        }

        /// <summary>
        /// 根据物品类型添加到对应物品组，Item,Material,Weapon,Gear
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool AddObjectById(int id, int num, bool forceAddAll = false)
        {
            if(!ObjectInfocontainer.Data.TryGetValue(id, out var info))
            {
                Debug.LogError($"AddObjectById:id={id} not found");
                return false;
            }

            if (!TryGetObjectGroups(info.Type, out List<ObjectGroup> objectGroups))
            {
                Debug.LogError($"AddObjectById:type={info.Type} not found");
                return false;
            }

            string parentId = GetParentId(info.Type);
            Debug.Log($"AddObjectById:{parentId},{id},{num}");
            List<ObjectGroup> foundGroups = new();
            foreach (var group in objectGroups)
            {
                if (group.Data.Id == id)
                {
                    foundGroups.Add(group);
                }
            }
            
            return AddAndCreatePackageGroupInternal(parentId, info, foundGroups, num, forceAddAll);
        }

        /// <summary>
        /// 从背包中删除物品，不指定物品组
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        public int RemoveObjectById(int id, int num = int.MaxValue)
        {
            if (num <= 0)
            {
                return 0;
            }

            if(!ObjectInfocontainer.Data.TryGetValue(id, out var info))
            {
                Debug.LogError($"RemoveObjectById:id={id} not found");
                return 0;
            }

            if (!TryGetObjectGroups(info.Type, out List<ObjectGroup> objectGroups))
            {
                Debug.LogError($"RemoveObjectById:type={info.Type} not found");
                return 0;
            }

            string parentId = GetParentId(info.Type);
            int removedNumber = 0;

            for (int i = 0; i < objectGroups.Count && removedNumber < num;)
            {
                ObjectGroup objectGroup = objectGroups[i];
                if (objectGroup.Data.Id != id)
                {
                    i++;
                    continue;
                }

                int removeCount = Mathf.Min(objectGroup.Number, num - removedNumber);
                objectGroup.Number -= removeCount;
                removedNumber += removeCount;

                if (objectGroup.Number <= 0)
                {
                    objectGroups.RemoveAt(i);
                    OnObjectGroupRemoved?.Invoke(parentId, objectGroup);
                }
                else
                {
                    OnObjectGroupChanged?.Invoke(parentId, objectGroup);
                    i++;
                }
            }

            if (removedNumber > 0)
            {
                NotifyObjectLost(info.ToObjectData(), removedNumber);
            }

            return removedNumber;
        }

        public void RemoveObjectById(string parentId,int id, int num = int.MaxValue)
        {
            RemoveObjectById(id, num);
        }

        public int RemoveObjectGroup(string groupId, int num = int.MaxValue)
        {
            if (string.IsNullOrWhiteSpace(groupId) || num <= 0)
            {
                return 0;
            }

            ObjectGroup objectGroup = GetObjectGroup(groupId);
            if (objectGroup == null)
            {
                return 0;
            }

            if (!TryGetObjectGroups(objectGroup.Data.GetObjectType(), out List<ObjectGroup> objectGroups))
            {
                return 0;
            }

            string parentId = GetParentId(objectGroup.Data.GetObjectType());
            int removeCount = Mathf.Min(objectGroup.Number, num);
            objectGroup.Number -= removeCount;

            if (objectGroup.Number <= 0)
            {
                objectGroups.Remove(objectGroup);
                OnObjectGroupRemoved?.Invoke(parentId, objectGroup);
            }
            else
            {
                OnObjectGroupChanged?.Invoke(parentId, objectGroup);
            }

            if (removeCount > 0)
            {
                NotifyObjectLost(objectGroup.Data, removeCount);
            }

            return removeCount;
        }

        public int GetObjectCountById(int id)
        {
            int count = 0;
            foreach (ObjectGroup group in GetAllObjectGroups())
            {
                if (group.Data.Id == id)
                {
                    count += group.Number;
                }
            }

            return count;
        }

        public Tools.CustomObject.Data.ObjectGroup GetObjectGroup(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return null;
            }

            foreach (ObjectGroup group in GetAllObjectGroups())
            {
                if (group.GroupId == groupId)
                {
                    return group;
                }
            }

            return null;
        }

        public IReadOnlyList<Tools.CustomObject.Data.ObjectGroup> GetObjectGroups(Tools.CustomObject.Data.ObjectType type)
        {
            return TryGetObjectGroups(type, out List<ObjectGroup> objectGroups)
                ? objectGroups
                : Array.Empty<ObjectGroup>();
        }

        public void Sort(Tools.CustomObject.Data.ObjectType type, Comparison<Tools.CustomObject.Data.ObjectGroup> comparison)
        {
            if (comparison == null)
            {
                return;
            }

            if (TryGetObjectGroups(type, out List<ObjectGroup> objectGroups))
            {
                objectGroups.Sort(comparison);
            }
        }

        /// <summary>
        /// 仅可填入Item,Material,Weapon,Gear
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<ObjectGroup> GetSelectedObjectGroups(string key)
        {
            return TryGetObjectGroups(key, out List<ObjectGroup> objectGroups)
                ? objectGroups
                : new List<ObjectGroup>();
        }

        private List<ObjectGroup> GetObjectGroups(string type)
        {
            return TryGetObjectGroups(type, out List<ObjectGroup> objectGroups) ? objectGroups : null;
        }

        private bool TryGetObjectGroups(string type, out List<ObjectGroup> objectGroups)
        {
            objectGroups = null;
            if (!ObjectInfoExtension.TryParseObjectType(type, out ObjectType objectType))
            {
                Debug.LogError($"GetObjectGroups:type={type} not found");
                return false;
            }

            return TryGetObjectGroups(objectType, out objectGroups);
        }

        private bool TryGetObjectGroups(ObjectType type, out List<ObjectGroup> objectGroups)
        {
            objectGroups = type switch
            {
                ObjectType.Item => _itemGroups,
                ObjectType.Material => _materialGroups,
                ObjectType.Weapon => _weaponGroups,
                ObjectType.Gear => _gearGroups,
                _ => null
            };

            return objectGroups != null;
        }

        public PackageObjectArchiveData CaptureArchiveData()
        {
            var archiveData = new PackageObjectArchiveData();
            foreach (ObjectGroup group in GetAllObjectGroups())
            {
                archiveData.groups.Add(group.ToArchiveData());
            }

            return archiveData;
        }

        public void LoadArchiveData(PackageObjectArchiveData archiveData)
        {
            ClearAllObjects();
            if (archiveData?.groups == null)
            {
                return;
            }

            foreach (PackageObjectGroupArchiveData groupArchiveData in archiveData.groups)
            {
                ObjectGroup group = groupArchiveData.ToObjectGroup(ObjectInfocontainer);
                if (!TryGetObjectGroups(group.Data.GetObjectType(), out List<ObjectGroup> objectGroups))
                {
                    continue;
                }

                objectGroups.Add(group);
                OnObjectGroupAdded?.Invoke(GetParentId(group.Data.GetObjectType()), group);
            }
        }

        public void ClearAllObjects()
        {
            _weaponGroups.Clear();
            _gearGroups.Clear();
            _materialGroups.Clear();
            _itemGroups.Clear();
        }

        private bool AddAndCreatePackageGroupInternal(
            string parentId,
            ObjectInfo objectInfoData,
            List<ObjectGroup> existedPackageGroups,
            int number,
            bool forceAddAll
        )
        {
            if (number <= 0)
            {
                return true;
            }

            List<ObjectGroup> _objectGroups = GetObjectGroups(objectInfoData.Type);
            if (_objectGroups == null)
            {
                return false;
            }
            
            // 记录实际获得的数量和是否不能获得更多物品
            var addNumber = 0;
            var notGetMore = false;
            var addSuccess = true;

            var objectData = objectInfoData.ToObjectData();
            // 判断物品组是否存在限制
            if (objectData.QuantitativeRestriction == PackageQuantityRestriction.OnlyOneGroup)
            {
                // 如果是仅允许有一个物品组，则判断当前是否存在物品组，并添加物品数量，如果数量超过限制就发送拒绝事件
                if (existedPackageGroups.Count == 0)
                {
                    var objectGroup = ObjectGroup.CreateNew(objectData, 0);
                    if (number <= objectData.GroupMaximum)
                    {
                        objectGroup.Number = number;
                        addNumber += number;
                        _objectGroups.Add(objectGroup);
                        OnObjectGroupAdded?.Invoke(parentId,objectGroup);
                    }
                    else
                    {
                        // 物品数量超出限制，就判断本次操作是否强制添加所有物品，是则视为添加失败
                        if (forceAddAll)
                        {
                            notGetMore = true;
                            addSuccess = false;
                        }
                        else
                        {
                            objectGroup.Number = objectData.GroupMaximum;
                            addNumber += objectData.GroupMaximum;
                            notGetMore = true;
                            _objectGroups.Add(objectGroup);
                            OnObjectGroupAdded?.Invoke(parentId,objectGroup);
                        }
                    }
                }
                else
                {
                    var objectGroup = existedPackageGroups[0];
                    // 根据已存在物品组的数量限制执行不同逻辑
                    if (objectGroup.Number >= objectGroup.Data.GroupMaximum)
                    {
                        notGetMore = true;
                        addSuccess = false;
                    }
                    else
                    {
                        var gap = objectGroup.Data.GroupMaximum - objectGroup.Number;
                        if (number <= gap)
                        {
                            objectGroup.Number += number;
                            addNumber += number;
                            OnObjectGroupChanged?.Invoke(parentId,objectGroup);
                        }
                        else
                        {
                            // 物品数量超出限制，就判断本次操作是否强制添加所有物品，是则视为添加失败
                            if (forceAddAll)
                            {
                                notGetMore = true;
                                addSuccess = false;
                            }
                            else
                            {
                                objectGroup.Number += gap;
                                addNumber += gap;
                                notGetMore = true;
                                OnObjectGroupChanged?.Invoke(parentId,objectGroup);
                            }
                        }
                    }
                }
            }
            else
            {
                // 如果允许有多个组，就将数量拆分为每组允许的数量依次添加，先添加到已有组，再创建新组
                // 这里是添加到已有组
                foreach (var objectGroup in existedPackageGroups)
                {
                    if (objectGroup.Number >= objectGroup.Data.GroupMaximum)
                    {
                        continue;
                    }

                    var gap = objectGroup.Data.GroupMaximum - objectGroup.Number;
                    if (number <= gap)
                    {
                        objectGroup.Number += number;
                        addNumber += number;
                    }
                    else
                    {
                        objectGroup.Number += gap;
                        addNumber += gap;
                    }

                    OnObjectGroupChanged?.Invoke(parentId,objectGroup);
                    number -= gap;
                    if (number <= 0)
                    {
                        break;
                    }
                }

                // 这里是创建新组
                while (number > 0)
                {
                    var objectGroup = ObjectGroup.CreateNew(objectData, 0);
                    if (number <= objectData.GroupMaximum)
                    {
                        objectGroup.Number = number;
                        addNumber += number;
                    }
                    else
                    {
                        objectGroup.Number = objectData.GroupMaximum;
                        addNumber += objectData.GroupMaximum;
                    }

                    _objectGroups.Add(objectGroup);
                    OnObjectGroupAdded?.Invoke(parentId,objectGroup);
                    number -= objectData.GroupMaximum;
                }
            }

            if (addNumber > 0)
            {
                NotifyObjectGet(objectData, addNumber);
            }

            if (notGetMore)
            {
                NotifyObjectNotGetMore(objectData);
            }

            return addSuccess;
        }

        private IEnumerable<ObjectGroup> GetAllObjectGroups()
        {
            foreach (ObjectGroup group in _weaponGroups)
            {
                yield return group;
            }

            foreach (ObjectGroup group in _gearGroups)
            {
                yield return group;
            }

            foreach (ObjectGroup group in _materialGroups)
            {
                yield return group;
            }

            foreach (ObjectGroup group in _itemGroups)
            {
                yield return group;
            }
        }

        private string GetParentId(string type)
        {
            return ObjectInfoExtension.TryParseObjectType(type, out ObjectType objectType)
                ? GetParentId(objectType)
                : string.Empty;
        }

        private string GetParentId(ObjectType type)
        {
            string key = type.GetString();
            foreach (BagData bagData in typeList)
            {
                if (bagData.key == key)
                {
                    return bagData.id;
                }
            }

            return key;
        }

        private void NotifyObjectGet(ObjectData objectData, int number)
        {
            GameApplication.Instance?.EventCenter?.TriggerEvent<NotificationGetEventParameter>(
                GameEvents.NotificationGet,
                new NotificationGetEventParameter
                {
                    ThumbnailAtlas = objectData.ThumbnailAtlas,
                    ThumbnailName = objectData.ThumbnailName,
                    Name = objectData.Name,
                    Number = number
                });
        }

        private void NotifyObjectLost(ObjectData objectData, int number)
        {
            GameApplication.Instance?.EventCenter?.TriggerEvent<NotificationLostEventParameter>(
                GameEvents.NotificationLost,
                new NotificationLostEventParameter
                {
                    ThumbnailAtlas = objectData.ThumbnailAtlas,
                    ThumbnailName = objectData.ThumbnailName,
                    Name = objectData.Name,
                    Number = number
                });
        }

        private void NotifyObjectNotGetMore(ObjectData objectData)
        {
            GameApplication.Instance?.EventCenter?.TriggerEvent<NotificationNotGetMoreEventParameter>(
                GameEvents.NotificationNotGetMore,
                new NotificationNotGetMoreEventParameter
                {
                    ThumbnailAtlas = objectData.ThumbnailAtlas,
                    ThumbnailName = objectData.ThumbnailName,
                    Name = objectData.Name,
                });
        }
    }
}
