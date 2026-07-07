using System;
using Common;
using Common.Constant;
using Game.TimeScale;
using Tools.Battle.Data;
using Tools.InputHandle;
using Tools.Skill;
using UnityEngine;
using UnityEngine.Events;

namespace UI.UIAction.SkillBallUI
{
    /// <summary>
    /// Runtime bridge between the combat skill-ball UI and the active player controller.
    /// The UI only displays dispatcher events and reports clicked visible slots; the controller owns consume rules.
    /// </summary>
    public class ActionUISkillBallRuntimeBinder : MonoBehaviour
    {
        [Serializable]
        public class SkillBallSlotClickedEvent : UnityEvent<int>
        {
        }

        [Header("References")]
        [SerializeField] private ActionUISkillBallBar skillBallBar;
        [SerializeField] private ActionStateController targetController;

        [Header("Binding")]
        [SerializeField] private bool bindOnEnable = true;
        [SerializeField] private bool autoFindPlayerController = true;
        [SerializeField] private bool refreshVisibleBallsOnBind = true;

        [Header("Input")]
        [Tooltip("When enabled, clicking a UI ball writes the same temporary input data as keyboard skill-ball input.")]
        [SerializeField] private bool forwardClickToController = true;
        [SerializeField] private SkillBallSlotClickedEvent skillBallSlotClicked = new();

        private readonly RuntimeSkillDispatcher dispatcher = new();
        private ActionStateController boundController;
        private bool witchTimeEventRegistered;

        public BaseSkillDispatcher Dispatcher => dispatcher;
        public ActionStateController BoundController => boundController;

        private void Awake()
        {
            if (skillBallBar == null)
            {
                skillBallBar = GetComponent<ActionUISkillBallBar>();
            }
        }

        private void OnEnable()
        {
            if (skillBallBar != null)
            {
                skillBallBar.SkillBallSlotClicked -= HandleSkillBallSlotClicked;
                skillBallBar.SkillBallSlotClicked += HandleSkillBallSlotClicked;
            }

            if (bindOnEnable)
            {
                Bind(targetController);
            }

            TryRegisterWitchTimeEvents();
        }

        private void Start()
        {
            if (bindOnEnable && boundController == null)
            {
                Bind(targetController);
            }
        }

        private void OnDisable()
        {
            if (skillBallBar != null)
            {
                skillBallBar.SkillBallSlotClicked -= HandleSkillBallSlotClicked;
            }

            Unbind();
            TryUnregisterWitchTimeEvents();
        }

        public void Bind(ActionStateController controller)
        {
            if (controller == null && autoFindPlayerController)
            {
                controller = FindPlayerController();
            }

            if (skillBallBar == null)
            {
                Debug.LogWarning("[ActionUISkillBallRuntimeBinder] Missing ActionUISkillBallBar.", this);
                return;
            }

            if (controller == null)
            {
                skillBallBar.Bind(null);
                boundController = null;
                return;
            }

            boundController = controller;
            boundController.SkillBallManager.Initialize(boundController, dispatcher);
            skillBallBar.Bind(dispatcher);

            if (refreshVisibleBallsOnBind)
            {
                dispatcher.NotifySkillBallsAdded(boundController.SkillBallManager.BuildVisibleViewData());
            }
        }

        public void Unbind()
        {
            if (skillBallBar != null)
            {
                skillBallBar.Bind(null);
            }

            boundController = null;
        }

        public void RefreshVisibleBalls()
        {
            if (boundController == null)
            {
                return;
            }

            dispatcher.NotifySkillBallsAdded(boundController.SkillBallManager.BuildVisibleViewData());
        }

        public void NotifyMatrixModeChanged(bool isInMatrixMode)
        {
            dispatcher.NotifyMatrixModeChanged(isInMatrixMode);
        }

        private void TryRegisterWitchTimeEvents()
        {
            if (witchTimeEventRegistered)
            {
                return;
            }

            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter == null)
            {
                return;
            }

            eventCenter.AddEventListener<WitchTimeRuntimeInfo>(
                GlobalEvents.OnWitchTimeStarted,
                HandleWitchTimeStarted);
            eventCenter.AddEventListener<WitchTimeRuntimeInfo>(
                GlobalEvents.OnWitchTimeStopped,
                HandleWitchTimeStopped);
            witchTimeEventRegistered = true;
        }

        private void TryUnregisterWitchTimeEvents()
        {
            if (!witchTimeEventRegistered)
            {
                return;
            }

            var eventCenter = GameApplication.Instance?.EventCenter;
            if (eventCenter != null)
            {
                eventCenter.RemoveEventListener<WitchTimeRuntimeInfo>(
                    GlobalEvents.OnWitchTimeStarted,
                    HandleWitchTimeStarted);
                eventCenter.RemoveEventListener<WitchTimeRuntimeInfo>(
                    GlobalEvents.OnWitchTimeStopped,
                    HandleWitchTimeStopped);
            }

            witchTimeEventRegistered = false;
        }

        private void HandleWitchTimeStarted(WitchTimeRuntimeInfo info)
        {
            if (boundController == null || info.casterId != boundController.characterId)
            {
                return;
            }

            NotifyMatrixModeChanged(true);
        }

        private void HandleWitchTimeStopped(WitchTimeRuntimeInfo info)
        {
            if (boundController == null || info.casterId != boundController.characterId)
            {
                return;
            }

            NotifyMatrixModeChanged(false);
        }

        private void HandleSkillBallSlotClicked(int visibleIndex)
        {
            skillBallSlotClicked?.Invoke(visibleIndex);

            if (!forwardClickToController || boundController == null)
            {
                return;
            }

            if (!boundController.SkillBallManager.TryPrepareUseBall(visibleIndex))
            {
                return;
            }

            if (boundController.PlayerParameters == null)
            {
                boundController.PlayerParameters = new PlayerCharacterParameters();
            }

            boundController.PlayerParameters.isSkillBallInFrame = true;
            boundController.PlayerParameters.skillBallInputIndex = visibleIndex;
        }

        private static ActionStateController FindPlayerController()
        {
            ActionStateController[] controllers = FindObjectsOfType<ActionStateController>();
            foreach (ActionStateController controller in controllers)
            {
                if (controller != null && controller.isPlayer)
                {
                    return controller;
                }
            }

            return controllers.Length > 0 ? controllers[0] : null;
        }

        private class RuntimeSkillDispatcher : BaseSkillDispatcher
        {
        }
    }
}
