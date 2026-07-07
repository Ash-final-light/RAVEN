using Monster.Beast01;
using Tools.BehaviourTree;
using Tools.BehaviourTree.Nodes;
using Tools.BehaviourTree.Nodes.ActionNode;
using UnityEngine;

namespace Monster.Beast01.Nodes
{
    [NodeItemMenu("Action Node/Beast01 Play Action", TreeType.Beast01)]
    public class Beast01PlayActionNode : ActionNode
    {
        [SerializeField] private string actionName;
        [SerializeField] private bool loop;
        [SerializeField] private float waitAfterEnd;

        private Beast01Status status;
        private bool playRequested;
        private bool playSucceeded;
        private bool actionEnded;
        private float endTime;

        protected override void OnStart()
        {
            status = AiStatus as Beast01Status;
            playRequested = true;
            playSucceeded = false;
            actionEnded = false;
            endTime = 0f;

            if (status == null || status.ActionPlayer == null || string.IsNullOrEmpty(actionName))
            {
                Debug.LogWarning($"[Beast01PlayActionNode] Play skipped. action='{actionName}', hasStatus={status != null}, hasActionPlayer={status?.ActionPlayer != null}");
                return;
            }

            Debug.Log($"[Beast01PlayActionNode] Try play action '{actionName}'. loop={loop}");
            status.ActionPlayer.OnEndStage += HandleActionEnd;
            playSucceeded = AiStatus.TryToPlayAction(actionName, loop);
            Debug.Log($"[Beast01PlayActionNode] Play action '{actionName}' result={playSucceeded}");

            if (!playSucceeded)
            {
                Unsubscribe();
            }
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if (!playRequested || !playSucceeded)
            {
                return State.Failure;
            }

            if (loop)
            {
                return State.Success;
            }

            if (!actionEnded)
            {
                return State.Running;
            }

            if (Time.time - endTime < waitAfterEnd)
            {
                return State.Running;
            }

            return State.Success;
        }

        protected override void OnStop()
        {
            Unsubscribe();
        }

        protected override void OnAbort()
        {
            Unsubscribe();
        }

        private void HandleActionEnd()
        {
            actionEnded = true;
            endTime = Time.time;
            Debug.Log($"[Beast01PlayActionNode] Action '{actionName}' ended.");
        }

        private void Unsubscribe()
        {
            if (status?.ActionPlayer != null)
            {
                status.ActionPlayer.OnEndStage -= HandleActionEnd;
            }
        }
    }
}
