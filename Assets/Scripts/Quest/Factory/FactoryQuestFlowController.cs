using Tools.Quest.Event;
using UnityEngine;

namespace Quest.Factory
{
    public sealed class FactoryQuestFlowController : MonoBehaviour
    {
        private bool completed;
        private bool subscribed;

        private void OnEnable()
        {
            if (Application.isPlaying)
                Subscribe();
        }
        private void Start() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (subscribed)
                return;
            QuestEventHub.Instance.Subscribe(FactoryQuestEvents.FinalEnemyKilled, HandleFinalEnemyKilled);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;
            if (QuestEventHub.TryGetInstance(out var hub))
                hub.Unsubscribe(FactoryQuestEvents.FinalEnemyKilled, HandleFinalEnemyKilled);
            subscribed = false;
        }

        private void HandleFinalEnemyKilled(QuestEventMessage message)
        {
            if (completed)
                return;
            completed = true;
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                FactoryQuestEvents.QuestCompleted,
                QuestEventValue.FromBool(true),
                QuestEventRecordMode.Latch,
                "factory_quest_flow",
                gameObject));
        }
    }
}
