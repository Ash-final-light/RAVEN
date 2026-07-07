using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Tools.Quest.Goal;
using UnityEngine;

namespace Tools.Quest.Step
{
    public enum QuestStepGoalRelation
    {
        Linear,
        ParallelAnd,
        ParallelOr,
    }
    [Serializable]
    public abstract class BaseQuestStep
    {
        [Title("任务步骤目标关系"), ShowInInspector,ReadOnly]
        public QuestStepGoalRelation goalRelation = QuestStepGoalRelation.Linear;

        [InlineButton("ResetId")] public string id = Guid.NewGuid().ToString();
        private void ResetId()
        {
            id = Guid.NewGuid().ToString();
        }
        public string description;
        [Title("任务步骤目标列表"), SerializeReference,TypeFilter("GetGoalFilteredTypeList")]
        public List<BaseQuestGoal> goals = new();
        
        
        private IEnumerable<Type> GetGoalFilteredTypeList()
        {
            var q = typeof(BaseQuestGoal).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(BaseQuestGoal).IsAssignableFrom(x));
            return q;
        }

        public abstract void StartStep(QuestStep step);
        public abstract void UpdateStep(QuestStep step,float deltaTime);
        public abstract void EndStep(QuestStep step);
        public abstract void StopStep(QuestStep step);
        public abstract void InterruptStep(QuestStep step);
    }
}