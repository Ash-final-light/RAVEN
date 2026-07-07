using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Daily.Archive
{
    public class DailyArchiveSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text weekText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private Button button;

        private int dayIndex;
        private Action<int> clickHandler;

        private void Awake()
        {
            BindComponents();
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
            }
        }

        public void SetInfo(int valueDayIndex, string weekName, bool hasSave, bool isCurrentDay, Action<int> onClick)
        {
            BindComponents();

            dayIndex = valueDayIndex;
            clickHandler = onClick;

            if (weekText != null)
            {
                weekText.text = weekName;
            }

            if (stateText != null)
            {
                if (!hasSave)
                {
                    stateText.text = "无记录";
                }
                else
                {
                    stateText.text = isCurrentDay ? "当前记录" : "已保存";
                }
            }

            if (button != null)
            {
                button.interactable = hasSave;
            }
        }

        private void HandleClick()
        {
            if (clickHandler != null)
            {
                clickHandler.Invoke(dayIndex);
            }
        }

        private void BindComponents()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (weekText == null)
            {
                Transform found = transform.Find("WeekText");
                if (found != null)
                {
                    weekText = found.GetComponent<TMP_Text>();
                }
            }

            if (stateText == null)
            {
                Transform found = transform.Find("StateText");
                if (found != null)
                {
                    stateText = found.GetComponent<TMP_Text>();
                }
            }
        }
    }
}
