using System;
using System.Collections.Generic;
using Tools.Action.Data.Event;
using Tools.Blackboard.Data;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Action.Running
{
    public abstract class StatusController : MonoBehaviour
    {
        private readonly Dictionary<Status, float> activeStatusTimers = new();

        protected ActionStateController owner;
        private Blackboard.Data.Blackboard characterRunningBlackboard;

        [SerializeField] private Status[] status;
        [SerializeReference] protected List<StatusEventListener> statusEventListeners = new();

        public event Action<Status> StatusEntered;
        public event Action<Status> StatusExited;
        public event Action<Status, float> StatusTicked;

        protected virtual void Awake()
        {
            owner = GetComponent<ActionStateController>();
            InitializeStatusEventListeners();
        }

        protected virtual void Start()
        {
            TryBindRunningBlackboard();
        }

        protected virtual void Update()
        {
            if (!TryBindRunningBlackboard())
            {
                return;
            }

            CheckStatusConditions();
            TickActiveStatuses(Time.deltaTime);
        }

        public bool IsActive(Status targetStatus)
        {
            return targetStatus != null && activeStatusTimers.ContainsKey(targetStatus);
        }

        public void EnterStatus(Status targetStatus)
        {
            if (targetStatus == null)
            {
                return;
            }

            bool wasActive = activeStatusTimers.ContainsKey(targetStatus);
            activeStatusTimers[targetStatus] = Mathf.Max(0f, targetStatus.continueTime);
            SetActiveBool(targetStatus, true);

            if (!wasActive)
            {
                NotifyStatusEntered(targetStatus);
                StatusEntered?.Invoke(targetStatus);
            }
        }

        public void ExitStatus(Status targetStatus)
        {
            if (targetStatus == null)
            {
                return;
            }

            if (!activeStatusTimers.Remove(targetStatus))
            {
                return;
            }

            SetActiveBool(targetStatus, false);
            ApplyBlackboardWrites(targetStatus.exitBlackboardWrites, targetStatus);
            NotifyStatusExited(targetStatus);
            StatusExited?.Invoke(targetStatus);
        }

        public bool ExitStatusByName(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName) || status == null)
            {
                return false;
            }

            foreach (Status targetStatus in status)
            {
                if (targetStatus == null || targetStatus.StatusName != statusName)
                {
                    continue;
                }

                if (!IsActive(targetStatus))
                {
                    return false;
                }

                ExitStatus(targetStatus);
                return true;
            }

            return false;
        }

        private bool TryBindRunningBlackboard()
        {
            if (characterRunningBlackboard != null)
            {
                return true;
            }

            if (owner == null)
            {
                owner = GetComponent<ActionStateController>();
            }

            characterRunningBlackboard = owner != null ? owner.RunningBlackboard : null;
            return characterRunningBlackboard != null;
        }

        private void CheckStatusConditions()
        {
            if (status == null)
            {
                return;
            }

            foreach (Status targetStatus in status)
            {
                if (targetStatus == null)
                {
                    continue;
                }

                StatusTriggerContext triggerContext = new(owner);
                bool conditionsSatisfied = IsStatusSatisfied(targetStatus, triggerContext);
                bool isActive = activeStatusTimers.ContainsKey(targetStatus);

                if (conditionsSatisfied)
                {
                    if (!isActive || targetStatus.refreshDurationWhenSatisfied)
                    {
                        EnterStatus(targetStatus);
                    }

                    continue;
                }

                if (isActive && targetStatus.exitWhenConditionFailed)
                {
                    ExitStatus(targetStatus);
                }
            }
        }

        private bool IsStatusSatisfied(
            Status targetStatus,
            StatusTriggerContext triggerContext)
        {
            bool hasConditions = targetStatus.Conditions != null && targetStatus.Conditions.Count > 0;
            bool hasTriggers = targetStatus.Triggers != null && targetStatus.Triggers.Count > 0;

            if (!hasConditions && !hasTriggers)
            {
                return false;
            }

            return (!hasConditions || AreConditionsSatisfied(targetStatus)) &&
                   (!hasTriggers || AreTriggersSatisfied(targetStatus, triggerContext));
        }

        private bool AreConditionsSatisfied(Status targetStatus)
        {
            IReadOnlyList<BlackboardCondition> conditions = targetStatus.Conditions;
            if (conditions == null || conditions.Count == 0)
            {
                return false;
            }

            foreach (BlackboardCondition condition in conditions)
            {
                if (condition == null || !condition.Satisfy(characterRunningBlackboard))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreTriggersSatisfied(
            Status targetStatus,
            StatusTriggerContext triggerContext)
        {
            IReadOnlyList<StatusTrigger> triggers = targetStatus.Triggers;
            if (triggers == null || triggers.Count == 0)
            {
                return false;
            }

            foreach (StatusTrigger trigger in triggers)
            {
                if (trigger == null || !trigger.Satisfy(triggerContext))
                {
                    return false;
                }
            }

            return true;
        }

        private void TickActiveStatuses(float deltaTime)
        {
            if (activeStatusTimers.Count == 0)
            {
                return;
            }

            List<Status> expiredStatuses = null;
            List<Status> activeStatuses = new(activeStatusTimers.Keys);

            foreach (Status activeStatus in activeStatuses)
            {
                float remainingTime = activeStatusTimers[activeStatus];
                if (activeStatus.continueTime > 0f)
                {
                    remainingTime = Mathf.Max(0f, remainingTime - deltaTime);
                    activeStatusTimers[activeStatus] = remainingTime;
                }

                NotifyStatusTicked(activeStatus, remainingTime);
                StatusTicked?.Invoke(activeStatus, remainingTime);

                if (activeStatus.continueTime > 0f && remainingTime <= 0f)
                {
                    expiredStatuses ??= new List<Status>();
                    expiredStatuses.Add(activeStatus);
                }
            }

            if (expiredStatuses == null)
            {
                return;
            }

            foreach (Status expiredStatus in expiredStatuses)
            {
                ExitStatus(expiredStatus);
            }
        }

        private void InitializeStatusEventListeners()
        {
            if (statusEventListeners == null)
            {
                return;
            }

            foreach (StatusEventListener listener in statusEventListeners)
            {
                listener?.Initialize(this, owner);
            }
        }

        private void NotifyStatusEntered(Status targetStatus)
        {
            if (statusEventListeners == null)
            {
                return;
            }

            foreach (StatusEventListener listener in statusEventListeners)
            {
                listener?.HandleStatusEntered(targetStatus);
            }
        }

        private void NotifyStatusExited(Status targetStatus)
        {
            if (statusEventListeners == null)
            {
                return;
            }

            foreach (StatusEventListener listener in statusEventListeners)
            {
                listener?.HandleStatusExited(targetStatus);
            }
        }

        private void NotifyStatusTicked(Status targetStatus, float remainingTime)
        {
            if (statusEventListeners == null)
            {
                return;
            }

            foreach (StatusEventListener listener in statusEventListeners)
            {
                listener?.HandleStatusTicked(targetStatus, remainingTime);
            }
        }

        private void SetActiveBool(Status targetStatus, bool isActive)
        {
            if (characterRunningBlackboard == null ||
                string.IsNullOrWhiteSpace(targetStatus.activeBoolKey))
            {
                return;
            }

            if (!characterRunningBlackboard.ContainsKey(targetStatus.activeBoolKey))
            {
                Debug.LogWarning(
                    $"[StatusController] Blackboard key not found. status={targetStatus.StatusName}, key={targetStatus.activeBoolKey}",
                    this);
                return;
            }

            characterRunningBlackboard.SetBoolParameter(targetStatus.activeBoolKey, isActive);
        }

        private void ApplyBlackboardWrites(
            IReadOnlyList<StatusBlackboardWrite> writes,
            Status sourceStatus)
        {
            if (characterRunningBlackboard == null || writes == null)
            {
                return;
            }

            foreach (StatusBlackboardWrite write in writes)
            {
                ApplyBlackboardWrite(write, sourceStatus);
            }
        }

        private void ApplyBlackboardWrite(
            StatusBlackboardWrite write,
            Status sourceStatus)
        {
            if (write == null || string.IsNullOrWhiteSpace(write.key))
            {
                return;
            }

            if (!characterRunningBlackboard.ContainsKey(write.key))
            {
                Debug.LogWarning(
                    $"[StatusController] Blackboard key not found. status={sourceStatus.StatusName}, key={write.key}",
                    this);
                return;
            }

            switch (write.type)
            {
                case BlackboardValueType.Bool:
                    characterRunningBlackboard.SetBoolParameter(write.key, write.boolValue);
                    break;
                case BlackboardValueType.Int:
                    characterRunningBlackboard.SetIntParameter(write.key, write.intValue);
                    break;
                case BlackboardValueType.Float:
                    characterRunningBlackboard.SetFloatParameter(write.key, write.floatValue);
                    break;
                case BlackboardValueType.String:
                    characterRunningBlackboard.SetStringParameter(write.key, write.stringValue);
                    break;
                default:
                    Debug.LogWarning(
                        $"[StatusController] Unsupported blackboard write type. status={sourceStatus.StatusName}, key={write.key}, type={write.type}",
                        this);
                    break;
            }
        }
    }
}
