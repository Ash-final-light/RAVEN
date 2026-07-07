using System;
using System.Collections.Generic;
using System.Reflection;
using Animancer;
using Tools.Action.Data;
using Tools.Action.Data.Collide;
using Tools.Action.Data.Effect;
using Tools.Action.Running;
using Tools.Battle.Data;
using Tools.Damage.SO;
using Tools.InputHandle;
using Tools.InputHandle.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.Skill
{
    public class SkillBallRuntimeTester : MonoBehaviour
    {
        private const string TesterRootName = "SkillBallMechanicTester";
        private const string ResultTextName = "SkillBallMechanicTestResult";
        private const string IdleActionName = "idle01";
        private const string BlueSkillActionName = "SkillBall_Test_Blue";
        private const string TestSliceId = "SkillBall_Test_Slice";

        [SerializeField] private Text resultText;

        [ContextMenu("Run Skill Ball Runtime Tests")]
        public void RunSkillBallRuntimeTests()
        {
            string report = RunTests();
            Debug.Log(report);
            SetResultText(report);
        }

        public static void SetupInActionUISceneAndRun()
        {
            GameObject root = GameObject.Find(TesterRootName);
            if (root == null)
            {
                root = new GameObject(TesterRootName);
            }

            SkillBallRuntimeTester tester = root.GetComponent<SkillBallRuntimeTester>();
            if (tester == null)
            {
                tester = root.AddComponent<SkillBallRuntimeTester>();
            }

            tester.resultText = FindOrCreateResultText();
            tester.RunSkillBallRuntimeTests();
        }

        private string RunTests()
        {
            List<string> lines = new();
            bool allPassed = true;

            void Check(bool condition, string message)
            {
                lines.Add($"{(condition ? "PASS" : "FAIL")} - {message}");
                allPassed &= condition;
            }

            SkillBallTestActionController controller = GetOrCreateController();
            ActionClip idleClip = CreateActionClip(IdleActionName, CharacterStateType.Idle);
            ActionClip skillClip = CreateActionClip(BlueSkillActionName, CharacterStateType.Skill);
            DamageHitConfigSO defaultDamage = CreateDamageConfig("DefaultDamage");
            DamageHitConfigSO tripleDamage = CreateDamageConfig("TriplePingDamage");
            ActionCollideDetectionClipData collideClip = CreateCollideClip(defaultDamage, tripleDamage);
            skillClip.collideDetection.collideDetectionClips.Add(collideClip);

            controller.InitializeForSkillBallTest(idleClip, skillClip);

            ComboInfoContainer combo = CreateComboInfo();
            PlayerCharacterEvaluator evaluator = new(
                combo,
                ScriptableObject.CreateInstance<global::Tools.Blackboard.Data.Blackboard>());

            controller.SkillBallManager.TryAppendBall(new SkillBallData(
                201,
                SkillBallColor.Blue,
                SkillBallConsumeMode.AlwaysTripleCheck,
                "test-blue-icon"));
            controller.SkillBallManager.TryAppendBall(new SkillBallData(
                202,
                SkillBallColor.Blue,
                SkillBallConsumeMode.AlwaysTripleCheck,
                "test-blue-icon"));

            bool twoPrepared = controller.SkillBallManager.TryPrepareUseBall(1);
            Check(twoPrepared, "slot 1 has two adjacent blue balls and can prepare consume");
            Check(controller.PlayerParameters.consumedSignalInputName == SkillBallInputNames.Blue2, "Blue two-ping maps to SkillBallBlue2 combo input");
            Check(controller.PlayerParameters.consumedSignalCount == 2, "two adjacent blue balls resolve as 2-ping");

            string twoPingAction = evaluator.GetNextAction(
                controller.ActionPlayer.CurrentActionClipName,
                controller.PlayerParameters.consumedSignalInputName);
            Check(twoPingAction == BlueSkillActionName, "CurrentAction + SkillBallBlue2 resolves to blue skill action");

            controller.SkillBallManager.CancelPreparedConsume();
            controller.SkillBallManager.ClearBalls();

            controller.SkillBallManager.SetMatrixMode(true);
            controller.SkillBallManager.TryAppendBall(new SkillBallData(
                301,
                SkillBallColor.Red,
                SkillBallConsumeMode.AlwaysTripleCheck,
                "test-red-icon"));

            bool matrixPrepared = controller.SkillBallManager.TryPrepareUseBall(0);
            Check(matrixPrepared, "witch time matrix mode can prepare a single red ball");
            Check(controller.PlayerParameters.consumedSignalInputName == SkillBallInputNames.Red3, "witch time maps single red ball to Red three-ping input");
            Check(controller.PlayerParameters.consumedSignalCount == 3, "witch time treats single red ball as 3-ping");

            controller.SkillBallManager.CancelPreparedConsume();
            controller.SkillBallManager.ClearBalls();
            controller.SkillBallManager.SetMatrixMode(false);

            controller.SkillBallManager.TryAppendBall(new SkillBallData(
                101,
                SkillBallColor.Blue,
                SkillBallConsumeMode.AlwaysTripleCheck,
                "test-blue-icon"));
            controller.SkillBallManager.TryAppendBall(new SkillBallData(
                102,
                SkillBallColor.Blue,
                SkillBallConsumeMode.AlwaysTripleCheck,
                "test-blue-icon"));
            controller.SkillBallManager.TryAppendBall(new SkillBallData(
                103,
                SkillBallColor.Blue,
                SkillBallConsumeMode.AlwaysTripleCheck,
                "test-blue-icon"));

            bool prepared = controller.SkillBallManager.TryPrepareUseBall(0);
            Check(prepared, "slot 0 has a skill ball and can prepare consume");
            Check(controller.PlayerParameters.consumedSignalColor == SkillBallColor.Blue, "slot 0 color resolved as Blue");
            Check(controller.PlayerParameters.consumedSignalInputName == SkillBallInputNames.Blue3, "Blue three-ping maps to SkillBallBlue3 combo input");
            Check(controller.PlayerParameters.consumedSignalCount == 3, "three adjacent blue balls resolve as 3-ping");

            string nextAction = evaluator.GetNextAction(
                controller.ActionPlayer.CurrentActionClipName,
                controller.PlayerParameters.consumedSignalInputName);
            Check(nextAction == BlueSkillActionName, "CurrentAction + SkillBallBlue3 resolves to blue skill action");

            controller.PlayerParameters.consumedSkillActionName = nextAction;
            bool marked = controller.SkillBallManager.MarkPreparedConsumeForActionStart(nextAction);
            Check(marked, "consume command marked before action starts");

            controller.ActionPlayer.SetActionClipAndPlay(controller, skillClip, false);
            Check(controller.SkillBallManager.GetBalls().Count == 0, "skill balls are removed at ActionPlayer Start stage");
            Check(controller.ActionPlayer.RuntimeContext.ConsumedSignalCount == 3, "RuntimeContext stores consumed signal count");
            Check(controller.ActionPlayer.RuntimeContext.ConsumedSignalInputName == SkillBallInputNames.Blue3, "RuntimeContext stores consumed signal input name");
            Check(controller.PlayerParameters.consumedSignalCount == 0, "PlayerCharacterParameters temporary consume count is released after Start");
            Check(controller.PlayerParameters.consumedSignalInputName == null, "PlayerCharacterParameters temporary signal input is released after Start");

            DamageHitConfigSO resolvedDamage = ResolveDamageConfig(controller, collideClip.sliceId);
            Check(resolvedDamage == tripleDamage, "SelfCollideManager resolves 3-ping DamageHitConfigSO from RuntimeContext");

            controller.ActionPlayer.SetActionClipAndPlay(controller, idleClip, true);
            Check(controller.ActionPlayer.RuntimeContext.ConsumedSignalCount == 0, "RuntimeContext clears when action changes");

            lines.Insert(0, allPassed ? "Skill ball runtime tests: PASS" : "Skill ball runtime tests: FAIL");
            return string.Join(Environment.NewLine, lines);
        }

        private SkillBallTestActionController GetOrCreateController()
        {
            SkillBallTestActionController controller = GetComponent<SkillBallTestActionController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<SkillBallTestActionController>();
            }

            if (GetComponent<AnimancerComponent>() == null)
            {
                gameObject.AddComponent<AnimancerComponent>();
            }

            if (GetComponent<Animator>() == null)
            {
                gameObject.AddComponent<Animator>();
            }

            return controller;
        }

        private static ActionClip CreateActionClip(string actionName, CharacterStateType stateType)
        {
            ActionClip clip = ScriptableObject.CreateInstance<ActionClip>();
            clip.name = actionName;
            clip.frameRate = 30;
            clip.duration = 1f;
            clip.totalTicks = 30;
            clip.StateType = stateType;
            clip.process = new ActionProcess
            {
                anticipationTick = 1,
                judgmentTick = 5,
                recoveryTick = 20,
                anticipationTime = 1f / 30f,
                judgmentTime = 5f / 30f,
                recoveryTime = 20f / 30f
            };
            clip.animation = new ActionAnimation();
            clip.audio = new ActionAudio();
            clip.effect = new ActionEffect();
            clip.collideDetection = new ActionCollideDetection();
            clip.events = new ActionEvent();
            return clip;
        }

        private static ActionCollideDetectionClipData CreateCollideClip(
            DamageHitConfigSO defaultDamage,
            DamageHitConfigSO tripleDamage)
        {
            ActionCollideDetectionClipData clip = new()
            {
                sliceId = TestSliceId,
                collideEventType = ActionCollideEventType.CauseDamage,
                collideHitConfig = defaultDamage,
                startTick = 5,
                durationTicks = 5,
                data = new ActionCollideDetectionEmptyData()
            };
            clip.signalBallDamageConfigs.Add(new SignalBallDamageConfig
            {
                consumedSignalCount = 3,
                damageConfig = tripleDamage
            });
            return clip;
        }

        private static DamageHitConfigSO CreateDamageConfig(string name)
        {
            DamageHitConfigSO config = ScriptableObject.CreateInstance<DamageHitConfigSO>();
            config.name = name;
            return config;
        }

        private static ComboInfoContainer CreateComboInfo()
        {
            ComboInfoContainer combo = ScriptableObject.CreateInstance<ComboInfoContainer>();
            Input2ActionTable table = ScriptableObject.CreateInstance<Input2ActionTable>();
            table.currentActionName = IdleActionName;
            table.inputName = SkillBallInputNames.Blue3;
            table.comboActionName = BlueSkillActionName;
            table.priority = 1;
            Input2ActionTable twoPingTable = ScriptableObject.CreateInstance<Input2ActionTable>();
            twoPingTable.currentActionName = IdleActionName;
            twoPingTable.inputName = SkillBallInputNames.Blue2;
            twoPingTable.comboActionName = BlueSkillActionName;
            twoPingTable.priority = 1;
            combo.inputTables = new List<Input2ActionTable> { table, twoPingTable };
            return combo;
        }

        private static DamageHitConfigSO ResolveDamageConfig(
            SkillBallTestActionController controller,
            string sliceId)
        {
            SelfCollideManager manager = new(controller);
            MethodInfo method = typeof(SelfCollideManager).GetMethod(
                "TryResolveDamageConfig",
                BindingFlags.Instance | BindingFlags.NonPublic);

            object[] args = { sliceId, null };
            bool resolved = method != null && (bool)method.Invoke(manager, args);
            return resolved ? args[1] as DamageHitConfigSO : null;
        }

        private void SetResultText(string report)
        {
            if (resultText == null)
            {
                resultText = FindOrCreateResultText();
            }

            resultText.text = report;
        }

        private static Text FindOrCreateResultText()
        {
            GameObject existing = GameObject.Find(ResultTextName);
            if (existing != null && existing.TryGetComponent(out Text existingText))
            {
                return existingText;
            }

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new("Canvas_SkillBallRuntimeTest", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
            }

            GameObject textGo = new(ResultTextName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(canvas.transform, false);
            RectTransform rect = textGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -24f);
            rect.sizeDelta = new Vector2(620f, 360f);

            Text text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }
    }

    public class SkillBallTestActionController : ActionStateController
    {
        private new void Start()
        {
        }

        protected override void UpdateBehavior()
        {
        }

        public void InitializeForSkillBallTest(ActionClip idleClip, ActionClip skillClip)
        {
            SetEmptyParticleInfo();
            PlayerParameters ??= new PlayerCharacterParameters();
            idle = idleClip;
            actionClipContainerSo = ScriptableObject.CreateInstance<ActionClipContainerSo>();
            actionClipContainerSo.ActionClips = new Dictionary<string, ActionClip>
            {
                { idleClip.name, idleClip },
                { skillClip.name, skillClip }
            };

            _actionPlayer = new ActionPlayer(idleClip, this, true);
            _actionPlayer.Start();
            SkillBallManager.Initialize(this, null);
        }

        private void SetEmptyParticleInfo()
        {
            FieldInfo field = typeof(ActionStateController).GetField(
                "test_particleinfo",
                BindingFlags.Instance | BindingFlags.NonPublic);

            field?.SetValue(this, Array.Empty<ParticlePrefabInfo>());
        }
    }
}
