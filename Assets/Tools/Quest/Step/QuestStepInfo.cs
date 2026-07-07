using System;
using Tools.Quest.Goal;

namespace Tools.Quest.Step
{
    [Serializable]
    public class QuestStepInfo
    {
        public string id; // 任务步骤id
        public string description; // 任务步骤标题
        public QuestStepGoalRelation goalRelation; // 任务目标联系
        [NonSerialized] public QuestGoalInfo[] goals; // 任务步骤目标数组
        [NonSerialized] public QuestStepStart Start; // 任务步骤开始函数委托
        [NonSerialized] public QuestStepUpdate Update; // 任务步骤更新函数委托
        [NonSerialized] public QuestStepComplete Complete; // 任务步骤完成函数委托
        [NonSerialized] public QuestStepStop Stop; // 任务步骤停止函数委托
        [NonSerialized] public QuestStepInterrupt Interrupt; // 任务步骤中断函数委托
    }

    public delegate void QuestStepStart(QuestStep step);

    public delegate void QuestStepUpdate(QuestStep step, float deltaTime);

    public delegate void QuestStepComplete(QuestStep step);

    public delegate void QuestStepInterrupt(QuestStep step);

    public delegate void QuestStepStop(QuestStep step);
}