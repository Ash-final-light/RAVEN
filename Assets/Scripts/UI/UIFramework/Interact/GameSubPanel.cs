using UI.UIFramework.Control;
using UI.UIFramework.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.UIFramework.Interact
{
    public class GameSubPanel:MonoBehaviour
    {
        public SubPanelName subpanelName;
        public GameObject firstSelectedButton;
        GameButton[] gameButtons;
        int lastButtonID;

        public virtual void InitSubPanel()
        {
            gameButtons = GetComponentsInChildren<GameButton>(true);
            gameObject.SetActive(true); //this is to warm up the buttons
            gameObject.SetActive(false);
        }

        public virtual void ShowSubPanel()
        {
            if (firstSelectedButton)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            }
            gameObject.SetActive(true);
        }

        public void HideSubPanel()
        {
            gameObject.SetActive(false);
        }

        public void EnableSubPanelButtons()
        {
            FocusLastButton();
            foreach (GameButton button in gameButtons)
            {
                button.SetInteractable(true);
                button.ShowButton();
            }
        }

        public void DisableSubPanelButtons()
        {
            SetLastButtonID(UIController.instance.CurButtonID);
            foreach (GameButton button in gameButtons)
            {
                button.SetInteractable(false);
            }
        }

        void SetLastButtonID(int id)
        {
            lastButtonID = id;
        }

        void FocusLastButton()
        {
            if (lastButtonID == 0)
                return;
            UIController.instance.SetLastFocusBtn(lastButtonID);
        }

        void OnValidate()
        {
            if (subpanelName == SubPanelName.None)
            {
                Debug.Log("subpanel name not set");
            }
        }
    }
}