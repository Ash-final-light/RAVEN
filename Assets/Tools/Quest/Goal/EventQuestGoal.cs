using System;
using Sirenix.OdinInspector;
using Tools.Quest.Event;
using UnityEngine;

namespace Tools.Quest.Goal
{
    [Serializable]
    public class EventQuestGoal : BaseQuestGoal
    {
        [Title("事件计数目标")] public string eventName;
        public string sourceId;
        [MinValue(1)] public int requiredAmount = 1;
        public bool includeRecordedProgress;

        [ShowInInspector, ReadOnly] private int currentAmount;
        private QuestGoal runtimeGoal;
        private bool subscribed;

        public override void StartGoal(QuestGoal goal)
        {
            runtimeGoal = goal;
            if (includeRecordedProgress &&
                QuestEventHub.TryGetInstance(out var hub) &&
                hub.TryGetRecordedValue(eventName, sourceId, out var value))
                currentAmount = Mathf.Max(currentAmount, value.AsInt());
            Subscribe();
        }

        public override void UpdateGoal(QuestGoal goal, float deltaTime)
        {
            runtimeGoal = goal;
            goal.completed = currentAmount >= Mathf.Max(1, requiredAmount);
        }

        public override void EndGoal(QuestGoal goal)
        {
            Unsubscribe();
        }

        public override void StopGoal(QuestGoal goal)
        {
            Unsubscribe();
        }

        public override void InterruptGoal(QuestGoal goal)
        {
            Unsubscribe();
        }

        protected override string FormatDescription()
        {
            return (description ?? string.Empty)
                .Replace("{Current}", currentAmount.ToString())
                .Replace("{Target}", Mathf.Max(1, requiredAmount).ToString())
                .Replace("{Event}", eventName ?? string.Empty);
        }

        protected override string SerializeToState()
        {
            return currentAmount.ToString();
        }

        protected override void DeserializeFromState(string state)
        {
            if (!string.IsNullOrWhiteSpace(state) && int.TryParse(state, out var value))
                currentAmount = value;
        }

        private void HandleEvent(QuestEventMessage message)
        {
            if (!string.IsNullOrEmpty(sourceId) && message.sourceId != sourceId)
                return;

            currentAmount += Mathf.Max(1, message.value.AsInt(1));
            if (runtimeGoal != null)
            {
                runtimeGoal.completed = currentAmount >= Mathf.Max(1, requiredAmount);
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
