using Tools.AI;
using Tools.InputHandle;
using Tools.InputHandle.Data;
using UnityEngine;

namespace Monster.Beast01
{
    public class TestBeast01StateMachine:BaseStateMachine
    {
        private AgentCDController _agentCDController;
        public TestBeast01StateMachine(ActionStateController actionStateController, AgentCDController agentCdController) : base(actionStateController)
         {
             _agentCDController = agentCdController;
             Owner._actionPlayer.OnEndStage += HandleIdle;
          }

        private void HandleIdle()
        {
            if (Owner.idle == null || Owner._actionPlayer.CurrentActionClipName == Owner.idle.name)
            {
                return;
            }

            Owner.StopQueuedMovement();
            SetCharacterStateType(CharacterStateType.Idle);
            Owner._actionPlayer.SetActionClipAndPlay(Owner, Owner.idle, true);
        }

        protected override void UpdateCurrentState(float deltaTime)
        {
            _agentCDController.Tick(deltaTime);
        }

        public override bool TryPlayAction(string clipName, bool loop, CharacterStateType? targetState = null)
        {
            //Debug.Log($"[Beast01StateMachine] Try action '{clipName}'. loop={loop}, currentState={_currentStateType}, currentAction='{CurrentAction}', targetState={targetState?.ToString() ?? "<clip>"}");

            if (!_agentCDController.CanExecute(clipName))
            {
                //Debug.Log($"[Beast01StateMachine] Action '{clipName}' blocked by cooldown.");
                return false;
            }

            if (!Owner.actionClipContainerSo.ActionClips.TryGetValue(clipName, out var clip))
            {
                //Debug.LogWarning($"[Beast01StateMachine] Action '{clipName}' missing from action container.");
                return false;
            }

            if (!CanTransitionTo(clip.StateType))
            {
                //Debug.Log($"[Beast01StateMachine] Action '{clipName}' blocked by state transition. currentState={_currentStateType}, nextState={clip.StateType}");
                return false;
            }

            bool played = base.TryPlayAction(clipName, loop, targetState);
            //Debug.Log($"[Beast01StateMachine] Action '{clipName}' play result={played}.");

            if (!played)
            {
                return false;
            }

            _agentCDController.UpdateCooldown(clipName);
            //Debug.Log($"[Beast01StateMachine] Action '{clipName}' cooldown updated.");
            return true;
        }
    }
}
