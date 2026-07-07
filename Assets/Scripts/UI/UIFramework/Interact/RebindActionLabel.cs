using UI.UIFramework.Control;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIFramework.Interact
{
    public class RebindActionLabel : MonoBehaviour
    {
        [SerializeField] private Text textActionName;
        [SerializeField] private string actionNameKey;

        private void Awake()
        {
            if (!textActionName)
            {
                textActionName = GetComponent<Text>();
                if (!textActionName)
                {
                    textActionName = GetComponentInChildren<Text>(true);
                }
            }

            if (string.IsNullOrEmpty(actionNameKey) && textActionName)
            {
                actionNameKey = textActionName.text;
            }
        }

        private void OnEnable()
        {
            UIDataManager.Instance.LanguageChanged += RefreshLabel;
            RefreshLabel();
        }

        private void OnDisable()
        {
            if (!UIDataManager.HasInstance)
            {
                return;
            }

            UIDataManager.Instance.LanguageChanged -= RefreshLabel;
        }

        private void RefreshLabel()
        {
            if (textActionName && !string.IsNullOrEmpty(actionNameKey))
            {
                textActionName.text = UIDataManager.Instance.GetLanUIContent(actionNameKey);
            }
        }
    }
}
