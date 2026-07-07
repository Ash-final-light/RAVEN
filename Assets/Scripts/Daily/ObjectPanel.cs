using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Daily
{
    public class ObjectPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [ShowInInspector] private List<rewardsInfo> rewards;

        private GameObject PanelRoot
        {
            get
            {
                return panelRoot == null ? gameObject : panelRoot;
            }
        }

        public void SetTitle(string title)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
        }

        public void SetContent(List<rewardsInfo> rewards)
        {
            this.rewards = rewards;
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
