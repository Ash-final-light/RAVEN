using UnityEngine;
using System.Collections.Generic;
using Tools.Damage.SO;

namespace Tools.Action.Data.Bullet
{
    public enum MoveType
    {
        Ground,
        Fly
    }

// 策划脚本委托
    public delegate Vector3 BulletTween(float timeElapsed, GameObject bullet, GameObject target);

    public delegate void BulletOnCreate(GameObject bullet);

    public delegate void BulletOnRemoved(GameObject bullet);

    public delegate GameObject BulletTargetingFunction(GameObject caster);

    [System.Serializable]
    public class BulletModel
    {
        public string abName;
        public string assetName;
        [SerializeReference]
        public ActionCollideDetectionBaseData collideDetectionData;
        public int hitTimes;
        public float sameTargetDelay;
        public MoveType moveType;
        [Min(0f)]
        public float speed = 1f;
        public string colliderLayerName;
        public int colliderLayerMask
        {
            get
            {
                if (string.IsNullOrEmpty(colliderLayerName))
                {
                    return 0;
                }

                int layer = LayerMask.NameToLayer(colliderLayerName);
                return layer < 0 ? 0 : 1 << layer;
            }
        }
        public bool removeOnObstacled;
        public bool hitFoe;
        public bool hitAlly;
        [Header("Hit Effect")]
        public DamageHitConfigSO damageHitConfig;

        // 回调函数
        public BulletOnCreate onCreate;
        public System.Action<Collider> onHit;
        public BulletOnRemoved onRemoved;

        // 深拷贝方法，用于创建克隆体防止运行时篡改原始配置
        public BulletModel Clone() => (BulletModel)this.MemberwiseClone();
    }
}
