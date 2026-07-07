using System;
using System.Collections.Generic;
using Framework.Core.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Quest.Event
{
    public class QuestEventHub : MonoSingleton<QuestEventHub>
    {
        private readonly Dictionary<string, Action<QuestEventMessage>> listeners = new();
        private readonly Dictionary<string, QuestEventValue> ledger = new();

        [SerializeField] private QuestEventCatalog catalog;
        [SerializeField, Min(1)] private int recentEventLimit = 12;
        [ShowInInspector, ReadOnly] private readonly List<string> activeListeners = new();
        [ShowInInspector, ReadOnly] private readonly List<string> recentEvents = new();

        public event Action<QuestEventMessage> Published;
        public QuestEventCatalog Catalog => catalog;

        public void SetCatalog(QuestEventCatalog value)
        {
            catalog = value;
        }

        public void Subscribe(string eventName, Action<QuestEventMessage> listener)
        {
            if (string.IsNullOrWhiteSpace(eventName) || listener == null)
                return;

            listeners.TryGetValue(eventName, out var actions);
            actions += listener;
            listeners[eventName] = actions;
            RefreshListenerDebug();
        }

        public void Unsubscribe(string eventName, Action<QuestEventMessage> listener)
        {
            if (string.IsNullOrWhiteSpace(eventName) || listener == null || !listeners.TryGetValue(eventName, out var actions))
                return;

            actions -= listener;
            if (actions == null)
                listeners.Remove(eventName);
            else
                listeners[eventName] = actions;
            RefreshListenerDebug();
        }

        public void Publish(QuestEventMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.eventName))
            {
                Debug.LogWarning("[QuestEventHub] Ignored event with an empty name.");
                return;
            }

            if (catalog != null && catalog.TryGet(message.eventName, out var definition))
            {
                message.recordMode = definition.recordMode;
                if (definition.valueType != QuestEventValueType.None && message.value.type != definition.valueType)
                {
                    Debug.LogWarning($"[QuestEventHub] Event '{message.eventName}' expects {definition.valueType}, got {message.value.type}.");
                }
            }

            Record(message, message.sourceId);
            if (!string.IsNullOrEmpty(message.sourceId))
                Record(message, string.Empty);

            if (listeners.TryGetValue(message.eventName, out var actions))
                actions?.Invoke(message);

            Published?.Invoke(message);
            recentEvents.Insert(0, $"{message.eventName} | source={message.sourceId} | value={message.value}");
            if (recentEvents.Count > recentEventLimit)
                recentEvents.RemoveRange(recentEventLimit, recentEvents.Count - recentEventLimit);
        }

        public bool TryGetRecordedValue(string eventName, string sourceId, out QuestEventValue value)
        {
            return ledger.TryGetValue(BuildKey(eventName, sourceId), out value);
        }

        public void ClearRuntimeData()
        {
            listeners.Clear();
            ledger.Clear();
            activeListeners.Clear();
            recentEvents.Clear();
            Published = null;
        }

        public override void Dispose()
        {
            ClearRuntimeData();
            base.Dispose();
        }

        protected override void OnDestroy()
        {
            // MonoSingleton.OnDestroy calls Dispose(), whose default implementation
            // destroys the same GameObject again. Unity already owns the destruction
            // at this point, so only release this hub's runtime state here.
            ClearRuntimeData();
            ReleaseInstanceReference();
        }

        private void Record(QuestEventMessage message, string sourceId)
        {
            if (message.recordMode == QuestEventRecordMode.Pulse)
                return;

            var key = BuildKey(message.eventName, sourceId);
            ledger.TryGetValue(key, out var previous);
            ledger[key] = message.recordMode switch
            {
                QuestEventRecordMode.Accumulate => QuestEventValue.FromInt(previous.AsInt() + message.value.AsInt(1)),
                QuestEventRecordMode.Latch => QuestEventValue.FromBool(previous.AsInt() > 0 || message.value.AsInt(1) > 0),
                _ => message.value,
            };
        }

        private static string BuildKey(string eventName, string sourceId)
        {
            return $"{eventName}\u001f{sourceId ?? string.Empty}";
        }

        private void RefreshListenerDebug()
        {
            activeListeners.Clear();
            foreach (var pair in listeners)
                activeListeners.Add($"{pair.Key} ({pair.Value?.GetInvocationList().Length ?? 0})");
            activeListeners.Sort(StringComparer.Ordinal);
        }
    }
}
