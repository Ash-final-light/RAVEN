using System.Collections.Generic;
using Common;
using Tools.Action.Data.Collide;
using Tools.Buff.Data;
using Tools.DesignMode;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Action.Data.Bullet
{
    public class BulletAbility
    {
        private static readonly System.Action<Collider> EmptyCollideHandler = _ => { };

        private ActionStateController _stateController;    
        
        private ObjectPool<GameObject> _myPool;
        private List<BulletState> _activeBullets;
        
        private GameObject prefab;
        
        /// <summary>
        /// 初始化子弹能力
        /// </summary>
        /// <param name="stateController"></param>
        public BulletAbility(ActionStateController stateController)
        {
            _stateController = stateController;
            _myPool = new ObjectPool<GameObject>(
                createFunction: CreateBulletLogic, // 绑定创建逻辑
                destroyFunction: (obj) => { if (obj) UnityEngine.Object.Destroy(obj); },
                defaultCapacity: 5,
                maxSize: 20
            );
            _activeBullets =  new List<BulletState>();
        }
        /// <summary>
        /// 指定子弹模型调用，实现预加载
        /// </summary>
        /// <param name="bulletModel"></param>
        public void SetBulletModel(string abName,string assetName)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(assetName) && assetName.StartsWith("Assets/"))
            {
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetName);
                return;
            }
#endif
            prefab = AbManager.Instance.LoadRes<GameObject>(abName,assetName);
        }
        
       
        // 加载与生成物体部分
        private GameObject CreateBulletLogic()
        {
            if (!prefab)
            {
                Debug.LogError("BulletAbility: prefab is null");
                return null;
            }
            //实例化物体
            GameObject go = UnityEngine.Object.Instantiate(prefab);
            go.SetActive(false);
            return go;
        }
        // ========================================================

        public void Fire(BulletLauncher launcher)
        {
            if(_myPool == null)
                return;
            GameObject bulletGo = _myPool.Get(obj => {
                obj.transform.position = launcher.firePosition;
                obj.transform.rotation = Quaternion.Euler(0, launcher.fireDegree, 0);
                obj.SetActive(true);
            });

            BulletState state = bulletGo.GetComponent<BulletState>();
            if (!state) state = bulletGo.AddComponent<BulletState>();

            int layerMask = launcher.model?.colliderLayerMask ?? 0;
            CollideDetectionDelegate @delegate = null;
            if (layerMask == 0)
            {
                Debug.LogWarning(
                    $"[BulletAbility] Bullet collider layer is not configured or invalid. " +
                    $"bullet={launcher.model?.assetName}, layer={launcher.model?.colliderLayerName}");
            }
            else if (launcher.model?.collideDetectionData != null)
            {
                @delegate = _stateController.CollideAbility?.
                    GetCollideDetectionDelegate(state.transform,state.transform,
                        layerMask,launcher.model.collideDetectionData,EmptyCollideHandler);
            }
            
            state.Init(launcher, this, @delegate);
            
            if (!_activeBullets.Contains(state))
            {
                _activeBullets.Add(state);
            }
        }
        /// <summary>
        /// 【核心新增】：驱动所有活跃子弹的逻辑更新
        /// 由外部的 ActionStateController 在 Update 中调用
        /// </summary>
        public void Tick(float dt)
        {
            // 倒序遍历，防止子弹在 Tick 中回收导致 List 索引崩溃
            for (int i = _activeBullets.Count - 1; i >= 0; i--)
            {
                if (_activeBullets[i] != null)
                {
                    _activeBullets[i].Tick(dt);
                }
                else
                {
                    _activeBullets.RemoveAt(i);
                }
            }
        }
        public void Recycle(GameObject bullet)
        {
            BulletState state = bullet.GetComponent<BulletState>();
            if (state != null)
            {
                // 【新增】：从活跃列表中移除
                _activeBullets.Remove(state);
            }

            if (_myPool != null)
            {
                _myPool.Release(bullet, obj => obj.SetActive(false));
            }
        }
    }
}
