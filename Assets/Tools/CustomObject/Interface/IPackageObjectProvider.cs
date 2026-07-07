using System.Collections.Generic;
using Tools.CustomObject.Data;

namespace Tools.CustomObject.Interface
{
    public interface IPackageObjectProvider
    {
        int GetObjectCountById(int id);
        ObjectGroup GetObjectGroup(string groupId);
        IReadOnlyList<ObjectGroup> GetObjectGroups(ObjectType type);
        List<ObjectGroup> GetSelectedObjectGroups(string key);
        ObjectInfo GetObjectInfoById(int id);
    }
}
