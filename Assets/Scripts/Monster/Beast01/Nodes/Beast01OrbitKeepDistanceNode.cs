using Tools.BehaviourTree;
using Tools.BehaviourTree.Nodes;
using Tools.BehaviourTree.Nodes.ActionNode;
using Tools.InputHandle.Data;
using UnityEngine;

namespace Monster.Beast01.Nodes
{
    [NodeItemMenu("Action Node/Beast01 Orbit Keep Distance", TreeType.Beast01)]
    public class Beast01OrbitKeepDistanceNode : ActionNode
    {
        [SerializeField] private float targetDistance = 5f;
        [SerializeField] private float distanceTolerance = 0.5f;
        [SerializeField] private float orbitWeight = 1f;
        [SerializeField] private float radialWeight = 0.35f;
        [SerializeField] private bool clockwise = true;
        [SerializeField] private float rotateSpeed = 360f;
        [SerializeField] private float maxDuration = 0.25f;

        [SerializeField] private string moveForwardAction = "Walk_F";
        [SerializeField] private string moveBackwardAction = "Walk_B";
        [SerializeField] private string moveLeftAction = "Walk_L";
        [SerializeField] private string moveRightAction = "Walk_R";
        [SerializeField] private bool moveActionLoop = true;
        [SerializeField] private string stopActionName = "idle1";

        private Beast01Status status;
        private bool keepCurrentStateOnStop;
        private float startTime;

        protected override void OnStart()
        {
            status = AiStatus as Beast01Status;
            keepCurrentStateOnStop = false;
            startTime = Time.time;

            float distance = status != null && status.HasTarget
                ? Vector3.Distance(Flat(status.SelfPosition), Flat(status.TargetPosition))
                : -1f;
            //Debug.Log($"[Beast01OrbitKeepDistanceNode] Enter. targetDistance={targetDistance:0.00}, tolerance={distanceTolerance:0.00}, maxDuration={maxDuration:0.00}, clockwise={clockwise}, distance={distance:0.00}, hasTarget={status?.HasTarget ?? false}");
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if (status == null || !status.HasTarget)
            {
                StopMove(true);
                return State.Failure;
            }

            float safeMaxDuration = Mathf.Max(0f, maxDuration);
            if (safeMaxDuration > 0f && Time.time - startTime >= safeMaxDuration)
            {
                float exitDistance = Vector3.Distance(Flat(status.SelfPosition), Flat(status.TargetPosition));
                //Debug.Log($"[Beast01OrbitKeepDistanceNode] Exit by maxDuration. elapsed={Time.time - startTime:0.00}, distance={exitDistance:0.00}");
                StopMove(true);
                return State.Success;
            }

            Vector3 offset = status.TargetPosition - status.SelfPosition;
            offset.y = 0f;

            if (offset.sqrMagnitude <= Mathf.Epsilon)
            {
                StopMove(true);
                return State.Running;
            }

            status.FaceTarget(rotateSpeed, deltaTime);

            Vector3 towards = offset.normalized;
            Vector3 tangent = Vector3.Cross(Vector3.up, towards);
            if (!clockwise)
            {
                tangent = -tangent;
            }

            float safeTargetDistance = Mathf.Max(0f, targetDistance);
            float safeTolerance = Mathf.Max(0f, distanceTolerance);
            float distance = offset.magnitude;
            float radial = 0f;

            if (distance > safeTargetDistance + safeTolerance)
            {
                radial = 1f;
            }
            else if (distance < safeTargetDistance - safeTolerance)
            {
                radial = -1f;
            }

            Vector3 moveDirection = tangent * orbitWeight + towards * (radial * radialWeight);
            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                status.StopMove();
                return State.Running;
            }

            string moveActionName = GetMoveActionName(moveDirection);
            if (!TryEnterMoveAction(moveActionName))
            {
                keepCurrentStateOnStop = true;
                status.StopMove();
                return State.Failure;
            }

            status.MoveDirection(moveDirection);
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

        private string GetMoveActionName(Vector3 worldMoveDirection)
        {
            Vector3 localMove = status.WorldToLocalDirection(worldMoveDirection.normalized);
            localMove.y = 0f;

            if (Mathf.Abs(localMove.z) >= Mathf.Abs(localMove.x))
            {
                return localMove.z >= 0f ? moveForwardAction : moveBackwardAction;
            }

            return localMove.x >= 0f ? moveRightAction : moveLeftAction;
        }

        private bool TryEnterMoveAction(string moveActionName)
        {
            if (string.IsNullOrEmpty(moveActionName))
            {
                return false;
            }

            if (status.IsCurrentAction(moveActionName))
            {
                return true;
            }

            if (IsConfiguredMoveAction(status.CurrentActionName))
            {
                status.SetCharacterStateType(CharacterStateType.Idle);
            }

            return status.TryToPlayAction(moveActionName, moveActionLoop, CharacterStateType.Move);
        }

        private bool IsConfiguredMoveAction(string actionName)
        {
            return actionName == moveForwardAction ||
                   actionName == moveBackwardAction ||
                   actionName == moveLeftAction ||
                   actionName == moveRightAction;
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
