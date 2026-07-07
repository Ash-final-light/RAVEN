using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Action.Data.Slide
{
    [RequireComponent(typeof(Collider))]
    public class SlideObjMono : MonoBehaviour
    {
        public event Action<SlideObjMono> OnSuccessSlide;

        private Collider _slideCollider;

        private bool _isSuccessTriggered = false;

        public bool IsJudging { get; private set; }

        private void Start()
        {
            EnsureCollider();
            SetJudgeActive(false);
            IsJudging = false;
        }

        [Button("Start slide judge")]
        public void StartCollideJudge()
        {
            EnsureCollider();
            _isSuccessTriggered = false;
            IsJudging = true;
            SetJudgeActive(true);
        }

        [Button("Stop slide judge")]
        public void StopCollideJudge()
        {
            EnsureCollider();
            _isSuccessTriggered = false;
            IsJudging = false;
            SetJudgeActive(false);
        }

        public bool HandleSuccessSlide()
        {
            if (!IsJudging)
            {
                Debug.Log("[PerfectDodge] Ignored because slide judge is not active.");
                return false;
            }

            if (_isSuccessTriggered)
            {
                Debug.Log("[PerfectDodge] Ignored because this slide has already triggered.");
                return false;
            }

            _isSuccessTriggered = true;
            Debug.Log("[PerfectDodge] Success slide triggered.");
            OnSuccessSlide?.Invoke(this);
            return true;
        }

        private void EnsureCollider()
        {
            if (_slideCollider == null)
            {
                _slideCollider = gameObject.GetComponent<Collider>();
            }
        }

        private void SetJudgeActive(bool active)
        {
            if (_slideCollider != null)
            {
                _slideCollider.enabled = active;
            }

            if (gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }
    }
}
