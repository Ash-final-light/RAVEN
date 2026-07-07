using Sirenix.OdinInspector;
using Tools.Quest.Event;

namespace Tools.Quest.Goal
{
    [System.Obsolete("Use EventValueQuestGoal with player.level.changed instead.")]
    public class LevelQuestQuestGoal : EventValueQuestGoal
    {
        [Title("等级任务目标"),ShowInInspector] public int level;

        public override void StartGoal(QuestGoal goal)
        {
            eventName = "player.level.changed";
            comparison = QuestValueComparison.GreaterOrEqual;
            expectedValue = QuestEventValue.FromInt(level);
            base.StartGoal(goal);
        }
    }
}
