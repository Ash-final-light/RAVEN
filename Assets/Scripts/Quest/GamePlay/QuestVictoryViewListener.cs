using Common;
using Tools.Quest.Event;
using Tools.RecycleViewFramework.View;
using UnityEngine;
using VContainer;

namespace Quest.GamePlay
{
    public sealed class QuestVictoryViewListener : MonoBehaviour
    {
        [SerializeField] private string questCompletedEvent = QuestGamePlayEvents.QuestCompleted;
        [SerializeField] private string viewAddress = QuestGamePlayEvents.VictoryViewAddress;
        [SerializeField] private bool openOnce = true;

        [Inject] private UIManager uiManager;

        private bool opened;
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

        public void ResetListener()
        {
            opened = false;
        }

        private void Subscribe()
        {
            if (subscribed || string.IsNullOrWhiteSpace(questCompletedEvent))
            {
                return;
            }

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestVictoryViewListener)}] QuestEventHub is missing. Event '{questCompletedEvent}' was not subscribed.", this);
                return;
            }

            hub.Subscribe(questCompletedEvent, HandleQuestCompleted);
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
                hub.Unsubscribe(questCompletedEvent, HandleQuestCompleted);
            }

            subscribed = false;
        }

        private void HandleQuestCompleted(QuestEventMessage message)
        {
            if (openOnce && opened)
            {
                return;
            }

            var manager = uiManager ?? GameViewManager.UIManager;
            if (manager == null)
            {
                Debug.LogError($"[{nameof(QuestVictoryViewListener)}] Cannot open '{viewAddress}' because UIManager is not ready.", this);
                return;
            }

            opened = true;
            manager.Open(viewAddress);
        }
    }
}
