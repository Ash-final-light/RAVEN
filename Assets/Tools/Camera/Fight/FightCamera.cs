using Cinemachine;
using Sirenix.OdinInspector;
using Tools.InputHandle;
using UnityEngine;
using VContainer;

namespace Tools.Camera.Fight
{
    public class FightCamera : MonoBehaviour
    {
        [Title("Camera References")]
        public GameObject CamTPS;
        public GameObject CamLock;
        
        [Title("Settings")]
        public float farDis = 43f;
        public float nearDis = 2f;
        
        public float _valueMul = 30f;
        public float _fwdRotMul = 2f;
        
        [Title("Dependencies")]
        public Transform TargetRotRef;
        public GameObject TPSlockAssiant;

        // 缓存组件，避免 GetComponent 导致的 GC 和 CPU 开销
        private CinemachineFreeLook _freeLookTPS;
        private CinemachineVirtualCamera _vcamTPS;
        private CinemachineVirtualCamera _vcamLock;
        
        private PlayerInputManager inputManager => PlayerInputManager.Instance;

        [ShowInInspector, ReadOnly] 
        public bool isTPS = true;
        
        public Transform Target { get; private set; }
        public Transform PlayerTarget { get; private set; }
        
        private FightCamRot _camRotHandler;

        public bool IsTPS => isTPS;

        private void Awake()
        {
            // 预先缓存所有组件
            _freeLookTPS = CamTPS.GetComponent<CinemachineFreeLook>();
            _vcamTPS = CamTPS.GetComponent<CinemachineVirtualCamera>();
            _vcamLock = CamLock.GetComponent<CinemachineVirtualCamera>();
        }

        private void Start()
        {
            var brain = UnityEngine.Camera.main.GetComponent<CinemachineBrain>();
    
            _camRotHandler = new FightCamRot(brain, _freeLookTPS, this, inputManager);
            _camRotHandler.SetValueMul(_valueMul);
            _camRotHandler.SetFwdRotMul(_fwdRotMul);
            
            Target = null;
            SwitchCamToTPS(true);
        }

        private void Update()
        {
            // 只有在锁定模式且有目标时才进行距离检测
            if (!isTPS && Target)
            {
                // 更新辅助点位置
                if (TPSlockAssiant)
                    TPSlockAssiant.transform.position = Target.position;

                // 使用 sqrMagnitude 会更快，但为了保持逻辑一致沿用 Distance
                float dis = Vector3.Distance(transform.position, Target.position);

                if (dis > farDis || dis < nearDis)
                {
                    SwitchCamToTPS(true);
                }
            }
            
            // 如果是在 TPS 模式下同步坐标（原逻辑中有此需求）
            if (!isTPS && _freeLookTPS != null)
            {
                CamTPS.transform.SetPositionAndRotation(CamLock.transform.position, CamLock.transform.rotation);
            }
            _camRotHandler?.Tick();
        }

        public void LockEnemy(Transform enemy)
        {
            if (enemy == null)
            {
                SwitchCamToTPS(true);
                return;
            }

            Target = enemy;
            SwitchCamToTPS(false); // 锁定敌人时自动切换到锁定相机
        }
        public void SetPlayerTarget(Transform playerTarget)
        {
            _freeLookTPS.LookAt = playerTarget;
            _freeLookTPS.Follow = playerTarget;
            PlayerTarget = playerTarget;
        }

        [Button]
        public void SwitchCamToTPS(bool toTPS)
        {
            this.isTPS = toTPS;
            
            if (toTPS) Target = null;

            // 优化 Priority 设置逻辑
            int tpsPriority = toTPS ? 10 : 8;
            int lockPriority = toTPS ? 8 : 10;

            if (_freeLookTPS) _freeLookTPS.Priority = tpsPriority;
            if (_vcamTPS) _vcamTPS.Priority = tpsPriority;
            if (_vcamLock) _vcamLock.Priority = lockPriority;
        }

        public void SetNull()
        {
            Target = null;
        }
    }
}