using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tools.RecycleViewFramework.Widgets.RecyclerView.Scroller
{
    public class ScrollbarEx : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform handle;
        private Scrollbar scrollbar;

        public System.Action OnDragEnd;

        private bool dragging;
        private bool hovering;

        private void Awake()
        {
            scrollbar = GetComponent<Scrollbar>();
            handle = scrollbar.handleRect;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
            if (!hovering)
            {
                if (scrollbar.direction == Scrollbar.Direction.TopToBottom ||
                    scrollbar.direction == Scrollbar.Direction.BottomToTop)
                {
                    handle.DOScaleX(1f, 0.2f);
                }
                else
                {
                    handle.DOScaleY(1f, 0.2f);
                }
            }

            OnDragEnd?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
            if (scrollbar.direction == Scrollbar.Direction.TopToBottom ||
                scrollbar.direction == Scrollbar.Direction.BottomToTop)
            {
                handle.DOScaleX(2f, 0.2f);
            }
            else
            {
                handle.DOScaleY(2f, 0.2f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            if (!dragging)
            {
                if (scrollbar.direction == Scrollbar.Direction.TopToBottom ||
                    scrollbar.direction == Scrollbar.Direction.BottomToTop)
                {
                    handle.DOScaleX(1f, 0.2f);
                }
                else
                {
                    handle.DOScaleY(1f, 0.2f);
                }
            }
        }
    }
}
