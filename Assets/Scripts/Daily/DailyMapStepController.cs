using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Daily
{
    public enum DailyMapMoveState
    {
        Idle,
        Previewing,
        Moving
    }

    public class DailyMapStepController : MonoBehaviour
    {
        [Header("Map")]
        [SerializeField] private DailyMapNode currentNode;
        [SerializeField] private DailyMapMover mover;
        [SerializeField] private List<DailyMapNode> allNodes = new List<DailyMapNode>();

        [Header("Cards")]
        [SerializeField] private DailyStepCardHand cardHand;

        [Header("Daily Action Limit")]
        [SerializeField] private int dailyActionUsed = 2;
        [SerializeField] private int dailyActionPrimeLimit = 10;
        
        [SerializeField] private int dailyActionLimit = 10;

        [Header("Views")]
        [SerializeField] private DailyActionCounterView actionCounterView;

        private readonly List<DailyMapNode> pendingPath = new List<DailyMapNode>();
        private DailyStepCardButton pendingCard;
        private DailyMapMoveState state = DailyMapMoveState.Idle;
        [ReadOnly]public int crossValidationCount = 0;   

        public int DailyActionUsed { get { return dailyActionUsed; } }
        public int DailyActionLimit { get { return dailyActionLimit; } }
        public DailyMapMoveState State { get { return state; } }
        public DailyMapNode CurrentNode { get { return currentNode; } }
        public IReadOnlyList<DailyMapNode> AllNodes { get { return allNodes; } }

        private void Awake()
        {
            if (mover == null)
            {
                mover = GetComponentInChildren<DailyMapMover>(true);
            }

            if (cardHand == null)
            {
                cardHand = GetComponentInChildren<DailyStepCardHand>(true);
            }

            if (actionCounterView == null)
            {
                actionCounterView = GetComponentInChildren<DailyActionCounterView>(true);
            }

            if (allNodes.Count == 0)
            {
                allNodes.AddRange(GetComponentsInChildren<DailyMapNode>(true));
            }
        }

        private void Start()
        {
            if (cardHand != null)
            {
                cardHand.BindController(this);
            }

            RefreshActionCounter();
            ClearAllPreview();
            SetOnlyCurrentNodeLit();
        }

        public void ClearActionCounter()
        {
            if (state == DailyMapMoveState.Moving)
            {
                return;
            }

            dailyActionLimit = Mathf.Max(0, dailyActionPrimeLimit);
            dailyActionUsed = 0;

            ClearPreview();
            ClearAllPreview();
            SetOnlyCurrentNodeLit();
            RefreshActionCounter();

            if (cardHand != null)
            {
                cardHand.SetCardsInteractable(true);
            }
        }

        public void OnStepCardClicked(DailyStepCardButton card)
        {
            if (card == null || state == DailyMapMoveState.Moving)
            {
                return;
            }

            if (dailyActionUsed >= dailyActionLimit)
            {
                // ShowMessage("今日行动次数已用完");
                return;
            }

            if (state == DailyMapMoveState.Previewing && pendingCard == card)
            {
                StartCoroutine(ConfirmMoveRoutine(card));
                return;
            }

            PreviewMove(card);
        }

        public void SetDailyActionCount(int used, int limit)
        {
            dailyActionUsed = Mathf.Max(0, used);
            dailyActionLimit = Mathf.Max(0, limit);
            RefreshActionCounter();
        }

        public void ClearPreview()
        {
            ClearPendingPathPreview();
            pendingCard = null;
            if (state == DailyMapMoveState.Previewing)
            {
                state = DailyMapMoveState.Idle;
            }
        }

        public void CancelPreviewByBlankClick()
        {
            if (state != DailyMapMoveState.Previewing)
            {
                return;
            }

            ClearPreview();
        }

        public bool MoveByRewardSteps(int stepCount)
        {
            if (state != DailyMapMoveState.Idle || stepCount <= 0)
            {
                return false;
            }

            StartCoroutine(MoveByRewardStepsRoutine(stepCount));
            return true;
        }

        private void PreviewMove(DailyStepCardButton card)
        {
            ClearPreview();
            pendingCard = card;

            if (!BuildForwardPath(currentNode, card.StepCount, pendingPath))
            {
                pendingCard = null;
                // ShowMessage("路径配置不完整");
                return;
            }

            for (int i = 0; i < pendingPath.Count; i++)
            {
                pendingPath[i].SetPreview(true);
            }

            state = DailyMapMoveState.Previewing;
            // ShowMessage("再次点击确认调动");
        }

        private IEnumerator ConfirmMoveRoutine(DailyStepCardButton card)
        {
            state = DailyMapMoveState.Moving;
            if (cardHand != null)
            {
                cardHand.SetCardsInteractable(false);
            }

            DailyMapNode[] movePath = pendingPath.ToArray();
            pendingPath.Clear();
            pendingCard = null;

            dailyActionUsed++;
            RefreshActionCounter();

            if (cardHand != null)
            {
                yield return cardHand.RemoveCard(card);
            }

            if (currentNode != null)
            {
                currentNode.SetCurrent(false);
            }

            for (int i = 0; i < movePath.Length; i++)
            {
                DailyMapNode targetNode = movePath[i];
                targetNode.SetPreview(false);

                if (mover != null)
                {
                    yield return mover.MoveTo(targetNode);
                }

                currentNode = targetNode;
                SetOnlyCurrentNodeLit();
            }

            ClearAllPreview();
            SetOnlyCurrentNodeLit();
            if (currentNode != null)
            {
                currentNode.Arrive();
                currentNode.TriggerStayEvent(this);
            }

            if (cardHand != null)
            {
                yield return cardHand.PushGeneratedCard();
                cardHand.SetCardsInteractable(true);
            }

            state = DailyMapMoveState.Idle;
        }

        private IEnumerator MoveByRewardStepsRoutine(int stepCount)
        {
            state = DailyMapMoveState.Moving;
            if (cardHand != null)
            {
                cardHand.SetCardsInteractable(false);
            }

            ClearPreview();

            List<DailyMapNode> rewardPath = new List<DailyMapNode>();
            if (!BuildForwardPath(currentNode, stepCount, rewardPath))
            {
                // ShowMessage("调动步数路径配置不完整");
                if (cardHand != null)
                {
                    cardHand.SetCardsInteractable(true);
                }

                state = DailyMapMoveState.Idle;
                yield break;
            }

            if (currentNode != null)
            {
                currentNode.SetCurrent(false);
            }

            for (int i = 0; i < rewardPath.Count; i++)
            {
                DailyMapNode targetNode = rewardPath[i];
                targetNode.SetPreview(false);

                if (mover != null)
                {
                    yield return mover.MoveTo(targetNode);
                }

                currentNode = targetNode;
                SetOnlyCurrentNodeLit();
            }

            ClearAllPreview();
            SetOnlyCurrentNodeLit();
            if (currentNode != null)
            {
                currentNode.Arrive();
                currentNode.TriggerStayEvent(this);
            }

            if (cardHand != null)
            {
                cardHand.SetCardsInteractable(true);
            }

            state = DailyMapMoveState.Idle;
        }

        private bool BuildForwardPath(DailyMapNode startNode, int stepCount, List<DailyMapNode> results)
        {
            results.Clear();
            if (startNode == null || stepCount <= 0)
            {
                return false;
            }

            DailyMapNode cursor = startNode;
            for (int i = 0; i < stepCount; i++)
            {
                cursor = cursor.NextNode;
                if (cursor == null)
                {
                    results.Clear();
                    return false;
                }

                results.Add(cursor);
            }

            return true;
        }

        private void SetOnlyCurrentNodeLit()
        {
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i] != null)
                {
                    allNodes[i].SetCurrent(allNodes[i] == currentNode);
                }
            }
        }

        private void ClearAllPreview()
        {
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i] != null)
                {
                    allNodes[i].SetPreview(false);
                }
            }
        }

        private void ClearPendingPathPreview()
        {
            for (int i = 0; i < pendingPath.Count; i++)
            {
                if (pendingPath[i] != null)
                {
                    pendingPath[i].SetPreview(false);
                }
            }

            pendingPath.Clear();
        }

        private void RefreshActionCounter()
        {
            if (actionCounterView != null)
            {
                actionCounterView.Refresh(dailyActionUsed, dailyActionLimit);
            }
        }
        
    }
}
