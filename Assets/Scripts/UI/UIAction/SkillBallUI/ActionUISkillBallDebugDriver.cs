using System.Collections;
using System.Collections.Generic;
using Tools.Skill;
using UnityEngine;

namespace UI.UIAction.SkillBallUI
{
    public class ActionUISkillBallDebugDriver : MonoBehaviour
    {
        [SerializeField] private ActionUISkillBallBar skillBallBar;
        [SerializeField] private float refreshAfterConsumeDelay = 0.28f;
        [SerializeField] private bool enableInPlayMode;

        [Header("Inspector Test")]
        [SerializeField] private SkillBallColor debugAddColor = SkillBallColor.Red;
        [SerializeField, Min(1)] private int debugAddCount = 1;
        [SerializeField, Min(0)] private int debugConsumeVisibleIndex;

        private readonly LocalSkillDispatcher dispatcher = new();
        private readonly List<SkillBallViewData> visibleBalls = new();
        private int nextRuntimeId = 1;
        private bool isInMatrixMode;
        private Coroutine delayedRefreshRoutine;

        private void Awake()
        {
            if (skillBallBar == null)
            {
                skillBallBar = GetComponent<ActionUISkillBallBar>();
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying && !enableInPlayMode)
            {
                return;
            }

            Bind();
        }

        private void OnDisable()
        {
            if (skillBallBar != null)
            {
                skillBallBar.EntityClicked -= OnEntityClicked;
                skillBallBar.Unbind();
            }
        }

        [ContextMenu("Skill Ball UI Test/Bind")]
        public void Bind()
        {
            if (skillBallBar == null)
            {
                skillBallBar = GetComponent<ActionUISkillBallBar>();
            }

            if (skillBallBar != null)
            {
                skillBallBar.EntityClicked -= OnEntityClicked;
                skillBallBar.EntityClicked += OnEntityClicked;
            }

            skillBallBar?.Bind(dispatcher);
            dispatcher.NotifyMatrixModeChanged(isInMatrixMode);
            RefreshVisibleBalls();
        }

        [ContextMenu("Skill Ball UI Test/Add Ball")]
        public void AddBall()
        {
            SkillBallColor color = (SkillBallColor)((nextRuntimeId - 1) % 4);
            AddBall(color);
        }

        public void AddBall(SkillBallColor color)
        {
            visibleBalls.Add(new SkillBallViewData(
                nextRuntimeId,
                color,
                $"debug-{color}",
                visibleBalls.Count));

            nextRuntimeId++;
            RefreshVisibleBalls();
        }

        [ContextMenu("Skill Ball UI Test/Add Configured Balls")]
        public void AddConfiguredBalls()
        {
            int count = Mathf.Max(1, debugAddCount);
            for (int i = 0; i < count; i++)
            {
                AddBall(debugAddColor);
            }
        }

        [ContextMenu("Skill Ball UI Test/Add 3 Balls")]
        public void AddThreeBalls()
        {
            AddBall();
            AddBall();
            AddBall();
        }

        [ContextMenu("Skill Ball UI Test/Consume First")]
        public void ConsumeFirst()
        {
            ConsumeByMatchRule(0);
        }

        [ContextMenu("Skill Ball UI Test/Consume First 3")]
        public void ConsumeFirstThree()
        {
            ConsumeAt(0, Mathf.Min(3, visibleBalls.Count));
        }

        [ContextMenu("Skill Ball UI Test/Consume Configured")]
        public void ConsumeConfigured()
        {
            ConsumeByMatchRule(debugConsumeVisibleIndex);
        }

        [ContextMenu("Skill Ball UI Test/Matrix On")]
        public void MatrixOn()
        {
            isInMatrixMode = true;
            dispatcher.NotifyMatrixModeChanged(true);
        }

        [ContextMenu("Skill Ball UI Test/Matrix Off")]
        public void MatrixOff()
        {
            isInMatrixMode = false;
            dispatcher.NotifyMatrixModeChanged(false);
        }

        [ContextMenu("Skill Ball UI Test/Reset")]
        public void ResetTestBalls()
        {
            visibleBalls.Clear();
            nextRuntimeId = 1;
            isInMatrixMode = false;
            dispatcher.NotifyMatrixModeChanged(false);
            RefreshVisibleBalls();
        }

        [ContextMenu("Skill Ball UI Test/Smoke Sequence")]
        public void RunSmokeSequence()
        {
            StopDelayedRefresh();
            ResetTestBalls();
            StartCoroutine(SmokeSequenceRoutine());
        }

        private IEnumerator SmokeSequenceRoutine()
        {
            AddBall();
            yield return new WaitForSecondsRealtime(0.18f);
            AddBall();
            yield return new WaitForSecondsRealtime(0.18f);
            AddBall();
            yield return new WaitForSecondsRealtime(0.18f);
            MatrixOn();
            yield return new WaitForSecondsRealtime(0.5f);
            AddBall();
            yield return new WaitForSecondsRealtime(0.24f);
            ConsumeFirstThree();
            yield return new WaitForSecondsRealtime(0.7f);
            MatrixOff();
        }

        private void ConsumeAt(int startIndex, int count)
        {
            if (visibleBalls.Count == 0 || count <= 0)
            {
                return;
            }

            if (startIndex < 0 || startIndex >= visibleBalls.Count)
            {
                return;
            }

            int consumeCount = Mathf.Min(count, visibleBalls.Count - startIndex);
            if (consumeCount <= 0)
            {
                return;
            }

            int[] runtimeIds = new int[consumeCount];
            SkillBallColor color = visibleBalls[startIndex].Color;
            for (int i = 0; i < consumeCount; i++)
            {
                runtimeIds[i] = visibleBalls[startIndex + i].RuntimeId;
            }

            visibleBalls.RemoveRange(startIndex, consumeCount);
            dispatcher.NotifySkillBallsConsumed(new SkillBallConsumeViewData(
                runtimeIds,
                startIndex,
                consumeCount,
                color));

            StopDelayedRefresh();
            if (Application.isPlaying)
            {
                delayedRefreshRoutine = StartCoroutine(RefreshAfterConsume());
            }
            else
            {
                RefreshVisibleBalls();
            }
        }

        private void ConsumeByMatchRule(int visibleIndex)
        {
            if (!TryGetMatchedRange(visibleIndex, out int startIndex, out int consumeCount))
            {
                return;
            }

            ConsumeAt(startIndex, consumeCount);
        }

        private bool TryGetMatchedRange(int visibleIndex, out int startIndex, out int consumeCount)
        {
            startIndex = visibleIndex;
            consumeCount = 0;

            if (visibleIndex < 0 || visibleIndex >= visibleBalls.Count)
            {
                return false;
            }

            SkillBallColor color = visibleBalls[visibleIndex].Color;
            int left = visibleIndex;
            while (left > 0 && visibleBalls[left - 1].Color == color)
            {
                left--;
            }

            int right = visibleIndex;
            while (right < visibleBalls.Count - 1 && visibleBalls[right + 1].Color == color)
            {
                right++;
            }

            int sameColorCount = right - left + 1;
            consumeCount = Mathf.Min(3, sameColorCount);

            if (sameColorCount >= 3)
            {
                // Prefer the contiguous triple that contains the selected ball.
                startIndex = Mathf.Clamp(visibleIndex - 2, left, right - 2);
            }
            else
            {
                startIndex = visibleIndex;
                consumeCount = 1;
            }

            return consumeCount > 0;
        }

        private void ConsumeRuntimeId(int runtimeId)
        {
            int index = visibleBalls.FindIndex(ball => ball.RuntimeId == runtimeId);
            if (index < 0)
            {
                return;
            }

            ConsumeByMatchRule(index);
        }

        private void OnEntityClicked(int runtimeId)
        {
            ConsumeRuntimeId(runtimeId);
        }

        private IEnumerator RefreshAfterConsume()
        {
            yield return new WaitForSecondsRealtime(refreshAfterConsumeDelay);
            delayedRefreshRoutine = null;
            RefreshVisibleBalls();
        }

        private void StopDelayedRefresh()
        {
            if (delayedRefreshRoutine == null)
            {
                return;
            }

            StopCoroutine(delayedRefreshRoutine);
            delayedRefreshRoutine = null;
        }

        private void RefreshVisibleBalls()
        {
            List<SkillBallViewData> refreshed = new(visibleBalls.Count);
            for (int i = 0; i < visibleBalls.Count; i++)
            {
                SkillBallViewData ball = visibleBalls[i];
                refreshed.Add(new SkillBallViewData(
                    ball.RuntimeId,
                    ball.Color,
                    ball.IconAddress,
                    i));
            }

            dispatcher.NotifySkillBallsAdded(refreshed);
        }

        private class LocalSkillDispatcher : BaseSkillDispatcher
        {
        }
    }
}
