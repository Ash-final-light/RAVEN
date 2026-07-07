using Sirenix.OdinInspector;
using Tools.Battle;
using Tools.Battle.Data;
using Tools.Damage;
using Tools.Damage.SO;
using UnityEngine;

namespace Test
{
    public class TestDamgeHandle : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private TestDamageDummyController attacker;
        [SerializeField] private TestDamageDummyController target;
        [SerializeField] private DamageHitConfigSO damageHitConfig;

        [Header("Character")]
        [SerializeField] private string attackerId = "popup_test_attacker";
        [SerializeField] private string targetId = "popup_test_target";
        [SerializeField] private int attackerInfoId = 3001;
        [SerializeField] private int targetInfoId = 3001;
        [SerializeField] private int maxHealth = 1000;
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private int defense = 0;

        [Header("Damage")]
        [SerializeField] private DamageType damageType = DamageType.DirectDamage;
        [SerializeField] private DamageValue damageValue = new()
        {
            noType = 10
        };
        [SerializeField, Range(0f, 1f)] private float criticalRate = 0.35f;
        [SerializeField] private bool autoJoinOnStart = true;
        [SerializeField] private bool testOnStart;

        private bool _hasJoined;

        private void Reset()
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        private void Awake()
        {
            if (battleManager == null)
            {
                battleManager = FindFirstObjectByType<BattleManager>();
            }
        }

        private void Start()
        {
            if (autoJoinOnStart)
            {
                JoinTestCharacters();
            }

            if (testOnStart)
            {
                ApplyTestDamage();
            }
        }

        [Button("Join Test Characters")]
        public void JoinTestCharacters()
        {
            if (_hasJoined)
            {
                return;
            }

            if (!ValidateReferences())
            {
                return;
            }

            attacker.Configure(attackerId, attackerInfoId, false);
            target.Configure(targetId, targetInfoId, false);

            RegisterCharacter(attackerId, attackerInfoId, attacker);
            RegisterCharacter(targetId, targetInfoId, target);
            _hasJoined = true;
        }

        [Button("Apply Test Damage")]
        public void ApplyTestDamage()
        {
            if (!_hasJoined)
            {
                JoinTestCharacters();
            }

            if (!ValidateReferences())
            {
                return;
            }

            if (damageHitConfig != null)
            {
                battleManager.AddDamage(attackerId, targetId, damageHitConfig, criticalRate);
                return;
            }

            var runtimeDamageConfig = ScriptableObject.CreateInstance<DamageHitConfigSO>();
            runtimeDamageConfig.hitDamageType = damageType;
            runtimeDamageConfig.hitDamage = damageValue;
            battleManager.AddDamage(attackerId, targetId, runtimeDamageConfig, criticalRate);
            Destroy(runtimeDamageConfig);
        }

        [Button("Show Popup Only")]
        public void ShowPopupOnly()
        {
            if (target == null)
            {
                return;
            }

            var damageInfo = new DamageInfo
            {
                TargetId = targetId,
                Type = damageType,
                Resource = DamageInfo.CalculateResource(damageValue),
                IsCritical = criticalRate >= 1f
            };

            //PopupTextManager.Instance.ShowDamage(damageInfo, target.transform);
        }

        private bool ValidateReferences()
        {
            if (battleManager == null)
            {
                Debug.LogError("[TestDamgeHandle] BattleManager is missing.");
                return false;
            }

            if (attacker == null || target == null)
            {
                Debug.LogError("[TestDamgeHandle] Attacker or target is missing.");
                return false;
            }

            return true;
        }

        private void RegisterCharacter(string id, int infoId, TestDamageDummyController controller)
        {
            var property = new CharacterProperty
            {
                Info = new CharacterInfoData
                {
                    Id = infoId,
                    Name = id,
                    Health = maxHealth,
                    CritRate = 0,
                    Damage = baseDamage,
                    Defense = defense,
                    Speed = 0
                },
                gameObjId = id,
                CurrentHealth = maxHealth,
                CurrentDamage = baseDamage,
                CurrentCritRate = 0,
                CurrentDefense = defense,
                CurrentSpeed = 0,
                CurrentShield = 0,
                CurrentMp = 0,
                CurrentBreak = 0,
                dead = false,
                isActive = true,
                isPlayer = false,
                targetTransform = controller.transform,
                OnCharacterDie = characterId => Debug.Log($"[TestDamgeHandle] {characterId} dead."),
                OnCharacterBreak = characterId => Debug.Log($"[TestDamgeHandle] {characterId} broken.")
            };

            battleManager.Data[id] = property;
            battleManager.Controllers[id] = controller;
        }
    }

}
