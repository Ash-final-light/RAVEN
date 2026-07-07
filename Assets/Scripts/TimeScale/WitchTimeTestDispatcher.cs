using TimeScale;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Game.TimeScale
{
    public class WitchTimeTestDispatcher : MonoBehaviour, ITimeScaleDispatcher
    {
        [SerializeField] private string instanceId = "witch_test_target";

        private float _logicScale = 1f;
        private float _bulletScale = 1f;
        private float _effectScale = 1f;
        private float _audioScale = 1f;

        public string InstanceId => instanceId;
        public string Tag => gameObject.tag;
        public float LogicScale => _logicScale;

        public void Configure(string id)
        {
            instanceId = id;
        }

        private void Awake()
        {
            TimeScaleManager.Instance.Register(this);
            Debug.Log($"[WitchTimeTest] Dispatcher registered. id={instanceId}");
        }

        public void ApplyScale(TimeScaleType type, float scale)
        {
            switch (type)
            {
                case TimeScaleType.Bullet:
                    if (Mathf.Approximately(_bulletScale, scale)) return;
                    _bulletScale = scale;
                    break;
                case TimeScaleType.Effect:
                    if (Mathf.Approximately(_effectScale, scale)) return;
                    _effectScale = scale;
                    break;
                case TimeScaleType.Audio:
                    if (Mathf.Approximately(_audioScale, scale)) return;
                    _audioScale = scale;
                    break;
                case TimeScaleType.Logic:
                    if (Mathf.Approximately(_logicScale, scale)) return;
                    _logicScale = scale;
                    break;
                default:
                    return;
            }

            Debug.Log($"[WitchTimeTest] Scale changed. id={instanceId}, type={type}, scale={scale}");
        }
    }
}
