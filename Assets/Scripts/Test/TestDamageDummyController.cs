using Tools.Damage;
using Tools.InputHandle;
using UnityEngine;

namespace Test
{
    public class TestDamageDummyController : ActionStateController
    {
        private void Start()
        {
        }

        public void Configure(string id, int characterInfoId, bool player)
        {
            characterId = id;
            infoId = characterInfoId;
            isPlayer = player;
        }

        protected override void UpdateBehavior()
        {
        }

        public override void OnCharacterHit(DamageInfo damageInfo)
        {
            Debug.Log($"[TestDamageDummyController] {characterId} hit, hp resource: {damageInfo.Resource.hp}");
            //PopupTextManager.Instance.ShowDamage(damageInfo, transform);
        }
    }
}
