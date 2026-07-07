using System.Collections.Generic;
using Common;
using Common.Constant;
using Game.TimeScale;
using Tools.Action.Running;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Skill
{
    public enum SkillBallConsumeMode
    {
        AlwaysTripleCheck,
        TripleCheckOnlyOutsideMatrix,
        AlwaysSingle
    }

    /// <summary>
    /// 技能球池中的一个球。球只描述自身颜色和消除规则，不直接绑定动作名。
    /// 实际技能动作由 PlayerStateMachine 使用“当前动作 + 球颜色输入名”查询连招表得到。
    /// </summary>
    public readonly struct SkillBallData
    {
        public readonly int RuntimeId;
        public readonly SkillBallColor Color;
        public readonly SkillBallConsumeMode ConsumeMode;
        public readonly string IconAddress;

        public SkillBallData(
            int runtimeId,
            SkillBallColor color,
            SkillBallConsumeMode consumeMode,
            string iconAddress)
        {
            RuntimeId = runtimeId;
            Color = color;
            ConsumeMode = consumeMode;
            IconAddress = iconAddress;
        }

        public SkillBallData(
            int runtimeId,
            SkillBallColor color,
            SkillBallConsumeMode consumeMode,
            string skillActionName,
            string iconAddress)
            : this(runtimeId, color, consumeMode, iconAddress)
        {
        }
    }

    /// <summary>
    /// 一次槽位输入解析出的消除结果。
    /// SignalInputName 是颜色输入名，用于连招表查询；RuntimeIds 用于 UI 精确播放消除动画。
    /// </summary>
    public readonly struct SkillBallConsumeResult
    {
        public readonly bool Success;
        public readonly int StartIndex;
        public readonly int ConsumeCount;
        public readonly int SignalCount;
        public readonly SkillBallColor Color;
        public readonly string SignalInputName;
        public readonly int[] RuntimeIds;

        public SkillBallConsumeResult(
            bool success,
            int startIndex,
            int consumeCount,
            SkillBallColor color,
            string signalInputName,
            int[] runtimeIds,
            int signalCount = -1)
        {
            Success = success;
            StartIndex = startIndex;
            ConsumeCount = consumeCount;
            SignalCount = signalCount > 0 ? signalCount : consumeCount;
            Color = color;
            SignalInputName = signalInputName;
            RuntimeIds = runtimeIds;
        }

        public static SkillBallConsumeResult Failed()
        {
            return new SkillBallConsumeResult(false, -1, 0, default, null, null);
        }
    }

    /// <summary>
    /// 待动作 Start 阶段消费的命令。
    /// ActionName 是状态机已经解析出的 ActionClip 名，用于确保 Start 阶段属于目标技能动作。
    /// </summary>
    public readonly struct SkillBallConsumeCommand
    {
        public readonly string ActionName;
        public readonly SkillBallConsumeResult ConsumeResult;

        public SkillBallConsumeCommand(string actionName, SkillBallConsumeResult consumeResult)
        {
            ActionName = actionName;
            ConsumeResult = consumeResult;
        }
    }

    public class SkillBallManager
    {
        public const int MaxStoreCount = 16;
        public const int MaxVisibleCount = 8;

        private readonly List<SkillBallData> balls = new();

        private ActionStateController owner;
        private BaseSkillDispatcher dispatcher;
        private bool hasPreparedConsume;
        private SkillBallConsumeResult preparedConsumeResult = SkillBallConsumeResult.Failed();
        private bool hasPendingConsumeCommand;
        private SkillBallConsumeCommand pendingConsumeCommand;
        private bool isMatrixMode;
        private bool witchTimeEventRegistered;

        public void Initialize(ActionStateController owner, BaseSkillDispatcher dispatcher)
        {
            this.owner = owner;
            if (dispatcher != null)
            {
                this.dispatcher = dispatcher;
            }

            TryRegisterWitchTimeEvents();

            if (this.owner?.ActionPlayer != null)
            {
                this.owner.ActionPlayer.OnStartStage -= TryCommitPendingConsumeOnActionStart;
                this.owner.ActionPlayer.OnStartStage += TryCommitPendingConsumeOnActionStart;
            }
        }

        public void SetMatrixMode(bool active)
        {
            isMatrixMode = active;
        }

        public void Dispose()
        {
            if (owner?.ActionPlayer != null)
            {
                owner.ActionPlayer.OnStartStage -= TryCommitPendingConsumeOnActionStart;
            }

            TryUnregisterWitchTimeEvents();
            owner = null;
            dispatcher = null;
        }

        
        /// <summary>
        /// 输入阶段只询问槽位并准备消除结果，不立刻删球。
        /// 这里会根据槽位上的球颜色生成连招输入名，例如 SkillBallBlue。
        /// </summary>
        public bool TryPrepareUseBall(int visibleIndex)
        {
            if (!TryResolveConsume(visibleIndex, out SkillBallConsumeResult result))
            {
                return false;
            }

            preparedConsumeResult = result;
            hasPreparedConsume = true;
            FillPlayerParameters(result);
            return true;
        }

        public bool TryUseBall(int visibleIndex)
        {
            if (!TryPrepareUseBall(visibleIndex))
            {
                return false;
            }

            CommitPreparedConsume();
            return true;
        }

        /// <summary>
        /// 动作播放器进入 Start 阶段时提交消球。
        /// 消球结果会写入 ActionPlayer.RuntimeContext，随后释放命令和 PlayerParameters 临时字段。
        /// </summary>
        public bool CommitPreparedConsume()
        {
            if (!hasPreparedConsume || !preparedConsumeResult.Success)
            {
                return false;
            }

            SkillBallConsumeResult result = preparedConsumeResult;
            owner?.ActionPlayer?.SetRuntimeContext(new ActionRuntimeContext
            {
                ConsumedSignalColor = result.Color,
                ConsumedSignalInputName = result.SignalInputName,
                ConsumedSignalCount = result.SignalCount,
                ConsumedSkillActionName = hasPendingConsumeCommand ? pendingConsumeCommand.ActionName : null,
                ConsumedSignalRuntimeIds = result.RuntimeIds
            });
            RemoveBalls(result.StartIndex, result.ConsumeCount);
            dispatcher?.NotifySkillBallsConsumed(result.ToViewData());
            dispatcher?.NotifySkillBallsAdded(BuildVisibleViewData());
            ClearPreparedConsume();
            ClearPendingConsumeCommand();
            ClearPlayerParameters();
            return true;
        }

        public void CancelPreparedConsume()
        {
            ClearPreparedConsume();
            ClearPendingConsumeCommand();
            ClearPlayerParameters();
        }

        /// <summary>
        /// 状态机决定要播放哪个技能动作后，注册一个“动作 Start 阶段消球”的命令。
        /// 必须在 TryPlayAction 之前调用，因为 ActionPlayer 会同步触发 Start 阶段。
        /// </summary>
        public bool MarkPreparedConsumeForActionStart(string actionName)
        {
            if (!hasPreparedConsume || !preparedConsumeResult.Success || string.IsNullOrEmpty(actionName))
            {
                return false;
            }

            pendingConsumeCommand = new SkillBallConsumeCommand(actionName, preparedConsumeResult);
            hasPendingConsumeCommand = true;
            return true;
        }

        public bool TryAppendBall(SkillBallData ball)
        {
            if (balls.Count >= MaxStoreCount)
            {
                return false;
            }

            balls.Add(ball);
            dispatcher?.NotifySkillBallsAdded(BuildVisibleViewData());
            return true;
        }

        public bool TryRemoveAt(int visibleIndex, out SkillBallData removedBall)
        {
            removedBall = default;

            if (visibleIndex < 0 || visibleIndex >= balls.Count)
            {
                return false;
            }

            removedBall = balls[visibleIndex];
            balls.RemoveAt(visibleIndex);
            dispatcher?.NotifySkillBallsAdded(BuildVisibleViewData());
            return true;
        }

        public int RemoveRange(int startIndex, int count)
        {
            if (startIndex < 0 || startIndex >= balls.Count || count <= 0)
            {
                return 0;
            }

            int removeCount = Mathf.Min(count, balls.Count - startIndex);
            balls.RemoveRange(startIndex, removeCount);
            dispatcher?.NotifySkillBallsAdded(BuildVisibleViewData());
            return removeCount;
        }

        public int ClearBalls()
        {
            int count = balls.Count;
            if (count <= 0)
            {
                return 0;
            }

            balls.Clear();
            dispatcher?.NotifySkillBallsAdded(BuildVisibleViewData());
            return count;
        }

        public IReadOnlyList<SkillBallData> GetBalls()
        {
            return balls;
        }

        public IReadOnlyList<SkillBallViewData> BuildVisibleViewData()
        {
            int count = Mathf.Min(MaxVisibleCount, balls.Count);
            var viewData = new List<SkillBallViewData>(count);

            for (int i = 0; i < count; i++)
            {
                SkillBallData ball = balls[i];
                viewData.Add(new SkillBallViewData(
                    ball.RuntimeId,
                    ball.Color,
                    ball.IconAddress,
                    i));
            }

            return viewData;
        }

        private bool TryResolveConsume(int visibleIndex, out SkillBallConsumeResult result)
        {
            result = SkillBallConsumeResult.Failed();

            if (visibleIndex < 0 || visibleIndex >= MaxVisibleCount)
            {
                return false;
            }

            if (visibleIndex >= balls.Count)
            {
                return false;
            }

            SkillBallData selected = balls[visibleIndex];
            int consumeCount = ResolveConsumeCount(visibleIndex, selected);
            int signalCount = ResolveSignalCount(selected, consumeCount);
            int startIndex = ResolveConsumeStartIndex(visibleIndex, selected.Color, consumeCount);

            int[] runtimeIds = new int[consumeCount];
            for (int i = 0; i < consumeCount; i++)
            {
                runtimeIds[i] = balls[startIndex + i].RuntimeId;
            }

            result = new SkillBallConsumeResult(
                true,
                startIndex,
                consumeCount,
                selected.Color,
                SkillBallInputNames.FromColorAndCount(selected.Color, signalCount),
                runtimeIds,
                signalCount);

            return true;
        }

        private int ResolveSignalCount(SkillBallData selected, int consumeCount)
        {
            if (IsMatrixMode() && selected.Color != SkillBallColor.White)
            {
                return 3;
            }

            return consumeCount;
        }

        private int ResolveConsumeCount(int index, SkillBallData selected)
        {
            if (selected.ConsumeMode == SkillBallConsumeMode.AlwaysSingle)
            {
                return 1;
            }

            if (selected.ConsumeMode == SkillBallConsumeMode.TripleCheckOnlyOutsideMatrix && IsMatrixMode())
            {
                return 1;
            }

            int left = index;
            while (left > 0 && balls[left - 1].Color == selected.Color)
            {
                left--;
            }

            int right = index;
            while (right < balls.Count - 1 && balls[right + 1].Color == selected.Color)
            {
                right++;
            }

            int sameColorCount = right - left + 1;
            if (sameColorCount >= 3)
            {
                return 3;
            }

            return sameColorCount >= 2 ? 2 : 1;
        }

        private int ResolveConsumeStartIndex(int selectedIndex, SkillBallColor color, int consumeCount)
        {
            if (consumeCount >= 3)
            {
                return ResolveTripleStartIndex(selectedIndex, color);
            }

            if (consumeCount == 2)
            {
                return selectedIndex > 0 && balls[selectedIndex - 1].Color == color
                    ? selectedIndex - 1
                    : selectedIndex;
            }

            return selectedIndex;
        }

        private int ResolveTripleStartIndex(int selectedIndex, SkillBallColor color)
        {
            int left = selectedIndex;
            while (left > 0 && balls[left - 1].Color == color)
            {
                left--;
            }

            int right = selectedIndex;
            while (right < balls.Count - 1 && balls[right + 1].Color == color)
            {
                right++;
            }

            return Mathf.Clamp(selectedIndex - 1, left, right - 2);
        }

        private bool IsMatrixMode()
        {
            return isMatrixMode;
        }

        private void TryRegisterWitchTimeEvents()
        {
            if (witchTimeEventRegistered)
            {
                return;
            }

            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null)
            {
                return;
            }

            eventCenter.AddEventListener<WitchTimeRuntimeInfo>(
                GlobalEvents.OnWitchTimeStarted,
                HandleWitchTimeStarted);
            eventCenter.AddEventListener<WitchTimeRuntimeInfo>(
                GlobalEvents.OnWitchTimeStopped,
                HandleWitchTimeStopped);
            witchTimeEventRegistered = true;
        }

        private void TryUnregisterWitchTimeEvents()
        {
            if (!witchTimeEventRegistered)
            {
                return;
            }

            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null)
            {
                witchTimeEventRegistered = false;
                return;
            }

            eventCenter.RemoveEventListener<WitchTimeRuntimeInfo>(
                GlobalEvents.OnWitchTimeStarted,
                HandleWitchTimeStarted);
            eventCenter.RemoveEventListener<WitchTimeRuntimeInfo>(
                GlobalEvents.OnWitchTimeStopped,
                HandleWitchTimeStopped);
            witchTimeEventRegistered = false;
        }

        private void HandleWitchTimeStarted(WitchTimeRuntimeInfo info)
        {
            if (owner == null || info.casterId != owner.characterId)
            {
                return;
            }

            SetMatrixMode(true);
        }

        private void HandleWitchTimeStopped(WitchTimeRuntimeInfo info)
        {
            if (owner == null || info.casterId != owner.characterId)
            {
                return;
            }

            SetMatrixMode(false);
        }

        private void FillPlayerParameters(SkillBallConsumeResult result)
        {
            if (owner == null || owner.PlayerParameters == null)
            {
                return;
            }

            owner.PlayerParameters.consumedSignalCount = result.SignalCount;
            owner.PlayerParameters.consumedSignalColor = result.Color;
            owner.PlayerParameters.consumedSignalInputName = result.SignalInputName;
            owner.PlayerParameters.consumedSignalRuntimeIds = result.RuntimeIds;
        }

        private void ClearPlayerParameters()
        {
            if (owner == null || owner.PlayerParameters == null)
            {
                return;
            }

            owner.PlayerParameters.consumedSignalCount = 0;
            owner.PlayerParameters.consumedSignalColor = default;
            owner.PlayerParameters.consumedSignalInputName = null;
            owner.PlayerParameters.consumedSkillActionName = null;
            owner.PlayerParameters.consumedSignalRuntimeIds = null;
        }

        private void ClearPreparedConsume()
        {
            hasPreparedConsume = false;
            preparedConsumeResult = SkillBallConsumeResult.Failed();
        }

        private void ClearPendingConsumeCommand()
        {
            hasPendingConsumeCommand = false;
            pendingConsumeCommand = default;
        }

        private void TryCommitPendingConsumeOnActionStart()
        {
            if (!hasPendingConsumeCommand)
            {
                return;
            }

            if (owner == null || owner.ActionPlayer == null)
            {
                CancelPreparedConsume();
                return;
            }

            if (owner.ActionPlayer.CurrentActionClipName != pendingConsumeCommand.ActionName)
            {
                return;
            }

            CommitPreparedConsume();
        }

        private void RemoveBalls(int startIndex, int count)
        {
            balls.RemoveRange(startIndex, count);
        }
    }

    public static class SkillBallConsumeResultExtensions
    {
        public static SkillBallConsumeViewData ToViewData(this SkillBallConsumeResult result)
        {
            return new SkillBallConsumeViewData(
                result.RuntimeIds,
                result.StartIndex,
                result.ConsumeCount,
                result.Color);
        }
    }
}
