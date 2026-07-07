using System;
using UnityEngine;

namespace Tools.Action.Data.Collide
{
    public class BindSelfCollideMono:BindCollideMono
    {
        private Collider m_collider;
        private Action<Collider> m_callback;
        private int m_layerMask;
        public void SetParam(ActionCollideDetectionBaseData baseData)
        {
            m_collider = GetComponent<Collider>();
            if (m_collider == null)
                m_collider = gameObject.AddComponent<SphereCollider>();
            m_collider.isTrigger = true;
        }

        public void Detect(Action<Collider> callback, int layerMask)
        {
            m_callback = callback;
            m_collider.enabled = true;
            m_layerMask = layerMask;
        }

        public void CancelDetect()
        {
            m_callback = null;
            m_collider.enabled = false;
        }

        public override void DrawGizmos()
        {
            
        }

        private void OnCollisionEnter(Collision other)
        {
            m_callback?.Invoke(other.gameObject.GetComponent<Collider>());
        }

        private void OnCollisionStay(Collision other)
        {
            Debug.Log(other.gameObject.name);
            m_callback?.Invoke(other.gameObject.GetComponent<Collider>());
        }

        private void OnTriggerEnter(Collider other)
        {
            m_callback?.Invoke(other.gameObject.GetComponent<Collider>());
        }

        private void OnTriggerStay(Collider other)
        {
            Debug.Log(other.gameObject.name);
            m_callback?.Invoke(other.gameObject.GetComponent<Collider>());
        }

        
    }
}