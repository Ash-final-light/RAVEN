using Animancer;
using Sirenix.OdinInspector;
using Tools.Action.Data;
using Tools.Action.Data.Bullet;
using Tools.Action.Data.Collide;
using Tools.Action.Data.Effect;
using Tools.Action.Data.Event;
using Tools.Action.Data.Interact;
using Tools.Action.Data.Slide;
using Tools.Action.Running;
using Tools.Audio;
using Tools.Battle;
using Tools.Battle.Data;
using Tools.Buff;
using Tools.Damage;
using Tools.InputHandle.Data;
using Tools.Skill;
using Tools.TimeScale.Receiver;
using UnityEngine;
using VContainer;

namespace Tools.InputHandle
{
    [System.Serializable]
    public abstract class ActionStateController : MonoBehaviour
    {
        [Inject] protected readonly BuffManager buffManager;
        protected PlayerInputManager playerInputManager => PlayerInputManager.Instance;
        
        
        public event System.Action OnCharacterDieAction;
        
        // Logic action player.
                public ActionPlayer _actionPlayer;
                [TimeScalable(Category = TimeScaleType.Logic)]
                public ActionPlayer ActionPlayer => _actionPlayer;

        #region Abilities
        
        private CollideAbility _collideAbility;
        [TimeScalable(Category = TimeScaleType.Logic)]
        public CollideAbility CollideAbility
        {
            get
            {
                if ( _collideAbility == null)
                {
                    _collideAbility = _collideAbility ?? new CollideAbility(this);
                }

                return _collideAbility;
            }
        }

        
       private AudioAbility _audioAbility;
        [TimeScalable(Category = TimeScaleType.Audio)]
        public AudioAbility AudioAbility
        {
            get
            {
                if (_audioAbility == null)
                {
                    _audioAbility = _audioAbility ?? new AudioAbility
                        (gameObject.GetComponent<GameAudioManager>());
                }
                return _audioAbility;
            }
        }

        // Particle ability.
        [SerializeField] private ParticlePrefabInfo[] test_particleinfo;
       private ParticleAbility _particleAbility;
        [TimeScalable(Category = TimeScaleType.Effect)]
        public ParticleAbility ParticleAbility
        {
            get
            {
                if (_particleAbility == null)
                {
                    // Pass this in as the coroutine driver.
                    _particleAbility = new ParticleAbility(this, test_particleinfo, this.transform);
                }

                return _particleAbility;
            }
        }
        
        private AnimationAbility _animAbility;
        [TimeScalable(Category = TimeScaleType.Logic)]
        public AnimationAbility AnimationAbility
        {
            get
            {
                if (_animAbility == null)
                {
                    _animAbility = new AnimationAbility
                        (this,gameObject.GetComponent<AnimancerComponent>(),_idleStringAsset);
                }

                return _animAbility;
            }
        }


        protected EventAbility _eventAbility;

        [TimeScalable(Category = TimeScaleType.Logic)]
        public EventAbility EventAbility
        {
            get
            {
                if (_eventAbility == null)
                {
                    _eventAbility = new EventAbility(this,buffManager);
                }
                return _eventAbility;
            }
        }
        #endregion
        // Interaction ability.
        private InteractAbility _interactAbility;
        public InteractAbility InteractAbility
        {
            get
            {
                if (_interactAbility == null)
                {
                    _interactAbility = gameObject.GetComponentInChildren<InteractAbility>(true);
                }

                return _interactAbility;
            }
        }
        // Bullet ability.
        private BulletAbility _bulletAbility;
        [TimeScalable(Category = TimeScaleType.Bullet)]
        public BulletAbility BulletAbility
        {
            get
            {
                if (_bulletAbility == null)
                {
                    _bulletAbility = new BulletAbility(this);
                }

                return _bulletAbility;
            }
        }
        private SkillBallManager _skillBallManager;
        public SkillBallManager SkillBallManager
        {
            get
            {
                if (_skillBallManager == null)
                {
                    _skillBallManager = new SkillBallManager();
                    _skillBallManager.Initialize(this, null);
                }

                return _skillBallManager;
            }
        }

        [Header("Skill Ball Energy")]
        [SerializeField] private float skillBallEnergyPerBall = 100f;

        private SkillBallPitcher _skillBallPitcher;
        private ISkillBallSpawnProvider _skillBallSpawnProvider;
        private ActionPlayer temporaryFlagRegisteredPlayer;
        private bool actionOpenedImmune;
        private bool actionOpenedEndure;
        private bool actionOpenedUnbreakable;

        public SkillBallPitcher SkillBallPitcher
        {
            get
            {
                if (_skillBallPitcher == null)
                {
                    ReinitializeSkillBallPitcher();
                }

                return _skillBallPitcher;
            }
        }

        public void OpenSlideJudge()
        {
            _slideAction?.OpenJudge();
        }

        public void CloseSlideJudge()
        {
            _slideAction?.CloseJudge();
        }

        public void ForceCloseSlideJudge()
        {
            _slideAction?.ForceCloseJudge();
        }

        public void SetTemporaryCharacterPropertyFlag(CharacterPropertyFlagType flagType, bool value)
        {
            EnsureTemporaryFlagCleanupRegistered();

            if (_characterProperty == null)
            {
                Debug.LogWarning($"[ActionStateController] CharacterProperty is null. flag={flagType}, value={value}", this);
                return;
            }

            switch (flagType)
            {
                case CharacterPropertyFlagType.Immune:
                    _characterProperty.immune = value;
                    actionOpenedImmune |= value;
                    break;
                case CharacterPropertyFlagType.Endure:
                    _characterProperty.endure = value;
                    actionOpenedEndure |= value;
                    break;
                case CharacterPropertyFlagType.Unbreakable:
                    _characterProperty.unbreakable = value;
                    actionOpenedUnbreakable |= value;
                    break;
                default:
                    Debug.LogWarning($"[ActionStateController] Unsupported temporary flag. flag={flagType}", this);
                    break;
            }
        }

        public ISkillBallSpawnProvider SkillBallSpawnProvider
        {
            get
            {
                if (_skillBallSpawnProvider == null)
                {
                    _skillBallSpawnProvider = GetComponent<ISkillBallSpawnProvider>();
                }

                return _skillBallSpawnProvider;
            }
        }

        public void ReinitializeSkillBallPitcher()
        {
            _skillBallManager?.Initialize(this, null);
            _skillBallPitcher ??= new SkillBallPitcher();
            _skillBallPitcher.Initialize(
                this,
                SkillBallManager,
                SkillBallSpawnProvider,
                skillBallEnergyPerBall);
        }

        // Dodge ability.
                protected SlideAction _slideAction;
                protected BaseStateMachine _stateMachine;
        
                public BaseStateMachine StateMachine => _stateMachine;
                
                
        [Header("Movement")]
                        [SerializeField] protected CharacterMovementDriver movementDriver = new();
                        public CharacterMovementDriver MovementDriver => movementDriver;
                
        [Header("Attack Assist")]
        [SerializeField] private AttackAssistConfigSo attackAssistConfig;
        public AttackAssistConfigSo AttackAssistConfig => attackAssistConfig;

        private Vector3 attackAssistStepRemaining;
        private float attackAssistStepTimeRemaining;
        private float attackAssistTurnSpeed;
        private Quaternion attackAssistTargetRotation;
        private bool hasAttackAssistTurn;
                
                
                
                
        // Currently running state tag.
        /// <summary>
        /// Stores player character state data.
        /// </summary>
        public PlayerCharacterParameters PlayerParameters;
        public CharacterStateConfigSo _characterStateConfigSo;
        public ActionClipContainerSo actionClipContainerSo;
        public ComboInfoContainer comboInfoContainerSo;
        public SlideConfigSo slideConfigSo;
        
        [SerializeField] protected Blackboard.Data.Blackboard _blackboardPrefab;
        [ReadOnly]public Blackboard.Data.Blackboard RunningBlackboard;
        protected PlayerCharacterEvaluator _evaluator;
        public ActionClip idle;
        
        private Collider _defaultWeaponCollider;

        [SerializeField] private StringAsset _idleStringAsset;
        public float _inputBufferTimeout = 0.2f;
        
        
        protected PlayerCharacterBrain _playerCharacterBrain;
        
        
        
        
        
        public string characterId = "test1";
        public int infoId = 3001;
        public bool isPlayer = false;

        [Header("Lock Target")]
        [SerializeField] private bool faceLockTargetBeforeAttack = true;
        public bool FaceLockTargetBeforeAttack => faceLockTargetBeforeAttack;
        
        protected Collider _characterCollider;
        /// <summary>
        /// Holds the existing reference here; do not create a new instance.
        /// </summary>
        private CharacterProperty _characterProperty;
        public CharacterProperty CharacterProperty => _characterProperty;
        
        public bool joinTimeScale = true;
        
        protected void Start()
        {
            SendCharacterInfoToBattleManager();
            if(_blackboardPrefab == null)
            {
                Debug.LogError($"[ActionStateController] _blackboardPrefab is null");
                return;
            }
            if (movementDriver == null)
            {
                movementDriver = new CharacterMovementDriver();
            }
            movementDriver.Initialize(this);
        }

        protected virtual void FixedUpdate()
        {
            if (movementDriver != null && !movementDriver.UseRenderFrameMovement)
            {
                ApplyPendingMovement(Time.fixedDeltaTime);
            }
        }
        /// <summary>
        /// Prepares collision events.
        /// </summary>
        protected void InitCollide()
        {
            // Iterate through the container config and register collision events for each container.
            foreach (var actionClip in actionClipContainerSo.ActionClips.Values)
            {
                foreach (var collide in actionClip.collideDetection.collideDetectionClips)
                {
                    // Declare a local variable to capture the current loop value.
                    string currentId = collide.sliceId; 

                    _actionPlayer.RegisterCollideEvent(currentId, (collider) => 
                    {
                        // Capture the copy generated for this loop iteration.
                        CollideAbility._selfCollideManager.HandleCollide(currentId, collider);
                    });
                }
            }
            _characterCollider = gameObject.GetComponent<Collider>();
            if(_characterCollider == null)
            {
                Debug.LogError($"[ActionStateController] _characterCollider is null");
                return;
            }
        }
        protected void InitTimeScale()
        {
            if (!joinTimeScale) return;
            CharacterTimeScaleDispatcher dispatcher = gameObject.GetComponent<CharacterTimeScaleDispatcher>();
            if(dispatcher == null)
            {
                Debug.LogError($"[ActionStateController] dispatcher is null, can not initialize CharacterTimeScaleDispatcher");
                return;
            }
            dispatcher.Initialize(this);
        }
        private void SendCharacterInfoToBattleManager()
        {
            _characterProperty = BattleManager.Instance.JoinBattle(isPlayer,characterId,infoId,this);
            if(_characterProperty == null)
            {
                Debug.LogError($"[ActionStateController] _characterProperty is null");
                return;
            }
        }
        public void QueueMovement(Vector3 worldDir)
        {
            movementDriver?.QueueMovement(worldDir);
        }

        public void StopQueuedMovement()
        {
            movementDriver?.StopQueuedMovement();
        }

        public void FaceTargetImmediately(Transform target)
        {
            if (target == null)
            {
                return;
            }

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        public void StartAttackAssistStep(Vector3 worldDelta, float duration, float turnSpeed, Vector3 faceDirection)
        {
            attackAssistStepRemaining = worldDelta;
            attackAssistStepTimeRemaining = Mathf.Max(0.001f, duration);
            attackAssistTurnSpeed = Mathf.Max(0f, turnSpeed);

            faceDirection.y = 0f;
            hasAttackAssistTurn = faceDirection.sqrMagnitude > 0.0001f && attackAssistTurnSpeed > 0f;
            if (hasAttackAssistTurn)
            {
                attackAssistTargetRotation = Quaternion.LookRotation(faceDirection.normalized, Vector3.up);
            }
        }

        public void StartAttackAssistTurn(float turnSpeed, Vector3 faceDirection)
        {
            faceDirection.y = 0f;
            if (faceDirection.sqrMagnitude <= 0.0001f || turnSpeed <= 0f)
            {
                return;
            }

            attackAssistTurnSpeed = turnSpeed;
            attackAssistTargetRotation = Quaternion.LookRotation(faceDirection.normalized, Vector3.up);
            hasAttackAssistTurn = true;
        }

        public void MoveByRigidbody(Vector3 worldDir, float fixedDeltaTime)
        {
            movementDriver?.MoveByRigidbody(worldDir, fixedDeltaTime);
        }

        protected void MoveByTransform(Vector3 worldDir, float deltaTime)
        {
            movementDriver?.MoveByTransform(worldDir, deltaTime);
        }

        protected void ApplyPendingMovement(float fixedDeltaTime)
        {
            movementDriver?.ApplyPendingMovement(fixedDeltaTime);
        }

        protected void ApplyPendingMovementRenderFrame(float deltaTime)
        {
            movementDriver?.ApplyPendingMovementRenderFrame(deltaTime);
        }

        protected void TickActionAbilities(float deltaTime)
        {
            EnsureTemporaryFlagCleanupRegistered();
            _slideAction?.Tick(deltaTime);
            CollideAbility.Tick(deltaTime);
            ParticleAbility.Tick(deltaTime);
            BulletAbility.Tick(deltaTime);
            ApplyAttackAssistStep(deltaTime);
        }

        private void ApplyAttackAssistStep(float deltaTime)
        {
            if (hasAttackAssistTurn)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    attackAssistTargetRotation,
                    attackAssistTurnSpeed * deltaTime);

                if (Quaternion.Angle(transform.rotation, attackAssistTargetRotation) <= 0.01f)
                {
                    hasAttackAssistTurn = false;
                }
            }

            if (attackAssistStepTimeRemaining <= 0f)
            {
                return;
            }

            float consumeTime = Mathf.Min(deltaTime, attackAssistStepTimeRemaining);
            Vector3 frameDelta = attackAssistStepRemaining * (consumeTime / attackAssistStepTimeRemaining);

            attackAssistStepRemaining -= frameDelta;
            attackAssistStepTimeRemaining -= consumeTime;
            movementDriver?.MoveWorldDelta(frameDelta);
        }

        protected abstract void UpdateBehavior();

        private void EnsureTemporaryFlagCleanupRegistered()
        {
            if (_actionPlayer == null || temporaryFlagRegisteredPlayer == _actionPlayer)
            {
                return;
            }

            if (temporaryFlagRegisteredPlayer != null)
            {
                temporaryFlagRegisteredPlayer.OnEndStage -= ClearActionTemporaryState;
                temporaryFlagRegisteredPlayer.OnStopStage -= ClearActionTemporaryState;
            }

            temporaryFlagRegisteredPlayer = _actionPlayer;
            temporaryFlagRegisteredPlayer.OnEndStage -= ClearActionTemporaryState;
            temporaryFlagRegisteredPlayer.OnStopStage -= ClearActionTemporaryState;
            temporaryFlagRegisteredPlayer.OnEndStage += ClearActionTemporaryState;
            temporaryFlagRegisteredPlayer.OnStopStage += ClearActionTemporaryState;
        }

        public void ClearActionTemporaryState()
        {
            ForceCloseSlideJudge();
            ClearTemporaryCharacterPropertyFlags();
        }

        private void ClearTemporaryCharacterPropertyFlags()
        {
            if (_characterProperty != null)
            {
                if (actionOpenedImmune)
                {
                    _characterProperty.immune = false;
                }

                if (actionOpenedEndure)
                {
                    _characterProperty.endure = false;
                }

                if (actionOpenedUnbreakable)
                {
                    _characterProperty.unbreakable = false;
                }
            }

            actionOpenedImmune = false;
            actionOpenedEndure = false;
            actionOpenedUnbreakable = false;
        }

        protected virtual void OnDestroy()
        {
            if (temporaryFlagRegisteredPlayer != null)
            {
                temporaryFlagRegisteredPlayer.OnEndStage -= ClearActionTemporaryState;
                temporaryFlagRegisteredPlayer.OnStopStage -= ClearActionTemporaryState;
                temporaryFlagRegisteredPlayer = null;
            }

            ClearActionTemporaryState();
            _particleAbility?.ClearAllPools();
            _skillBallManager?.Dispose();
            _slideAction?.Dispose();
        }
        public virtual void OnCharacterDie()
        {
            OnCharacterDieAction?.Invoke();
            Debug.Log($"[ActionStateController] OnCharacterDieAction is null");
        }
      
        public virtual void OnCharacterHit(DamageInfo damageInfo)
        {
            Debug.Log($"[ActionStateController] OnCharacterHitAction is null");
        }
        [Button("SetEndure")]
        private void SetEndure(bool isEndure = true)
        {
            _characterProperty.endure = isEndure;
        }
        public virtual void OnCharacterBreak()
        {
            Debug.Log($"[ActionStateController] OnCharacterBreakAction is null");
        }
    }
}

