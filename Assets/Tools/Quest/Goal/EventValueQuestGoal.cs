using System;
using Sirenix.OdinInspector;
using Tools.Quest.Event;

namespace Tools.Quest.Goal
{
    [Serializable]
    public class EventValueQuestGoal : BaseQuestGoal
    {
        [Title("事件值目标")] public string eventName;
        public string sourceId;
        public QuestValueComparison comparison = QuestValueComparison.GreaterOrEqual;
        public QuestEventValue expectedValue = new() { type = QuestEventValueType.Integer, intValue = 1 };
        public bool readExistingValue = true;

        [ShowInInspector, ReadOnly] private bool hasValue;
        [ShowInInspector, ReadOnly] private QuestEventValue currentValue;
        private QuestGoal runtimeGoal;
        private bool subscribed;

        public override void StartGoal(QuestGoal goal)
        {
            runtimeGoal = goal;
            if (readExistingValue)
                hasValue = QuestEventHub.TryGetInstance(out var hub) &&
                           hub.TryGetRecordedValue(eventName, sourceId, out currentValue);
            Subscribe();
        }

        public override void UpdateGoal(QuestGoal goal, float deltaTime)
        {
            runtimeGoal = goal;
            goal.completed = QuestEventComparisonUtility.Compare(hasValue, currentValue, comparison, expectedValue);
        }

        public override void EndGoal(QuestGoal goal) => Unsubscribe();
        public override void StopGoal(QuestGoal goal) => Unsubscribe();
        public override void InterruptGoal(QuestGoal goal) => Unsubscribe();

        protected override string FormatDescription()
        {
            return (description ?? string.Empty)
                .Replace("{Current}", hasValue ? currentValue.ToString() : "-")
                .Replace("{Target}", expectedValue.ToString())
                .Replace("{Event}", eventName ?? string.Empty);
        }

        protected override string SerializeToState()
        {
            return hasValue ? currentValue.ToString() : string.Empty;
        }

        protected override void DeserializeFromState(string state)
        {
        }

        private void HandleEvent(QuestEventMessage message)
        {
            if (!string.IsNullOrEmpty(sourceId) && message.sourceId != sourceId)
                return;

            currentValue = message.value;
            hasValue = true;
            if (runtimeGoal != null)
            {
                runtimeGoal.completed = QuestEventComparisonUtility.Compare(true, currentValue, comparison, expectedValue);
                runtimeGoal.description = FormatDescription();
                runtimeGoal.state = SerializeToState();
            }
        }

        private void Subscribe()
        {
            if (subscribed || string.IsNullOrWhiteSpace(eventName))
                return;
            QuestEventHub.Instance.Subscribe(eventName, HandleEvent);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;
            if (QuestEventHub.TryGetInstance(out var hub))
                hub.Unsubscribe(eventName, HandleEvent);
            subscribed = false;
            runtimeGoal = null;
        }
    }
}
