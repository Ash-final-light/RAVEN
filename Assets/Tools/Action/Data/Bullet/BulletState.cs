using System.Collections.Generic;
using Tools.Action.Data.Collide;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Action.Data.Bullet
{
    /*碰撞检测发现 Collider
        ↓
    BulletState.HandleCollide()
        ↓
    判断是否允许命中 CanHit()
        ↓
    记录命中、扣 hp、处理销毁
        ↓
    model.onHit?.Invoke(collider)
        ↓
    真正的伤害 / Buff / 特效 / 其他业务效果*/
    public class BulletState : MonoBehaviour
    {
        // 基础数据
        public BulletModel model;
        public GameObject caster;
        public object propWhileCast; // 简略表示，可替换为具体的属性类
        public int hp;

        // 时间相关
        public float duration;
        public float timeElapsed;
        public float canHitAfterCreated;

        // 移动相关
        public BulletTween tween;
        public float fireDegree;
        public bool useFireDegreeForever;
        public Vector3 velocity;
        public MoveType currentMoveType;
        public bool smoothMove;
        public GameObject followingTarget;

        private BulletAbility _belongsToAbility;
        private CollideDetectionDelegate collideDetectionDelegate;
        // 记录命中的历史
        public class HitRecord
        {
            public GameObject target;
            public float timeToCanHit;
        }

        private List<HitRecord> hitRecords = new List<HitRecord>();
        public Dictionary<string, object> param = new Dictionary<string, object>();

        public void Init(BulletLauncher launcher, BulletAbility belongsTo,CollideDetectionDelegate collideDetectionDelegate)
        {
            this._belongsToAbility = belongsTo;

            this.model = launcher.model.Clone();
            this.caster = launcher.caster;
            this.hp = model.hitTimes;
            this.duration = launcher.duration;
            this.canHitAfterCreated = launcher.canHitAfterCreated;
            this.tween = launcher.tween;
            this.fireDegree = launcher.fireDegree;
            this.currentMoveType = model.moveType;
            this.smoothMove = !model.removeOnObstacled;
            this.collideDetectionDelegate = collideDetectionDelegate;
            
            // 【重要】：重置状态数据，因为对象是复用的
            this.timeElapsed = 0; 
            hitRecords.Clear();
            param.Clear();

            model.onCreate?.Invoke(this.gameObject);
        }

        public void HandleCollide(Collider collider)
        {
            if (!collider)
            {
                return;
            }

            if (!CanHit(collider))
            {
                return;
            }

            GameObject target = ResolveHitRoot(collider);
            hitRecords.Add(new HitRecord
            {
                target = target,
                timeToCanHit = model.sameTargetDelay
            });

            model.onHit?.Invoke(collider);

            hp--;
            if (hp <= 0)
            {
                DestroyBullet();
            }
        }
        public void Tick(float dt)
        {
            timeElapsed += dt;

            if (timeElapsed >= duration)
            {
                DestroyBullet();
                return;
            }

            // 处理命中冷却
            for (int i = hitRecords.Count - 1; i >= 0; i--)
            {
                hitRecords[i].timeToCanHit -= dt;
                if (hitRecords[i].timeToCanHit <= 0) hitRecords.RemoveAt(i);
            }

            UpdateMovement(dt);
            UpdateCollision();
        }

        private void UpdateMovement(float dt)
        {
            // 执行策划编写的Tween逻辑
            Vector3 moveForce = Vector3.forward; // 默认方向
            if (tween != null)
            {
                moveForce = tween(timeElapsed, this.gameObject, followingTarget);
            }

            // 计算旋转与位移
            float finalDegree = useFireDegreeForever ? fireDegree : transform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, finalDegree, 0);
            float speed = model != null && model.speed > 0f ? model.speed : 1f;
            velocity = rotation * moveForce * speed; // 这里简化了速度合成逻辑

            // 逻辑位置更新 (2D平面的3D表现)
            transform.position += velocity * dt;
        }

        private void UpdateCollision()
        {
            if (timeElapsed < canHitAfterCreated) return;

            // 1. 地形碰撞检测 (此处简化)
            if (CheckObstacle())
            {
                if (model.removeOnObstacled)
                {
                    DestroyBullet();
                    return;
                }
            }

            // 2. 角色碰撞检测
            collideDetectionDelegate?.Tick(HandleCollide);
        }

        private bool CanHit(Collider collider)
        {
            GameObject target = ResolveHitRoot(collider);
            if (target == null) return false;
            if (target == gameObject) return false;
            if (caster != null)
            {
                if (collider.transform == caster.transform || collider.transform.IsChildOf(caster.transform))
                {
                    return false;
                }

                var casterController = caster.GetComponentInParent<ActionStateController>();
                var targetController = collider.GetComponentInParent<ActionStateController>();
                if (casterController != null && targetController != null && casterController == targetController)
                {
                    return false;
                }

                if (target == caster) return false;
            }
            if (target.GetComponentInParent<BulletState>() != null) return false;
            // 判断冷却记录
            if (hitRecords.Exists(r => r.target == target)) return false;
            // 阵营判断逻辑 (伪代码)
            // int targetSide = target.GetComponent<ChaState>().side;
            return true;
        }

        private GameObject ResolveHitRoot(Collider collider)
        {
            var stateController = collider.GetComponentInParent<ActionStateController>();
            if (stateController != null)
            {
                return stateController.gameObject;
            }

            return collider.gameObject;
        }

        private bool CheckObstacle()
        {
            // 根据 moveType 射线检测前方是否有阻挡
            return false;
        }

        public void DestroyBullet()
        {
            // 1. 触发策划回调
            model.onRemoved?.Invoke(this.gameObject);

            // 2. 判断是回池子还是真的销毁
            if (_belongsToAbility != null)
            {
                // 回归对象池
                _belongsToAbility.Recycle(this.gameObject);
                collideDetectionDelegate?.Destroy();
                collideDetectionDelegate = null;
            }
            else
            {
                // 如果没有池子引用（例如中途脚本被毁），则销毁
                collideDetectionDelegate?.Destroy();
                collideDetectionDelegate = null;
                Destroy(this.gameObject);
            }
        }
    }
}
