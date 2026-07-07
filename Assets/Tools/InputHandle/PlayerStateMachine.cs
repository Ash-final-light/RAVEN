using Common.Constant;
using Tools.Action.Data.Slide;
using Tools.Action.Running;
using Tools.Battle;
using Tools.Battle.Data;
using Tools.Battle.Extension;
using Tools.InputHandle.Data;
using UnityEngine;

namespace Tools.InputHandle
{
    public class PlayerStateMachine : BaseStateMachine
    {
        private readonly PlayerCharacterEvaluator _playerCharacterEvaluator;
        private readonly SlideAction _slideAction;

        public PlayerStateMachine(
            ActionStateController actionStateController,
            PlayerCharacterEvaluator playerCharacterEvaluator,
            SlideAction slideAction) : base(actionStateController)
        {
            _playerCharacterEvaluator = playerCharacterEvaluator;
            _slideAction = slideAction;

            Owner._actionPlayer.OnEndStage += HandleIdle;
        }

        protected void HandleIdle()
        {
            if (Owner._actionPlayer.CurrentActionClipName == AnimActionConstants.Run ||
                Owner._actionPlayer.CurrentActionClipName == AnimActionConstants.idle01)
            {
                return;
            }

            if (Owner._actionPlayer.CurrentActionClipName == AnimActionConstants.RunStart &&
                Owner.PlayerParameters.playerInputRawValueInFrame.sqrMagnitude > 0.001f)
            {
                TryPlayAction(AnimActionConstants.Run, true, CharacterStateType.Move);
                return;
            }

            _currentStateType = CharacterStateType.Idle;
            Owner.StopQueuedMovement();
            Owner.ForceCloseSlideJudge();
            Owner._actionPlayer.SetActionClipAndPlay(Owner, Owner.idle, true);
        }

        protected void HandleSlide()
        {
            if (_slideAction == null || !_slideAction.CanSlide())
            {
                return;
            }

            if (TryPlayAction(AnimActionConstants.SlideBehind, false, CharacterStateType.Slide))
            {
                _slideAction.RecordSlide();
            }
        }

        protected void HandleMove()
        {
            Vector3 worldDir = Owner.PlayerParameters.playerInputMovementInFrame;

            if (worldDir.sqrMagnitude < 0.001f)
            {
                Owner.StopQueuedMovement();
                if (_currentStateType == CharacterStateType.Move)
                {
                    _currentStateType = CharacterStateType.Idle;
                    TryPlayAction(AnimActionConstants.RunStop, false);
                }
                return;
            }

            if (_currentStateType != CharacterStateType.Move)
            {
                TryPlayAction(AnimActionConstants.RunStart, false, CharacterStateType.Move);
                return;
            }

            ApplyMovement(worldDir);
        }

        private void ApplyMovement(Vector3 worldDir)
        {
            Owner.QueueMovement(worldDir);
        }

        protected override void UpdateCurrentState(float deltaTime)
        {
            if (Owner.PlayerParameters.isUltimateInFrame && HandleUltimate())
            {
                return;
            }

            if (Owner.PlayerParameters.isSkillBallInFrame)
            {
                HandleSkillBall();
                return;
            }

            if (Owner.PlayerParameters.isAttackInFrame)
            {
                string nextAction = _playerCharacterEvaluator.GetNextAction(CurrentAction, InputConstants.PlayerAttack);
                if (nextAction != null)
                {
                    AttackAssistRunner.Apply(Owner);
                    TryPlayAction(nextAction, false);
                    return;
                }
                
            }

            if (Owner.PlayerParameters.isSlideInFrame && CanTransitionTo(CharacterStateType.Slide))
            {
                HandleSlide();
            }
            else if (CanTransitionTo(CharacterStateType.Move))
            {
                HandleMove();
            }
        }

        private bool HandleUltimate()
        {
            if (!HasFullUltimateEnergy())
            {
                return false;
            }

            string nextAction = _playerCharacterEvaluator.GetNextAction(CurrentAction, InputConstants.PlayerUltimate);
            if (string.IsNullOrEmpty(nextAction))
            {
                return false;
            }

            if (!TryPlayAction(nextAction, false, CharacterStateType.Ultimate))
            {
                return false;
            }

            if (!BattleManager.Instance.TryConsumeAllMpForUltimate(Owner.characterId))
            {
                Debug.LogWarning($"[StateMachine] Ultimate played but MP consume failed. character={Owner.characterId}");
            }

            return true;
        }

        private bool HasFullUltimateEnergy()
        {
            CharacterProperty property = Owner.CharacterProperty;
            if (property == null)
            {
                return false;
            }

            int maxMp = property.GetCurrentMaxMp();
            return maxMp > 0 && property.CurrentMp >= maxMp;
        }

        private void HandleSkillBall()
        {
            string signalInputName = Owner.PlayerParameters.consumedSignalInputName;

            if (string.IsNullOrEmpty(signalInputName))
            {
                Owner.SkillBallManager.CancelPreparedConsume();
                return;
            }

            string nextAction = _playerCharacterEvaluator.GetNextAction(CurrentAction, signalInputName);
            if (string.IsNullOrEmpty(nextAction))
            {
                Owner.SkillBallManager.CancelPreparedConsume();
                return;
            }

            Owner.PlayerParameters.consumedSkillActionName = nextAction;

            if (!Owner.SkillBallManager.MarkPreparedConsumeForActionStart(nextAction))
            {
                Owner.SkillBallManager.CancelPreparedConsume();
                return;
            }

            if (TryPlayAction(nextAction, false, CharacterStateType.Skill))
            {
                return;
            }

            Owner.SkillBallManager.CancelPreparedConsume();
        }

    }
}
