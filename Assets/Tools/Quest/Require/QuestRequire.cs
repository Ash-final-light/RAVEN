using System;

namespace Tools.Quest.Require
{
    [Serializable]
    public class QuestRequire
    {
        public QuestRequirementInfo info; // 任务需求配置信息
        public string description; //  任务需求描述
        public bool meet; // 任务需求是否满足
    }
}