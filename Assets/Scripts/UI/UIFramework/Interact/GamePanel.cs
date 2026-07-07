using UI.UIFramework.Control;
using UI.UIFramework.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.UIFramework.Interact
{
    public class GamePanel:MonoBehaviour
    {
        public PanelName panelName;
        public GameObject firstSelectedButton;

        GameButton[] gameButtons;
        GameText[] gameTexts;
        InputGuideItem[] inputGuideItems;
        int lastButtonID;

        public virtual void InitPanel()
        {
            gameButtons = GetComponentsInChildren<GameButton>(true);
            gameTexts = GetComponentsInChildren<GameText>(true);
            inputGuideItems = GetComponentsInChildren<InputGuideItem>(true);
            gameObject.SetActive(true); //this is to warm up the buttons
            gameObject.SetActive(false);
        }

        public virtual void ShowPanel()
        {
            if (firstSelectedButton)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);

            gameObject.SetActive(true);
            ShowPanelButtons();
        }

        public virtual void HidePanel()
        {
            SetLastButtonID(UIController.instance.CurButtonID);
            gameObject.SetActive(false);
        }

        public void EnablePanelButtons()
        {
            FocusLastButton();
            ShowPanelButtons();
        }

        void ShowPanelButtons()
        {
            foreach (GameButton button in gameButtons)
            {
                button.SetInteractable(true);
                button.ShowButton();
            }
            foreach (InputGuideItem item in inputGuideItems)
            {
                item.ShowInputGuide();
            }
            foreach (GameText text in gameTexts)
            {
                text.ShowText();
            }
        }

        public void DisablePanelButtons()
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
            if (panelName == PanelName.None)
            {
                Debug.Log("panel name not set");
            }
        }
    }
}