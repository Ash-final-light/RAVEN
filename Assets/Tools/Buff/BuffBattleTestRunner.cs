using System.Collections;
using System.Reflection;
using Sirenix.OdinInspector;
using Tools.Damage;
using UnityEngine;
using BattleManager = Tools.Battle.BattleManager;
using LuaManager = Common.LuaManager;

namespace Tools.Buff
{
    public class BuffBattleTestRunner : MonoBehaviour
    {
        [SerializeField] private string attackerId = "buff_test_attacker";
        [SerializeField] private string targetId = "buff_test_target";
        [SerializeField] private int attackerInfoId = 3001;
        [SerializeField] private int targetInfoId = 3001;
        [SerializeField] private int testBuffId = 2099;
        [SerializeField] private bool runOnStart = true;

        private BuffManager buffManager;
        private BattleManager battleManager;
        private LuaManager localLuaManager;
        private bool usingLocalBuffManager;

        private IEnumerator Start()
        {
            yield return null;

            battleManager = BattleManager.Instance ?? FindObjectOfType<BattleManager>();
            buffManager = ResolveBuffManager(battleManager);
            buffManager ??= CreateLocalBuffManager();

            if (runOnStart)
            {
                RunSmokeTest();
            }
        }

        [Button("Run Buff Smoke Test")]
        public void RunSmokeTest()
        {
            battleManager ??= BattleManager.Instance ?? FindObjectOfType<BattleManager>();
            buffManager ??= ResolveBuffManager(battleManager);
            buffManager ??= CreateLocalBuffManager();

            if (battleManager == null)
            {
                Debug.LogError("[BuffBattleTestRunner] BattleManager not found.");
                return;
            }

            if (buffManager == null)
            {
                Debug.LogError("[BuffBattleTestRunner] BuffManager not resolved. Check VContainer injection in BuffTest.");
                return;
            }

            EnsureCharacters();

            Debug.Log("[BuffBattleTestRunner] Create attacker buff stack=1 and test OnHit.");
            buffManager.CreateBuff(attackerId, attackerId, testBuffId, 1);
            TriggerDamageCallbacks("attacker stack 1", hit: true, beHurt: false, kill: false, die: false);

            Debug.Log("[BuffBattleTestRunner] Stack attacker buff to stack=2 and test OnHit.");
            buffManager.CreateBuff(attackerId, attackerId, testBuffId, 1);
            TriggerDamageCallbacks("attacker stack 2", hit: true, beHurt: false, kill: false, die: false);

            Debug.Log("[BuffBattleTestRunner] Stack attacker buff to stack=3 and test OnHit.");
            buffManager.CreateBuff(attackerId, attackerId, testBuffId, 1);
            TriggerDamageCallbacks("attacker stack 3", hit: true, beHurt: false, kill: true, die: false);

            Debug.Log("[BuffBattleTestRunner] Try stacking attacker buff beyond MaxStack=3.");
            buffManager.CreateBuff(attackerId, attackerId, testBuffId, 1);
            TriggerDamageCallbacks("attacker stack capped", hit: true, beHurt: false, kill: false, die: false);

            Debug.Log("[BuffBattleTestRunner] Create target buff stack=1/2/3 and test OnBeHurt.");
            buffManager.CreateBuff(attackerId, targetId, testBuffId, 1);
            TriggerDamageCallbacks("target stack 1", hit: false, beHurt: true, kill: false, die: false);

            buffManager.CreateBuff(attackerId, targetId, testBuffId, 1);
            TriggerDamageCallbacks("target stack 2", hit: false, beHurt: true, kill: false, die: false);

            buffManager.CreateBuff(attackerId, targetId, testBuffId, 1);
            TriggerDamageCallbacks("target stack 3", hit: false, beHurt: true, kill: false, die: true);

            Debug.Log("[BuffBattleTestRunner] Advance buff time until automatic remove.");
            for (int i = 0; i < 24; i++)
            {
                buffManager.UpdateBuff(1f);
            }
        }

        private void TriggerDamageCallbacks(string label, bool hit, bool beHurt, bool kill, bool die)
        {
            var damageInfo = new DamageInfo
            {
                AttackerId = attackerId,
                TargetId = targetId,
                Value = new DamageValue { physics = 100 },
                TriggerFlags = 0,
            };

            if (hit)
            {
                buffManager.OnHitTriggerBuff(ref damageInfo);
            }

            if (beHurt)
            {
                buffManager.OnBeHurtTriggerBuff(ref damageInfo);
            }

            if (kill)
            {
                buffManager.OnKillTriggerBuff(ref damageInfo);
            }

            if (die)
            {
                buffManager.OnDieTriggerBuff(ref damageInfo);
            }

            Debug.Log($"[BuffBattleTestRunner] {label}: physics={damageInfo.Value.physics}, fire={damageInfo.Value.fire}, ice={damageInfo.Value.ice}, flags={damageInfo.TriggerFlags}");
        }

        private void EnsureCharacters()
        {
            if (!battleManager.Data.ContainsKey(attackerId))
            {
                battleManager.JoinBattle(true, attackerId, attackerInfoId);
            }

            if (!battleManager.Data.ContainsKey(targetId))
            {
                battleManager.JoinBattle(false, targetId, targetInfoId);
            }
        }

        private static BuffManager ResolveBuffManager(BattleManager manager)
        {
            if (manager == null)
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            return typeof(BattleManager).GetField("buffManager", flags)?.GetValue(manager) as BuffManager;
        }

        private BuffManager CreateLocalBuffManager()
        {
            localLuaManager = new LuaManager();
            localLuaManager.Start();

            var manager = new BuffManager();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(BuffManager).GetField("luaManager", flags)?.SetValue(manager, localLuaManager);
            manager.Init();

            usingLocalBuffManager = true;
            Debug.Log("[BuffBattleTestRunner] Created local BuffManager for isolated scene verification.");
            return manager;
        }

        private void OnDestroy()
        {
            if (usingLocalBuffManager)
            {
                buffManager?.Dispose();
                localLuaManager?.Dispose();
            }
        }
    }
}
