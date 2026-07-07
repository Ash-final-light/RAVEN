using Tools.Quest.Event;

namespace Tools.Quest.Progression
{
    public interface IPlayerProgressionService
    {
        int Level { get; }
        int Experience { get; }
        void AddExperience(int amount);
        void SetLevel(int level);
    }

    public class PlayerProgressionService : IPlayerProgressionService
    {
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }

        public PlayerProgressionService()
        {
            PublishState();
        }

        public void AddExperience(int amount)
        {
            Experience += amount;
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                "player.exp.changed",
                QuestEventValue.FromInt(Experience),
                QuestEventRecordMode.LatestValue,
                "player"));
        }

        public void SetLevel(int level)
        {
            Level = System.Math.Max(1, level);
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                "player.level.changed",
                QuestEventValue.FromInt(Level),
                QuestEventRecordMode.LatestValue,
                "player"));
        }

        private void PublishState()
        {
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                "player.level.changed",
                QuestEventValue.FromInt(Level),
                QuestEventRecordMode.LatestValue,
                "player"));
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                "player.exp.changed",
                QuestEventValue.FromInt(Experience),
                QuestEventRecordMode.LatestValue,
                "player"));
        }
    }
}
