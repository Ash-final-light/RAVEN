using System;
using System.Collections.Generic;
using Tools.Action.Running;
using UnityEngine;

namespace Scripts.Action.Alpha
{
    public class AlphaStatusController : StatusController
    {
        [SerializeField] private AlphaSwordIntentSkillBallEffect swordIntentSkillBallEffect;

        protected override void Awake()
        {
            EnsureAlphaListener();
            base.Awake();
        }

        private void EnsureAlphaListener()
        {
            statusEventListeners ??= new List<StatusEventListener>();

            foreach (StatusEventListener listener in statusEventListeners)
            {
                if (listener is AlphaSwordIntentStatusListener alphaListener)
                {
                    alphaListener.SetSkillBallEffect(swordIntentSkillBallEffect);
                    return;
                }
            }

            statusEventListeners.Add(new AlphaSwordIntentStatusListener(swordIntentSkillBallEffect));
        }
    }

    [Serializable]
    public class AlphaSwordIntentStatusListener : StatusEventListener
    {
        [SerializeField] private string bluePreparedStatusName = "Alpha_BlueThreePingPrepared";
        [SerializeField] private string swordIntentStatusName = "Alpha_SwordIntent";
        [SerializeField] private AlphaSwordIntentSkillBallEffect skillBallEffect;

        public AlphaSwordIntentStatusListener()
        {
        }

        public AlphaSwordIntentStatusListener(AlphaSwordIntentSkillBallEffect skillBallEffect)
        {
            this.skillBallEffect = skillBallEffect;
        }

        public void SetSkillBallEffect(AlphaSwordIntentSkillBallEffect skillBallEffect)
        {
            this.skillBallEffect = skillBallEffect;
        }

        protected override bool Matches(Status status)
        {
            return status != null &&
                   (status.StatusName == bluePreparedStatusName ||
                    status.StatusName == swordIntentStatusName);
        }

        protected override void OnStatusEntered(Status status)
        {
            if (status.StatusName == bluePreparedStatusName)
            {
                SavePreparedRuntimeIds();
                return;
            }

            if (status.StatusName == swordIntentStatusName)
            {
                StatusController?.ExitStatusByName(bluePreparedStatusName);
                EnterSwordIntent();
            }
        }

        protected override void OnStatusExited(Status status)
        {
            if (status.StatusName == swordIntentStatusName)
            {
                ExitSwordIntent();
            }
        }

        protected override void OnStatusTicked(Status status, float remainingTime)
        {
        }

        private void SavePreparedRuntimeIds()
        {
            if (Owner == null ||
                Owner.RunningBlackboard == null ||
                !Owner.RunningBlackboard.ContainsKey(AlphaSkillBallKeys.BlueThreePingRuntimeIds))
            {
                return;
            }

            string runtimeIds = AlphaAnyThreePingAfterBluePreparedTrigger.SerializeRuntimeIds(
                Owner.ActionPlayer?.RuntimeContext?.ConsumedSignalRuntimeIds);
            Owner.RunningBlackboard.SetStringParameter(AlphaSkillBallKeys.BlueThreePingRuntimeIds, runtimeIds);
        }

        private void EnterSwordIntent()
        {
            if (Owner == null)
            {
                return;
            }

            int triggerConsumedBallCount = Mathf.Max(
                0,
                Owner.ActionPlayer?.RuntimeContext?.ConsumedSignalCount ?? 0);

            if (skillBallEffect != null)
            {
                skillBallEffect.ApplyEnter(Owner, triggerConsumedBallCount);
                return;
            }

            AlphaSwordIntentSkillBallEffect.ApplyDefaultEnter(Owner, triggerConsumedBallCount);
        }

        private void ExitSwordIntent()
        {
            if (Owner == null)
            {
                return;
            }

            if (skillBallEffect != null)
            {
                skillBallEffect.ApplyExit(Owner);
                return;
            }

            AlphaSwordIntentSkillBallEffect.ApplyDefaultExit(Owner);
        }
    }
}
