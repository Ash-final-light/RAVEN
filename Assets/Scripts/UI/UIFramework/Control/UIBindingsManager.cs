using Common.Constant;
using Framework.Core.Singleton;
using UI.UIFramework.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInputManager = Tools.InputHandle.PlayerInputManager;

namespace UI.UIFramework.Control
{
    public class UIBindingsManager : MonoBehaviour, ISingleton
    {
        public ManagerStatus Status { get; private set; }
        public InputData_SO InputData;
        //Get Attributes
        public InputType CurInputType { get; private set; }
        
        void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            AddAnyKeyEvent();
        }

        private void Start()
        {
            if (Status != ManagerStatus.Started)
            {
                Startup();
            }
        }

        void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            RemoveAnyKeyEvent();
        }

        public void Startup()
        {
            Status = ManagerStatus.Started;
        }
        //当CurGameStatus != GameStatus.Started，即UI页面时，调用UI map -> GameManger.cs/SetGameStatus(）
        public void SwitchToUI()
        {
            PlayerInputManager.Instance.SwitchCurrentActionMap(InputConstants.UIMap);
        }

        //当CurGameStatus == GameStatus.Started，即游玩时，调用Player map -> GameManger.cs/SetGameStatus(）
        public void SwitchToPlayer()
        {
            PlayerInputManager.Instance.SwitchCurrentActionMap(InputConstants.GamePlayMap);
        }

        //获取按键icon
        public Sprite GetBindingIcon(string _actionName)
        {
            string controlScheme = CurInputType == InputType.Keyboard ? InputConstants.KeyboardDevice : InputConstants.GamepadDevice;
            int bindingID = PlayerInputManager.Instance.PlayerInput.actions[_actionName].
                GetBindingIndex(InputBinding.MaskByGroup(controlScheme));
            string path = PlayerInputManager.Instance.PlayerInput.actions[_actionName].
                bindings[bindingID].effectivePath;
            return InputData.GetIconSprite(path, CurInputType);
        }

        public Sprite GetBindingIconForMove()
        {
            if (CurInputType == InputType.Keyboard)
                return InputData.MoveKeyboard;
            else
                return InputData.MoveGamePad;
        }

        public Sprite GetBindingIconForNavigate()
        {
            if (CurInputType == InputType.Keyboard)
                return InputData.NaviKeyboard;
            else
                return InputData.NaviGamePad;
        }

        #region InputType switch

        //当在<游戏运行过程>中连接手柄，或断开手柄，会改变CurInputType，改变按键提示
        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is Gamepad)
            {
                if (change == InputDeviceChange.Added)
                {
                    Debug.Log("Gamepad connected: " + device.name);
                    if (device.name.Contains(InputConstants.DualShockGamepadDevice) || 
                        device.name.Contains(InputConstants.DualSenseGamepadDevice))
                        SetInputType(InputType.PS);
                    else
                        SetInputType(InputType.Xbox);
                }
                else if (change == InputDeviceChange.Removed)
                {
                    Debug.Log("Gamepad disconnected: " + device.name);
                    SetInputType(InputType.Keyboard);
                }
            }
        }

        //当在<游戏运行过程>中按下键盘/手柄，会改变CurInputType，改变按键提示
        void AddAnyKeyEvent()
        {
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.AnyGamepad,SwitchToGamePad);
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.AnyGamepadUI,SwitchToGamePad);
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.AnyKey,SwitchToKeyboard);
            PlayerInputManager.Instance.RegisterActionPerformed(InputConstants.AnyKeyUI,SwitchToKeyboard);
        }

        void RemoveAnyKeyEvent()
        {
            if (!PlayerInputManager.HasInstance)
            {
                return;
            }

            PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.AnyGamepad,SwitchToGamePad);
            PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.AnyGamepadUI,SwitchToGamePad);
            PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.AnyKey,SwitchToKeyboard);
            PlayerInputManager.Instance?.UnregisterActionPerformed(InputConstants.AnyKeyUI,SwitchToKeyboard);
        }

        void SwitchToKeyboard(InputAction.CallbackContext context)
        {
            if (CurInputType != InputType.Keyboard)
            {
                SetInputType(InputType.Keyboard);
            }
        }

        void SwitchToGamePad(InputAction.CallbackContext context)
        {
            if (CurInputType == InputType.Keyboard)
            {
                if (Gamepad.current.device.name.Contains(InputConstants.DualShockGamepadDevice) ||
                    Gamepad.current.device.name.Contains(InputConstants.DualSenseGamepadDevice))
                    SetInputType(InputType.PS);
                else
                    SetInputType(InputType.Xbox);
            }
        }

        void SetInputType(InputType _type)
        {
            CurInputType = _type;
        }

        public static UIBindingsManager Instance => MonoSingletonProperty<UIBindingsManager>.GetInstance(true);

        public void OnSingletonInit()
        {
            MonoSingletonProperty<UIBindingsManager>.InitSingleton();
        }

        public void Dispose()
        {
            MonoSingletonProperty<UIBindingsManager>.Dispose();
        }
        #endregion
    }
}
