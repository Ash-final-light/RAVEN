using System;
using System.Collections.Generic;
using System.Linq;
using Quest;
using Sirenix.OdinInspector;
using Tools.Quest.Goal;
using Tools.Quest.Require;
using Tools.Quest.Step;
using UnityEngine;

namespace Tools.Quest.Data
{
    [Serializable]
    public class QuestConfig:ScriptableObject
    {
        public string id;
        public string title;
        public string description;
        public string awaitSubmitDescription;
        public string afterCompletedDescription;
        public QuestType questType;
        
        [Title("任务前置需求列表"),SerializeReference,TypeFilter("GetRequire")]
        public List<BaseQuestRequire> preRequires = new ();
        [Title("任务步骤列表"),SerializeReference,TypeFilter("GetStep")]
        public List<BaseQuestStep> steps = new ();
        [Title("任务奖励列表"),SerializeReference,TypeFilter("GetReward")]
        public List<BaseQuestRewards> rewards = new ();
        
        
        IEnumerable<Type> GetRequire()
        {
            var q = typeof(BaseQuestRequire).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(BaseQuestRequire).IsAssignableFrom(x));
            return q;
        }
        IEnumerable<Type> GetStep()
        {
            var q = typeof(BaseQuestStep).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(BaseQuestStep).IsAssignableFrom(x));
            return q;
        }
        IEnumerable<Type> GetReward()
        {
            var q = typeof(BaseQuestRewards).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(BaseQuestRewards).IsAssignableFrom(x));
            return q;
        }
        
        public QuestConfig Clone()
        {
            var questConfig = Instantiate(this);
            return questConfig;
        }
    }

    public static class QuestConfigExtensions
    {
        public static QuestInfo ToQuestInfo(this QuestConfig questConfig)
        {
            return new QuestInfo()
            {
                id = questConfig.id,
                title = questConfig.title,
                description = questConfig.description,
                awaitSubmitDescription = questConfig.awaitSubmitDescription,
                completedDescription = questConfig.afterCompletedDescription,
                requirements = questConfig.preRequires.Select(x => x.ToQuestRequirementInfo()).ToArray(),
                steps = questConfig.steps.Select(x => x.ToQuestStepInfo()).ToArray(),
                rewards = questConfig.rewards.Select(x => x.ToQuestRewardInfo()).ToArray(),
                questType = questConfig.questType,
            };
        }
        public static QuestRequirementInfo ToQuestRequirementInfo(this BaseQuestRequire require)
        {
            return new QuestRequirementInfo()
            {
                id = require.id,
                Update = require.UpdateStatus
            };
        }
        public static QuestStepInfo ToQuestStepInfo(this BaseQuestStep step)
        {
            return new QuestStepInfo()
            {
                id = step.id,
                description = step.description,
                goals = step.goals.Select(x => x.ToQuestGoalInfo()).ToArray(),
                goalRelation = step.goalRelation,

                Start = step.StartStep,
                Update = step.UpdateStep,
                Complete = step.EndStep,
                Stop = step.StopStep,
                Interrupt = step.InterruptStep,
            };
        }
        public static QuestGoalInfo ToQuestGoalInfo(this BaseQuestGoal questGoal)
        {
            return new QuestGoalInfo()
            {
                id = questGoal.id,
                description = questGoal.description,
                Start = questGoal.Start,
                Update = questGoal.Update,
                Complete = questGoal.Complete,
                Interrupt = questGoal.Interrupt,
            };
        }
        public static QuestRewardInfo ToQuestRewardInfo(this BaseQuestRewards rewards)
        {
            return new QuestRewardInfo()
            {
                id = rewards.id,
                GiveReward = rewards.GiveReward
            };
        }
    }
}
