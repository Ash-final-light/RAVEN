using Daily;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Quest
{
    public sealed class DailyBagIconButton : MonoBehaviour, IPointerClickHandler
    {
        [Inject] private DailyPackageService dailyPackageService;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.RemoveListener(OpenPackageView);
                button.onClick.AddListener(OpenPackageView);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OpenPackageView);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (button == null)
            {
                OpenPackageView();
            }
        }

        private void OpenPackageView()
        {
            dailyPackageService?.OpenPackageView();
        }
    }
}
