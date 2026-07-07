using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Action.Data.Collide
{
    public abstract class BindCollideMono:MonoBehaviour
    {
        [ShowInInspector]protected Transform center;
        [ShowInInspector] protected Vector3 localPosition;
        protected Quaternion localRotation =  Quaternion.identity;

        protected Quaternion Rotation
        {
            get
            {
                return center.rotation * localRotation;
            }
        }

        protected Vector3 Position
        {
            get
            {
                return center.position + localPosition;
            }
        }
        private void OnDrawGizmos()
        {
            DrawGizmos();
        }

        public abstract void DrawGizmos();
        
    }
}