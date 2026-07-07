using System;
using Tools.Quest.Event;
using UnityEngine;
using UnityEngine.Events;

namespace Quest.Factory
{
    public interface IFactoryQuestDialogueGateway
    {
        void Play(string dialogueId, Action onCompleted);
    }

    public sealed class QuestDialogueEventListener : MonoBehaviour
    {
        [SerializeField] private string interactionEvent = FactoryQuestEvents.SecondDeviceInteracted;
        [SerializeField] private string dialogueCompletedEvent = FactoryQuestEvents.SecondDialogueCompleted;
        [SerializeField] private string dialogueId = "factory.second.device";
        [SerializeField] private UnityEvent onDialogueRequested;
        private IFactoryQuestDialogueGateway dialogueGateway;
        private bool requested;
        private bool completed;
        private bool subscribed;

        public event Action<string> DialogueRequested;

        public void SetDialogueGateway(IFactoryQuestDialogueGateway gateway) => dialogueGateway = gateway;

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
            QuestEventHub.Instance.Subscribe(interactionEvent, HandleInteraction);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;
            if (QuestEventHub.TryGetInstance(out var hub))
                hub.Unsubscribe(interactionEvent, HandleInteraction);
            subscribed = false;
        }

        private void HandleInteraction(QuestEventMessage message)
        {
            if (requested)
                return;
            requested = true;

            if (dialogueGateway != null)
                dialogueGateway.Play(dialogueId, NotifyDialogueCompleted);
            else
            {
                DialogueRequested?.Invoke(dialogueId);
                onDialogueRequested?.Invoke();
            }
        }

        public void NotifyDialogueCompleted()
        {
            if (!requested || completed)
                return;
            completed = true;
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                dialogueCompletedEvent,
                QuestEventValue.FromBool(true),
                QuestEventRecordMode.Latch,
                dialogueId,
                gameObject));
        }
    }
}
