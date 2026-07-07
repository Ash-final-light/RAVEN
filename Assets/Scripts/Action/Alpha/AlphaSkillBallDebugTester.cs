using System.Text;
using System.Collections;
using Sirenix.OdinInspector;
using Tools.Action.Running;
using Tools.InputHandle;
using Tools.Skill;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Scripts.Action.Alpha
{
    [DefaultExecutionOrder(-10000)]
    public class AlphaSkillBallDebugTester : MonoBehaviour
    {
        [SerializeField] private ActionStateController owner;
        [SerializeField] private AlphaStatusController statusController;
        [SerializeField] private Status swordIntentStatus;

        [Header("Runtime View")]
        [SerializeField] private bool bluePrepared;
        [SerializeField] private bool swordIntentActive;
        [SerializeField] private string currentBalls;
        [SerializeField] private string lastSmokeTestResult;

        private Coroutine smokeTestRoutine;
        private bool hasPendingKeyPress;
        private Key pendingKey;
        private int pendingKeyPhase;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void Update()
        {
            ProcessPendingKeyPress();
            RefreshRuntimeView();
        }

        [ContextMenu("Alpha Skill Ball Test/Add Blue Triple")]
        [Button("Add Blue Triple")]
        public void AddBlueTriple()
        {
            AddTriple(SkillBallColor.Blue);
        }
        [ContextMenu("Alpha Skill Ball Test/Add Blue Single")]
        [Button("Add Blue Single")]
        public void AddBlueSingle()
        {
            ResolveReferences();
            owner?.SkillBallPitcher.TryAppendBall(
                SkillBallColor.Blue,
                SkillBallConsumeMode.AlwaysTripleCheck);
            RefreshRuntimeView();
        }
        [ContextMenu("Alpha Skill Ball Test/Add Red Triple")]
        [Button("Add Red Triple")]
        public void AddRedTriple()
        {
            AddTriple(SkillBallColor.Red);
        }
        [ContextMenu("Alpha Skill Ball Test/Add Red Single")]
        [Button("Add Red Single")]
        public void AddRedSingle()
        {
            ResolveReferences();
            owner?.SkillBallPitcher.TryAppendBall(
                SkillBallColor.Red,
                SkillBallConsumeMode.AlwaysTripleCheck);
            RefreshRuntimeView();
        }
        [ContextMenu("Alpha Skill Ball Test/Add Yellow Triple")]
        [Button("Add Yellow Triple")]
        public void AddYellowTriple()
        {
            AddTriple(SkillBallColor.Yellow);
        }
        [ContextMenu("Alpha Skill Ball Test/Add Yellow Single")]
        [Button("Add Yellow Single")]
        public void AddYellowSingle()
        {
            ResolveReferences();
            owner?.SkillBallPitcher.TryAppendBall(
                SkillBallColor.Yellow,
                SkillBallConsumeMode.AlwaysTripleCheck);
            RefreshRuntimeView();
        }

        [ContextMenu("Alpha Skill Ball Test/Add White Triple")]
        [Button("Add White Triple")]
        public void AddWhiteTriple()
        {
            AddTriple(SkillBallColor.White);
        }

        [ContextMenu("Alpha Skill Ball Test/Add White Single")]
        [Button("Add White Single")]
        public void AddWhiteSingle()
        {
            ResolveReferences();
            owner?.SkillBallPitcher.TryAppendBall(
                SkillBallColor.White,
                SkillBallConsumeMode.AlwaysSingle);
            RefreshRuntimeView();
        }

        [ContextMenu("Alpha Skill Ball Test/Clear Balls")]
        [Button("Clear Balls")]
        public void ClearBalls()
        {
            ResolveReferences();
            owner?.SkillBallManager.ClearBalls();
            RefreshRuntimeView();
        }

        [ContextMenu("Alpha Skill Ball Test/Force Exit Sword Intent")]
        [Button("Force Exit Sword Intent")]
        public void ForceExitSwordIntent()
        {
            ResolveReferences();
            if (statusController != null && swordIntentStatus != null)
            {
                statusController.ExitStatus(swordIntentStatus);
            }

            RefreshRuntimeView();
        }

        [ContextMenu("Alpha Skill Ball Test/Refresh Runtime View")]
        [Button("Refresh Runtime View")]
        public void RefreshRuntimeView()
        {
            ResolveReferences();
            bluePrepared = ReadBool(AlphaSkillBallKeys.BlueThreePingPrepared);
            swordIntentActive = ReadBool(SkillBallBlackboardKeys.SwordIntentActive);
            currentBalls = BuildBallListText();
        }

        [ContextMenu("Alpha Skill Ball Test/Input Press 1")]
        [Button("Input Press 1")]
        public void PressSkillBall0()
        {
            StartCoroutine(PressKeyRoutine(Key.Digit1));
        }

        [ContextMenu("Alpha Skill Ball Test/Input Press 2")]
        [Button("Input Press 2")]
        public void PressSkillBall1()
        {
            StartCoroutine(PressKeyRoutine(Key.Digit2));
        }

        [ContextMenu("Alpha Skill Ball Test/Input Press 3")]
        [Button("Input Press 3")]
        public void PressSkillBall2()
        {
            StartCoroutine(PressKeyRoutine(Key.Digit3));
        }

        [ContextMenu("Alpha Skill Ball Test/Input Press 4")]
        [Button("Input Press 4")]
        public void PressSkillBall3()
        {
            StartCoroutine(PressKeyRoutine(Key.Digit4));
        }

        [ContextMenu("Alpha Skill Ball Test/Run Sword Intent Keyboard Smoke Test")]
        [Button("Run Sword Intent Keyboard Smoke Test")]
        public void RunSwordIntentKeyboardSmokeTest()
        {
            if (!Application.isPlaying)
            {
                lastSmokeTestResult = "Enter Play Mode before running the smoke test.";
                Debug.LogWarning($"[AlphaSkillBallDebugTester] {lastSmokeTestResult}", this);
                return;
            }

            if (smokeTestRoutine != null)
            {
                StopCoroutine(smokeTestRoutine);
            }

            smokeTestRoutine = StartCoroutine(SwordIntentKeyboardSmokeTestRoutine());
        }

        private void AddTriple(SkillBallColor color)
        {
            ResolveReferences();
            SkillBallConsumeMode consumeMode = color == SkillBallColor.White
                ? SkillBallConsumeMode.AlwaysSingle
                : SkillBallConsumeMode.AlwaysTripleCheck;

            owner?.SkillBallPitcher.AppendTriple(color, consumeMode);
            RefreshRuntimeView();
        }

        private IEnumerator SwordIntentKeyboardSmokeTestRoutine()
        {
            ResolveReferences();
            lastSmokeTestResult = "Running...";

            if (owner == null || owner.RunningBlackboard == null)
            {
                SetSmokeResult("Failed: owner or RunningBlackboard is missing.");
                yield break;
            }

            ClearBalls();
            AddBlueTriple();
            yield return PressKeyRoutine(Key.Digit1);
            yield return WaitUntilOrTimeout(
                () => ReadBool(AlphaSkillBallKeys.BlueThreePingPrepared),
                2f);

            if (!ReadBool(AlphaSkillBallKeys.BlueThreePingPrepared))
            {
                SetSmokeResult("Failed: blue triple did not set prepared blackboard.");
                yield break;
            }

            AddRedTriple();
            yield return PressKeyRoutine(Key.Digit1);
            yield return WaitUntilOrTimeout(
                () => ReadBool(SkillBallBlackboardKeys.SwordIntentActive),
                2f);

            if (!ReadBool(SkillBallBlackboardKeys.SwordIntentActive))
            {
                SetSmokeResult("Failed: second triple did not enter sword intent.");
                yield break;
            }

            int whiteCount = CountBalls(SkillBallColor.White);
            if (whiteCount != 5)
            {
                SetSmokeResult($"Failed: sword intent generated {whiteCount} white balls, expected 5.");
                yield break;
            }

            yield return WaitUntilOrTimeout(
                () => !ReadBool(SkillBallBlackboardKeys.SwordIntentActive),
                24f);

            RefreshRuntimeView();
            if (ReadBool(SkillBallBlackboardKeys.SwordIntentActive))
            {
                SetSmokeResult("Failed: sword intent did not auto exit.");
                yield break;
            }

            SetSmokeResult($"Passed: sword intent entered, generated 5 white balls, auto exited. Balls={currentBalls}");
        }

        private IEnumerator PressKeyRoutine(Key key)
        {
            ScheduleKeyPress(key);
            yield return null;
            yield return null;
            RefreshRuntimeView();
        }

        private void ScheduleKeyPress(Key key)
        {
            pendingKey = key;
            pendingKeyPhase = 0;
            hasPendingKeyPress = true;
        }

        private void ProcessPendingKeyPress()
        {
            if (!hasPendingKeyPress)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                hasPendingKeyPress = false;
                Debug.LogWarning("[AlphaSkillBallDebugTester] Keyboard.current is null.", this);
                return;
            }

            if (pendingKeyPhase == 0)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(pendingKey));
                InputSystem.Update();
                pendingKeyPhase = 1;
                return;
            }

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            hasPendingKeyPress = false;
        }

        private IEnumerator WaitUntilOrTimeout(System.Func<bool> predicate, float timeout)
        {
            float endTime = Time.realtimeSinceStartup + timeout;
            while (!predicate() && Time.realtimeSinceStartup < endTime)
            {
                yield return null;
            }
        }

        private int CountBalls(SkillBallColor color)
        {
            if (owner == null)
            {
                return 0;
            }

            int count = 0;
            foreach (SkillBallData ball in owner.SkillBallManager.GetBalls())
            {
                if (ball.Color == color)
                {
                    count++;
                }
            }

            return count;
        }

        private void SetSmokeResult(string result)
        {
            smokeTestRoutine = null;
            lastSmokeTestResult = result;
            RefreshRuntimeView();
            Debug.Log($"[AlphaSkillBallDebugTester] {result}", this);
        }

        private void ResolveReferences()
        {
            if (owner == null)
            {
                owner = GetComponent<ActionStateController>();
            }

            if (statusController == null)
            {
                statusController = GetComponent<AlphaStatusController>();
            }
        }

        private bool ReadBool(string key)
        {
            if (owner == null ||
                owner.RunningBlackboard == null ||
                string.IsNullOrWhiteSpace(key) ||
                !owner.RunningBlackboard.ContainsKey(key))
            {
                return false;
            }

            return owner.RunningBlackboard.GetBoolParameter(key);
        }

        private string BuildBallListText()
        {
            if (owner == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new();
            foreach (SkillBallData ball in owner.SkillBallManager.GetBalls())
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(ball.Color);
                builder.Append('/');
                builder.Append(ball.ConsumeMode);
            }

            return builder.ToString();
        }
    }
}
