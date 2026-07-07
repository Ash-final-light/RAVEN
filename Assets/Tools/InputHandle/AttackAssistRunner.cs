using Tools.Battle.Data;
using Tools.Camera;
using Tools.InputHandle.Data;
using UnityEngine;

namespace Tools.InputHandle
{
    public static class AttackAssistRunner
    {
        public static void Apply(ActionStateController owner)
        {
            if (owner == null)
            {
                return;
            }

            AttackAssistConfigSo config = owner.AttackAssistConfig;
            if (config == null || !config.enableAttackAssist)
            {
                return;
            }

            MainCameraController cameraController = Object.FindFirstObjectByType<MainCameraController>();
            CharacterProperty targetProperty = cameraController?.ResolveAttackTarget(true);
            Transform target = targetProperty?.targetTransform;
            if (target == null)
            {
                return;
            }

            Vector3 toTarget = target.position - owner.transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance <= 0.001f || distance > config.assistRadius)
            {
                return;
            }

            owner.StartAttackAssistTurn(config.turnSpeed, toTarget);
            ApplyStepMove(owner, toTarget, distance, config);
        }

        private static void ApplyStepMove(
            ActionStateController owner,
            Vector3 toTarget,
            float distance,
            AttackAssistConfigSo config)
        {
            if (distance < config.minStepTriggerDistance ||
                distance > config.maxStepTriggerDistance)
            {
                return;
            }

            float stepDistance = Mathf.Min(
                config.maxStepDistance,
                Mathf.Max(0f, distance - config.stopDistance));

            if (stepDistance <= 0.001f)
            {
                return;
            }

            owner.StartAttackAssistStep(
                toTarget.normalized * stepDistance,
                config.stepDuration,
                config.turnSpeed,
                toTarget);
        }
    }
}
