using UnityEngine;

namespace Tools.InputHandle.Data
{
    [CreateAssetMenu(menuName = "InputHandle/Attack Assist Config")]
    public class AttackAssistConfigSo : ScriptableObject
    {
        [Header("Enable")]
        public bool enableAttackAssist = true;

        [Header("Target Range")]
        public float assistRadius = 6f;

        [Header("Turn")]
        public float turnSpeed = 720f;

        [Header("Step Move")]
        public float minStepTriggerDistance = 1.2f;
        public float maxStepTriggerDistance = 5f;
        public float stopDistance = 1.1f;
        public float maxStepDistance = 2f;
        public float stepDuration = 0.08f;
    }
}
