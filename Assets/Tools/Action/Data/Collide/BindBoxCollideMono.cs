using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Action.Data.Collide
{
    public class BindBoxCollideMono:BindCollideMono,ICollideObj
    {
        protected Vector3 boxSize;

        //[Button("设置参数")]
        public void SetParam(ActionCollideDetectionBaseData baseData)
        {
            var boxData = baseData as ActionCollideDetectionBoxData;
            if (boxData == null)
                return;
            boxSize = boxData.size;
            center = transform;
            localPosition = boxData.localPosition;
            localRotation = boxData.localRotation;
            
        }
        public void SetParam(Transform center,ActionCollideDetectionBaseData baseData)
        {
            var boxData = baseData as ActionCollideDetectionBoxData;
            if (boxData == null)
                return;
            boxSize = boxData.size;
            this.center = center;
            this.localPosition = boxData.localPosition;
            this.localRotation = boxData.localRotation;
            
        }
        //[Button("检测")]
        public void Detect(Action<Collider> callback,int layerMask)
        {
            var result = Physics.OverlapBox(Position, boxSize / 2, Rotation,layerMask);
            foreach (var item in result)
            {
                callback?.Invoke(item);
                Debug.Log(item.name);
            }
        }

        public void CancelDetect()
        {
           
        }

        private void OnDrawGizmos()
        {
            DrawGizmos();
        }
        
        public override void DrawGizmos()
        {
            if (center == null)
                return;
            var matrix = new Matrix4x4();
            matrix.SetTRS(
                Position,
                Rotation,
                Vector3.one
            );
            Gizmos.matrix = matrix;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }

        public void ResetStatus()
        {
            center = null;
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
            boxSize = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
}