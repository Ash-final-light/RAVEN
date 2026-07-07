using System;
using UnityEngine;

namespace Tools.Action.Running
{
    public enum StatusEventMatchMode
    {
        StatusAsset,
        StatusName,
        StatusId
    }

    /// <summary>
    /// Base class owned by StatusController.
    /// Character-specific subclasses decide what to do when a matching status enters, exits, or ticks.
    /// </summary>
    [Serializable]
    public abstract class StatusEventListener
    {
        [SerializeField] private StatusEventMatchMode matchMode = StatusEventMatchMode.StatusName;
        [SerializeField] private Status statusAsset;
        [SerializeField] private string statusName;
        [SerializeField] private int statusId;

        protected StatusController StatusController { get; private set; }
        protected Tools.InputHandle.ActionStateController Owner { get; private set; }

        public virtual void Initialize(
            StatusController statusController,
            Tools.InputHandle.ActionStateController owner)
        {
            StatusController = statusController;
            Owner = owner;
        }

        public void HandleStatusEntered(Status status)
        {
            if (Matches(status))
            {
                OnStatusEntered(status);
            }
        }

        public void HandleStatusExited(Status status)
        {
            if (Matches(status))
            {
                OnStatusExited(status);
            }
        }

        public void HandleStatusTicked(Status status, float remainingTime)
        {
            if (Matches(status))
            {
                OnStatusTicked(status, remainingTime);
            }
        }

        protected abstract void OnStatusEntered(Status status);

        protected abstract void OnStatusExited(Status status);

        protected abstract void OnStatusTicked(Status status, float remainingTime);

        protected virtual bool Matches(Status status)
        {
            if (status == null)
            {
                return false;
            }

            return matchMode switch
            {
                StatusEventMatchMode.StatusAsset => statusAsset == status,
                StatusEventMatchMode.StatusName => status.StatusName == statusName,
                StatusEventMatchMode.StatusId => status.status == statusId,
                _ => false
            };
        }
    }
}
