using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Daily
{
    [RequireComponent(typeof(Button))]
    public class DailyStepCardButton : MonoBehaviour
    {
        [SerializeField] private int stepCount = 3;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text stepText;
        [SerializeField] private DailyMapStepController controller;

        public int StepCount { get { return stepCount; } }
        public RectTransform RectTransform { get { return transform as RectTransform; } }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (stepText == null)
            {
                stepText = GetComponentInChildren<TMP_Text>(true);
            }

            RefreshText();
            button.onClick.AddListener(NotifyClicked);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(NotifyClicked);
            }
        }

        private void OnValidate()
        {
            RefreshText();
        }

        public void SetController(DailyMapStepController value)
        {
            controller = value;
        }

        public void SetStepCount(int value)
        {
            stepCount = value;
            RefreshText();
        }

        public void SetInteractable(bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }
        }

        private void NotifyClicked()
        {
            if (controller != null)
            {
                controller.OnStepCardClicked(this);
            }
        }

        private void RefreshText()
        {
            if (stepText != null)
            {
                stepText.text = stepCount.ToString();
            }
        }
    }
}
