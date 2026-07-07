using System;
using System.Collections.Generic;
using Animancer;
using Animancer.TransitionLibraries;
using Sirenix.OdinInspector;
using Tools.InputHandle;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Tools.Action.Data
{
    public class AnimationAbility:ITimeScalable
    {
        [Title("核心组件")] 
        [SerializeField, Required] private AnimancerComponent animancer;
        private ActionStateController actionStateController;
        public AnimancerComponent Animancer => animancer;

        [Title("全局设置")] 
        [SerializeField] private float defaultFadeDuration = AnimancerGraph.DefaultFadeDuration;
        [Tooltip("当调用停止或重置时，默认播放的动画Key")]
        [SerializeField] private StringAsset idleAnimationKey;

        private float _speed = 1f;
        public float SpeedFactor => _speed;
        protected float TimeScaleFactor => _speed <= 0f ? 1000f : 1f / _speed;

        [Title("事件监听")] 
        [ShowInInspector]private List<ICharacterAnimationListen> animationListeners = new();

        // --- 分层管理 ---
        // 第0层：普通动作（Idle, Move, Skill）
        public AnimancerLayer MainLayer => Animancer?.Layers[0];
        // 第10层：死亡动作（高优先级，确保覆盖所有底层动作）
        public AnimancerLayer DieLayer => Animancer?.Layers[10];

        public AnimationAbility(ActionStateController actionStateController, AnimancerComponent animancer,StringAsset idleAnimationKey)
        {
            this.actionStateController = actionStateController;
            this.animancer = animancer;
            this.idleAnimationKey = idleAnimationKey;
            
            MainLayer?.SetDebugName("Main Layer");
            DieLayer?.SetDebugName("Die Layer");
            // 死亡层初始化权重为0
            if(DieLayer != null)
            {
                DieLayer.Weight = 0;
            }
        }

        public void Dispose()
        {
            MainLayer.Stop();
            DieLayer.Stop();
            Animancer.Graph.Destroy();
        }
        /// <summary>
        /// 切换动画库（可选，如果使用 TransitionLibrary 模式）
        /// </summary>
        public void SwitchTransitionLibrary(TransitionLibraryAsset transitionLibrary)

        {

            if (Animancer.Transitions == transitionLibrary) return;

            Animancer.Transitions = transitionLibrary;

        }

        #region 播放逻辑

        /// <summary>
        /// 播放普通动作（Main Layer）
        /// </summary>
        public AnimancerState Play(ITransition transition, bool resetTime = true)
        {
            if (transition == null || MainLayer == null) return null;
            if (MainLayer.CurrentState != null) NotifyListeners(false, MainLayer.CurrentState);

            float fadeDuration = GetFadeDuration(MainLayer, transition);
            var state = MainLayer.Play(transition, fadeDuration, transition.FadeMode);
            
            ApplyStateSpeed(state);
            if (resetTime) state.Time = 0f;

            NotifyListeners(true, state);
            return state;
        }

        public AnimancerState Play(StringAsset name, bool resetTime = true)
        {
            if (name == null) return null;

            var asset = Animancer.Transitions as TransitionLibraryAsset;
            if (asset != null && asset.Library != null)
            {
                if (asset.Library.TryGetTransition(name, out var modifierGroup))
                {
                    return Play(modifierGroup.Transition, resetTime);
                }
            }

            if (Animancer.States.TryGet(name, out var state))
            {
                if (MainLayer.CurrentState != null) NotifyListeners(false, MainLayer.CurrentState);
                MainLayer.Play(state, defaultFadeDuration * TimeScaleFactor);
                ApplyStateSpeed(state);
                if (resetTime) state.Time = 0f;
                NotifyListeners(true, state);
                return state;
            }
            return null;
        }

        /// <summary>
        /// 播放死亡动画（Die Layer）
        /// </summary>
        public AnimancerState PlayDie(ITransition transition)
        {
            if (transition == null) return null;
            
            // 死亡层淡入到 1.0 权重
            DieLayer.StartFade(1, defaultFadeDuration * TimeScaleFactor);
            var state = DieLayer.Play(transition, defaultFadeDuration * TimeScaleFactor);
            ApplyStateSpeed(state);
            
            return state;
        }

        #endregion

        #region 停止与重置

        /// <summary>
        /// 停止当前所有动画并切换回 Idle
        /// </summary>
        public void StopToIdle(bool withFade = true)
        {
            // 1. 如果死亡层有东西，将其淡出
            if (DieLayer.Weight > 0)
            {
                DieLayer.StartFade(0, withFade ? defaultFadeDuration : 0);
            }

            // 2. 播放 Idle 动画
            if (idleAnimationKey != null)
            {
                Play(idleAnimationKey);
            }
            else
            {
                // 如果没配 Idle，就直接强停
                MainLayer.Stop();
                NotifyListeners(false, MainLayer.CurrentState);
            }
        }

        /// <summary>
        /// 强行停止所有层（通常用于彻底回收物体）
        /// </summary>
        public void StopAll()
        {
            MainLayer.Stop();
            DieLayer.Stop();
            DieLayer.Weight = 0;
        }

        #endregion

        #region 辅助方法

        public void SetAnimancerSpeed(float speed)
        {
            var originalSpeed = _speed;
            _speed = Mathf.Max(speed, 0f);
            
            if (originalSpeed > 0)
            {
                float relativeFactor = _speed / originalSpeed;
                if (MainLayer.CurrentState != null) MainLayer.CurrentState.Speed *= relativeFactor;
                if (DieLayer.CurrentState != null) DieLayer.CurrentState.Speed *= relativeFactor;
            }
        }

        private void ApplyStateSpeed(AnimancerState state)
        {
            if (state != null) state.Speed *= SpeedFactor;
        }

        private float GetFadeDuration(AnimancerLayer layer, ITransition transition)
        {
            var asset = Animancer.Transitions as TransitionLibraryAsset;
            if (asset != null && asset.Library != null)
            {
                return asset.Library.GetFadeDuration(layer, transition) * TimeScaleFactor;
            }
            return transition.FadeDuration * TimeScaleFactor;
        }

        private void NotifyListeners(bool isPlayed, AnimancerState state)
        {
            if(animationListeners == null) return;
            animationListeners.ForEach(l =>
            {
                if (isPlayed) l?.HandleAnimationPlayed(actionStateController, MainLayer, state);
                else l?.HandleAnimationStopped(actionStateController, MainLayer, state);
            });
        }
        
        #endregion

        private string id = Guid.NewGuid().ToString();
        public string Id => id;
        public void TimeScaleChanged(float scale, TimeScaleType type)
        {
            //_speed = scale;
            Debug.Log($"[TimeScale Test: AnimAbility] 组件收到缩放: 类型={type}, 值={scale}");
        }
    }
}