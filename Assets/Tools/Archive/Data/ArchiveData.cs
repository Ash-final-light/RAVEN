using System;
using Daily.Config;

namespace Archive.Data
{
    /// <summary>
    /// 存档数据，用于存储本地以及从本地读取。
    /// </summary>
    [Serializable]
    public class ArchiveData
    {
        public int id = -1;
        public bool auto = true;
        public float playTime = 0f;
        public long archiveTimestamp = 0;

        public DailyArchiveData daily = new DailyArchiveData();
    }
}
