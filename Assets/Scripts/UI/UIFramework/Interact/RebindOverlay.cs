using Common;
using Common.Constant;
using UI.UIFramework.Control;
using UI.UIFramework.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIFramework.Interact
{
    public class RebindOverlay : MonoBehaviour
    {
        [SerializeField] private Text textRebindPrompt;

        private void Awake()
        {
            if (!textRebindPrompt)
            {
                textRebindPrompt = GetComponentInChildren<Text>(true);
            }
        }

        private void OnEnable()
        {
            if (UIDataManager.Instance != null)
            {
                UIDataManager.Instance.LanguageChanged += RefreshPrompt;
            }

            RefreshPrompt();
            GameApplication.Instance.EventCenter.TriggerEvent(GlobalEvents.OnRebindStarted);
        }

        private void OnDisable()
        {
            if (UIDataManager.HasInstance)
            {
                UIDataManager.Instance.LanguageChanged -= RefreshPrompt;
            }

            if (GameApplication.Instance != null)
            {
                GameApplication.Instance.EventCenter.TriggerEvent(GlobalEvents.OnRebindCompleted);
            }
        }

        private void RefreshPrompt()
        {
            if (textRebindPrompt)
            {
                textRebindPrompt.text = UIDataManager.Instance.GetLanUIContent(TitleName.Wait.ToString());
            }
        }
    }
}
