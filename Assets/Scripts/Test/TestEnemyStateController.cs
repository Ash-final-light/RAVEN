using Common.Constant;
using Monster.Beast01;
using Tools.Action.Running;
using Tools.AI;
using Tools.Damage;
using Tools.InputHandle;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Test
{
    public class TestEnemyStateController:ActionStateController
    {
        private BehaviourTreeTimeScaleState _behaviourTreeTimeScale;

        [TimeScalable(Category = TimeScaleType.Logic)]
        public BehaviourTreeTimeScaleState BehaviourTreeTimeScale
        {
            get
            {
                if (_behaviourTreeTimeScale == null)
                {
                    _behaviourTreeTimeScale = new BehaviourTreeTimeScaleState($"{characterId}_BehaviourTree");
                }

                return _behaviourTreeTimeScale;
            }
        }
        public override void OnCharacterHit(DamageInfo damageInfo)
        {
            //TODO:根据伤害类型（击飞，普通）播放不同的受击动画
            Debug.Log($"[TestEnemyStateController] OnCharacterHit. damageInfo={damageInfo}");
            _stateMachine?.TriggerHurt(AnimActionConstants.hit01);
        }
        public float BehaviourTreeScale => BehaviourTreeTimeScale.Scale;

        protected override void UpdateBehavior()
        {
            
        }

        private void Update()
        {
            if (_stateMachine == null)
            {
                return;
            }

            UpdateBehavior();
            _stateMachine.Tick(Time.deltaTime);
            ApplyPendingMovementRenderFrame(Time.deltaTime);
            TickActionAbilities(Time.deltaTime);
        }

        public AgentAbilityConfig configSo;
        AgentCDController _agentCDController;
        private new void Start()
        {
            base.Start();
            _agentCDController = new AgentCDController(configSo);
            RunningBlackboard = _blackboardPrefab.Clone();
            _actionPlayer = new ActionPlayer(idle,this,true);
            _behaviourTreeTimeScale = new BehaviourTreeTimeScaleState($"{characterId}_BehaviourTree");
            
            _stateMachine = new TestBeast01StateMachine(this, _agentCDController);
            InitCollide();
            _actionPlayer.Start();
            InitTimeScale();
        }

        public class BehaviourTreeTimeScaleState : ITimeScalable
        {
            public string Id { get; }
            public float Scale { get; private set; } = 1f;

            public BehaviourTreeTimeScaleState(string id)
            {
                Id = id;
            }

            public void TimeScaleChanged(float scale, TimeScaleType type)
            {
                if (type != TimeScaleType.Logic)
                {
                    return;
                }

                Scale = Mathf.Max(0f, scale);
                Debug.Log($"[TimeScale Test: BehaviourTree] Scale changed. type={type}, scale={Scale:0.00}");
            }
        }
    }
}
