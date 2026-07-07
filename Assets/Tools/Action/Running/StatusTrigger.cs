using Tools.InputHandle;
using Tools.Skill;
using UnityEngine;

namespace Tools.Action.Running
{
    public readonly struct StatusTriggerContext
    {
        public readonly ActionStateController Owner;
        public readonly Tools.Blackboard.Data.Blackboard RunningBlackboard;
        public readonly string CurrentActionName;

        public readonly bool HasSkillBallConsume;
        public readonly SkillBallColor ConsumedSignalColor;
        public readonly int ConsumedSignalCount;
        public readonly string ConsumedSignalInputName;
        public readonly string ConsumedSkillActionName;
        public readonly int[] ConsumedSignalRuntimeIds;

        public StatusTriggerContext(ActionStateController owner)
        {
            Owner = owner;
            RunningBlackboard = owner != null ? owner.RunningBlackboard : null;
            CurrentActionName = owner?.ActionPlayer?.CurrentActionClipName;

            ActionRuntimeContext runtimeContext = owner?.ActionPlayer?.RuntimeContext;
            HasSkillBallConsume = runtimeContext != null && runtimeContext.ConsumedSignalCount > 0;
            ConsumedSignalColor = runtimeContext != null ? runtimeContext.ConsumedSignalColor : default;
            ConsumedSignalCount = runtimeContext != null ? runtimeContext.ConsumedSignalCount : 0;
            ConsumedSignalInputName = runtimeContext != null ? runtimeContext.ConsumedSignalInputName : null;
            ConsumedSkillActionName = runtimeContext != null ? runtimeContext.ConsumedSkillActionName : null;
            ConsumedSignalRuntimeIds = runtimeContext != null ? runtimeContext.ConsumedSignalRuntimeIds : null;
        }
    }

    /// <summary>
    /// Base class for status triggers that interpret current player information.
    /// Subclasses decide whether the current frame should enter or refresh a status.
    /// </summary>
    public abstract class StatusTrigger : ScriptableObject
    {
        public abstract bool Satisfy(StatusTriggerContext context);
    }
}
