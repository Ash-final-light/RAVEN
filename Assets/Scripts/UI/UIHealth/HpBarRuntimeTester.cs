using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.UIHealth
{
    public class HpBarRuntimeTester : MonoBehaviour
    {
        [SerializeField] private PlayerSegmentedHealthBarDriver playerHpBar;
        [SerializeField] private EnemySegmentedHealthBarDriver enemySegmentedHpBar;

        [Header("Player Test Values")]
        [SerializeField] private float playerOriginalHp = 6854f;
        [SerializeField] private float playerTestHp = 4200f;
        [SerializeField] private float playerTestShield = 1600f;
        [SerializeField] private float playerDamage = 600f;

        [Header("Enemy Test Values")]
        [SerializeField] private string enemyName = "Enemy";
        [SerializeField] private float enemyOriginalHp = 1000f;
        [SerializeField] private float enemyTestHp = 620f;
        [SerializeField] private float enemyTestShield = 260f;
        [SerializeField] private float enemyDamage = 180f;

        private void Reset()
        {
            AutoBind();
        }

        private void Awake()
        {
            AutoBind();
        }

        private void Start()
        {
            ApplyInitialValues();
        }

        [Button("Apply Initial Values")]
        [ContextMenu("Apply Initial Values")]
        public void ApplyInitialValues()
        {
            ApplyPlayerValues();
            ApplyEnemyValues();
        }

        [Button("Apply Player Values")]
        public void ApplyPlayerValues()
        {
            if (playerHpBar != null)
            {
                playerHpBar.SetOriginalHp(playerOriginalHp, false);
                playerHpBar.SetHpAndShield(playerTestHp, playerTestShield);
            }
        }

        [Button("Apply Enemy Values")]
        public void ApplyEnemyValues()
        {
            if (enemySegmentedHpBar != null)
            {
                enemySegmentedHpBar.SetSegmentHp(100f, false);
                enemySegmentedHpBar.SetHpAndShield(enemyTestHp, enemyTestShield);
            }
        }

        [Button("Damage Player")]
        public void DamagePlayer()
        {
            if (playerHpBar != null)
            {
                playerHpBar.TakeDamage(playerDamage);
            }
        }

        [Button("Damage Enemy")]
        public void DamageEnemy()
        {
            if (enemySegmentedHpBar != null)
            {
                ApplyDamageToEnemyDriver(enemyDamage);
            }
        }

        [Button("Damage Both")]
        [ContextMenu("Damage Both")]
        public void DamageBoth()
        {
            DamagePlayer();
            DamageEnemy();
        }

        [Button("Set Player Shield")]
        public void SetPlayerShield()
        {
            if (playerHpBar != null)
            {
                playerHpBar.SetShield(playerTestShield);
            }
        }

        [Button("Set Enemy Shield")]
        public void SetEnemyShield()
        {
            if (enemySegmentedHpBar != null)
            {
                enemySegmentedHpBar.SetShield(enemyTestShield);
            }
        }

        [Button("Max Shield Both")]
        [ContextMenu("Max Shield Both")]
        public void MaxShieldBoth()
        {
            if (playerHpBar != null)
            {
                playerHpBar.SetHpAndShield(playerOriginalHp * 0.45f, playerOriginalHp);
            }

            if (enemySegmentedHpBar != null)
            {
                enemySegmentedHpBar.SetHpAndShield(enemyOriginalHp * 0.45f, enemyOriginalHp);
            }
        }

        [Button("Refill Player")]
        public void RefillPlayer()
        {
            if (playerHpBar != null)
            {
                playerHpBar.SetOriginalHp(playerOriginalHp, true);
            }
        }

        [Button("Refill Enemy")]
        public void RefillEnemy()
        {
            if (enemySegmentedHpBar != null)
            {
                enemySegmentedHpBar.SetHpAndShield(enemyOriginalHp, 0f);
            }
        }

        [Button("Refill Both")]
        [ContextMenu("Refill Both")]
        public void RefillBoth()
        {
            RefillPlayer();
            RefillEnemy();
        }

        [Button("Zero HP With Shield")]
        [ContextMenu("Zero HP With Shield")]
        public void BreakHpKeepShield()
        {
            if (playerHpBar != null)
            {
                playerHpBar.SetHpAndShield(0f, playerOriginalHp * 0.2f);
            }

            if (enemySegmentedHpBar != null)
            {
                enemySegmentedHpBar.SetHpAndShield(0f, enemyOriginalHp * 0.2f);
            }
        }
    
        [Button("Set Enemy Name")]
        public void SetEnemyName()
        {
            if (enemySegmentedHpBar != null)
            {
                enemySegmentedHpBar.SetName(enemyName);
            }
        }

        private void AutoBind()
        {
            if (playerHpBar == null)
            {
                GameObject playerStatus = GameObject.Find("BattleHUDCanvas/TopLayer/PlayerStatus");
                if (playerStatus != null)
                {
                    playerHpBar = playerStatus.GetComponent<PlayerSegmentedHealthBarDriver>();
                }
            }

            if (enemySegmentedHpBar == null)
            {
                GameObject enemyStatus = GameObject.Find("BattleHUDCanvas/TopLayer/EnemyStatus");
                if (enemyStatus != null)
                {
                    enemySegmentedHpBar = enemyStatus.GetComponent<EnemySegmentedHealthBarDriver>();
                }
            }
        }

        private void ApplyDamageToEnemyDriver(float damage)
        {
            damage = Mathf.Max(0f, damage);

            float shieldDamage = Mathf.Min(enemySegmentedHpBar.CurrentShield, damage);
            float remainingShield = enemySegmentedHpBar.CurrentShield - shieldDamage;
            float remainingDamage = damage - shieldDamage;
            float remainingHp = Mathf.Max(0f, enemySegmentedHpBar.CurrentHp - remainingDamage);

            enemySegmentedHpBar.SetHpAndShield(remainingHp, remainingShield);
        }
    }
}
