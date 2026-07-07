using System;
using Tools.DesignMode;
using Tools.InputHandle;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Tools.Action.Data.Collide
{
    /// <summary>
    /// 给自定义碰撞检测用的能力（攻击，闪避，弹刀）
    /// TIPS: 碰撞委托工厂，不做帧驱动处理
    /// </summary>
    public class CollideAbility:ITimeScalable
    {
        private ActionStateController _controller;
        public readonly SelfCollideManager _selfCollideManager;
        
        // 碰撞检测的物体池
        public ObjectPool<BindBoxCollideMono> _boxColliderPool = new ObjectPool<BindBoxCollideMono>(CreateBoxCollider,DestroyCollideObj);
        public ObjectPool<BindSphereCollideMono> _sphereColliderPool = new ObjectPool<BindSphereCollideMono>(CreateSphereCollider,DestroyCollideObj);
        public ObjectPool<BindSectorCollideMono> _sectorColliderPool = new ObjectPool<BindSectorCollideMono>(CreateSectorCollider,DestroyCollideObj);

        public CollideAbility(ActionStateController controller)
        {
            _controller = controller;
            _selfCollideManager = new SelfCollideManager(_controller);
        }
        public void Tick(float deltaTime)
        {
            deltaTime *= _scale;
            _selfCollideManager.Tick(deltaTime);
        }
        
        //创建碰撞委托的池子
        public ObjectPool<CollideDetectionDelegate> _collideDetectionDelegatePool = new ObjectPool<CollideDetectionDelegate>
            (() => new CollideDetectionDelegate(),d=> d.Destroy());
        
        private static BindBoxCollideMono CreateBoxCollider()
        {
            var instance = new GameObject("Collide Detection Box").AddComponent<BindBoxCollideMono>();
            return instance;
        }

        private static BindSphereCollideMono CreateSphereCollider()
        {
            var  instance = new GameObject("Collide Detection Sphere").AddComponent<BindSphereCollideMono>();   
            return instance;
        }

        private static BindSectorCollideMono CreateSectorCollider()
        {
            var instance = new GameObject("Collide Detection Sector").AddComponent<BindSectorCollideMono>();
            return instance;
        }
        private static void DestroyCollideObj(ICollideObj instance)
        {
            Debug.Log("Reset");
            instance.ResetStatus();
        }


        public CollideDetectionDelegate GetCollideDetectionDelegate(Transform parent,Transform owner,
            int layerMask,ActionCollideDetectionBaseData baseData,System.Action<Collider> onCollideEvent)
        {
            CollideDetectionDelegate @delegate = _collideDetectionDelegatePool.Get();
            @delegate.Init(this,parent,owner, baseData,layerMask);
            @delegate.BindCollide(onCollideEvent);
            return @delegate;
        }
        public void RemoveDelegate(CollideDetectionDelegate @delegate)
        {
            _collideDetectionDelegatePool.Release(@delegate);
        }
        public void Clear()
        {
            _collideDetectionDelegatePool.Clear();
            _boxColliderPool.Clear();
            _sphereColliderPool.Clear();
            _sectorColliderPool.Clear();
        }

        private string _id = Guid.NewGuid().ToString();
        public string Id => _id;
        private float _scale = 1f;
        public void TimeScaleChanged(float scale, TimeScaleType type)
        {
            if (type == TimeScaleType.Logic)
            {
                _scale = scale;
                Debug.Log($"[TimeScale Test: CollideAbility] 组件收到缩放: 类型={type}, 值={scale}");
            }
        }
    }
}