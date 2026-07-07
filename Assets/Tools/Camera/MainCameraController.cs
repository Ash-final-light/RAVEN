using System;
using Common.Constant;
using Sirenix.OdinInspector;
using Tools.Battle;
using Tools.Battle.Data;
using Tools.Camera.Fight;
using Tools.InputHandle;
using Tools.InputHandle.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using PlayerInputManager = Tools.InputHandle.PlayerInputManager;

namespace Tools.Camera
{
    /// <summary>
    /// 挂在主相机上，用于控制主相机的属性
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera), typeof(AudioListener))]
    public class MainCameraController : MonoBehaviour
    {
        [ShowInInspector]private LockEnemy _lockEnemy;
        [ShowInInspector]private FightCamera _fightCameraConfig;
        private PlayerInputManager _playerInputManager => PlayerInputManager.Instance;
        public float ValueMul = 30f;

        public CharacterProperty CurrentPlayer { get; private set; }
        public CharacterProperty CurrentLockTarget { get; private set; }
        public event Action<CharacterProperty> OnLockTargetChanged;
        
        private void Start()
        {
            _lockEnemy = new LockEnemy();
            _fightCameraConfig = GetComponent<FightCamera>();
            RegisterInputActions();
            OnPlayerStart();
        }

        private void OnDestroy()
        {
            UnregisterInputActions();
        }

        private void Update()
        {
            EnsureCurrentPlayerBinding();

            if (_fightCameraConfig != null && _fightCameraConfig.IsTPS && CurrentLockTarget != null)
            {
                SetCurrentLockTarget(null);
            }
        }

        private void RegisterInputActions()
        {
            _playerInputManager?.RegisterActionPerformed(InputConstants.PlayerLock, LockAction);
        }

        private void UnregisterInputActions()
        {
            if (PlayerInputManager.HasInstance)
            {
                PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.PlayerLock, LockAction);
            }
        }

        private void LockAction(InputAction.CallbackContext obj)
        {
            var lockEnemy = ResolveAttackTarget(true);
            if (lockEnemy?.targetTransform == null)
            {
                return;
            }

            _fightCameraConfig.LockEnemy(lockEnemy.targetTransform);
        }

        public CharacterProperty ResolveAttackTarget(bool syncAsCurrentLockTarget = true)
        {
            if (!EnsureCurrentPlayerBinding())
            {
                if (syncAsCurrentLockTarget)
                {
                    SetCurrentLockTarget(null);
                }

                return null;
            }

            var lockEnemy = _lockEnemy.GetLockTargetProperty();
            if (syncAsCurrentLockTarget)
            {
                SetCurrentLockTarget(lockEnemy);
            }

            return lockEnemy;
        }


        /// <summary>
        /// TODO: 玩家切换时，更新锁定敌人，使用事件驱动机制
        /// </summary>
        /// <param name="player"></param>
        private void OnPlayerStart()
        {
            CharacterProperty player = FindPlayerFromBattleManager();

            if (player?.targetTransform == null)
            {
                Debug.LogWarning("[MainCameraController] Player target is missing, lock camera setup skipped.");
                return;
            }

            CurrentPlayer = player;
            BindCurrentPlayerToCameraAndLock();
        }

        private void SetCurrentLockTarget(CharacterProperty target)
        {
            if (CurrentLockTarget == target)
            {
                SyncPlayerLockData(target);
                return;
            }

            CurrentLockTarget = target;
            SyncPlayerLockData(target);
            OnLockTargetChanged?.Invoke(CurrentLockTarget);
        }

        private void SyncPlayerLockData(CharacterProperty target)
        {
            if (BattleManager.Instance == null)
            {
                return;
            }

            if (!IsRegisteredPlayer(CurrentPlayer))
            {
                EnsureCurrentPlayerBinding();
            }

            if (CurrentPlayer == null)
            {
                return;
            }

            if (!BattleManager.Instance.Controllers.TryGetValue(CurrentPlayer.gameObjId, out var playerController) ||
                playerController == null)
            {
                return;
            }

            ActionStateController targetController = null;
            if (target != null)
            {
                BattleManager.Instance.Controllers.TryGetValue(target.gameObjId, out targetController);
            }

            playerController.PlayerParameters.cameraLockData.@lock = targetController != null;
            playerController.PlayerParameters.cameraLockData.lockTarget = targetController;
        }

        private bool EnsureCurrentPlayerBinding()
        {
            if (!IsRegisteredPlayer(CurrentPlayer))
            {
                CurrentPlayer = FindPlayerFromBattleManager();
            }

            if (CurrentPlayer?.targetTransform == null)
            {
                return false;
            }

            BindCurrentPlayerToCameraAndLock();
            return true;
        }

        private void BindCurrentPlayerToCameraAndLock()
        {
            if (_lockEnemy == null)
            {
                _lockEnemy = new LockEnemy();
            }

            if (_fightCameraConfig == null)
            {
                _fightCameraConfig = GetComponent<FightCamera>();
            }

            _lockEnemy.SetPlayerTarget(CurrentPlayer);
            _fightCameraConfig?.SetPlayerTarget(CurrentPlayer.targetTransform);
        }

        private bool IsRegisteredPlayer(CharacterProperty player)
        {
            if (player == null || player.targetTransform == null || !player.isPlayer)
            {
                return false;
            }

            if (BattleManager.Instance == null)
            {
                return false;
            }

            return BattleManager.Instance.Controllers.ContainsKey(player.gameObjId);
        }

        private CharacterProperty FindPlayerFromBattleManager()
        {
            if (BattleManager.Instance == null)
            {
                return null;
            }

            foreach (var character in BattleManager.Instance.Data.Values)
            {
                if (character != null && character.isPlayer && character.targetTransform != null)
                {
                    return character;
                }
            }

            return null;
        }
    }
}
