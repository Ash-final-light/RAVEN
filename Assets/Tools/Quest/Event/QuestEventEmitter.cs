using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Quest.Event
{
    public class QuestEventEmitter : MonoBehaviour
    {
        [SerializeField] private string eventName;
        [SerializeField] private string sourceId;
        [SerializeField] private QuestEventRecordMode recordMode = QuestEventRecordMode.Pulse;
        [SerializeField] private QuestEventValue value = new() { type = QuestEventValueType.None };
        [SerializeField] private bool emitOnce;
        [ShowInInspector, ReadOnly] private bool emitted;

        public string EventName => eventName;

        public void Configure(string name, string source, QuestEventRecordMode mode, QuestEventValue eventValue, bool once)
        {
            eventName = name;
            sourceId = source;
            recordMode = mode;
            value = eventValue;
            emitOnce = once;
        }

        [Button]
        public void Emit()
        {
            if (emitOnce && emitted)
                return;

            QuestEventHub.Instance.Publish(new QuestEventMessage(eventName, value, recordMode, sourceId, gameObject));
            emitted = true;
        }

        public void EmitAmount(int amount)
        {
            value = QuestEventValue.FromInt(amount);
            Emit();
        }

        public void EmitBool(bool state)
        {
            value = QuestEventValue.FromBool(state);
            Emit();
        }

        public void ResetEmitter()
        {
            emitted = false;
        }
    }
}
