using Tools.Battle;
using Tools.Battle.Data;
using Tools.Damage;
using UnityEngine;

namespace UI.UIHealth
{
    /// <summary>
    /// Binds battle resource snapshots to player HP and ultimate energy UI.
    /// UI stays display-only; battle data is owned by CharacterProperty.
    /// </summary>
    public class PlayerBattleResourceBinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSegmentedHealthBarDriver hpBar;
        [SerializeField] private PlayerUltimateEnergyBarDriver ultimateEnergyBar;

        [Header("Binding")]
        [SerializeField] private string playerCharacterId = "test1";
        [SerializeField] private bool bindFirstPlayerWhenIdEmpty = true;
        [SerializeField] private bool instantRefresh = true;

        [Header("Retry")]
        [SerializeField] private bool retryBindUntilFound = true;
        [SerializeField, Min(0.05f)] private float retryBindInterval = 0.1f;
        [SerializeField, Min(0f)] private float retryBindTimeout = 3f;

        private BattleManager battleManager;
        private CharacterProperty activePlayer;
        private float retryTimer;
        private float retryDeadline;

        public CharacterProperty ActivePlayer => activePlayer;

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
            AutoBindUi();
            SubscribeBattleManager();
            ResetRetryWindow();
            TryBindFromBattleManager();
            RefreshView(instantRefresh);
        }

        /*private void Update()
    {
        if (activePlayer != null || !retryBindUntilFound)
        {
            return;
        }

        if (retryBindTimeout > 0f && Time.unscaledTime > retryDeadline)
        {
            return;
        }

        retryTimer -= Time.unscaledDeltaTime;
        if (retryTimer > 0f)
        {
            return;
        }

        retryTimer = retryBindInterval;

        if (battleManager == null)
        {
            SubscribeBattleManager();
        }

        TryBindFromBattleManager();
        RefreshView(instantRefresh);
    }*/

        private void OnDisable()
        {
            UnsubscribeBattleManager();
        }

        public void Bind(CharacterProperty characterProperty)
        {
            activePlayer = characterProperty;
            if (activePlayer != null && !string.IsNullOrEmpty(activePlayer.gameObjId))
            {
                playerCharacterId = activePlayer.gameObjId;
            }

            RefreshView(true);
        }

        public void SetPlayerCharacterId(string characterId)
        {
            playerCharacterId = characterId;
            TryBindFromBattleManager();
            RefreshView(true);
        }

        private void SubscribeBattleManager()
        {
            battleManager = FindFirstObjectByType<BattleManager>();
            if (battleManager == null)
            {
                return;
            }

            battleManager.OnCharacterResourceChanged -= HandleCharacterResourceChanged;
            battleManager.OnCharacterResourceChanged += HandleCharacterResourceChanged;
            battleManager.OnCharacterAdded -= HandleCharacterAdded;
            battleManager.OnCharacterAdded += HandleCharacterAdded;
            battleManager.OnCharacterRemoved -= HandleCharacterRemoved;
            battleManager.OnCharacterRemoved += HandleCharacterRemoved;
        }

        private void UnsubscribeBattleManager()
        {
            if (battleManager == null)
            {
                return;
            }

            battleManager.OnCharacterResourceChanged -= HandleCharacterResourceChanged;
            battleManager.OnCharacterAdded -= HandleCharacterAdded;
            battleManager.OnCharacterRemoved -= HandleCharacterRemoved;
        }

        private void TryBindFromBattleManager()
        {
            if (battleManager == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(playerCharacterId) &&
                battleManager.Data.TryGetValue(playerCharacterId, out CharacterProperty configuredPlayer))
            {
                Bind(configuredPlayer);
                return;
            }

            if (!bindFirstPlayerWhenIdEmpty)
            {
                return;
            }

            foreach (CharacterProperty property in battleManager.Data.Values)
            {
                if (property != null && property.isPlayer)
                {
                    Bind(property);
                    return;
                }
            }
        }

        private void HandleCharacterResourceChanged(CharacterProperty property, DamageInfo damageInfo)
        {
            if (!IsActivePlayer(property))
            {
                return;
            }

            RefreshView(instantRefresh);
        }

        private void HandleCharacterAdded(CharacterProperty property)
        {
            if (property == null || !property.isPlayer || activePlayer != null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(playerCharacterId) && property.gameObjId != playerCharacterId)
            {
                return;
            }

            Bind(property);
        }

        private void HandleCharacterRemoved(string characterId)
        {
            if (activePlayer == null || activePlayer.gameObjId != characterId)
            {
                return;
            }

            activePlayer = null;
            TryBindFromBattleManager();
            RefreshView(true);
        }

        private void RefreshView(bool instant)
        {
            if (activePlayer == null)
            {
                return;
            }

            if (hpBar != null)
            {
                hpBar.SetOriginalHp(activePlayer.Info != null ? activePlayer.Info.Health : 1, false);
                hpBar.SetHpAndShield(activePlayer.CurrentHealth, activePlayer.CurrentShield, instant);
            }

            if (ultimateEnergyBar != null)
            {
                ultimateEnergyBar.SetEnergy(activePlayer.CurrentMp, Mathf.Max(1, activePlayer.CurrentMaxMp));
            }
        }

        private bool IsActivePlayer(CharacterProperty property)
        {
            if (property == null)
            {
                return false;
            }

            if (activePlayer == null)
            {
                TryBindFromBattleManager();
            }

            return activePlayer != null && property.gameObjId == activePlayer.gameObjId;
        }

        private void AutoBindUi()
        {
            if (hpBar == null)
            {
                hpBar = GetComponentInChildren<PlayerSegmentedHealthBarDriver>(true);
            }

            if (ultimateEnergyBar == null)
            {
                ultimateEnergyBar = GetComponentInChildren<PlayerUltimateEnergyBarDriver>(true);
            }
        }

        private void ResetRetryWindow()
        {
            retryTimer = 0f;
            retryDeadline = retryBindTimeout > 0f
                ? Time.unscaledTime + retryBindTimeout
                : float.PositiveInfinity;
        }
    }
}
