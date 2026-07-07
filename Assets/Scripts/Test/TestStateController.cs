using System;
using System.Collections.Generic;
using Common.Constant;
using Sirenix.OdinInspector;
using Tools.Action.Data;
using Tools.Action.Data.Bullet;
using Tools.Action.Data.Collide;
using Tools.Action.Data.Event;
using Tools.Action.Data.Slide;
using Tools.Action.Running;
using Tools.Battle;
using Tools.Damage;
using Tools.InputHandle;
using Tools.InputHandle.Data;
using UnityEngine;

namespace Test
{
    [System.Serializable]
    public class TestStateController:ActionStateController
    {
        protected override void UpdateBehavior()
        {
        }
        private SlideObjMono _slideObjMono;
        private void Start()
        {
            base.Start();
            if(playerInputManager == null)
            {
                Debug.LogError("playerInputManager 为空");
                return;
            }
            RunningBlackboard = _blackboardPrefab.Clone();
            _actionPlayer = new ActionPlayer(idle,this);
            _eventAbility = new EventAbility(this,buffManager);
            _playerCharacterBrain = new PlayerCharacterBrain(this,null,playerInputManager);
            _evaluator = new PlayerCharacterEvaluator(comboInfoContainerSo,RunningBlackboard);
            InitCollide();
            InitSlide();
            _stateMachine = new PlayerStateMachine(this,_evaluator,_slideAction);
            _actionPlayer.Start();
            InitTimeScale();
        }
        private void SetCharacterStateType(CharacterStateType stateType)
        {
            StateMachine.SetCharacterStateType(stateType);
        }

        private void InitSlide()
        {
            _slideObjMono = transform.Find(CollideConstants.ChildSlideCollider).GetComponent<SlideObjMono>();
            if (_slideObjMono == null)
            {
                Debug.LogError($"未找到{CollideConstants.ChildSlideCollider}碰撞体");
                return;
            }
            _slideAction = new SlideAction(_slideObjMono,transform,slideConfigSo,this);
        }

        protected override void OnDestroy()
        {
            _slideAction?.Dispose();
            base.OnDestroy();
        }

        /// <summary>
        /// TODO：后续交给全局的时间管理器来决定
        /// </summary>
        private void Update()
        {
            _playerCharacterBrain.OnRenderThoughtsUpdated(Time.deltaTime);
            _stateMachine.Tick(Time.deltaTime);
            ApplyPendingMovementRenderFrame(Time.deltaTime);
            TickActionAbilities(Time.deltaTime);
        }
        public override void OnCharacterHit(DamageInfo damageInfo)
        {
            //TODO:根据伤害类型（击飞，普通）播放不同的受击动画
            _stateMachine?.TriggerHurt(AnimActionConstants.hit01);
        }
    }
}
