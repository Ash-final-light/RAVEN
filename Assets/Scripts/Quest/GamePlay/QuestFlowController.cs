using Tools.Quest.Event;
using UnityEngine;

namespace Quest.GamePlay
{
    public sealed class QuestFlowController : MonoBehaviour
    {
        [SerializeField] private string completeWhenEvent = QuestGamePlayEvents.FinalEnemyKilled;
        [SerializeField] private string questCompletedEvent = QuestGamePlayEvents.QuestCompleted;
        [SerializeField] private string completedSourceId = "quest_flow";
        [SerializeField] private bool unsubscribeAfterCompleted = true;

        private bool completed;
        private bool subscribed;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Start() => Subscribe();
        private void OnDisable() => Unsubscribe();

        public void ResetFlow()
        {
            completed = false;
        }

        private void Subscribe()
        {
            if (subscribed || string.IsNullOrWhiteSpace(completeWhenEvent))
            {
                return;
            }

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestFlowController)}] QuestEventHub is missing. Event '{completeWhenEvent}' was not subscribed.", this);
                return;
            }

            hub.Subscribe(completeWhenEvent, HandleCompletedCondition);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (QuestEventHub.TryGetInstance(out var hub))
            {
                hub.Unsubscribe(completeWhenEvent, HandleCompletedCondition);
            }

            subscribed = false;
        }

        private void HandleCompletedCondition(QuestEventMessage message)
        {
            if (completed)
            {
                return;
            }

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestFlowController)}] QuestEventHub is missing. Event '{questCompletedEvent}' was not published.", this);
                return;
            }

            completed = true;
            hub.Publish(new QuestEventMessage(
                questCompletedEvent,
                QuestEventValue.FromBool(true),
                QuestEventRecordMode.Latch,
                completedSourceId,
                gameObject));

            if (unsubscribeAfterCompleted)
            {
                Unsubscribe();
            }
        }
    }
}
