using System;
using Tools.Quest.Event;
using UnityEngine;
using UnityEngine.Events;

namespace Quest.GamePlay
{
    public sealed class QuestDialogueEventListener : MonoBehaviour
    {
        [SerializeField] private string interactionEvent = QuestGamePlayEvents.SecondDeviceInteracted;
        [SerializeField] private string dialogueCompletedEvent = QuestGamePlayEvents.SecondDialogueCompleted;
        [SerializeField] private string dialogueId = "factory.second.device";
        [SerializeField] private UnityEvent onDialogueRequested;

        private IQuestDialogueGateway dialogueGateway;
        private bool requested;
        private bool completed;
        private bool subscribed;

        public event Action<string> DialogueRequested;

        public void SetDialogueGateway(IQuestDialogueGateway gateway) => dialogueGateway = gateway;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Start() => Subscribe();
        private void OnDisable() => Unsubscribe();

        public void ResetListener()
        {
            requested = false;
            completed = false;
        }

        private void Subscribe()
        {
            if (subscribed || string.IsNullOrWhiteSpace(interactionEvent))
            {
                return;
            }

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestDialogueEventListener)}] QuestEventHub is missing. Event '{interactionEvent}' was not subscribed.", this);
                return;
            }

            hub.Subscribe(interactionEvent, HandleInteraction);
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
                hub.Unsubscribe(interactionEvent, HandleInteraction);
            }

            subscribed = false;
        }

        private void HandleInteraction(QuestEventMessage message)
        {
            if (requested)
            {
                return;
            }

            requested = true;

            if (dialogueGateway != null)
            {
                dialogueGateway.Play(dialogueId, NotifyDialogueCompleted);
                return;
            }

            DialogueRequested?.Invoke(dialogueId);
            onDialogueRequested?.Invoke();
        }

        public void NotifyDialogueCompleted()
        {
            if (!requested || completed)
            {
                return;
            }

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestDialogueEventListener)}] QuestEventHub is missing. Event '{dialogueCompletedEvent}' was not published.", this);
                return;
            }

            completed = true;
            hub.Publish(new QuestEventMessage(
                dialogueCompletedEvent,
                QuestEventValue.FromBool(true),
                QuestEventRecordMode.Latch,
                dialogueId,
                gameObject));
        }
    }
}
