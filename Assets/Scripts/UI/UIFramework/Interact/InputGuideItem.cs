using Common.Constant;
using UI.UIFramework.Control;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.UIFramework.Interact
{
    public class InputGuideItem : MonoBehaviour
    {
        [SerializeField] InputActionReference actionRefer;
        [SerializeField] Image imageIcon;
        [SerializeField] Text textAction;

        RectTransform rect, parentRect;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            parentRect = GetComponentInParent<RectTransform>();
        }

        void OnEnable()
        {
            UIDataManager.Instance.LanguageChanged += ShowInputGuide;
            ShowInputGuide();
        }

        void OnDisable()
        {
            if (!UIDataManager.HasInstance)
                return;

            UIDataManager.Instance.LanguageChanged -= ShowInputGuide;
        }

        public void ShowInputGuide()
        {
            ShowIcon();
            ShowText();
            RefreshWidth();
        }

        void ShowIcon()
        {
            if (actionRefer.action.name == InputConstants.Move)
                imageIcon.sprite = UIBindingsManager.Instance.GetBindingIconForMove();
            else if (actionRefer.action.name == InputConstants.Navigate)
                imageIcon.sprite = UIBindingsManager.Instance.GetBindingIconForNavigate();
            else
                imageIcon.sprite = UIBindingsManager.Instance.GetBindingIcon(actionRefer.action.name);
        }

        void ShowText()
        {
            textAction.text = UIDataManager.Instance.GetLanUIContent(actionRefer.action.name);
        }

        void RefreshWidth()
        {
            if (rect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            if (parentRect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }
    }
}
