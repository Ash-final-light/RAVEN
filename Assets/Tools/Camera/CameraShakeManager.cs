using Tools.DesignMode;
using UnityEngine;

namespace Tools.Camera
{
    public class CameraShakeManager : MonoSingleton<CameraShakeManager>
    {
        public void Shake(BaseCameraShakeData data)
        {
            if (data == null)
            {
                return;
            }

            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            Vector3 shakePosition = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
            data.GenerateShake(shakePosition);
        }
    }
}
