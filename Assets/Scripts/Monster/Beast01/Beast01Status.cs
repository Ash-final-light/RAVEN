using System;
using Tools.AI;
using Tools.Action.Running;
using Tools.BehaviourTree;
using Tools.InputHandle;
using Tools.InputHandle.Data;
using UnityEngine;

namespace Monster.Beast01
{
    [Serializable]
    public class Beast01Status : AiStatus
    {
        public BehaviourTree runningTree;

        private readonly Transform target;
        private float nextBlackboardLogTime;
        private bool hasLateFaceTarget;
        private float lateFaceRotateSpeed;
        private float lateFaceDeltaTime;

        public Beast01Status(
            BehaviourTree runningTree,
            Transform selfTransform,
            ActionStateController stateController,
            Transform target) : base(selfTransform, stateController)
        {
            this.runningTree = runningTree;
            this.target = target;
        }

        public bool HasTarget => target != null;

        public Vector3 SelfPosition => selfTransform.position;

        public Vector3 TargetPosition => HasTarget ? target.position : selfTransform.position;

        public ActionPlayer ActionPlayer => stateController.ActionPlayer;

        public string CurrentActionName => ActionPlayer?.CurrentActionClipName;

        public override void UpdateBlackboard()
        {
            if (runningTree == null || runningTree.blackboard == null)
            {
                return;
            }

            float playerDistance = float.MaxValue;

            if (HasTarget)
            {
                Vector3 delta = target.position - selfTransform.position;
                delta.y = 0;
                playerDistance = delta.magnitude;
            }

            runningTree.blackboard.SetFloatParameter(Beast01Constants.PlayerDistance, playerDistance);

            if (Time.time >= nextBlackboardLogTime)
            {
                nextBlackboardLogTime = Time.time + 0.5f;
                //Debug.Log($"[Beast01Blackboard] playerDistance={playerDistance:0.00}, hasTarget={HasTarget}, currentAction='{CurrentActionName}'");
            }
        }

        public override bool TryToPlayAction(string actionName, bool loop = false)
        {
            return stateController.StateMachine.TryPlayAction(actionName, loop);
        }

        public bool TryToPlayAction(string actionName, bool loop, CharacterStateType targetState)
        {
            return stateController.StateMachine.TryPlayAction(actionName, loop, targetState);
        }

        public void SetCharacterStateType(CharacterStateType stateType)
        {
            stateController.StateMachine.SetCharacterStateType(stateType);
        }

        public bool IsCurrentAction(string actionName)
        {
            return CurrentActionName == actionName;
        }

        public void MoveDirection(Vector3 worldDirection)
        {
            worldDirection.y = 0f;

            if (worldDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                StopMove();
                return;
            }

            stateController.QueueMovement(worldDirection.normalized);
        }

        public void FaceTarget(float rotateSpeed, float deltaTime)
        {
            if (!TryGetTargetRotation(out Quaternion targetRotation))
            {
                return;
            }

            RotateTo(targetRotation, rotateSpeed, deltaTime);
        }

        public void QueueLateFaceTarget(float rotateSpeed, float deltaTime)
        {
            hasLateFaceTarget = true;
            lateFaceRotateSpeed = rotateSpeed;
            lateFaceDeltaTime = deltaTime;
        }

        public void ApplyLateFaceTarget()
        {
            if (!hasLateFaceTarget)
            {
                return;
            }

            hasLateFaceTarget = false;
            if (TryGetTargetRotation(out Quaternion targetRotation))
            {
                RotateTo(targetRotation, lateFaceRotateSpeed, lateFaceDeltaTime);
            }
        }

        private bool TryGetTargetRotation(out Quaternion targetRotation)
        {
            targetRotation = selfTransform.rotation;
            if (!HasTarget)
            {
                return false;
            }
            
            Vector3 toTarget = TargetPosition - SelfPosition;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            targetRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            return true;
        }

        private void RotateTo(Quaternion targetRotation, float rotateSpeed, float deltaTime)
        {
            if (rotateSpeed <= 0f)
            {
                selfTransform.rotation = targetRotation;
                return;
            }

            selfTransform.rotation = Quaternion.RotateTowards(
                selfTransform.rotation,
                targetRotation,
                rotateSpeed * deltaTime);
        }

        public Vector3 WorldToLocalDirection(Vector3 worldDirection)
        {
            worldDirection.y = 0f;
            return selfTransform.InverseTransformDirection(worldDirection);
        }

        public void StopMove()
        {
            stateController.StopQueuedMovement();
        }
    }
}
