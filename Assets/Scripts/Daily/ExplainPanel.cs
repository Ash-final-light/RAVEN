using System;
using TMPro;
using UnityEngine;

namespace Daily
{
    public class ExplainPanel:MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;

        private GameObject PanelRoot
        {
            get
            {
                return panelRoot == null ? gameObject : panelRoot;
            }
        }

        private void Start()
        {
            if (titleText == null || contentText == null)
            {
                Debug.LogError($"[ExplainPanel] {name} no titleText or contentText");
            }
        }

        public void SetInfo(string title, string content)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (contentText != null)
            {
                contentText.text = content;
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
    }
}
