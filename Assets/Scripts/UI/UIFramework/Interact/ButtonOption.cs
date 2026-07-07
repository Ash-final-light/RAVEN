using Common;
using Common.Constant;
using UI.UIFramework.Control;
using UI.UIFramework.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PlayerInputManager = Tools.InputHandle.PlayerInputManager;

namespace UI.UIFramework.Interact
{
    public class ButtonOption : MonoBehaviour, ISelectHandler, IDeselectHandler, IUpdateSelectedHandler
    {
        public OptionName optionName;
        public Text textOptionTitle;
        public Text textOptionContent;

        private Text[] texts;
        private Slider slider;
        private Button button;
        private bool inited;
        private bool focused;
        private bool updateSelected;
        private OptionContent optionContent;
        private int curValue;
        private int optionMinValue;
        private int optionMaxValue;

        private void Awake()
        {
            InitBtnOption();
        }

        private void OnEnable()
        {
            if (slider)
            {
                slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
            }
            else if (button)
            {
                button.onClick.AddListener(OnButtonClick);
            }

            UIDataManager.Instance.LanguageChanged += HandleLanguageChanged;
            UIDataManager.Instance.FontSizeChanged += HandleFontSizeChanged;
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.Navigate, HandleNavigate);

            ShowBtnOption();
            inited = true;
        }

        private void OnDisable()
        {
            updateSelected = false;
            focused = false;

            if (button)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }

            if (slider)
            {
                slider.onValueChanged.RemoveAllListeners();
            }

            if (UIDataManager.HasInstance)
            {
                UIDataManager.Instance.LanguageChanged -= HandleLanguageChanged;
                UIDataManager.Instance.FontSizeChanged -= HandleFontSizeChanged;
            }
            if (PlayerInputManager.HasInstance)
            {
                PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.Navigate, HandleNavigate);
            }
        }

        public void InitBtnOption()
        {
            texts = GetComponentsInChildren<Text>(true);
            slider = GetComponentInChildren<Slider>();
            button = GetComponentInChildren<Button>();
            optionContent = UIDataManager.Instance.GetOptionContent(optionName.ToString());
            curValue = UIDataManager.Instance.GetOptionValue(optionName.ToString());

            if (slider)
            {
                switch (optionName)
                {
                    case OptionName.Music:
                        optionMinValue = 0;
                        optionMaxValue = GameApplication.Instance.GlobalSettingsDataManager.MaxMusicVolume;
                        slider.maxValue = optionMaxValue;
                        break;
                    case OptionName.SoundEffect:
                        optionMinValue = 0;
                        optionMaxValue = GameApplication.Instance.GlobalSettingsDataManager.MaxSoundVolume;
                        slider.maxValue = optionMaxValue;
                        break;
                }
            }
            else
            {
                if (optionContent == null)
                {
                    if (SupportsFallbackToggle())
                    {
                        optionMinValue = 0;
                        optionMaxValue = 1;
                        return;
                    }

                    return;
                }

                optionMinValue = 0;
                optionMaxValue = optionContent.GetOptionValuesLength() - 1;
            }
        }

        public void ShowBtnOption()
        {
            if (slider == null && optionContent == null && SupportsFallbackToggle() == false)
            {
                return;
            }

            if (slider)
            {
                UpdateSliderValue();
            }
            else
            {
                UpdateSelectValue();
            }

            textOptionTitle.text = UIDataManager.Instance.GetOptionTitle(optionName.ToString());
            SetTextStyle(focused);
        }

        private void UpdateSliderValue()
        {
            slider.SetValueWithoutNotify(curValue);
            textOptionContent.text = "[ " + curValue + " ]";
        }

        private void UpdateSelectValue()
        {
            if (optionContent == null)
            {
                if (SupportsFallbackToggle())
                {
                    textOptionContent.text = curValue == (int)OptionSetting.On ? "[ On ]" : "[ Off ]";
                    return;
                }

                textOptionContent.text = "[ " + curValue + " ]";
                return;
            }

            textOptionContent.text = "[ " + optionContent.GetOptionLanSelectedValue(
                curValue, UIDataManager.Instance.GetCurrentLanguageIndex()) + " ]";
        }

        private void PreviousOption()
        {
            if (UIController.instance == null || !UIController.instance.IsCurOption(optionName))
            {
                return;
            }

            if (slider)
            {
                curValue = curValue - 1 < optionMinValue ? optionMinValue : curValue - 1;
                UpdateSliderValue();
            }
            else
            {
                curValue = curValue - 1 < optionMinValue ? optionMaxValue : curValue - 1;
                UpdateSelectValue();
                ChangeOption();
            }
        }

        private void NextOption()
        {
            if (UIController.instance == null || !UIController.instance.IsCurOption(optionName))
            {
                return;
            }

            if (slider)
            {
                curValue = curValue + 1 > optionMaxValue ? optionMaxValue : curValue + 1;
                UpdateSliderValue();
            }
            else
            {
                curValue = curValue + 1 > optionMaxValue ? optionMinValue : curValue + 1;
                UpdateSelectValue();
                ChangeOption();
            }
        }

        private void ChangeOption()
        {
            UIDataManager.Instance.ApplyOptionValue(optionName.ToString(), curValue);
        }

        public void OnSliderValueChanged()
        {
            if (UIController.instance == null)
            {
                curValue = (int)slider.value;
                if (textOptionContent != null)
                {
                    textOptionContent.text = "[ " + curValue + " ]";
                }
                return;
            }

            if (!UIController.instance.IsCurOption(optionName))
            {
                OnFocused();
                UIController.instance.SetCurOption(optionName);
                EventSystem.current?.SetSelectedGameObject(gameObject);
            }

            curValue = (int)slider.value;
            textOptionContent.text = "[ " + curValue + " ]";
            ChangeOption();
        }

        public void OnSelect(BaseEventData eventData)
        {
            OnFocused();
            if (UIController.instance != null)
            {
                UIController.instance.SetCurOption(optionName);
            }
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
            if (UIController.instance != null)
            {
                UIController.instance.SetCurOption(optionName);
            }
        }

        public void OnFocused()
        {
            focused = true;
            SetTextStyle(true);
        }

        public void OnUnfocused()
        {
            focused = false;
            SetTextStyle(false);
        }

        protected void SetTextStyle(bool isFocused)
        {
            if (texts == null)
            {
                return;
            }

            foreach (var txt in texts)
            {
                txt.color = isFocused
                    ? UIDataManager.Instance.OptionDataSo.colorType.dark
                    : UIDataManager.Instance.OptionDataSo.colorType.light;
            }
        }
        private void OnButtonClick()
        {
            NextOption();
        }

        private void HandleNavigate(InputAction.CallbackContext context)
        {
            if (!gameObject.activeInHierarchy || (slider == null && optionContent == null && SupportsFallbackToggle() == false))
            {
                return;
            }

            Vector2 navigate = context.ReadValue<Vector2>();
            if (navigate.x > 0.1f)
            {
                NextOption();
            }
            else if (navigate.x < -0.1f)
            {
                PreviousOption();
            }
        }

        private void HandleLanguageChanged()
        {
            ShowBtnOption();
        }

        private void HandleFontSizeChanged()
        {
            SetTextStyle(focused);
        }

        private bool SupportsFallbackToggle()
        {
            return optionName == OptionName.FontSize;
        }
    }
}
