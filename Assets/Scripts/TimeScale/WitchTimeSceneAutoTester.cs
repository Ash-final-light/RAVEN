using System.Collections;
using Common;
using Common.Constant;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Game.TimeScale
{
    public class WitchTimeSceneAutoTester : MonoBehaviour
    {
        [SerializeField] private WitchTimeTestDispatcher caster;
        [SerializeField] private WitchTimeTestDispatcher target;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float worldScale = 0.1f;
        [SerializeField] private float selfScale = 1f;

        private bool _started;
        private bool _stopped;

        public void Configure(WitchTimeTestDispatcher casterDispatcher, WitchTimeTestDispatcher targetDispatcher)
        {
            caster = casterDispatcher;
            target = targetDispatcher;
        }

        private void OnEnable()
        {
            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null) return;

            eventCenter.AddEventListener<WitchTimeRuntimeInfo>(GlobalEvents.OnWitchTimeStarted, OnWitchTimeStarted);
            eventCenter.AddEventListener<WitchTimeRuntimeInfo>(GlobalEvents.OnWitchTimeStopped, OnWitchTimeStopped);
        }

        private void OnDisable()
        {
            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null) return;

            eventCenter.RemoveEventListener<WitchTimeRuntimeInfo>(GlobalEvents.OnWitchTimeStarted, OnWitchTimeStarted);
            eventCenter.RemoveEventListener<WitchTimeRuntimeInfo>(GlobalEvents.OnWitchTimeStopped, OnWitchTimeStopped);
        }

        private IEnumerator Start()
        {
            yield return null;

            if (caster == null || target == null)
            {
                Debug.LogError("[WitchTimeTest] Missing dispatcher reference.");
                yield break;
            }

            Debug.Log("[WitchTimeTest] Request sending.");
            GameApplication.Instance.EventCenter.TriggerEvent(
                GlobalEvents.OnWitchTimeRequested,
                new WitchTimeRequest
                {
                    commandId = "WitchTime_MinimalSceneTest",
                    sourceId = "WitchTimeSceneAutoTester",
                    casterId = caster.InstanceId,
                    duration = duration,
                    worldScale = worldScale,
                    selfScale = selfScale,
                    affectedType = TimeScaleType.All,
                    playPolicy = WitchTimePlayPolicy.RefreshDuration
                });

            yield return new WaitForSeconds(0.2f);

            bool activeScalePassed =
                _started &&
                Mathf.Approximately(caster.LogicScale, selfScale) &&
                Mathf.Approximately(target.LogicScale, worldScale);

            Debug.Log(activeScalePassed
                ? "[WitchTimeTest] ACTIVE_SCALE_PASS"
                : $"[WitchTimeTest] ACTIVE_SCALE_FAIL caster={caster.LogicScale}, target={target.LogicScale}, started={_started}");

            yield return new WaitForSeconds(duration + 0.3f);

            bool recoveryPassed =
                _stopped &&
                Mathf.Approximately(caster.LogicScale, 1f) &&
                Mathf.Approximately(target.LogicScale, 1f);

            Debug.Log(recoveryPassed
                ? "[WitchTimeTest] RECOVERY_PASS"
                : $"[WitchTimeTest] RECOVERY_FAIL caster={caster.LogicScale}, target={target.LogicScale}, stopped={_stopped}");

            Debug.Log(activeScalePassed && recoveryPassed
                ? "[WitchTimeTest] RESULT_PASS"
                : "[WitchTimeTest] RESULT_FAIL");
        }

        private void OnWitchTimeStarted(WitchTimeRuntimeInfo info)
        {
            _started = true;
            Debug.Log($"[WitchTimeTest] Started event received. commandId={info.commandId}");
        }

        private void OnWitchTimeStopped(WitchTimeRuntimeInfo info)
        {
            _stopped = true;
            Debug.Log($"[WitchTimeTest] Stopped event received. commandId={info.commandId}");
        }
    }
}
