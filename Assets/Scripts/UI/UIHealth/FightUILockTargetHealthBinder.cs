using Tools.Battle;
using Tools.Battle.Data;
using Tools.Camera;
using Tools.Damage;
using UnityEngine;

namespace UI.UIHealth
{
    /// <summary>
    /// Binds the enemy HP UI to the player's latest hit target, falling back to the lock target.
    /// </summary>
    public class FightUILockTargetHealthBinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MainCameraController cameraController;
        [SerializeField] private EnemySegmentedHealthBarDriver enemyHpBar;

        [Header("Display")]
        [SerializeField] private string playerCharacterId = "test1";
        [SerializeField] private bool hideWhenNoTarget = true;
        [SerializeField, Min(0.05f)] private float fallbackRefreshInterval = 0.25f;

        private CharacterProperty currentLockTarget;
        private CharacterProperty lastPlayerHitTarget;
        private CharacterProperty displayTarget;
        private BattleManager battleManager;
        private float fallbackRefreshTimer;

        private void Reset()
        {
            AutoBindUi();
        }

        private void Awake()
        {
            AutoBindUi();
        }

        private void OnEnable()
        {
            ResolveCameraController();
            SubscribeCameraController();
            SubscribeBattleManager();
            currentLockTarget = GetCurrentLockTarget();
            SetDisplayTarget(ResolveDisplayTarget(), true);
        }

        private void OnDisable()
        {
            UnsubscribeCameraController();
            UnsubscribeBattleManager();
        }

        private void Update()
        {
            if (IsValidTarget(displayTarget))
            {
                return;
            }

            fallbackRefreshTimer -= Time.deltaTime;
            if (fallbackRefreshTimer > 0f)
            {
                return;
            }

            fallbackRefreshTimer = fallbackRefreshInterval;
            SetDisplayTarget(ResolveDisplayTarget(), true);
        }

        private void SetDisplayTarget(CharacterProperty target, bool instant)
        {
            if (enemyHpBar == null)
            {
                displayTarget = target;
                return;
            }

            if (IsSameTarget(displayTarget, target))
            {
                if (displayTarget != null)
                {
                    if (hideWhenNoTarget && !enemyHpBar.gameObject.activeSelf)
                    {
                        enemyHpBar.gameObject.SetActive(true);
                    }

                    enemyHpBar.SetName(GetDisplayName(displayTarget));
                    enemyHpBar.RefreshFromTarget(instant);
                }
                else if (hideWhenNoTarget)
                {
                    enemyHpBar.gameObject.SetActive(false);
                }

                return;
            }

            displayTarget = target;

            bool hasTarget = displayTarget != null;
            if (hideWhenNoTarget)
            {
                enemyHpBar.gameObject.SetActive(hasTarget);
            }

            if (!hasTarget)
            {
                return;
            }

            enemyHpBar.Bind(displayTarget);
            enemyHpBar.SetName(GetDisplayName(displayTarget));
            enemyHpBar.RefreshFromTarget(instant);
        }

        private CharacterProperty ResolveDisplayTarget()
        {
            if (IsValidTarget(lastPlayerHitTarget))
            {
                return lastPlayerHitTarget;
            }

            lastPlayerHitTarget = null;

            CharacterProperty nearestTarget = ResolveNearestEnemyTarget();
            if (IsValidTarget(nearestTarget))
            {
                return nearestTarget;
            }

            return IsValidTarget(currentLockTarget) ? currentLockTarget : null;
        }

        private CharacterProperty GetCurrentLockTarget()
        {
            return cameraController != null ? cameraController.CurrentLockTarget : null;
        }

    private string GetDisplayName(CharacterProperty target)
    {
        if (!string.IsNullOrEmpty(target.gameObjId))
        {
            return target.gameObjId;
        }

        return target.Info?.Name ?? string.Empty;
    }

        private void ResolveCameraController()
        {
            if (cameraController != null)
            {
                return;
            }

            cameraController = FindFirstObjectByType<MainCameraController>();
            SubscribeCameraController();
        }

        private void SubscribeCameraController()
        {
            if (cameraController != null)
            {
                cameraController.OnLockTargetChanged -= HandleLockTargetChanged;
                cameraController.OnLockTargetChanged += HandleLockTargetChanged;
            }
        }

        private void UnsubscribeCameraController()
        {
            if (cameraController != null)
            {
                cameraController.OnLockTargetChanged -= HandleLockTargetChanged;
            }
        }

        private void SubscribeBattleManager()
        {
            battleManager = FindFirstObjectByType<BattleManager>();
            if (battleManager != null)
            {
                battleManager.OnCharacterResourceChanged -= HandleCharacterResourceChanged;
                battleManager.OnCharacterResourceChanged += HandleCharacterResourceChanged;
                battleManager.OnCharacterRemoved -= HandleCharacterRemoved;
                battleManager.OnCharacterRemoved += HandleCharacterRemoved;
            }
        }

        private void UnsubscribeBattleManager()
        {
            if (battleManager != null)
            {
                battleManager.OnCharacterResourceChanged -= HandleCharacterResourceChanged;
                battleManager.OnCharacterRemoved -= HandleCharacterRemoved;
            }
        }

        private void HandleLockTargetChanged(CharacterProperty target)
        {
            if (target == null)
            {
                return;
            }

            currentLockTarget = target;

            if (!IsValidTarget(lastPlayerHitTarget) && !IsValidTarget(displayTarget))
            {
                SetDisplayTarget(ResolveDisplayTarget(), true);
            }
        }

        private void HandleCharacterResourceChanged(CharacterProperty target, DamageInfo damageInfo)
        {
            if (target == null || enemyHpBar == null)
            {
                return;
            }

            if (IsPlayerHitEnemy(target, damageInfo))
            {
                lastPlayerHitTarget = target;
                SetDisplayTarget(ResolveDisplayTarget(), false);
                return;
            }

            if (IsSameTarget(target, displayTarget))
            {
                enemyHpBar.SetName(GetDisplayName(displayTarget));
                enemyHpBar.RefreshFromTarget(false);
            }
        }

        private void HandleCharacterRemoved(string characterId)
        {
            if (lastPlayerHitTarget != null && lastPlayerHitTarget.gameObjId == characterId)
            {
                lastPlayerHitTarget = null;
            }

            if (currentLockTarget != null && currentLockTarget.gameObjId == characterId)
            {
                currentLockTarget = null;
            }

            if (displayTarget != null && displayTarget.gameObjId == characterId)
            {
                SetDisplayTarget(ResolveDisplayTarget(), true);
            }
        }

        private bool IsPlayerHitEnemy(CharacterProperty target, DamageInfo damageInfo)
        {
            return target != null
                   && !target.isPlayer
                   && !string.IsNullOrEmpty(damageInfo.AttackerId)
                   && !string.IsNullOrEmpty(damageInfo.TargetId)
                   && damageInfo.AttackerId == playerCharacterId
                   && damageInfo.TargetId == target.gameObjId
                   && !damageInfo.Type.IsHeal();
        }

        private bool IsValidTarget(CharacterProperty target)
        {
            if (target == null || target.targetTransform == null)
            {
                return false;
            }

            if (battleManager == null)
            {
                return true;
            }

            return battleManager.Data.ContainsKey(target.gameObjId);
        }

        private CharacterProperty ResolveNearestEnemyTarget()
        {
            ResolveCameraController();
            return cameraController != null ? cameraController.ResolveAttackTarget(false) : null;
        }

        private bool IsSameTarget(CharacterProperty a, CharacterProperty b)
        {
            if (a == null || b == null)
            {
                return a == b;
            }

            return a.gameObjId == b.gameObjId;
        }

        private void AutoBindUi()
        {
            if (enemyHpBar == null)
            {
                Transform enemyStatus = transform.Find("TopLayer/EnemyStatus");
                if (enemyStatus == null)
                {
                    GameObject enemyStatusObject = GameObject.Find("BattleHUDCanvas/TopLayer/EnemyStatus");
                    enemyStatus = enemyStatusObject != null ? enemyStatusObject.transform : null;
                }

                if (enemyStatus != null)
                {
                    enemyHpBar = enemyStatus.GetComponent<EnemySegmentedHealthBarDriver>();
                }
            }
        }
    }
}
