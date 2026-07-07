using Common.Constant;
using Tools.InputHandle.Data;
using UnityEngine;

namespace Tools.InputHandle
{
    public abstract class BaseStateMachine
    {
        protected ActionStateController Owner;
        
        protected CharacterStateType _currentStateType = CharacterStateType.Idle;
        public string CurrentAction => Owner._actionPlayer.CurrentActionClipName;
        public bool _ignorePriority = false;
        
        public BaseStateMachine(ActionStateController actionStateController)
        {
            Owner = actionStateController;
        }
        protected abstract void UpdateCurrentState(float deltaTime);
        public void Tick(float deltaTime)
        {
            UpdateCurrentState(deltaTime);
            Owner._actionPlayer.Tick(deltaTime);
        }
        /// <summary>
        /// 尝试从SO中获取动画并播放。若失败则打印错误。
        /// </summary>
        public virtual bool TryPlayAction(string clipName, bool loop, CharacterStateType? targetState = null)
        {
            if (Owner.actionClipContainerSo.ActionClips.TryGetValue(clipName, out var clip))
            {
                if (!CanTransitionTo(clip.StateType))
                {
                    Debug.LogError($"[StateMachine] 无法切换到状态: {clip.StateType}");
                    return false;
                }
                if (targetState.HasValue)
                    _currentStateType = targetState.Value;
                else 
                    _currentStateType = clip.StateType;
                Owner.ForceCloseSlideJudge();
                Owner._actionPlayer.SetActionClipAndPlay(Owner, clip, loop);
                return true;
            }

            Debug.LogError($"[StateMachine] 未找到动画资源: {clipName}");
            return false;
        }
        public void SetCharacterStateType(CharacterStateType stateType)
        {
            _currentStateType = stateType;
        }
        /// <summary>
        /// 外部受伤接口：由战斗/碰撞系统调用
        /// </summary>
        /// <param name="hurtAnimName">受击动画名，可根据攻击来源计算（如：Hurt_Front, Hurt_Back）</param>
        public void TriggerHurt(string hurtAnimName)
        {
            // 1. 检查霸体（Endure）逻辑
            if (Owner.CharacterProperty != null && (Owner.CharacterProperty.endure || Owner.CharacterProperty.immune))
            {
                Debug.Log("[StateMachine] 霸体中，忽略受击动作");
                return;
            }

            // 2. 强制播放受击动画
            if (TryPlayAction(hurtAnimName, false, CharacterStateType.Hurt))
            {
                Debug.Log($"[StateMachine] 触发受击: {hurtAnimName}");
            }
        }
        protected bool CanTransitionTo(CharacterStateType nextState)
        {
            // 如果当前没有动作，或者当前是 Idle，或者忽视优先级，直接允许切换
            if (Owner._actionPlayer == null || _currentStateType == CharacterStateType.Idle || _ignorePriority) 
                return true;

            // 基于 SO 配置的优先级检查
            var currentConfig = Owner._characterStateConfigSo.GetConfig(_currentStateType);
            var nextConfig = Owner._characterStateConfigSo.GetConfig(nextState);
            
            
            if (currentConfig == null || nextConfig == null) return true;

            // 1. 如果新状态优先级更高，允许打断
            if (nextConfig.Priority > currentConfig.Priority) return true;

            // 2. 如果当前状态标记为“可被低优先级打断”（较少见，通常用于受击或特殊状态）
            if (currentConfig.CanBeInterrupted && currentConfig.AllowedInterruptTypes.Contains(nextState)) 
                return true;

            return false;
        }
    }
}
