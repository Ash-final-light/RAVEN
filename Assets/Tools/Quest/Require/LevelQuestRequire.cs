using Sirenix.OdinInspector;
using Tools.Quest.Event;

namespace Tools.Quest.Require
{
    [System.Obsolete("Use EventValueQuestRequire with player.level.changed instead.")]
    public class LevelQuestRequire : EventValueQuestRequire
    {
        [Title("任务前置等级"),ShowInInspector] public int level;

        public override void UpdateStatus(QuestRequire require, float deltaTime)
        {
            eventName = "player.level.changed";
            comparison = QuestValueComparison.GreaterOrEqual;
            expectedValue = QuestEventValue.FromInt(level);
            base.UpdateStatus(require, deltaTime);
        }
    }
}
