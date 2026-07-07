using System.Collections;
using Common;
using Common.Constant;
using UI.UIFramework.Control;
using UI.UIFramework.Data;
using UnityEngine;

namespace UI.UIFramework.Interact
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Modal : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private GameText gameText;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            gameText = GetComponentInChildren<GameText>(true);
            CloseModal();
        }

        private void OnEnable()
        {
            GameApplication.Instance.EventCenter.AddEventListener(
                GlobalEvents.OnSettingsSaved, OnSettingsSaved);
        }

        private void OnDisable()
        {
            if (GameApplication.Instance != null)
            {
                GameApplication.Instance.EventCenter.RemoveEventListener(
                    GlobalEvents.OnSettingsSaved, OnSettingsSaved);
            }
        }

        private void OnSettingsSaved()
        {
            if (gameText)
            {
                gameText.ShowText();
            }

            if (UIController.instance != null && UIController.instance.CurPanel == PanelName.PanelOption)
            {
                StartCoroutine(CloseModalDelay());
            }
        }

        private void CloseModal()
        {
            canvasGroup.alpha = 0f;
            // 关闭提示时同步禁用交互, 避免隐藏状态仍挡住点击.
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private IEnumerator CloseModalDelay()
        {
            // 显示提示时临时恢复可见状态.
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            yield return new WaitForSecondsRealtime(1f);
            CloseModal();
        }
    }
}
