using Common;
using Tools.Quest.Event;
using Tools.RecycleViewFramework.View;
using UnityEngine;
using VContainer;

namespace Quest.Factory
{
    public sealed class QuestVictoryViewListener : MonoBehaviour
    {
        [SerializeField] private string viewAddress = FactoryQuestEvents.VictoryViewAddress;
        [Inject] private UIManager uiManager;
        private bool opened;
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
            QuestEventHub.Instance.Subscribe(FactoryQuestEvents.QuestCompleted, HandleQuestCompleted);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;
            if (QuestEventHub.TryGetInstance(out var hub))
                hub.Unsubscribe(FactoryQuestEvents.QuestCompleted, HandleQuestCompleted);
            subscribed = false;
        }

        private void HandleQuestCompleted(QuestEventMessage message)
        {
            if (opened)
                return;
            opened = true;
            var manager = uiManager ?? GameViewManager.UIManager;
            if (manager == null)
            {
                Debug.LogError("[FactoryQuest] UIManager is unavailable; victory view cannot be opened.");
                return;
            }
            manager.Open(viewAddress);
        }
    }
}
