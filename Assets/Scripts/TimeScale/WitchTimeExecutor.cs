using Common;
using Common.Constant;
using Framework.Core.Event;
using TimeScale;
using TimeScale.Command;
using UnityEngine;

namespace Game.TimeScale
{
    public class WitchTimeExecutor : MonoBehaviour
    {
        private WitchTimeRequest _activeRequest;
        private float _remainingTime;
        private bool _isActive;
        private bool _registered;

        public bool IsActive => _isActive;

        private void OnEnable()
        {
            TryRegister();
        }

        private void Start()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            TryUnregister();
            if (_isActive)
            {
                CancelActiveCommand();
                FinishWitchTime();
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            _remainingTime -= Time.deltaTime;
            if (_remainingTime <= 0f)
            {
                FinishWitchTime();
                return;
            }

            Broadcast(GlobalEvents.OnWitchTimeUpdated, CreateRuntimeInfo());
        }

        public void Stop()
        {
            if (!_isActive) return;

            CancelActiveCommand();
            FinishWitchTime();
        }

        private void TryRegister()
        {
            if (_registered) return;

            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null) return;

            eventCenter.AddEventListener<WitchTimeRequest>(
                GlobalEvents.OnWitchTimeRequested,
                HandleWitchTimeRequested);
            _registered = true;
        }

        private void TryUnregister()
        {
            if (!_registered) return;

            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter != null)
            {
                eventCenter.RemoveEventListener<WitchTimeRequest>(
                    GlobalEvents.OnWitchTimeRequested,
                    HandleWitchTimeRequested);
            }

            _registered = false;
        }

        private void HandleWitchTimeRequested(WitchTimeRequest request)
        {
            if (!request.IsValid)
            {
                Debug.LogWarning("[WitchTime] Invalid request.");
                return;
            }

            if (string.IsNullOrEmpty(request.commandId))
            {
                request.commandId = CreateDefaultCommandId(request);
            }

            if (_isActive)
            {
                ApplyActivePolicy(request);
                return;
            }

            StartWitchTime(request);
        }

        private void ApplyActivePolicy(WitchTimeRequest request)
        {
            switch (request.playPolicy)
            {
                case WitchTimePlayPolicy.IgnoreIfActive:
                    Debug.Log("[WitchTime] Request ignored because witch time is active.");
                    break;
                case WitchTimePlayPolicy.Restart:
                case WitchTimePlayPolicy.RefreshDuration:
                    CancelActiveCommand();
                    StartWitchTime(request);
                    break;
                case WitchTimePlayPolicy.ExtendDuration:
                    request.duration += _remainingTime;
                    CancelActiveCommand();
                    StartWitchTime(request);
                    break;
                default:
                    Debug.LogWarning($"[WitchTime] Unsupported play policy: {request.playPolicy}");
                    break;
            }
        }

        private void StartWitchTime(WitchTimeRequest request)
        {
            _activeRequest = request;
            _remainingTime = request.duration;
            _isActive = true;

            TimeScaleManager.Instance.AddCommand(new TimeScaleWitchCommand(
                request.commandId,
                request.duration,
                request.worldScale,
                request.selfScale,
                request.casterId,
                request.affectedType));

            var info = CreateRuntimeInfo();
            Broadcast(GlobalEvents.OnWitchTimeStarted, info);
            Debug.Log($"[WitchTime] Started. commandId={request.commandId}, casterId={request.casterId}, duration={request.duration}");
        }

        private void FinishWitchTime()
        {
            if (!_isActive) return;

            _remainingTime = 0f;
            var info = CreateRuntimeInfo();
            _activeRequest = default;
            _isActive = false;

            Broadcast(GlobalEvents.OnWitchTimeStopped, info);
            Debug.Log("[WitchTime] Stopped.");
        }

        private void CancelActiveCommand()
        {
            if (!_isActive || string.IsNullOrEmpty(_activeRequest.commandId)) return;

            TimeScaleManager.Instance.RemoveCommand(_activeRequest.commandId);
        }

        private WitchTimeRuntimeInfo CreateRuntimeInfo()
        {
            return new WitchTimeRuntimeInfo(
                _activeRequest.commandId,
                _activeRequest.sourceId,
                _activeRequest.casterId,
                _activeRequest.duration,
                Mathf.Max(0f, _remainingTime));
        }

        private static string CreateDefaultCommandId(WitchTimeRequest request)
        {
            return $"WitchTime_{request.sourceId}_{request.casterId}";
        }

        private static void Broadcast(EventIdentity eventIdentity, WitchTimeRuntimeInfo info)
        {
            GameApplication.Instance?.EventCenter?.TriggerEvent(eventIdentity, info);
        }
    }
}
