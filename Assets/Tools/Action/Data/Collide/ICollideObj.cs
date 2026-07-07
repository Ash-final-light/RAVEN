using UnityEngine;

namespace Tools.Action.Data.Collide
{
    public interface ICollideObj
    {
        void SetParam(ActionCollideDetectionBaseData baseData);
        void Detect(System.Action<Collider> callback,int layerMask);
        void CancelDetect();
        void DrawGizmos();
        void ResetStatus();
    }
}