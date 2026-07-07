using DG.Tweening;
using System;
using Tools.Skill;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.UIAction.SkillBallUI
{
    public class ActionUISkillBallEntity : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private Text keyHintText;
        [SerializeField] private RectTransform auraRing;
        [SerializeField] private CanvasGroup auraGroup;

        [Header("Motion")]
        [SerializeField] private float spawnScale = 0.76f;
        [SerializeField] private float consumePopScale = 1.28f;
        [SerializeField] private float consumeEndScale = 0.12f;
        [SerializeField] private float consumeDuration = 0.22f;
        [SerializeField] private float disappearDuration = 0.12f;

        [Header("Aura")]
        [SerializeField] private float auraMinScale = 0.92f;
        [SerializeField] private float auraMaxScale = 1.18f;
        [SerializeField] private float auraLoopDuration = 0.76f;
        [SerializeField] private float auraMaxAlpha = 0.78f;
        [SerializeField] private float auraMinAlpha = 0.36f;

        public RectTransform RectTransform => rectTransform;
        public float ConsumeDuration => consumeDuration;
        public int RuntimeId { get; private set; }
        public int VisibleIndex { get; private set; }
        public SkillBallColor Color { get; private set; }
        public event Action<int> Clicked;

        private Sequence mainSequence;
        private Sequence auraSequence;

        private void Reset()
        {
            rectTransform = transform as RectTransform;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void SetData(SkillBallViewData data, int keyNumber)
        {
            RuntimeId = data.RuntimeId;
            VisibleIndex = data.VisibleIndex;
            Color = data.Color;

            if (background != null)
            {
                background.color = GetBallColor(data.Color);
                background.raycastTarget = true;
            }

            if (keyHintText != null)
            {
                keyHintText.text = keyNumber.ToString();
            }

            if (icon != null)
            {
                icon.enabled = true;
                icon.raycastTarget = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(RuntimeId);
        }

        public void PrepareSpawn()
        {
            KillMainTween();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * spawnScale;
            }
        }

        public Tween PlayPushIn(Vector2 targetPosition, float duration)
        {
            KillMainTween();
            mainSequence = DOTween.Sequence().SetUpdate(true);

            if (rectTransform != null)
            {
                mainSequence.Join(rectTransform.DOAnchorPos(targetPosition, duration).SetEase(Ease.OutCubic));
                mainSequence.Join(rectTransform.DOScale(1f, duration).SetEase(Ease.OutBack));
            }

            if (canvasGroup != null)
            {
                mainSequence.Join(canvasGroup.DOFade(1f, duration * 0.75f));
            }

            return mainSequence;
        }

        public Tween MoveTo(Vector2 targetPosition, float duration)
        {
            KillMainTween();
            mainSequence = DOTween.Sequence().SetUpdate(true);

            if (rectTransform != null)
            {
                mainSequence.Join(rectTransform.DOAnchorPos(targetPosition, duration).SetEase(Ease.OutCubic));
                mainSequence.Join(rectTransform.DOScale(1f, duration).SetEase(Ease.OutQuad));
            }

            if (canvasGroup != null)
            {
                mainSequence.Join(canvasGroup.DOFade(1f, duration * 0.5f));
            }

            return mainSequence;
        }

        public void SetImmediatePosition(Vector2 targetPosition)
        {
            KillMainTween();

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = targetPosition;
                rectTransform.localScale = Vector3.one;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        public Tween PlayConsumeTween()
        {
            KillMainTween();
            mainSequence = DOTween.Sequence().SetUpdate(true);

            if (rectTransform != null)
            {
                mainSequence.Append(rectTransform.DOScale(consumePopScale, consumeDuration * 0.36f).SetEase(Ease.OutBack));
                mainSequence.Append(rectTransform.DOScale(consumeEndScale, consumeDuration * 0.64f).SetEase(Ease.InBack));
            }

            if (canvasGroup != null)
            {
                mainSequence.Insert(consumeDuration * 0.36f, canvasGroup.DOFade(0f, consumeDuration * 0.64f));
            }

            mainSequence.OnStart(() => SetAuraActive(false));
            return mainSequence;
        }

        public Tween PlayDisappearTween()
        {
            KillMainTween();
            mainSequence = DOTween.Sequence().SetUpdate(true);

            if (rectTransform != null)
            {
                mainSequence.Join(rectTransform.DOScale(0.7f, disappearDuration).SetEase(Ease.InQuad));
                mainSequence.Join(rectTransform.DOAnchorPos(
                    rectTransform.anchoredPosition + new Vector2(-12f, 0f),
                    disappearDuration).SetEase(Ease.InQuad));
            }

            if (canvasGroup != null)
            {
                mainSequence.Join(canvasGroup.DOFade(0f, disappearDuration));
            }

            mainSequence.OnStart(() => SetAuraActive(false));
            return mainSequence;
        }

        public void SetAuraActive(bool active)
        {
            KillAuraTween();

            if (auraRing == null || auraGroup == null)
            {
                return;
            }

            if (!active)
            {
                auraGroup.alpha = 0f;
                auraRing.localScale = Vector3.one;
                auraRing.gameObject.SetActive(false);
                return;
            }

            auraRing.gameObject.SetActive(true);
            auraRing.localScale = Vector3.one * auraMinScale;
            auraGroup.alpha = auraMinAlpha;

            auraSequence = DOTween.Sequence().SetUpdate(true).SetLoops(-1, LoopType.Restart);
            auraSequence.Append(auraRing.DOScale(auraMaxScale, auraLoopDuration * 0.5f).SetEase(Ease.OutSine));
            auraSequence.Join(auraGroup.DOFade(auraMaxAlpha, auraLoopDuration * 0.5f).SetEase(Ease.OutSine));
            auraSequence.Append(auraRing.DOScale(auraMinScale, auraLoopDuration * 0.5f).SetEase(Ease.InSine));
            auraSequence.Join(auraGroup.DOFade(auraMinAlpha, auraLoopDuration * 0.5f).SetEase(Ease.InSine));
        }

        public void KillTweens()
        {
            KillMainTween();
            KillAuraTween();
        }

        private void KillMainTween()
        {
            if (mainSequence != null && mainSequence.IsActive())
            {
                mainSequence.Kill();
            }

            mainSequence = null;
        }

        private void KillAuraTween()
        {
            if (auraSequence != null && auraSequence.IsActive())
            {
                auraSequence.Kill();
            }

            auraSequence = null;
        }

        private static UnityEngine.Color GetBallColor(SkillBallColor color)
        {
            return color switch
            {
                SkillBallColor.Red => new UnityEngine.Color(1f, 0.22f, 0.22f, 1f),
                SkillBallColor.Yellow => new UnityEngine.Color(1f, 0.82f, 0.22f, 1f),
                SkillBallColor.Blue => new UnityEngine.Color(0.22f, 0.56f, 1f, 1f),
                SkillBallColor.White => new UnityEngine.Color(0.88f, 0.96f, 1f, 1f),
                _ => UnityEngine.Color.white
            };
        }
    }
}
