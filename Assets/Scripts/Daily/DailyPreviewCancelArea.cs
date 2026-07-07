using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Daily
{
    [RequireComponent(typeof(Image))]
    public class DailyPreviewCancelArea : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private DailyMapStepController stepController;

        private void Awake()
        {
            if (stepController == null)
            {
                stepController = GetComponentInParent<DailyMapStepController>(true);
            }

            Image image = GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (stepController != null)
            {
                stepController.CancelPreviewByBlankClick();
            }
        }
    }
}
