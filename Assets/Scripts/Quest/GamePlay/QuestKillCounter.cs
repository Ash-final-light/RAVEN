using System.Collections.Generic;
using Tools.Quest.Event;
using UnityEngine;

namespace Quest.GamePlay
{
    public sealed class QuestKillCounter : MonoBehaviour
    {
        [SerializeField] private string enemyKilledEvent = QuestGamePlayEvents.FirstEnemyKilled;
        [SerializeField] private string completedEvent = QuestGamePlayEvents.FirstWaveCompleted;
        [SerializeField] private string completedSourceId = "quest_kill_counter";
        [SerializeField, Min(1)] private int requiredKills = 3;

        private readonly HashSet<string> defeatedEnemies = new();
        private bool completed;
        private bool subscribed;

        public int CurrentKills => Mathf.Min(defeatedEnemies.Count, requiredKills);
        public int RequiredKills => requiredKills;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Start() => Subscribe();
        private void OnDisable() => Unsubscribe();

        public void ResetCounter()
        {
            completed = false;
            defeatedEnemies.Clear();
        }

        private void Subscribe()
        {
            if (subscribed || string.IsNullOrWhiteSpace(enemyKilledEvent))
            {
                return;
            }

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestKillCounter)}] QuestEventHub is missing. Event '{enemyKilledEvent}' was not subscribed.", this);
                return;
            }

            hub.Subscribe(enemyKilledEvent, HandleEnemyKilled);
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
                hub.Unsubscribe(enemyKilledEvent, HandleEnemyKilled);
            }

            subscribed = false;
        }

        private void HandleEnemyKilled(QuestEventMessage message)
        {
            if (completed || string.IsNullOrWhiteSpace(message.sourceId) || !defeatedEnemies.Add(message.sourceId))
            {
                return;
            }

            if (defeatedEnemies.Count < requiredKills)
            {
                return;
            }

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestKillCounter)}] QuestEventHub is missing. Event '{completedEvent}' was not published.", this);
                return;
            }

            completed = true;
            hub.Publish(new QuestEventMessage(
                completedEvent,
                QuestEventValue.FromBool(true),
                QuestEventRecordMode.Latch,
                completedSourceId,
                gameObject));
        }
    }
}
