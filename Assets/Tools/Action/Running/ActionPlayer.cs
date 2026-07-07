using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.Action.Data;
using Tools.Action.Data.Collide;
using Tools.Action.Data.Event;
using Tools.InputHandle;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Tools.Action.Running
{
    public enum ActionStage
    {
        Idle,
        Start,
        Anticipation,
        Judgment,
        Recovery,
        End,
        Stop,
    }
    [Serializable]
    public class ActionPlayer : IActionClipPlay,ITimeScalable
    {
        public event System.Action OnStartStage;
        public event System.Action OnAnticipationStage;
        public event System.Action OnJudgmentStage;
        public event System.Action OnRecoveryStage;
        public event System.Action OnEndStage;
        public event System.Action OnStopStage;
        
        public int CurrentTick => _tick;
        public float CurrentTime => _time;
        public ActionRuntimeContext RuntimeContext { get; private set; } = ActionRuntimeContext.Empty();

        private float _tickDeltaTime;
        private bool _loop = false;
        public bool Loop => _loop;
#region 运行时数据
        [SerializeField] private int _tick = 0;
        [SerializeField] private float _time = 0f;
        //animation clip
        private   ActionClip _actionClip;
        public string CurrentActionClipName => _actionClip?.name;
        private ActionStateController _stateController;
        
        // 音效参数
        private   Dictionary<ActionAudioClipData, int> _playingAudioClips = new();

        // 特效参数
        private   Dictionary<ActionEffectClipData, GameObject> _playingEffectClips = new();

        // 碰撞检测参数
        [ShowInInspector]private   Dictionary<ActionCollideDetectionClipData,CollideDetectionDelegate>
            _playingCollideDetectionClips = new();
        // 碰撞发生时，对应的触发事件
        [ShowInInspector]private Dictionary<string,Action<Collider>> _collideDelegateEvent = new();

        // 动画片段状态
        private AnimancerState _playingAnimancerState;
        
        // 动作阶段，默认情况下只能前进不能后退，在循环时将跳过结束阶段重新开始
        private ActionStage _stage = ActionStage.Idle;
#endregion

        public void SetRuntimeContext(ActionRuntimeContext context)
        {
            RuntimeContext = context ?? ActionRuntimeContext.Empty();
        }

        public ActionStage Stage
        {
            private set
            {
                if (_stage >= value)
                {
                    if (!_loop || value != ActionStage.Start)
                    {
                        return;
                    }
                }

                _stage = value;
                switch (value)
                {
                    case ActionStage.Start:
                        StartInternal();
                        OnStartStage?.Invoke();
                        break;
                    case ActionStage.Anticipation:
                        OnAnticipationStage?.Invoke();
                        break;
                    case ActionStage.Judgment:
                        OnJudgmentStage?.Invoke();
                        break;
                    case ActionStage.Recovery:
                        OnRecoveryStage?.Invoke();
                        break;
                    case ActionStage.End:
                        EndAndStopInternal();
                        OnEndStage?.Invoke();
                        break;
                    case ActionStage.Stop:
                        EndAndStopInternal();
                        OnStopStage?.Invoke();
                        break;
                }
            }
            get => _stage;
        }
        private void StartInternal()
        {
            ClearData();

            _time = 0f;
            _tick = 0;
            _playingAnimancerState = null;
            _stage = ActionStage.Start;
            if (_actionClip)
            {
                _stateController?.AnimationAbility?.SwitchTransitionLibrary(_actionClip.animation.transitionLibrary);
            }
        }

        private void EndAndStopInternal()
        {
            /*if (_playingAnimancerState != null)
            {
                _stateController?.AnimationAbility?.StopToIdle();
            }*/
            //Debug.Log($"[Test] 动作结束");
            ClearData();
        }
        

        private void ClearData()
        {
            //TODO: 清除动画数据
            _time = 0f;
            _tick = 0;
            _stage = ActionStage.Idle;
            _playingAnimancerState = null;
            RuntimeContext = ActionRuntimeContext.Empty();

            // 清除音频数据
            _playingAudioClips.ForEach((kv) => { _stateController.AudioAbility?.StopSound(kv.Value); });
            _playingAudioClips.Clear();

            // 清除特效数据
            _stateController.ParticleAbility?.StopAllParticle();
            _playingEffectClips.Clear();

            // 清除碰撞检测数据
            _playingCollideDetectionClips.ForEach((kv) => kv.Value.Destroy());
            _playingCollideDetectionClips.Clear();
        }

        //必须要有初始数据
        public ActionPlayer(ActionClip clip,
            ActionStateController controller,bool loop = false)
        {
            _actionClip = clip;
            _stateController = controller;
            _loop = loop;
            _tickDeltaTime = 1f / _actionClip.frameRate;
        }
        public void SetActionClipAndPlay(ActionStateController controller,ActionClip clip,bool loop = false)
        {
            controller?.ClearActionTemporaryState();
            ClearData();
            _actionClip = clip;
            _stateController = controller;
            _loop = loop;
            _tickDeltaTime = 1f / _actionClip.frameRate;
            Start();
        }
        
        /// <summary>
        /// 测试用，后期删了
        /// </summary>
        public ActionPlayer(ActionStateController controller,ActionCollideDetectionClipData collideDetectionClip)
        {
            _actionClip = ScriptableObject.CreateInstance<ActionClip>();
            _actionClip.collideDetection = new ActionCollideDetection();
            _actionClip.collideDetection.collideDetectionClips = new List<ActionCollideDetectionClipData>();
            _actionClip.collideDetection.collideDetectionClips.Add(collideDetectionClip);
            _stateController = controller;
            _tickDeltaTime = 1f / _actionClip.frameRate;
        }
        /// <summary>
        /// 测试用，后期删了
        /// </summary>
        public ActionPlayer(ActionStateController controller,ActionAnimation animationClip)
        {
            _actionClip = ScriptableObject.CreateInstance<ActionClip>();
            
            _actionClip.animation = animationClip;
            
            _stateController = controller;
            _tickDeltaTime = 1f / _actionClip.frameRate;
        }
        public void Play()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }
        public void Start()
        {
            if (!_loop && Stage >= ActionStage.Start)
            {
                return;
            }

            Stage = ActionStage.Start;
            
        }
        
        private float _accumulator = 0f; // 蓄水池变量

        public void Tick(float deltaTime)
        {
            // 1. 安全检查
            if (_tickDeltaTime <= 0) return;

            // --- 蓄水池机制：累加时间 ---
            _accumulator += deltaTime * _scale;

            // 如果当前累积的时间还不足以推进到一个新的 Tick，直接退出，等待下一帧
            // 公式：当前总模拟时间 (_time + _accumulator) 必须超过下一个 Tick 的预定时间
            if (_time + _accumulator < GetTickTime(_tick + 1))
            {
                return;
            }

            // 从蓄水池中提取当前这次递归尝试消耗的 deltaTime
            // 注意：这里为了不破坏你原来的递归逻辑，我们取出当前的全部积攒量
            float currentTotalDelta = _accumulator;
            _accumulator = 0f; // 重置蓄水池，多余的部分将在递归末尾存回

            InternalTickRecursive(currentTotalDelta);
        }

// 将原有的逻辑包装成递归体，保持结构不变
        private void InternalTickRecursive(float deltaTime)
        {
            // 2. 计算目标 Tick
            var newestTick = GetTick(_tick * _tickDeltaTime + deltaTime);

            // 3. 状态检查
            if (Stage >= ActionStage.End)
            {
                if (!_loop || Stage != ActionStage.End)
                {
                    _time += deltaTime;
                    return;
                }
            }

            // 4. 递归终止条件
            if (_tick >= newestTick)
            {
                // 关键改动：将剩下的不足一帧的时间存回蓄水池，而不是直接加到 _time
                _accumulator = deltaTime; 
                return;
            }

            // 5. 帧推进逻辑
            _tick++;
            float timeToNextTick = GetTickTime(_tick) - _time;
            _time = GetTickTime(_tick);
            float remainingDeltaTime = deltaTime - timeToNextTick;

            // 6. 执行阶段逻辑
            if (Stage == ActionStage.End && _loop)
            {
                StartInternal(); 
            }
            else
            {
                CheckTimeLine();
            }

            // 7. 尾递归
            InternalTickRecursive(remainingDeltaTime);
        }
        private int GetTick(float time)
        {
            return (int)((time + 0.0001f) / _tickDeltaTime);
        }

        private float GetTickTime(int tick)
        {
            return tick * _tickDeltaTime;
        }
        
        public void CheckTimeLine()
        {
            CheckProcess();
            CheckAnimation();
            CheckEvent();
            CheckCollide();
            CheckEffect();
            CheckAudio();
        }

        public void CheckAnimation()
        {
            var animationClip = _actionClip.animation.animationClip;
            if (_tick >= animationClip.startTick)
            {
                if (_playingAnimancerState == null)
                {
                    // 立即播放，Fade 设置为 0 (瞬间切换)
                    _playingAnimancerState = _stateController.AnimationAbility?.Play(animationClip.transition, true);
                    _playingAnimancerState.Speed = animationClip.speed * _stateController.AnimationAbility.SpeedFactor * _scale;
                }
            }
        }
        private void CheckProcess()
        {
            if (_tick >= _actionClip.process.anticipationTick)
            {
                Stage = ActionStage.Anticipation;
            }

            if (_tick >= _actionClip.process.judgmentTick)
            {
                Stage = ActionStage.Judgment;
            }

            if (_tick >= _actionClip.process.recoveryTick)
            {
                Stage = ActionStage.Recovery;
            }
            
            if(_tick >= _actionClip.totalTicks)
            {
                Stage = ActionStage.End;
            }
        }
        public void CheckAudio()
        {
            if (_stateController.AudioAbility == null)
            {
                return;
            }

            foreach (var audioClip in _actionClip.audio.audioClips)
            {
                var startTick = audioClip.startTick;
                var endTick = audioClip.startTick + audioClip.durationTicks;
                if (_tick >= startTick && _tick <= endTick)
                {
                    if (!_playingAudioClips.ContainsKey(audioClip))
                    {
                        var id = _stateController.AudioAbility.PlaySound(audioClip.AudioClip, false, audioClip.volume);
                        _playingAudioClips.Add(audioClip, id);
                    }
                }

                if (_tick >= endTick && _playingAudioClips.TryGetValue(audioClip, out var id1))
                {
                    _stateController.AudioAbility.StopSound(id1);
                    _playingAudioClips.Remove(audioClip);
                }
            }
        }

        public void CheckEvent()
        {
            var eventClips = _actionClip.events?.eventClips;
            if (eventClips == null)
            {
                return;
            }

            bool isBulletLogTest = eventClips.Any(eventClip =>
                eventClip != null &&
                !string.IsNullOrEmpty(eventClip.name) &&
                eventClip.name.Contains("BulletLogTest"));

            if (isBulletLogTest)
            {
                //Debug.Log($"[ActionEventTick] clip={_actionClip.name}, tick={_tick}, events={eventClips.Count}");
            }

            foreach (var eventClip in eventClips.Where(eventClip => eventClip != null && _tick == eventClip.tick))
            {
                //Debug.Log($"[ActionEventCheck] clip={_actionClip.name}, tick={_tick}, event={eventClip.name}, type={eventClip.type}");
                _stateController.EventAbility.Dispatch(_stateController, this, _actionClip, _tick, eventClip);
            }
        }

        /// <summary>
        /// 这里用来判断动作是否发生了碰撞（例如闪避，弹刀，攻击），和动作产生的物体没有关系（原地范围伤害）
        /// </summary>
        public void CheckCollide()
        {
            foreach (var collideDetectionClip in _actionClip.collideDetection.collideDetectionClips)
            {
                var startTick = collideDetectionClip.startTick;
                var endTick = collideDetectionClip.startTick + collideDetectionClip.durationTicks;
                
                if (_tick >= startTick && _tick <= endTick)
                {
                    if (!_playingCollideDetectionClips.TryGetValue(collideDetectionClip,
                            out var collideDetectionDelegate))
                    {
                        Transform owner;
                        if (collideDetectionClip.type == ActionCollideDetectionType.Bind)
                        {
                            // 绑定到默认碰撞盒（武器)，目前暂定为自身
                            owner = _stateController.transform;
                        }
                        else
                        {
                           //绑定到自定义的碰撞盒，owner层为角色本身
                           owner = _stateController.transform;
                        }
                        
                        Action<Collider> collideDelegateEvent;
                        if (_collideDelegateEvent.TryGetValue(collideDetectionClip.sliceId, out collideDelegateEvent))
                        {
                            var @delegate = _stateController.CollideAbility?.GetCollideDetectionDelegate(
                                _stateController.transform, owner,
                                collideDetectionClip.colliderLayerMask, collideDetectionClip.data,
                                collideDelegateEvent);

                            if (@delegate != null)
                            {
                                collideDetectionDelegate = @delegate;
                                _playingCollideDetectionClips.Add(collideDetectionClip, @delegate);
                            }
                        }
                        else
                        {
                            Debug.LogError($"Collide Detection Clip Event{collideDetectionClip.sliceId} Not Found");
                        }

                    }
                    collideDetectionDelegate?.Tick(_collideDelegateEvent[collideDetectionClip.sliceId]);
                   
                }

                if (_tick >= endTick && _playingCollideDetectionClips.TryGetValue(collideDetectionClip,
                        out var collideDetection))
                {
                    collideDetection.Destroy();
                    _playingCollideDetectionClips.Remove(collideDetectionClip);
                }
            }
        }

        public void CheckEffect()
        {
            if (_stateController.ParticleAbility == null)
            {
                return;
            }

            foreach (var effectClip in _actionClip.effect.effectClips)
            {
                var startTick = effectClip.startTick;
                var endTick = effectClip.startTick + effectClip.durationTicks;
                if (_tick >= startTick && _tick <= endTick)
                {
                    if (!_playingEffectClips.ContainsKey(effectClip))
                    {
                        //TODO：支持调整粒子尺寸，在ActionClip中进行多的设置来进行判断,当前默认特效不跟随角色移动
                        var id = _stateController.ParticleAbility.PlayParticle(effectClip.abName, effectClip.assetName,
                            effectClip.localPosition, effectClip.localRotation, effectClip.localScale,
                            true, int.MaxValue,
                            _stateController.transform, false);
                        _playingEffectClips.Add(effectClip, id);
                    }
                }

                if (_tick >= endTick && _playingEffectClips.TryGetValue(effectClip, out var id1))
                {   
                    _stateController.ParticleAbility.StopParticle(id1);
                    _playingEffectClips.Remove(effectClip);
                }
            }
        }

        public void RegisterCollideEvent(string sliceId, Action<Collider> collideDelegateEvent)
        {
            _collideDelegateEvent.Add(sliceId, collideDelegateEvent);
        }

        private string id = Guid.NewGuid().ToString();
        public string Id => id;
        private float _scale = 1f;
        public void TimeScaleChanged(float scale, TimeScaleType type)
        {
            if(type != TimeScaleType.Logic)
            {
                return;
            }
            _scale = scale;
            if (_playingAnimancerState == null || _actionClip?.animation?.animationClip == null)
            {
                return;
            }
            _playingAnimancerState.Speed = _actionClip.animation.animationClip.speed * _stateController.AnimationAbility.SpeedFactor * _scale;
            //Debug.Log($"[TimeScale Test: ActionPlayer] 组件收到缩放: 类型={type}, 值={scale}");
        }
    }
}
