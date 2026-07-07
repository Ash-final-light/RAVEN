using System;
using Quest;
using Tools.Quest.Require;
using Tools.Quest.Step;

namespace Tools.Quest.Data
{
    [Serializable]
    public class QuestInfo
    {
        public string id; // 任务id
        public string title; // 任务标题
        public string description; // 任务描述
        public QuestType questType; // 任务类型
        public string awaitSubmitDescription; // 任务等待提交的描述
        public string completedDescription; // 任务完成后的描述
        [NonSerialized] public QuestRequirementInfo[] requirements; // 任务需求数组
        [NonSerialized] public QuestStepInfo[] steps; // 任务步骤数组
        [NonSerialized] public QuestRewardInfo[] rewards; // 任务奖励数组
    }
}