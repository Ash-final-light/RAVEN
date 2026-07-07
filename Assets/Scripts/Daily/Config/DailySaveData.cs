using System;
using System.Collections.Generic;
using Tools.CustomObject.Data;

namespace Daily.Config
{
    [Serializable]
    public class DailySaveData
    {
        public int version = 1;
        public DailyMissionSaveData mission = new DailyMissionSaveData();
        public DailyStepSaveData step = new DailyStepSaveData();
        public List<DailyNodeSaveData> nodes = new List<DailyNodeSaveData>();
        public DailyCardHandSaveData cards = new DailyCardHandSaveData();
    }

    [Serializable]
    public class DailyWeekSaveData
    {
        public int version = 1;
        public int currentDayIndex;
        public List<DailyDaySaveData> days = new List<DailyDaySaveData>();
    }

    [Serializable]
    public class DailyDaySaveData
    {
        public int dayIndex;
        public int archiveId = -1;
        public string weekName;
        public DailySaveData daily = new DailySaveData();
    }

    [Serializable]
    public class DailyArchiveData
    {
        public DailyWeekSaveData week = new DailyWeekSaveData();
        public PackageObjectArchiveData package = new PackageObjectArchiveData();
    }

    [Serializable]
    public class DailyMissionSaveData
    {
        public List<DailyMissionProgressSaveData> progresses = new List<DailyMissionProgressSaveData>();
    }

    [Serializable]
    public class DailyMissionProgressSaveData
    {
        public DailyWeeklyMissionType missionType;
        public int currentValue;
    }

    [Serializable]
    public class DailyStepSaveData
    {
        public string currentNodeId;
        public int dailyActionUsed;
        public int dailyActionLimit;
        public int crossValidationCount;
    }

    [Serializable]
    public class DailyNodeSaveData
    {
        public string nodeId;
        public bool revealed;
        public bool isVisited;
    }

    [Serializable]
    public class DailyCardHandSaveData
    {
        public List<int> stepCounts = new List<int>();
    }
}
