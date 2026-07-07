using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Action.Data.Collide
{
    public class BindSphereCollideMono:BindCollideMono,ICollideObj
    {
        protected float sphereRadius;
        [Button("设置参数")]
        public void SetParam(ActionCollideDetectionBaseData baseData)
        {
            var sphereData = baseData as ActionCollideDetectionSphereData;
            if (sphereData == null)
                return;
            sphereRadius = sphereData.radius;
            center = transform;
            localPosition = sphereData.localPosition;
            
        }
        public void SetParam(Transform center,ActionCollideDetectionBaseData baseData)
        {
            var sphereData = baseData as ActionCollideDetectionSphereData;
            if (sphereData == null)
                return;
            sphereRadius = sphereData.radius;
            this.center = center;
            localPosition = sphereData.localPosition;
            
        }

        public void Detect(Action<Collider> callback, int layerMask)
        {
            var result = Physics.OverlapSphere(Position, sphereRadius,layerMask);
            foreach (var item in result)
            {
                callback?.Invoke(item);
                Debug.Log(item.name);
            }
        }

        public void CancelDetect()
        {
            throw new NotImplementedException();
        }

        public override void DrawGizmos()
        {
            if (center == null)
                return;
            Gizmos.DrawWireSphere(center.position, sphereRadius);
        }

        public void ResetStatus()
        {
            center = null;
            localPosition = Vector3.zero;
            sphereRadius = 0;
            gameObject.SetActive(false);
        }
    }
}