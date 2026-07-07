using System;

namespace Tools.CustomObject.Data.Extension
{
    public static class ObjectArchiveExtension
    {
        public static PackageObjectGroupArchiveData ToArchiveData(this ObjectGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            return new PackageObjectGroupArchiveData
            {
                id = group.Data.Id,
                groupId = group.GroupId,
                number = group.Number,
                isNew = group.New,
                getTimestamp = group.GetTimestamp
            };
        }

        public static ObjectGroup ToObjectGroup(
            this PackageObjectGroupArchiveData archiveData,
            ObjectInfocontainer objectInfoContainer)
        {
            if (archiveData == null)
            {
                throw new ArgumentNullException(nameof(archiveData));
            }

            if (objectInfoContainer == null ||
                !objectInfoContainer.Data.TryGetValue(archiveData.id, out ObjectInfo objectInfo))
            {
                throw new ArgumentException($"ObjectInfo id={archiveData.id} not found.");
            }

            return new ObjectGroup
            {
                GroupId = archiveData.groupId,
                Number = archiveData.number,
                New = archiveData.isNew,
                GetTimestamp = archiveData.getTimestamp,
                Data = objectInfo.ToObjectData(),
            };
        }
    }
}
