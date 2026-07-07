using System;
using System.Collections.Generic;
using System.Text;
using Tools.Quest.Require;
using Tools.Quest.Step;

namespace Tools.Quest.Data
{
    [Serializable]
    public class Quest
    {
        public QuestInfo info; // 任务配置信息，这个数据是预设数据，不可改变
        public QuestState state; // 任务状态
        public QuestRequire[] requirements; // 任务需求运行数据数组
        public QuestStep[] steps; // 任务步骤运行数据数组
        public QuestReward[] rewards; // 任务奖励运行数据数组

        /// <summary>
        /// 任务步骤索引，如果全部任务步骤都完成，则返回步骤数组长度，默认返回线性的未完成的步骤索引
        /// </summary>
        public int StepIndex
        {
            get
            {
                for (var i = 0; i < steps.Length; i++)
                {
                    var step = steps[i];
                    if (!step.completed)
                    {
                        return i;
                    }
                }

                return steps.Length;
            }
        }
        public string CurrentStepShowGoal
        {
            get
            {
                if (steps == null || steps.Length == 0 || StepIndex >= steps.Length)
                    return string.Empty;

                var sb = new StringBuilder();
                
                QuestStep currentStep = steps[StepIndex];
                switch (currentStep.info.goalRelation)
                {
                    case QuestStepGoalRelation.Linear:
                        foreach (var goal in currentStep.goals)
                        {
                            var temp = goal.description;
                            if (!goal.completed)
                                return temp;
                        }
                        break;
                    case QuestStepGoalRelation.ParallelAnd:
                        //TODO：并行与关系任务目标描述格式化
                        throw new Exception("并行与关系任务目标描述格式化未实现");
                        break;
                    case QuestStepGoalRelation.ParallelOr:
                        //TODO：并行或关系任务目标描述格式化
                        throw new Exception("并行或关系任务目标描述格式化未实现");
                        break;
                }

                return sb.ToString();
            }
        }
    }
}
