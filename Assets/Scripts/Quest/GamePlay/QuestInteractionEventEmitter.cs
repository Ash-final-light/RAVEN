using Tools.Quest.Event;
using UnityEngine;

namespace Quest.GamePlay
{
    /// <summary>
    /// Emits a quest event when triggered by InteractAbility or another gameplay interaction.
    /// </summary>
    [DisallowMultipleComponent]
    public class QuestInteractionEventEmitter : MonoBehaviour, IQuestInteractable
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
            {
                return;
            }

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestInteractionEventEmitter)}] QuestEventHub is missing. Event '{eventName}' was not published.", this);
                return;
            }

            emitted = true;

            hub.Publish(new QuestEventMessage(
                eventName,
                QuestEventValue.FromBool(true),
                QuestEventRecordMode.Latch,
                sourceId,
                gameObject));

            if (disableColliderAfterInteraction)
            {
                SetCollidersEnabled(false);
            }
        }

        public void ResetEmitter()
        {
            emitted = false;
            SetCollidersEnabled(true);
        }

        private void SetCollidersEnabled(bool enabled)
        {
            foreach (var targetCollider in GetComponentsInChildren<Collider>())
            {
                targetCollider.enabled = enabled;
            }
        }
    }
}
