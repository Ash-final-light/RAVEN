using Common;
using Common.Constant;
using Tools.RecycleViewFramework.View;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PlayerInputManager = Tools.InputHandle.PlayerInputManager;

namespace Quest.GamePlay
{
    [RequireComponent(typeof(Button))]
    public sealed class QuestViewOpenButton : MonoBehaviour
    {
        [SerializeField] private string viewName = "QuestView";

        private Button button;
        private UIManager subscribedManager;
        private bool pendingOpen;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            button.onClick.AddListener(()=>{Debug.Log("[button click]");});
            button.onClick.RemoveListener(OpenQuestView);
            button.onClick.AddListener(OpenQuestView);
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.OpenTask, OpenQuestViewAction);
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OpenQuestView);
            }
            if (PlayerInputManager.HasInstance)
            {
               PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.OpenTask, OpenQuestViewAction);
            }
            UnsubscribeLoaded();
            pendingOpen = false;
        }

        private void OpenQuestViewAction(InputAction.CallbackContext obj)
        {
            OpenQuestView();
        }
        public void OpenQuestView()
        {
            UIManager uiManager = GameViewManager.UIManager;
            Debug.Log($"[{nameof(QuestViewOpenButton)}] OpenQuestView");
            if (uiManager == null)
            {
                Debug.LogWarning($"[{nameof(QuestViewOpenButton)}] UIManager is not ready.", this);
                return;
            }

            if (uiManager.HasViewResource(viewName))
            {
                pendingOpen = false;
                uiManager.Open(viewName);
                return;
            }

            if (uiManager.ResourcesLoaded)
            {
                Debug.LogError($"[{nameof(QuestViewOpenButton)}] View '{viewName}' is not registered in Addressables.", this);
                return;
            }

            pendingOpen = true;
            SubscribeLoaded(uiManager);
        }

        private void SubscribeLoaded(UIManager uiManager)
        {
            if (subscribedManager == uiManager)
            {
                return;
            }

            UnsubscribeLoaded();
            subscribedManager = uiManager;
            subscribedManager.OnLoaded += HandleUiResourcesLoaded;
        }

        private void UnsubscribeLoaded()
        {
            if (subscribedManager == null)
            {
                return;
            }

            subscribedManager.OnLoaded -= HandleUiResourcesLoaded;
            subscribedManager = null;
        }

        private void HandleUiResourcesLoaded()
        {
            UnsubscribeLoaded();
            if (pendingOpen)
            {
                OpenQuestView();
            }
        }
    }
}
