using System;
using System.Collections.Generic;
using Tools.Action.Data.Event;
using Tools.Blackboard.Data;
using UnityEngine;

namespace Tools.Action.Running
{
    [Serializable]
    public class StatusBlackboardWrite
    {
        public BlackboardValueType type;
        public string key;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;
    }

    [CreateAssetMenu(fileName = "Status", menuName = "Action/Status")]
    public class Status : ScriptableObject, IBlackBoardProvider
    {
        // This blackboard is only used by the editor drawer as a condition reference.
        // Runtime checks must read the character's RunningBlackboard.
        [SerializeField] private Blackboard.Data.Blackboard blackboard;
        [SerializeField] private List<BlackboardCondition> conditions = new();
        [SerializeField] private List<StatusTrigger> triggers = new();

        public Blackboard.Data.Blackboard Blackboard => blackboard;
        public IReadOnlyList<BlackboardCondition> Conditions => conditions;
        public IReadOnlyList<StatusTrigger> Triggers => triggers;

        public string StatusName;
        public int status;
        public float continueTime;

        [Tooltip("When true, satisfying the condition again resets the remaining duration.")]
        public bool refreshDurationWhenSatisfied = true;

        [Tooltip("When true, an active status exits immediately once its conditions fail.")]
        public bool exitWhenConditionFailed;

        [Tooltip("Optional bool key written to the character RunningBlackboard on enter/exit.")]
        public string activeBoolKey;

        [Tooltip("Optional blackboard values written when this status exits.")]
        public List<StatusBlackboardWrite> exitBlackboardWrites = new();
    }
}
