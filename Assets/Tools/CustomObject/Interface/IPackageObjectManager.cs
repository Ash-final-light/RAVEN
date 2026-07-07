using System;
using System.Collections.Generic;
using Tools.CustomObject.Data;

namespace Tools.CustomObject.Interface
{
    /// <summary>
    /// 采用了注入的方式,和玩家背包系统的约定
    /// </summary>
    public interface IPackageObjectManager
    {
        bool AddObjectById(int objId, int num, bool forceAddAll = false);
        int RemoveObjectById(int objId, int num = int.MaxValue);
        int RemoveObjectGroup(string groupId, int num = int.MaxValue);
        void Sort(ObjectType type, Comparison<ObjectGroup> comparison);
    }
}
