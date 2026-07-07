using System;
using System.Collections.Generic;

namespace Tools.CustomObject.Data
{
    [Serializable]
    public class PackageObjectArchiveData
    {
        public List<PackageObjectGroupArchiveData> groups = new();
    }

    [Serializable]
    public class PackageObjectGroupArchiveData
    {
        public int id;
        public string groupId;
        public int number;
        public bool isNew;
        public long getTimestamp;
    }
}
