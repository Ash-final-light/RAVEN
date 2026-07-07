using Tools.BehaviourTree;
using Tools.BehaviourTree.Nodes;
using Tools.BehaviourTree.Nodes.ActionNode;
using UnityEngine;

namespace Monster.Beast01.Nodes
{
    [NodeItemMenu("Action Node/Beast01 Turn To Target Jump", TreeType.Beast01)]
    public class Beast01TurnToTargetJumpNode : ActionNode
    {
        [SerializeField] private string turnActionName = "Jump1";
        [SerializeField] private float faceAngleThreshold = 10f;
        [SerializeField] private float rotateSpeed = 720f;
        [SerializeField] private float turnDuration = 0.6f;
        [SerializeField] private bool finishWhenFacingTarget = true;
        [SerializeField] private bool finishWhenActionEnds = true;

        private Beast01Status status;
        private bool playSucceeded;
        private bool actionEnded;
        private float elapsed;

        protected override void OnStart()
        {
            status = AiStatus as Beast01Status;
            playSucceeded = false;
            actionEnded = false;
            elapsed = 0f;

            if (status == null || !status.HasTarget)
            {
                Debug.LogWarning($"[Beast01TurnToTargetJumpNode] Failed: target missing. hasStatus={status != null}");
                return;
            }

            float angle = GetFaceAngle();
            if (angle <= Mathf.Max(0f, faceAngleThreshold))
            {
                //Debug.Log($"[Beast01TurnToTargetJumpNode] Already facing target. angle={angle:0.00}");
                playSucceeded = true;
                actionEnded = true;
                return;
            }

            if (string.IsNullOrEmpty(turnActionName))
            {
                Debug.LogWarning("[Beast01TurnToTargetJumpNode] Failed: turn action is empty.");
                return;
            }

            if (status.ActionPlayer != null)
            {
                status.ActionPlayer.OnEndStage += HandleActionEnd;
            }

            status.StopMove();
            //Debug.Log($"[Beast01TurnToTargetJumpNode] Start turn action '{turnActionName}'. angle={angle:0.00}, rotateSpeed={rotateSpeed:0.00}, duration={turnDuration:0.00}");
            playSucceeded = status.TryToPlayAction(turnActionName, false);

            if (!playSucceeded)
            {
                Debug.LogWarning($"[Beast01TurnToTargetJumpNode] Failed: action '{turnActionName}' could not play.");
                Unsubscribe();
            }
        }

        protected override State OnTick(float deltaTime = 0f)
        {
            if (status == null || !status.HasTarget)
            {
                return State.Failure;
            }

            if (!playSucceeded)
            {
                return State.Failure;
            }

            if (actionEnded)
            {
                return State.Success;
            }

            elapsed += Mathf.Max(0f, deltaTime);
            status.StopMove();
            status.FaceTarget(rotateSpeed, deltaTime);
            status.QueueLateFaceTarget(rotateSpeed, deltaTime);

            float angle = GetFaceAngle();
            //Debug.Log($"[Beast01TurnToTargetJumpNode] Turning. angle={angle:0.00}, deltaTime={deltaTime:0.0000}, elapsed={elapsed:0.00}");
            if (finishWhenFacingTarget && angle <= Mathf.Max(0f, faceAngleThreshold))
            {
                //Debug.Log($"[Beast01TurnToTargetJumpNode] Finish by angle. angle={angle:0.00}, elapsed={elapsed:0.00}");
                return State.Success;
            }

            float safeDuration = Mathf.Max(0f, turnDuration);
            if (safeDuration > 0f && elapsed >= safeDuration)
            {
                //Debug.Log($"[Beast01TurnToTargetJumpNode] Finish by duration. angle={angle:0.00}, elapsed={elapsed:0.00}");
                return State.Success;
            }

            return State.Running;
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
            if (!finishWhenActionEnds)
            {
                return;
            }

            actionEnded = true;
            //Debug.Log($"[Beast01TurnToTargetJumpNode] Finish by action end. elapsed={elapsed:0.00}");
        }

        private float GetFaceAngle()
        {
            Vector3 toTarget = status.TargetPosition - status.SelfPosition;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                return 0f;
            }

            Vector3 localToTarget = status.WorldToLocalDirection(toTarget.normalized);
            return Vector3.Angle(Vector3.forward, localToTarget);
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
