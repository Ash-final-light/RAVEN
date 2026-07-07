using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Daily
{
    public class DailyStepCardHand : MonoBehaviour
    {
        [SerializeField] private DailyMapStepController controller;
        [SerializeField] private RectTransform cardRoot;
        [SerializeField] private DailyStepCardButton cardPrefab;
        [SerializeField] private List<DailyStepCardButton> cards = new List<DailyStepCardButton>();

        [Header("Layout")]
        [SerializeField] private Vector2 firstCardPosition = new Vector2(64f, 78f);
        [SerializeField] private float cardSpacing = 132f;

        [Header("Animation")]
        [SerializeField] private float removeDuration = 0.22f;
        [SerializeField] private float pushDuration = 0.28f;
        [SerializeField] private float relayoutDuration = 0.18f;
        [SerializeField] private Vector2 removeOffset = new Vector2(0f, -80f);
        [SerializeField] private Vector2 pushOffset = new Vector2(180f, 0f);
        [SerializeField] private Ease removeEase = Ease.InBack;
        [SerializeField] private Ease pushEase = Ease.OutBack;
        [SerializeField] private Ease relayoutEase = Ease.OutQuad;

        [Header("Generate")]
        [SerializeField] private int[] generatedStepCandidates = new int[] { 1, 2, 3, 4, 5, 6 };

        private readonly List<DailyStepCardButton> pooledCards = new List<DailyStepCardButton>();

        private void Awake()
        {
            if (cardRoot == null)
            {
                cardRoot = transform as RectTransform;
            }

            RegisterExistingCards();
            BindController(controller);
        }

        public void BindController(DailyMapStepController value)
        {
            controller = value;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].SetController(controller);
                }
            }
        }

        public void SetCardsInteractable(bool value)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].SetInteractable(value);
                }
            }
        }

        public IEnumerator RemoveCard(DailyStepCardButton card)
        {
            if (card == null)
            {
                yield break;
            }

            cards.Remove(card);
            CanvasGroup group = GetOrAddCanvasGroup(card.gameObject);
            RectTransform rect = card.RectTransform;
            Sequence sequence = DOTween.Sequence();
            if (rect != null)
            {
                sequence.Join(rect.DOAnchorPos(rect.anchoredPosition + removeOffset, removeDuration).SetEase(removeEase));
                sequence.Join(rect.DOScale(0.85f, removeDuration).SetEase(removeEase));
            }

            sequence.Join(group.DOFade(0f, removeDuration));
            yield return sequence.WaitForCompletion();

            card.SetInteractable(false);
            if (cardPrefab == null)
            {
                card.gameObject.SetActive(false);
                pooledCards.Add(card);
            }
            else
            {
                Destroy(card.gameObject);
            }

            yield return RelayoutCards();
        }

        public IEnumerator PushGeneratedCard()
        {
            int step = GenerateStepCount();
            yield return PushCard(step);
        }

        public IEnumerator PushCard(int stepCount)
        {
            DailyStepCardButton card = TakeCardInstance();
            if (card == null)
            {
                yield break;
            }

            card.transform.SetParent(cardRoot, false);
            card.gameObject.SetActive(true);
            card.SetController(controller);
            card.SetStepCount(stepCount);
            card.SetInteractable(false);

            CanvasGroup group = GetOrAddCanvasGroup(card.gameObject);
            group.alpha = 0f;

            RectTransform rect = card.RectTransform;
            Vector2 targetPosition = GetCardPosition(cards.Count);
            if (rect != null)
            {
                rect.localScale = Vector3.one * 0.92f;
                rect.anchoredPosition = targetPosition + pushOffset;
            }

            cards.Add(card);

            Sequence sequence = DOTween.Sequence();
            if (rect != null)
            {
                sequence.Join(rect.DOAnchorPos(targetPosition, pushDuration).SetEase(pushEase));
                sequence.Join(rect.DOScale(1f, pushDuration).SetEase(pushEase));
            }

            sequence.Join(group.DOFade(1f, pushDuration));
            yield return sequence.WaitForCompletion();

            card.SetInteractable(true);
        }

        [ContextMenu("Register Existing Cards")]
        private void RegisterExistingCards()
        {
            if (cardRoot == null)
            {
                cardRoot = transform as RectTransform;
            }

            if (cards.Count == 0 && cardRoot != null)
            {
                DailyStepCardButton[] foundCards = cardRoot.GetComponentsInChildren<DailyStepCardButton>(true);
                for (int i = 0; i < foundCards.Length; i++)
                {
                    if (!cards.Contains(foundCards[i]))
                    {
                        cards.Add(foundCards[i]);
                    }
                }
            }
        }

        private IEnumerator RelayoutCards()
        {
            Sequence sequence = DOTween.Sequence();
            bool hasTween = false;
            for (int i = 0; i < cards.Count; i++)
            {
                DailyStepCardButton card = cards[i];
                if (card == null || card.RectTransform == null)
                {
                    continue;
                }

                hasTween = true;
                sequence.Join(card.RectTransform.DOAnchorPos(GetCardPosition(i), relayoutDuration).SetEase(relayoutEase));
            }

            if (hasTween)
            {
                yield return sequence.WaitForCompletion();
            }
        }

        private DailyStepCardButton TakeCardInstance()
        {
            if (pooledCards.Count > 0)
            {
                DailyStepCardButton pooled = pooledCards[0];
                pooledCards.RemoveAt(0);
                return pooled;
            }

            DailyStepCardButton source = cardPrefab;
            if (source == null && cards.Count > 0)
            {
                source = cards[0];
            }

            if (source == null)
            {
                Debug.LogWarning("DailyStepCardHand has no card prefab or existing card to clone.", this);
                return null;
            }

            return Instantiate(source, cardRoot);
        }

        private Vector2 GetCardPosition(int index)
        {
            return firstCardPosition + new Vector2(cardSpacing * index, 0f);
        }

        private int GenerateStepCount()
        {
            if (generatedStepCandidates == null || generatedStepCandidates.Length == 0)
            {
                return 3;
            }

            return generatedStepCandidates[Random.Range(0, generatedStepCandidates.Length)];
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = target.AddComponent<CanvasGroup>();
            }

            return group;
        }
    }
}
