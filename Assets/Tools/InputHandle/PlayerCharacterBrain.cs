using Common.Constant;
using JetBrains.Annotations;
using Tools.Camera;

using Tools.InputHandle.Data;
using UnityEngine;

namespace Tools.InputHandle
{
    public class PlayerCharacterBrain
    {
        private PlayerInputManager _playerInputManager;
        private ICameraModel cameraModel;
        
        private ActionStateController Owner;
        private Transform _cameraTransform;
        
        private bool debugInputBuffer = false;
        private bool allowInput = true;
        private readonly InputBuffer _inputBuffer = new();

        public PlayerCharacterBrain(ActionStateController actionStateController, 
            [CanBeNull]ICameraModel cameraModel, PlayerInputManager playerInputManager
            )
        {
            Owner = actionStateController;
            this.cameraModel = cameraModel;
            this._playerInputManager = playerInputManager;
            _inputBuffer.Init(playerInputManager.PlayerInput,
                Owner._inputBufferTimeout,true);   
            OnBrianInit();
        }
        ~PlayerCharacterBrain()
        {
            UnRegisterStageActions();
            _inputBuffer.Clear();
        }

        private void OnBrianInit()
        {
            if (UnityEngine.Camera.main != null) _cameraTransform = UnityEngine.Camera.main.transform;

            RegisterInputActions();
            RegisterStageActions();
        }

        private void RegisterInputActions()
        {
            _inputBuffer.Register(InputConstants.PlayerInteract);
            _inputBuffer.Register(InputConstants.PlayerAttack);
            _inputBuffer.Register(InputConstants.PlayerSlide);
            _inputBuffer.Register(InputConstants.PlayerUltimate);
            
            _inputBuffer.Register(InputConstants.SkillBall0);
            _inputBuffer.Register(InputConstants.SkillBall1);
            _inputBuffer.Register(InputConstants.SkillBall2);
            _inputBuffer.Register(InputConstants.SkillBall3);
            _inputBuffer.Register(InputConstants.SkillBall4);
            _inputBuffer.Register(InputConstants.SkillBall5);
            _inputBuffer.Register(InputConstants.SkillBall6);
            _inputBuffer.Register(InputConstants.SkillBall7);
        }

        private void RegisterStageActions()
        {
            Owner._actionPlayer.OnStopStage += DisableInput;
            Owner._actionPlayer.OnStartStage += DisableInput;
            Owner._actionPlayer.OnStartStage += DisableIgnorePriority;
            //前摇阶段接受任意输入，但存在优先级判断
            Owner._actionPlayer.OnAnticipationStage += EnablePreInput;
            //判断阶段接受任意输入，无优先级判断，可以连击
            Owner._actionPlayer.OnJudgmentStage += EnablePreInput;
            Owner._actionPlayer.OnJudgmentStage += EnableIgnorePriority;
            Owner._actionPlayer.OnJudgmentStage += EnableComboWindow;
            //后摇阶段接受任意输入，无优先级判断，连击中断
            Owner._actionPlayer.OnRecoveryStage += EnablePreInput;
            Owner._actionPlayer.OnRecoveryStage += EnableIgnorePriority;
            Owner._actionPlayer.OnRecoveryStage += DisableComboWindow;
            Owner._actionPlayer.OnEndStage += EnablePreInput;
        }
        private void UnRegisterStageActions()
        {
            Owner._actionPlayer.OnStopStage -= DisableInput;
            Owner._actionPlayer.OnStartStage -= DisableInput;
            Owner._actionPlayer.OnStartStage -= DisableIgnorePriority;
            //前摇阶段接受任意输入，但存在优先级判断
            Owner._actionPlayer.OnAnticipationStage -= EnablePreInput;
            //判断阶段接受任意输入，无优先级判断，可以连击
            Owner._actionPlayer.OnJudgmentStage -= EnablePreInput;
            Owner._actionPlayer.OnJudgmentStage -= EnableIgnorePriority;
            Owner._actionPlayer.OnJudgmentStage -= EnableComboWindow;
            //后摇阶段接受任意输入，无优先级判断，连击中断
            Owner._actionPlayer.OnRecoveryStage -= EnablePreInput;
            Owner._actionPlayer.OnRecoveryStage -= EnableIgnorePriority;
            Owner._actionPlayer.OnRecoveryStage -= DisableComboWindow;
            Owner._actionPlayer.OnEndStage -= EnablePreInput;
        }

        private void DisableInput()
        {
            allowInput = false;
        }

        private void EnablePreInput()
        {
            allowInput = true;
        }
        private void EnableIgnorePriority()
        {
            Owner.StateMachine._ignorePriority = true;
        }
        private void DisableIgnorePriority()
        {
            Owner.StateMachine._ignorePriority = false;
        }
        private void EnableComboWindow()
        {
            Owner.RunningBlackboard.SetBoolParameter(ComboBlackboardConstants.ComboWindow,true);
        }
        private void DisableComboWindow()
        {
            Owner.RunningBlackboard.SetBoolParameter(ComboBlackboardConstants.ComboWindow,false);
        }

        /// <summary>
        /// 玩家角色大脑的渲染帧负责玩家的输入以及时响应玩家输入的业务数据更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public void OnRenderThoughtsUpdated(float deltaTime)
        {
            // 更新角色参数
            UpdateParameters();

            // 如果允许输入响应，则处理输入相关
            if (Owner.PlayerParameters.allowInputReaction && allowInput)
            {
                // 每帧执行输入缓存
                _inputBuffer.Tick(deltaTime);
                // 这里只获取每帧渲染时玩家的输入，不涉及具体逻辑数据
                GetPlayerInput();
                // 处理交互输入
                HandlePlayerInteract();
                CalculatePlayerInput();
            }

            return;

            void UpdateParameters()
            {
                // 更新数据
                //Owner.PlayerParameters.cameraLockData = cameraModel.GetLock();
                Owner.PlayerParameters.evadeAttackCountdown =
                    Mathf.Max(Owner.PlayerParameters.evadeAttackCountdown - deltaTime, 0f);

                // 清除按键输入
                Owner.PlayerParameters.isInteractInFrame = false;
                Owner.PlayerParameters.isAttackInFrame = false;
                Owner.PlayerParameters.isSlideInFrame = false;
                Owner.PlayerParameters.isUltimateInFrame = false;
                Owner.PlayerParameters.isSkillBallInFrame = false;
                Owner.PlayerParameters.skillBallInputIndex = -1;

                // 清除移动输入
                Owner.PlayerParameters.playerInputRawValueInFrame = Vector2.zero;
                Owner.PlayerParameters.playerInputMovementInFrame = Vector3.zero;
                Owner.PlayerParameters.playerInputCharacterMovementInFrame = Vector3.zero;
            }

            void GetPlayerInput()
            {
                if (_playerInputManager.WasPerformedThisFrame(InputConstants.PlayerInteract))  
                {
                    _inputBuffer.ConsumeAction(InputConstants.PlayerInteract);
                    Owner.PlayerParameters.isInteractInFrame = true;
                }

                // 玩家连招输入采用每帧玩家输入以及预输入混合，仅最后一次输入有效
                if (_playerInputManager.WasPerformedThisFrame(InputConstants.PlayerSlide) ||
                    _inputBuffer.WasPerformedThisFrame(InputConstants.PlayerSlide))
                {
                    _inputBuffer.ConsumeAction(InputConstants.PlayerSlide);
                    Owner.PlayerParameters.isSlideInFrame = true;   
                }
                
                if (_playerInputManager.WasPerformedThisFrame(InputConstants.PlayerAttack) ||
                    _inputBuffer.WasPerformedThisFrame(InputConstants.PlayerAttack))
                {
                    _inputBuffer.ConsumeAction(InputConstants.PlayerAttack);
                    Owner.PlayerParameters.isAttackInFrame = true;
                }

                if (_playerInputManager.WasPerformedThisFrame(InputConstants.PlayerUltimate) ||
                    _inputBuffer.WasPerformedThisFrame(InputConstants.PlayerUltimate))
                {
                    _inputBuffer.ConsumeAction(InputConstants.PlayerUltimate);
                    Owner.PlayerParameters.isUltimateInFrame = true;
                }
                
                TryHandleSkillBallInput(InputConstants.SkillBall0, 0);
                TryHandleSkillBallInput(InputConstants.SkillBall1, 1);
                TryHandleSkillBallInput(InputConstants.SkillBall2, 2);
                TryHandleSkillBallInput(InputConstants.SkillBall3, 3);
                TryHandleSkillBallInput(InputConstants.SkillBall4, 4);
                TryHandleSkillBallInput(InputConstants.SkillBall5, 5);
                TryHandleSkillBallInput(InputConstants.SkillBall6, 6);
                TryHandleSkillBallInput(InputConstants.SkillBall7, 7);
                
            }

            void TryHandleSkillBallInput(string actionName, int visibleIndex)
            {
                if (!_playerInputManager.WasPerformedThisFrame(actionName) &&
                    !_inputBuffer.WasPerformedThisFrame(actionName))
                {
                    return;
                }

                _inputBuffer.ConsumeAction(actionName);

                if (!Owner.SkillBallManager.TryPrepareUseBall(visibleIndex))
                {
                    return;
                }

                Owner.PlayerParameters.isSkillBallInFrame = true;
                Owner.PlayerParameters.skillBallInputIndex = visibleIndex;
            }

            void HandlePlayerInteract()
            {
                if (Owner.PlayerParameters.isInteractInFrame)
                {
                    Owner.InteractAbility.Interact();
                }
            }

            void CalculatePlayerInput()
            {
                // 1. 获取玩家原始操作输入
                var moveInputAction = _playerInputManager.GetInputAction(InputConstants.PlayerMove);
                Vector2 rawInput = moveInputAction == null ? Vector2.zero : moveInputAction.ReadValue<Vector2>();
                Owner.PlayerParameters.playerInputRawValueInFrame = rawInput;

                // 2. 输入有效性及归一化处理（解决手柄推不到底或对角线数值过大问题）
                if (rawInput.sqrMagnitude < 0.001f)
                {
                    Owner.PlayerParameters.playerInputMovementInFrame = Vector3.zero;
                    Owner.PlayerParameters.playerInputCharacterMovementInFrame = Vector3.zero;
                    return;
                }
                Vector2 normalizedInput = rawInput.normalized;

                // 3. 计算摄像机在 XZ 平面的投影向量并重新归一化
                // 哪怕相机垂直看地面，也能保证计算出的水平分量是单位向量
                Vector3 camForwardProjection = new Vector3(_cameraTransform.forward.x, 0, _cameraTransform.forward.z);
                camForwardProjection.Normalize(); 
    
                Vector3 camRightProjection = new Vector3(_cameraTransform.right.x, 0, _cameraTransform.right.z);
                camRightProjection.Normalize();

                // 4. 根据摄像机坐标系重新计算世界空间的输入方向方向
                // 此时 worldMoveDir 的模长在 normalizedInput 的作用下理论上为 1
                Vector3 worldMoveDir = camForwardProjection * normalizedInput.y + camRightProjection * normalizedInput.x;
                Owner.PlayerParameters.playerInputMovementInFrame = worldMoveDir;

                // 5. 将世界坐标系下的移动方向转为角色本地坐标系
                // 使用 InverseTransformDirection 屏蔽 Scale (100倍缩放) 的影响
                Vector3 localMoveDir = Owner.transform.InverseTransformDirection(worldMoveDir);

                // 6. 最终归一化并赋值（确保动画混合树等参数在 0-1 范围内精确受控）
                if (localMoveDir.sqrMagnitude > 0.001f)
                {
                    localMoveDir.Normalize();
                }
                Owner.PlayerParameters.playerInputCharacterMovementInFrame = localMoveDir;
            }
        }
        
    }
}
