using Tools.BehaviourTree;
using Tools.BehaviourTree.Nodes;
using Tools.BehaviourTree.Nodes.ActionNode;
using Tools.InputHandle.Data;
using UnityEngine;

namespace Monster.Beast01.Nodes
{
    [NodeItemMenu("Action Node/Beast01 Move To Target", TreeType.Beast01)]
    public class Beast01MoveToTargetNode : ActionNode
    {
        [SerializeField] private float stopDistance = 5f;
        [SerializeField] private string moveActionName = "Walk_F";
        [SerializeField] private bool moveActionLoop = true;
        [SerializeField] private string stopActionName;

        private Beast01Status status;
        private bool moveAllowed;
        private bool keepCurrentStateOnStop;

        protected override void OnStart()
        {
            status = AiStatus as Beast01Status;
            moveAllowed = false;
            keepCurrentStateOnStop = false;

            float distance = status != null && status.HasTarget
                ? Vector3.Distance(Flat(status.SelfPosition), Flat(status.TargetPosition))
                : -1f;
            Debug.Log($"[Beast01MoveToTargetNode] Enter. stopDistance={stopDistance:0.00}, moveAction='{moveActionName}', distance={distance:0.00}, hasTarget={status?.HasTarget ?? false}");
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if (status == null || !status.HasTarget)
            {
                StopMove(true);
                return State.Failure;
            }

            Vector3 flatOffset = status.TargetPosition - status.SelfPosition;
            flatOffset.y = 0f;

            float safeStopDistance = Mathf.Max(0f, stopDistance);
            if (flatOffset.magnitude <= safeStopDistance)
            {
                StopMove(true);
                return State.Success;
            }

            if (!TryEnterMoveAction())
            {
                keepCurrentStateOnStop = true;
                status.StopMove();
                return State.Failure;
            }

            status.MoveDirection(flatOffset);
            return State.Running;
        }

        protected override void OnStop()
        {
            StopMove(!keepCurrentStateOnStop);
        }

        protected override void OnAbort()
        {
            StopMove(false);
        }

        private bool TryEnterMoveAction()
        {
            if (string.IsNullOrEmpty(moveActionName))
            {
                moveAllowed = false;
                return false;
            }

            if (moveAllowed && status.IsCurrentAction(moveActionName))
            {
                return true;
            }

            if (status.IsCurrentAction(moveActionName))
            {
                moveAllowed = true;
                return true;
            }

            moveAllowed = status.TryToPlayAction(moveActionName, moveActionLoop, CharacterStateType.Move);
            return moveAllowed;
        }

        private void StopMove(bool switchToIdle)
        {
            if (status == null)
            {
                return;
            }

            status.StopMove();
            if (!switchToIdle)
            {
                return;
            }

            status.SetCharacterStateType(CharacterStateType.Idle);

            if (!string.IsNullOrEmpty(stopActionName) && !status.IsCurrentAction(stopActionName))
            {
                status.TryToPlayAction(stopActionName, true, CharacterStateType.Idle);
            }
        }

        private static Vector3 Flat(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
