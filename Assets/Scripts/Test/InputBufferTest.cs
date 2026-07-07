using Sirenix.OdinInspector;
using Tools.InputHandle.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInputManager = Tools.InputHandle.PlayerInputManager;

namespace Test
{
    public class InputBufferTest : MonoBehaviour
    {
        [Header("组件引用")]
        public PlayerInputManager inputManager;
        
        [Header("配置")]
        public float bufferTimeout = 3f;
        public string map1 = "Player";
        public string map2 = "UI";

        private InputBuffer _inputBuffer;

        void Awake()
        {
            if (inputManager == null) inputManager = GetComponent<PlayerInputManager>();

            // 1. 初始化 InputBuffer
            _inputBuffer = new InputBuffer();
            _inputBuffer.Init(inputManager.PlayerInput, bufferTimeout, false);

            // 2. 注册要缓冲的动作
            // 这里需要确保你的 InputActions 资产里有这些名字的 Action
            RegisterAction("PlayerAttack");
            RegisterAction("PlayerInteract");
        }

        private void RegisterAction(string actionName)
        {
            _inputBuffer.Register(actionName);
            
            // 3. 同时在管理器注册 Debug 输出，测试事件分发
            inputManager.RegisterActionPerformed(actionName, (ctx) => {
                Debug.Log($"<color=green>[Event]</color> 动作 {actionName} 被触发 (Map: {inputManager.PlayerInput.currentActionMap.name})");
            });
        }

        [Button("enable only last input performed")]
        private void EnableOnlyLastInputPerformed()
        {
            _inputBuffer._onlyLastInputPerformed = true;
        }
        [Button("disable only last input performed")]
        private void DisableOnlyLastInputPerformed()
        {
            _inputBuffer._onlyLastInputPerformed = false;
        }
        [Button("Ask status")]
        private void AskStatus()
        {
           TestInputBuffer("PlayerAttack");
           TestInputBuffer("PlayerInteract");   
        }

        void Update()
        {
            // 4. 驱动缓冲区计时
            _inputBuffer.Tick(Time.deltaTime);

            // 5. 测试切换 Map 的快捷键 (手动测试用)
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                Debug.Log("<color=yellow>切换至 Map: " + map1 + "</color>");
                inputManager.SwitchCurrentActionMap(map1);
            }
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                Debug.Log("<color=yellow>切换至 Map: " + map2 + "</color>");
                inputManager.SwitchCurrentActionMap(map2);
            }
        }

        // --- 供 VisualTester 调用的接口 ---

        /// <summary>
        /// 模拟底层 InputSystem 的触发（用于自动化测试）
        /// </summary>
        public void SimulateInput(string actionName)
        {
            var action = inputManager.GetInputAction(actionName);
            if (action != null)
            {
                Debug.Log($"[Test] 模拟按下: {actionName}");
                // 注意：由于 InputSystem 限制，直接从代码触发 Action 较难
                // 建议在测试时手动点击键盘，或通过 InputTestFixture 实现
            }
        }

        /// <summary>
        /// 检查缓冲区内是否有该指令
        /// </summary>
        public bool TestInputBuffer(string actionName)
        {
            bool isBuffered = _inputBuffer.WasPerformedThisFrame(actionName);
            Debug.Log($"[Buffer Check] {actionName} 缓冲状态: {isBuffered}");
            return isBuffered;
        }

        private void OnDestroy()
        {
            _inputBuffer.Destroy();
        }
    }
}