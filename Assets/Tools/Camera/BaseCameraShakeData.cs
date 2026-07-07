using System;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Camera
{
    [Serializable]
    public abstract class BaseCameraShakeData
    {
        public abstract void GenerateShake(Vector3 shakePosition);
    }

    public enum CameraShakeShape
    {
        Custom,
        Recoil,
        Bump,
        Explosion,
        Rumble,
    }

    [Serializable]
    public class CameraShakeUniformData : BaseCameraShakeData
    {
        [LabelText("震动通道")]
        public int channel = 1;

        [LabelText("震动时间"), MinValue(0f)]
        public float duration = 0.2f;

        [LabelText("震动力度")]
        public float force = 1f;

        [LabelText("传播距离")]
        public float dissipationDistance = 100f;

        [LabelText("配置震动方向")]
        public Vector3 defaultVelocity = Vector3.up;

        [NonSerialized] private Vector3 _runtimeVelocity;
        [NonSerialized] private bool _hasRuntimeVelocity;
        [NonSerialized] private CinemachineFixedSignal _runtimeSignal;

        public void SetVelocity(Vector3 velocity)
        {
            _runtimeVelocity = velocity;
            _hasRuntimeVelocity = true;
        }

        public override void GenerateShake(Vector3 shakePosition)
        {
            var impulseDefinition = new CinemachineImpulseDefinition
            {
                m_ImpulseChannel = channel,
                m_RawSignal = GetOrCreateSignal(),
                m_AmplitudeGain = 1f,
                m_FrequencyGain = 1f,
                m_RepeatMode = CinemachineImpulseDefinition.RepeatMode.Stretch,
                m_Randomize = false,
                m_TimeEnvelope = new CinemachineImpulseManager.EnvelopeDefinition
                {
                    m_AttackTime = 0f,
                    m_SustainTime = 0f,
                    m_DecayTime = duration,
                    m_ScaleWithImpact = false,
                    m_HoldForever = false
                },
                m_ImpactRadius = 100f,
                m_DirectionMode = CinemachineImpulseManager.ImpulseEvent.DirectionMode.Fixed,
                m_DissipationMode = CinemachineImpulseManager.ImpulseEvent.DissipationMode.ExponentialDecay,
                m_DissipationDistance = dissipationDistance,
                m_PropagationSpeed = 343f
            };

            Vector3 direction = _hasRuntimeVelocity ? _runtimeVelocity : defaultVelocity;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = Vector3.up;
            }

            Vector3 velocity = direction.normalized * force;
            impulseDefinition.CreateEvent(shakePosition, velocity);
            _hasRuntimeVelocity = false;
        }

        private CinemachineFixedSignal GetOrCreateSignal()
        {
            if (_runtimeSignal != null)
            {
                return _runtimeSignal;
            }

            _runtimeSignal = ScriptableObject.CreateInstance<CinemachineFixedSignal>();
            _runtimeSignal.hideFlags = HideFlags.HideAndDontSave;
            _runtimeSignal.m_XCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1f, 0f));
            _runtimeSignal.m_YCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.25f, -1f),
                new Keyframe(0.5f, 1f),
                new Keyframe(0.75f, -0.5f),
                new Keyframe(1f, 0f));
            _runtimeSignal.m_ZCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1f, 0f));
            return _runtimeSignal;
        }
    }
}
