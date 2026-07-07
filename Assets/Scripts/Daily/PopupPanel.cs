using TMPro;
using UnityEngine;

namespace Daily
{
    public class PopupPanel:MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text contentText;

        private GameObject PanelRoot
        {
            get
            {
                return panelRoot == null ? gameObject : panelRoot;
            }
        }

        public void Show()
        {
            PanelRoot.SetActive(true);
        }

        public void Hide()
        {
            PanelRoot.SetActive(false);
        }

        public void SetContent(string content)
        {
            if (contentText != null)
            {
                contentText.text = content;
            }
        }

        public void SetInfo(string content)
        {
            SetContent(content);
        }
    }
}
