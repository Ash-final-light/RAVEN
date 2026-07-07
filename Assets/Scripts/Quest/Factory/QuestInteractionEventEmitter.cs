using Tools.Action.Data.Interact;
using Tools.Quest.Event;
using UnityEngine;

namespace Quest.Factory
{
    [DisallowMultipleComponent]
    public sealed class QuestInteractionEventEmitter : MonoBehaviour, IInteractable
    {
        [SerializeField] private string eventName;
        [SerializeField] private string sourceId;
        [SerializeField] private string interactionPrompt = "按 I 交互";
        [SerializeField] private bool emitOnce = true;
        [SerializeField] private bool disableColliderAfterInteraction = true;
        private bool emitted;

        public bool CanInteract => isActiveAndEnabled && (!emitOnce || !emitted);
        public string InteractionPrompt => interactionPrompt;

        public void Configure(string name, string source, string prompt = "按 I 交互", bool once = true)
        {
            eventName = name;
            sourceId = source;
            interactionPrompt = prompt;
            emitOnce = once;
        }

        public void Interact() => Emit();

        public void Emit()
        {
            if (!CanInteract || string.IsNullOrWhiteSpace(eventName))
                return;

            emitted = true;
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                eventName,
                QuestEventValue.FromBool(true),
                QuestEventRecordMode.Latch,
                sourceId,
                gameObject));

            if (disableColliderAfterInteraction)
            {
                foreach (var targetCollider in GetComponentsInChildren<Collider>())
                    targetCollider.enabled = false;
            }
        }

        public void ResetEmitter()
        {
            emitted = false;
            foreach (var targetCollider in GetComponentsInChildren<Collider>())
                targetCollider.enabled = true;
        }
    }
}
