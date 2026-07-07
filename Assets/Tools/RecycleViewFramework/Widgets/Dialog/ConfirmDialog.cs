using TMPro;
using Tools.RecycleViewFramework.View;
using UnityEngine.UI;

namespace Tools.RecycleViewFramework.Widgets.Dialog
{
    public class ConfirmDialog : Dialog
    {
        private TMP_Text titleText;
        private TMP_Text contentText;
        private Button confirmBtn;
        private Button cancelBtn;

        private System.Action OnConfirm;

        public override void OnCreate(UIManager viewManager)
        {
            base.OnCreate(viewManager);

            titleText = transform.Find("TitleText").GetComponent<TMP_Text>();
            contentText = transform.Find("ContentText").GetComponent<TMP_Text>();
            confirmBtn = transform.Find("ButtonContainer/ConfirmBtn").GetComponent<Button>();
            cancelBtn = transform.Find("ButtonContainer/CancelBtn").GetComponent<Button>();

            confirmBtn.onClick.AddListener(OnConfirmClick);
            cancelBtn.onClick.AddListener(OnCancelClick);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void OnConfirmClick()
        {
            Close();
            OnConfirm?.Invoke();
        }

        private void OnCancelClick()
        {
            Close();
        }

        public void SetTitle(string title)
        {
            titleText.text = title;
        }

        public void SetContent(string content)
        {
            contentText.text = content;
        }

        public void SetConfirmCallback(System.Action action)
        {
            OnConfirm = action;
        }
    }
}
