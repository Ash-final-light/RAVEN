using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Tools.Action.Data.Effect
{
    [Serializable]
    public struct ParticlePrefabInfo
    {
        public string abName;
        public string assetName;
    }
    /// <summary>
    /// 局部粒子能力，如果创建全局物体请调用 TODO：GlobalParticleAbility
    /// </summary>
    public class ParticleAbility:ITimeScalable
    {
        private MonoBehaviour _owner; // 宿主，用于开启协程
        private Transform _poolRoot;  // 粒子回收后的父节点
        
        [ShowInInspector]private Dictionary<string, Tools.DesignMode.ObjectPool<GameObject>> _particlePools;

        [ShowInInspector] private Dictionary<GameObject, ParticleInfo> _playingParticles;
            
        
        [ShowInInspector]private List<GameObject> _allParticleInstances;
        

        public ParticleAbility(MonoBehaviour owner, ParticlePrefabInfo[] test_particleinfo,Transform poolRoot = null)
        {
            _particlePools = new Dictionary<string, Tools.DesignMode.ObjectPool<GameObject>>();
            _playingParticles = new Dictionary<GameObject, ParticleInfo>();
            _allParticleInstances = new List<GameObject>();
            _owner = owner;
            _poolRoot = poolRoot;
            Init(test_particleinfo);
        }
        /// <summary>
        /// 编辑器调用，更新所有播放中的粒子时间，性能较低
        /// </summary>
        /// <param name="deltaTime"></param>
        public void TickInEditor(float deltaTime)
        {
            var toRemovedEffects = new List<GameObject>();
            _playingParticles.ForEach(pair =>
            {
                //Debug.Log($"更新粒子时间: {pair.Value.instance.name}");
                pair.Value.time += deltaTime;
                if (pair.Value.instance.TryGetComponent<ParticleSystem>(out var particleSystem))
                {
                    particleSystem.Simulate(
                        pair.Value.time,
                        true,
                        true
                    );
                    //Debug.Log("特效时间: " + particleSystem.time);
                    particleSystem.Pause();
                }

                if (pair.Value.time >= pair.Value.duration)
                {
                    toRemovedEffects.Add(pair.Value.instance);
                }
            });
            toRemovedEffects.ForEach(StopParticle);
        }
        
        public void Tick(float deltaTime)
        {
            var toRemovedEffects = new List<GameObject>();
            deltaTime *= _scale;

            // 建议使用 foreach 替代 ForEach 扩展，方便调试且性能略好
            foreach (var pair in _playingParticles)
            {
                var info = pair.Value;
                info.time += deltaTime;

                if (info.instance.TryGetComponent<ParticleSystem>(out var particleSystem))
                {
                    // 精准模拟控制：
                    // 1. deltaTime: 只让粒子前进这一帧的时间
                    // 2. withChildren: true, 包含所有子节点
                    // 3. restart: false, 【关键】不要从头开始，而是从当前帧继续
                    // 4. fixedTimeStep: true, 使用固定步长，确保每一帧的粒子位移是精确的
                    particleSystem.Simulate(deltaTime, true, false, true);
            
                    // 保持暂停状态，防止引擎在 Tick 之外的时间自动模拟
                    if (!particleSystem.isPaused)
                    {
                        particleSystem.Pause();
                    }

                    //Debug.Log($"实例:{info.instance.name} | 模拟进度:{info.time:F3}s | 粒子数:{particleSystem.particleCount}");
                }

                // 到期回收
                if (info.time >= info.duration)
                {
                    toRemovedEffects.Add(info.instance);
                }
            }

            // 执行回收
            for (int i = 0; i < toRemovedEffects.Count; i++)
            {
                StopParticle(toRemovedEffects[i]);
            }
        }
        /// <summary>
        /// 提前加载场景中可能存在的粒子，减少战斗卡顿
        /// </summary>
        public void Init(ParticlePrefabInfo[] particles)
        {
            // 加载所有可能存在的粒子
            foreach (var particle in particles)
            {
                string key = $"{particle.abName}_{particle.assetName}";
                GameObject prefab = AbManager.Instance.LoadRes<GameObject>(particle.abName, particle.assetName);
                if (prefab == null) continue;
                CreatePoolForParticle(key, prefab);
            }
        }

        #region 同步播放
        public GameObject PlayParticle(string abName, string assetName, Vector3 position, Quaternion rotation, Vector3 scale,
            bool isFixedTime, float fixedTime,
            Transform parent = null, bool follow = true)
        {
            string key = $"{abName}_{assetName}";
            
            // 确保池已存在
            if (!_particlePools.ContainsKey(key))
            {
                GameObject prefab = AbManager.Instance.LoadRes<GameObject>(abName, assetName);
                if (prefab == null) return null;
                CreatePoolForParticle(key, prefab);
            }

            GameObject instance = _particlePools[key].Get();
            SetupAndAddToPool(instance, key, position, rotation, scale, parent, follow, isFixedTime, fixedTime);
            return instance;
        }
        #endregion

        #region 异步播放
        public void PlayParticleAsync(string abName, string assetName, Vector3 position, Quaternion rotation, Vector3 scale,
            bool isFixedTime, float fixedTime,
            Action<GameObject> onComplete = null, Transform parent = null, bool follow = true)
        {
            string key = $"{abName}_{assetName}";

            // 如果池已存在，直接获取
            if (_particlePools.ContainsKey(key))
            {
                GameObject instance = _particlePools[key].Get();
                SetupAndAddToPool(instance, key, position, rotation, scale, parent, follow, isFixedTime, fixedTime);
                onComplete?.Invoke(instance);
                return;
            }

            // 异步加载资源
            AbManager.Instance.LoadResAsync<GameObject>(abName, assetName, (prefab) =>
            {
                if (prefab == null)
                {
                    Debug.LogError($"[ParticleManager] Async load failed: {assetName}");
                    onComplete?.Invoke(null);
                    return;
                }

                CreatePoolForParticle(key, prefab);
                GameObject instance = _particlePools[key].Get();
                SetupAndAddToPool(instance, key, position, rotation, scale, parent, follow, isFixedTime, fixedTime);
                onComplete?.Invoke(instance);
            });
        }
        #endregion
        
        
        
        
        
        private void CreatePoolForParticle(string key, GameObject prefab)
        {
            if (_particlePools.ContainsKey(key)) return;

            var pool = new Tools.DesignMode.ObjectPool<GameObject>(
                createFunction: () =>
                {
                    GameObject instance = UnityEngine.Object.Instantiate(prefab);
                    instance.SetActive(false);
                    instance.name = prefab.name + "(Pool)";
                    return instance;
                },
                destroyFunction: (obj) => { if (obj) UnityEngine.Object.Destroy(obj); },
                defaultCapacity: 5,
                maxSize: 20
            );

            _particlePools[key] = pool;
        }

        private void SetupAndAddToPool(GameObject instance, string key, Vector3 position, Quaternion rotation, Vector3 scale, 
            Transform parent, bool follow,
            bool isFixedTime,float fixedTime)
        {
            if (!instance) return;
            
            instance.SetActive(true);
            if (parent)
            {
                instance.transform.SetParent(parent);
                instance.transform.localPosition = position;
                instance.transform.localRotation = rotation;
                instance.transform.localScale = scale;
                //Debug.Log($"[ParticleManager] PlayParticleAsync: {key} with parent {parent.name}");
                if (!follow)
                {
                    instance.transform.SetParent(null);
                }
            }
            else
            {
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.transform.SetParent(null);
            }

            AddToPool(instance, key,isFixedTime,fixedTime);

            void AddToPool(GameObject instance, string key, bool isFixedTime, float fixedTime)
            {
                ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>();
                float maxDuration = 0f;

                foreach (var ps in systems)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    
                    ps.Play(true);
                   
                    float duration = ps.main.duration + ps.main.startDelayMultiplier;
                    if (duration > maxDuration) maxDuration = duration;
                }

                if (systems.Length == 0) maxDuration = 3f;
                if (isFixedTime) maxDuration = fixedTime;

                _playingParticles.Add(instance, new ParticleInfo
                    { poolKey = key, instance = instance, duration = maxDuration, time = 0f });
                _allParticleInstances.Add(instance);
            }
        }

        private void ImmediateRecycle(GameObject instance, string key)
        {
            if (!instance || !_playingParticles.ContainsKey(instance) || !_poolRoot) return;

            _playingParticles.Remove(instance);
            _allParticleInstances.Remove(instance);

            if (_particlePools.TryGetValue(key, out var pool))
            {
                // 重置状态
                ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in systems) { ps.Stop(); ps.Clear(); }
                
                instance.SetActive(false);
                pool.Release(instance);
            }
            else
            {
                UnityEngine.Object.Destroy(instance);
            }
        }

        public void StopAllParticle()
        {
            List<GameObject> tempList = new List<GameObject>(_playingParticles.Keys);
            foreach (var instance in tempList)
            {
                StopParticle(instance);
            }
            _playingParticles.Clear();
        }
        
        public void StopParticle(GameObject particleInstance)
        {
            if (particleInstance == null || !_playingParticles.TryGetValue(particleInstance, out var info)) return;
            ImmediateRecycle(particleInstance, info.poolKey);
        }
        
        
        /// <summary>
        /// 彻底清除所有粒子池
        /// </summary>
        public void ClearAllPools()
        {
            List<GameObject> tempList = new List<GameObject>(_playingParticles.Keys);
            foreach (var instance in tempList)
            {
                StopParticle(instance);
            }
            foreach (var pool in _particlePools.Values) pool.Clear();
            _particlePools.Clear();
            _playingParticles.Clear();
            _allParticleInstances.Clear();
        }

        [Serializable]
        private class ParticleInfo
        {
            public string poolKey;
            public GameObject instance;
            public float duration;
            public float time;
        }
// 时间缩放部分

        private string id = Guid.NewGuid().ToString();
        public string Id => id;
        private float _scale = 1f;

        public void TimeScaleChanged(float scale, TimeScaleType type)
        {
            // 这里的日志可以告诉你：
            // 1. 哪个类型的缩放传过来了
            // 2. 缩放值是多少
            // 3. 这个组件属于哪个角色（如果持有宿主引用）
            Debug.Log($"[TimeScale Test: ParticleAbility] 组件收到缩放: 类型={type}, 值={scale}");
            _scale = scale;
            
            // 实际逻辑：设置动画速度或计时器倍率
            // this.internalTimer.Speed = scale;
        }
    }
}