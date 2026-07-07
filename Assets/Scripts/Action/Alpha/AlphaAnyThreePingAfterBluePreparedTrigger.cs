using System;
using Tools.Action.Running;
using Tools.Skill;
using UnityEngine;

namespace Scripts.Action.Alpha
{
    [CreateAssetMenu(
        fileName = "AlphaAnyThreePingAfterBluePreparedTrigger",
        menuName = "Action/Status Trigger/Alpha/Any Three Ping After Blue Prepared")]
    public class AlphaAnyThreePingAfterBluePreparedTrigger : StatusTrigger
    {
        [SerializeField] private int requiredConsumeCount = 3;
        [SerializeField] private string preparedKey = AlphaSkillBallKeys.BlueThreePingPrepared;
        [SerializeField] private string preparedRuntimeIdsKey = AlphaSkillBallKeys.BlueThreePingRuntimeIds;

        public override bool Satisfy(StatusTriggerContext context)
        {
            if (!context.HasSkillBallConsume ||
                context.ConsumedSignalCount < requiredConsumeCount ||
                context.RunningBlackboard == null ||
                string.IsNullOrWhiteSpace(preparedKey) ||
                !context.RunningBlackboard.ContainsKey(preparedKey) ||
                !context.RunningBlackboard.GetBoolParameter(preparedKey))
            {
                return false;
            }

            string currentRuntimeIds = SerializeRuntimeIds(context.ConsumedSignalRuntimeIds);
            if (string.IsNullOrWhiteSpace(preparedRuntimeIdsKey) ||
                !context.RunningBlackboard.ContainsKey(preparedRuntimeIdsKey))
            {
                return false;
            }

            string preparedRuntimeIds = context.RunningBlackboard.GetStringParameter(preparedRuntimeIdsKey);
            return !string.IsNullOrEmpty(preparedRuntimeIds) &&
                   !string.Equals(preparedRuntimeIds, currentRuntimeIds, StringComparison.Ordinal);
        }

        public static string SerializeRuntimeIds(int[] runtimeIds)
        {
            return runtimeIds == null || runtimeIds.Length == 0
                ? string.Empty
                : string.Join(",", runtimeIds);
        }
    }
}
