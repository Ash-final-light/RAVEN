using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Daily
{
    public class DailyMapMover : MonoBehaviour
    {
        [SerializeField] private RectTransform moveTarget;
        [SerializeField] private float moveDuration = 0.28f;
        [SerializeField] private Ease moveEase = Ease.InOutSine;

        private RectTransform CachedMoveTarget
        {
            get
            {
                if (moveTarget == null)
                {
                    moveTarget = transform as RectTransform;
                }

                return moveTarget;
            }
        }

        public IEnumerator MoveTo(DailyMapNode node)
        {
            if (node == null)
            {
                yield break;
            }

            RectTransform targetRect = CachedMoveTarget;
            RectTransform nodeRect = node.RectTransform;

            Tween tween;
            if (targetRect != null && nodeRect != null && targetRect.parent == nodeRect.parent)
            {
                tween = targetRect.DOAnchorPos(nodeRect.anchoredPosition, moveDuration).SetEase(moveEase);
            }
            else
            {
                Transform target = targetRect != null ? targetRect : transform;
                tween = target.DOMove(node.transform.position, moveDuration).SetEase(moveEase);
            }

            yield return tween.WaitForCompletion();
        }
    }
}
