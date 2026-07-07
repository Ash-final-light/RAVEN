using Tools.Camera;
using UnityEngine;

namespace Test
{
    public class ShakeTestTrigger : MonoBehaviour
    {
        [SerializeReference] public BaseCameraShakeData shakeData = new CameraShakeUniformData
        {
            duration = 0.3f,
            force = 1.5f,
            defaultVelocity = Vector3.up
        };

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                EnsureShakeData();
                shakeData.GenerateShake(transform.position);
                Debug.Log("[ShakeTest] Cinemachine impulse shake triggered.");
            }
        }

        private void OnValidate()
        {
            EnsureShakeData();
        }

        private void EnsureShakeData()
        {
            shakeData ??= new CameraShakeUniformData
            {
                duration = 0.3f,
                force = 1.5f,
                defaultVelocity = Vector3.up
            };
        }
    }
}
