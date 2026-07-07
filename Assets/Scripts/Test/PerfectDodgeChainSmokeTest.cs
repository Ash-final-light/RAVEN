using System.Collections;
using Tools.Action.Data.Collide;
using Tools.Action.Data.Slide;
using Tools.Battle;
using Tools.Battle.Data;
using Tools.InputHandle;
using UnityEngine;

namespace Test
{
    public class PerfectDodgeChainSmokeTest : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private ActionStateController attacker;
        [SerializeField] private ActionStateController player;

        [Header("Colliders")]
        [SerializeField] private Collider playerBodyCollider;
        [SerializeField] private SlideObjMono slideJudge;

        [Header("Config")]
        [SerializeField] private SlideConfigSo slideConfig;
        [SerializeField] private string damageSliceId = "PerfectDodge_TestHit";
        [SerializeField] private float judgeDuration = 0.3f;
        [SerializeField] private bool runOnStart;

        private SlideAction _slideAction;
        private bool _successTriggered;

        private IEnumerator Start()
        {
            if (!runOnStart)
            {
                yield break;
            }

            yield return null;
            RunSmokeTest();
        }

        [ContextMenu("Run Smoke Test")]
        public void RunSmokeTest()
        {
            _successTriggered = false;

            if (!ValidateReferences())
            {
                Debug.LogError("[PerfectDodgeTest] Failed: missing references.");
                return;
            }

            PrepareSceneState();
            PrepareBattleData();

            slideJudge.OnSuccessSlide += OnSuccessSlide;
            _slideAction?.Dispose();
            _slideAction = new SlideAction(slideJudge, player.transform, slideConfig, player);

            Debug.Log("[PerfectDodgeTest] Begin.");
            _slideAction.StartJudge(judgeDuration);

            attacker.CollideAbility._selfCollideManager.HandleCollide(damageSliceId, slideJudge.GetComponent<Collider>());
            attacker.CollideAbility._selfCollideManager.HandleCollide(damageSliceId, playerBodyCollider);

            bool judgingAfterHit = slideJudge.IsJudging;
            Debug.Log(
                $"[PerfectDodgeTest] Result success={_successTriggered}, " +
                $"judgingAfterHit={judgingAfterHit}, slice={damageSliceId}");

            slideJudge.StopCollideJudge();
            slideJudge.OnSuccessSlide -= OnSuccessSlide;

            if (_successTriggered)
            {
                Debug.Log("[PerfectDodgeTest] PASS: perfect dodge event triggered and follow-up body hit was sent.");
            }
            else
            {
                Debug.LogError("[PerfectDodgeTest] FAIL: perfect dodge event was not triggered.");
            }
        }

        private bool ValidateReferences()
        {
            if (attacker == null || player == null || slideJudge == null || slideConfig == null)
            {
                return false;
            }

            if (playerBodyCollider == null)
            {
                playerBodyCollider = player.GetComponent<Collider>();
            }

            return playerBodyCollider != null && attacker.actionClipContainerSo != null;
        }

        private void PrepareSceneState()
        {
            attacker.characterId = string.IsNullOrEmpty(attacker.characterId)
                ? "PerfectDodge_Attacker"
                : attacker.characterId;
            player.characterId = string.IsNullOrEmpty(player.characterId)
                ? "PerfectDodge_Player"
                : player.characterId;

            attacker.isPlayer = false;
            player.isPlayer = true;

            TrySetTag(attacker.gameObject, CollideConstants.Enemy);
            TrySetTag(player.gameObject, CollideConstants.Character);
            TrySetTag(slideJudge.gameObject, CollideConstants.Slide);

            playerBodyCollider.enabled = true;
            slideJudge.GetComponent<Collider>().enabled = false;
        }

        private void PrepareBattleData()
        {
            RegisterBattleProperty(attacker, false);
            RegisterBattleProperty(player, true);
        }

        private static void RegisterBattleProperty(ActionStateController controller, bool isPlayer)
        {
            var data = BattleManager.Instance.Data;
            var controllers = BattleManager.Instance.Controllers;

            data[controller.characterId] = new CharacterProperty
            {
                gameObjId = controller.characterId,
                isPlayer = isPlayer,
                Info = new CharacterInfoData
                {
                    Id = controller.infoId,
                    Name = controller.characterId,
                    Health = 100,
                    Damage = 10,
                    Defense = 0,
                    Speed = 1
                },
                CurrentHealth = 100,
                CurrentDamage = 10,
                CurrentDefense = 0,
                CurrentSpeed = 1,
                CurrentCritRate = 0f,
                targetTransform = controller.transform,
                isActive = true
            };

            controllers[controller.characterId] = controller;
        }

        private void OnSuccessSlide(SlideObjMono slideObj)
        {
            _successTriggered = true;
            Debug.Log("[PerfectDodgeTest] Success callback received.");
        }

        private static void TrySetTag(GameObject target, string tag)
        {
            try
            {
                target.tag = tag;
            }
            catch (UnityException e)
            {
                Debug.LogError($"[PerfectDodgeTest] Tag '{tag}' is missing. {e.Message}");
            }
        }

        private void OnDestroy()
        {
            _slideAction?.Dispose();
            if (slideJudge != null)
            {
                slideJudge.OnSuccessSlide -= OnSuccessSlide;
            }
        }
    }
}
