using Common;
using UI.UIFramework.Control;
using UI.UIFramework.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.UIFramework.Interact
{
    public class GameButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IUpdateSelectedHandler
    {
        public TitleName title_name;
        public int buttonID { get; private set; }

        protected Button button;
        protected Text[] texts;
        protected Image[] images;
        protected Image imageIcon;
        protected CanvasGroup canvasGroup;

        private bool focused;
        private bool updateSelected;

        private void Awake()
        {
            InitButton();
        }

        private void OnEnable()
        {
            UIDataManager.Instance.LanguageChanged += ShowButton;
            UIDataManager.Instance.FontSizeChanged += ShowButton;
            ShowButton();
            button.onClick.AddListener(GameButtonOnClick);
        }

        private void OnDisable()
        {
            focused = false;
            updateSelected = false;

            if (UIDataManager.HasInstance)
            {
                UIDataManager.Instance.LanguageChanged -= ShowButton;
                UIDataManager.Instance.FontSizeChanged -= ShowButton;
            }

            button.onClick.RemoveAllListeners();
        }

        public void InitButton()
        {
            button = GetComponent<Button>();
            images = GetComponentsInChildren<Image>(true);
            imageIcon = images.Length > 1 ? images[1] : null;
            texts = GetComponentsInChildren<Text>(true);
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        public void ShowButton()
        {
            if (texts.Length > 0 && title_name != TitleName.None)
            {
                texts[0].text = UIDataManager.Instance.GetLanUIContent(title_name.ToString());
            }

            SetTextStyle(focused);
            SetIconColor(focused);
        }

        public void SetButtonID(int m_id)
        {
            buttonID = m_id;
        }

        public void SetInteractable(bool interact)
        {
            button.interactable = interact;
            canvasGroup.alpha = interact ? 1f : 0.2f;
        }

        public void OnSelect(BaseEventData eventData)
        {
            OnFocused();
            UIController.instance.SetCurBtnID(buttonID);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            updateSelected = false;
            OnUnfocused();
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            if (updateSelected)
            {
                return;
            }

            updateSelected = true;
            OnFocused();
            UIController.instance.SetCurBtnID(buttonID);
        }

        public void OnFocused()
        {
            focused = true;
            SetTextStyle(true);
            SetIconColor(true);
        }

        public void OnUnfocused()
        {
            focused = false;
            SetTextStyle(false);
            SetIconColor(false);
        }

        protected void SetIconColor(bool isFocused)
        {
            if (imageIcon)
            {
                imageIcon.color = isFocused
                    ? UIDataManager.Instance.OptionDataSo.colorType.dark
                    : UIDataManager.Instance.OptionDataSo.colorType.light;
            }
        }

        protected void SetTextStyle(bool isFocused)
        {
            foreach (var txt in texts)
            {
                txt.color = isFocused
                    ? UIDataManager.Instance.OptionDataSo.colorType.dark
                    : UIDataManager.Instance.OptionDataSo.colorType.light;

                int curFontSize = UIDataManager.Instance.GetOptionValue(nameof(OptionName.FontSize));
                txt.fontSize = UIDataManager.Instance.OptionDataSo.GetFontSize(curFontSize).subSize;
            }
        }

        private void GameButtonOnClick()
        {
            switch (title_name)
            {
                case TitleName.StartGame:
                    GameManager.SwitchScene(SceneName.Daily);
                    GameManager.SetGameStatus(GameStatus.Playing);
                    break;
                case TitleName.Quit:
                    Application.Quit();
                    break;
                case TitleName.Continue:
                    UIController.instance.ContinueGame();
                    break;
                case TitleName.GoTitle:
                    GameManager.SetGameStatus(GameStatus.Loaded);
                    GameManager.SwitchScene(SceneName.Menu);
                    break;
                case TitleName.CompletePreset:
                    GameManager.SetGameStatus(GameStatus.Loaded);
                    GameManager.SwitchScene(SceneName.Menu);
                    break;
                case TitleName.Option:
                    UIController.instance.OpenPanel(PanelName.PanelOption);
                    break;
                case TitleName.Credits:
                    UIController.instance.OpenPanel(PanelName.PanelCredits);
                    break;
                case TitleName.ClosePanel:
                    UIController.instance.CloseCurPanel();
                    break;
                case TitleName.General:
                    UIController.instance.OpenSubPanel(SubPanelName.General);
                    break;
                case TitleName.Graphics:
                    UIController.instance.OpenSubPanel(SubPanelName.Graphics);
                    break;
                case TitleName.Sound:
                    UIController.instance.OpenSubPanel(SubPanelName.Sound);
                    break;
                case TitleName.Keyboard:
                    UIController.instance.OpenSubPanel(SubPanelName.Keyboard);
                    break;
                case TitleName.Gamepad:
                    UIController.instance.OpenSubPanel(SubPanelName.Gamepad);
                    break;
                case TitleName.Archive:
                    UIController.instance.OpenSubPanel(SubPanelName.Archive);
                    break;
                case TitleName.CloseSubPanel:
                    UIController.instance.CloseSubPanel();
                    break;
            }
        }

       
    }
}
