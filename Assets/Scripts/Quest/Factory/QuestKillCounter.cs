using System.Collections.Generic;
using Tools.Quest.Event;
using UnityEngine;

namespace Quest.Factory
{
    public sealed class QuestKillCounter : MonoBehaviour
    {
        [SerializeField] private string enemyKilledEvent = FactoryQuestEvents.FirstEnemyKilled;
        [SerializeField] private string completedEvent = FactoryQuestEvents.FirstWaveCompleted;
        [SerializeField, Min(1)] private int requiredKills = 3;
        private readonly HashSet<string> defeatedEnemies = new();
        private bool completed;
        private bool subscribed;

        public int CurrentKills => Mathf.Min(defeatedEnemies.Count, requiredKills);

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
            QuestEventHub.Instance.Subscribe(enemyKilledEvent, HandleEnemyKilled);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;
            if (QuestEventHub.TryGetInstance(out var hub))
                hub.Unsubscribe(enemyKilledEvent, HandleEnemyKilled);
            subscribed = false;
        }

        private void HandleEnemyKilled(QuestEventMessage message)
        {
            if (completed || string.IsNullOrWhiteSpace(message.sourceId) || !defeatedEnemies.Add(message.sourceId))
                return;
            if (defeatedEnemies.Count < requiredKills)
                return;

            completed = true;
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                completedEvent,
                QuestEventValue.FromBool(true),
                QuestEventRecordMode.Latch,
                "factory_wave_first",
                gameObject));
        }
    }
}
