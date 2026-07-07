using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.InputHandle
{
    [Serializable]
    public class CharacterMovementDriver
    {
        [TitleGroup("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Rigidbody rigidbody;

        [TitleGroup("Speed")]
        [MinValue(0f)]
        [SerializeField] private float moveSpeed = 10f;
        [MinValue(0f)]
        [SerializeField] private float turnSpeed = 20f;

        [TitleGroup("Update Mode")]
        [SerializeField] private bool useRenderFrameMovement = true;

        private Transform ownerTransform;
        private Vector3 pendingMovementWorldDir;
        private bool hasPendingMovement;

        public bool UseRenderFrameMovement => useRenderFrameMovement;

        public void Initialize(ActionStateController owner)
        {
            if (owner == null)
            {
                return;
            }

            ownerTransform = owner.transform;

            if (rigidbody == null)
            {
                rigidbody = owner.GetComponent<Rigidbody>();
            }

            if (characterController == null)
            {
                characterController = owner.GetComponent<CharacterController>();
            }

            ConfigureMovementDriver();
        }

        public void QueueMovement(Vector3 worldDir)
        {
            pendingMovementWorldDir = worldDir;
            hasPendingMovement = worldDir.sqrMagnitude > 0.001f;
        }

        public void StopQueuedMovement()
        {
            pendingMovementWorldDir = Vector3.zero;
            hasPendingMovement = false;
        }

        public void ApplyPendingMovement(float fixedDeltaTime)
        {
            if (!hasPendingMovement)
            {
                TryMoveByCharacterController(Vector3.zero, fixedDeltaTime);
                return;
            }

            MoveByRigidbody(pendingMovementWorldDir, fixedDeltaTime);
        }

        public void ApplyPendingMovementRenderFrame(float deltaTime)
        {
            if (!useRenderFrameMovement)
            {
                return;
            }

            if (!hasPendingMovement)
            {
                TryMoveByCharacterController(Vector3.zero, deltaTime);
                return;
            }

            MoveByTransform(pendingMovementWorldDir, deltaTime);
        }

        public void MoveByRigidbody(Vector3 worldDir, float fixedDeltaTime)
        {
            if (TryMoveByCharacterController(worldDir, fixedDeltaTime))
            {
                return;
            }

            if (rigidbody == null || worldDir.sqrMagnitude < 0.001f)
            {
                return;
            }

            Vector3 targetPosition = rigidbody.position + worldDir * (moveSpeed * fixedDeltaTime);
            rigidbody.MovePosition(targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(worldDir);
            Quaternion nextRotation = Quaternion.Slerp(
                rigidbody.rotation,
                targetRotation,
                turnSpeed * fixedDeltaTime
            );
            rigidbody.MoveRotation(nextRotation);
        }

        public void MoveByTransform(Vector3 worldDir, float deltaTime)
        {
            if (TryMoveByCharacterController(worldDir, deltaTime))
            {
                return;
            }

            if (ownerTransform == null || worldDir.sqrMagnitude < 0.001f)
            {
                return;
            }

            Vector3 targetPosition = ownerTransform.position + worldDir * (moveSpeed * deltaTime);
            ownerTransform.position = targetPosition;
            RotateTowards(worldDir, deltaTime);
        }

        public void MoveWorldDelta(Vector3 worldDelta)
        {
            if (worldDelta.sqrMagnitude < 0.000001f)
            {
                return;
            }

            if (characterController != null)
            {
                characterController.Move(worldDelta);
                return;
            }

            if (rigidbody != null && !rigidbody.isKinematic)
            {
                rigidbody.MovePosition(rigidbody.position + worldDelta);
                return;
            }

            if (ownerTransform != null)
            {
                ownerTransform.position += worldDelta;
            }
        }

        private bool TryMoveByCharacterController(Vector3 worldDir, float deltaTime)
        {
            if (characterController == null)
            {
                return false;
            }

            if (worldDir.sqrMagnitude > 0.001f)
            {
                RotateTowards(worldDir, deltaTime);
            }

            if (worldDir.sqrMagnitude < 0.001f)
            {
                return true;
            }

            Vector3 horizontalMove = worldDir.normalized * (moveSpeed * deltaTime);
            characterController.Move(horizontalMove);
            return true;
        }

        private void RotateTowards(Vector3 worldDir, float deltaTime)
        {
            if (ownerTransform == null || worldDir.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(worldDir);
            ownerTransform.rotation = Quaternion.Slerp(
                ownerTransform.rotation,
                targetRotation,
                turnSpeed * deltaTime
            );
        }

        private void ConfigureMovementDriver()
        {
            if (rigidbody == null)
            {
                return;
            }

            if (characterController != null || useRenderFrameMovement)
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                rigidbody.interpolation = RigidbodyInterpolation.None;
            }
        }
    }
}
