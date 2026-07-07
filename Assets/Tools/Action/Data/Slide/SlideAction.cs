using Common;
using Common.Constant;
using Game.TimeScale;
using Tools.Action.Data.Collide;
using Tools.Battle.Data;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Action.Data.Slide
{
    public class SlideAction
    {
        private readonly SlideObjMono _slideObjMono;
        private readonly Transform _ownerTransform;
        private readonly SlideConfigSo _slideConfigSo;
        private readonly ActionStateController _owner;

        private bool _isSlide = false;
        private SlideLimiter _slideLimiter;
        private float _witchTimeCooldownTimer;

        public SlideAction(
            SlideObjMono slideObjMono,
            Transform ownerTransform,
            SlideConfigSo slideConfigSo,
            ActionStateController owner)
        {
            if (slideObjMono == null)
            {
                Debug.LogError("[SlideAction] slideObjMono is null.");
                return;
            }

            _slideObjMono = slideObjMono;
            _ownerTransform = ownerTransform;
            _slideConfigSo = slideConfigSo;
            _owner = owner;

            string tagValue = CollideConstants.Slide;
            Debug.Log($"[SlideAction] Set slide judge tag '{tagValue}' to {_slideObjMono.gameObject.name}.");

            if (string.IsNullOrEmpty(tagValue))
            {
                Debug.LogError("[SlideAction] CollideConstants.Slide is empty.");
                return;
            }

            try
            {
                _slideObjMono.gameObject.tag = tagValue;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SlideAction] Failed to set slide judge tag '{tagValue}'. Error: {e.Message}");
                return;
            }

            if (slideConfigSo == null)
            {
                Debug.LogError("[SlideAction] slideConfigSo is null.");
                return;
            }

            _slideLimiter = new SlideLimiter(
                slideConfigSo._maxSlides,
                slideConfigSo._windowTime,
                slideConfigSo._lockDuration);

            _slideObjMono.OnSuccessSlide += HandlePerfectDodge;
        }

        public bool CanSlide()
        {
            return _slideLimiter != null && _slideLimiter.CanSlide();
        }

        public void StartJudge(float maxTime)
        {
            RecordSlide();
            OpenJudge();
            //Debug.Log("[PerfectDodge] Judge started by legacy StartJudge call. Close now depends on ActionClip event.");
        }

        public void Tick(float deltaTime)
        {
            if (_witchTimeCooldownTimer > 0f)
            {
                _witchTimeCooldownTimer = Mathf.Max(0f, _witchTimeCooldownTimer - deltaTime);
            }
        }

        public void RecordSlide()
        {
            _slideLimiter?.RecordSlide();
        }

        public void OpenJudge()
        {
            ExecuteCollideJudge();
            //Debug.Log("[PerfectDodge] Judge opened by ActionClip event.");
        }

        public void CloseJudge()
        {
            StopCollideJudge();
            //Debug.Log("[PerfectDodge] Judge closed by ActionClip event.");
        }

        public void ForceCloseJudge()
        {
            StopCollideJudge();
        }

        public void Dispose()
        {
            if (_slideObjMono != null)
            {
                _slideObjMono.OnSuccessSlide -= HandlePerfectDodge;
            }
        }

        protected virtual void ExecuteCollideJudge()
        {
            if (_slideObjMono == null || _ownerTransform == null)
            {
                return;
            }

            _isSlide = true;
            _slideObjMono.StartCollideJudge();
        }

        protected virtual void StopCollideJudge()
        {
            if (_slideObjMono == null || _ownerTransform == null)
            {
                return;
            }

            _isSlide = false;
            _slideObjMono.StopCollideJudge();
            SetPerfectDodgeImmune(false);
        }

        protected virtual void HandlePerfectDodge(SlideObjMono slideObj)
        {
            if (_owner == null || _slideConfigSo == null)
            {
                //Debug.LogWarning("[PerfectDodge] Missing owner or slide config.");
                return;
            }

            SetPerfectDodgeImmune(true);

            if (!_slideConfigSo.enablePerfectDodgeWitchTime)
            {
                //Debug.Log("[PerfectDodge] Witch time disabled by config.");
                return;
            }

            if (_witchTimeCooldownTimer > 0f)
            {
                //Debug.Log($"[PerfectDodge] Witch time ignored by cooldown. remaining={_witchTimeCooldownTimer:0.00}");
                return;
            }

            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null)
            {
                Debug.LogWarning("[PerfectDodge] EventCenter is missing, witch time request skipped.");
                return;
            }

            eventCenter.TriggerEvent(
                GlobalEvents.OnWitchTimeRequested,
                new WitchTimeRequest
                {
                    commandId = $"PerfectDodge_{_owner.characterId}",
                    sourceId = "PerfectDodge",
                    casterId = _owner.characterId,
                    duration = _slideConfigSo.witchTimeDuration,
                    worldScale = _slideConfigSo.witchSlowScale,
                    selfScale = _slideConfigSo.witchSelfScale,
                    affectedType = _slideConfigSo.witchAffectedType,
                    playPolicy = WitchTimePlayPolicy.RefreshDuration
                });

            _witchTimeCooldownTimer = Mathf.Max(0f, _slideConfigSo.witchCooldown);

            /*Debug.Log(
                $"[PerfectDodge] Witch time requested. caster={_owner.characterId}, " +
                $"duration={_slideConfigSo.witchTimeDuration:0.00}, " +
                $"slow={_slideConfigSo.witchSlowScale:0.00}, self={_slideConfigSo.witchSelfScale:0.00}");*/
        }

        private void SetPerfectDodgeImmune(bool active)
        {
            CharacterProperty property = _owner?.CharacterProperty;
            if (property == null)
            {
                return;
            }

            property.immune = active;

            if (property is PlayerProperty player)
            {
                player.inPerfectSlide = active;
            }

            //Debug.Log($"[PerfectDodge] Immune set to {active}. owner={_owner.characterId}");
        }
    }
}
