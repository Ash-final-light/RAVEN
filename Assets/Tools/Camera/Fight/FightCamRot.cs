using System;
using Cinemachine;
using Common.Constant;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using PlayerInputManager = Tools.InputHandle.PlayerInputManager;

namespace Tools.Camera.Fight
{
    [Serializable]
    public class FightCamRot
    {
        private CinemachineBrain _cinemachineBrain;
        private CinemachineFreeLook _freeLook;
        
        private Transform _followTgt => _fightCamera.PlayerTarget;
        private FightCamera _fightCamera; // 配合新的 FightCamera 类

        private float _currentInputX;
        private bool _isUpdateFwdTurn;
        
        public float _valueMul = 30f;
        public float _fwdRotMul = 2f;

        public FightCamRot(CinemachineBrain brain, CinemachineFreeLook freeLook,
            FightCamera fightCamera, PlayerInputManager playerInputManager)
        {
            _cinemachineBrain = brain;
            _freeLook = freeLook;
            _fightCamera = fightCamera;

            // 注册输入事件
            playerInputManager.RegisterActionPerformed(InputConstants.PlayerLook, OnRotateAction);
        }
        public void SetValueMul(float value)
        {
            _valueMul = value;
        }
        public void SetFwdRotMul(float value)
        {
            _fwdRotMul = value;
        }

        /// <summary>
        /// 需要在 FightCamera 的 Update 中调用
        /// </summary>
        public void Tick()
        {
            if (_isUpdateFwdTurn)
            {
                UpdateFwdTurn();
            }
            else
            {
                ApplyXAxisRotation();
            }
        }

        public void OnRotateAction(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _currentInputX = 0;
                return;
            }

            Vector2 delta = context.ReadValue<Vector2>();

            // 只要有位移，就直接驱动动力
            if (delta.magnitude > 0.01f)
            {
                if (_fightCamera && !_fightCamera.IsTPS)
                {
                    _fightCamera.SwitchCamToTPS(true);
                }

                // delta.x 代表鼠标滑动的速度（马达动力）
                // 这里的系数 0.5f 需要根据你 Cinemachine 的 MaxSpeed 进行微调
                _currentInputX = - delta.x * 0.5f; 
        
                // 手动干预时停止自动对齐
                _isUpdateFwdTurn = false;
            }
        }

        private void ApplyXAxisRotation()
        {
            if (_cinemachineBrain.ActiveVirtualCamera != null && 
                (Object)_cinemachineBrain.ActiveVirtualCamera == _freeLook)
            {
                // 1. 将动力实时传递给 Cinemachine
                _freeLook.m_XAxis.TouchInputValue = _currentInputX;

                // 2. 核心：极速衰减（关键！）
                // 如果不衰减，鼠标停下后，TouchInputValue 还会保持上一帧的值，导致相机继续飘
                // 提高这个系数（比如 50f），能让马达在鼠标停下的一瞬间也停下
                _currentInputX = Mathf.Lerp(_currentInputX, 0, Time.deltaTime * _valueMul);
            }
        }

        private void UpdateFwdTurn()
        {
            float angle = Vector3.SignedAngle(UnityEngine.Camera.main.transform.forward, _followTgt.forward, Vector3.up);
            float absAngle = Mathf.Abs(angle);

            if (absAngle < 1.0f)
            {
                _isUpdateFwdTurn = false;
                _freeLook.m_XAxis.TouchInputValue = 0;
                return;
            }

            // 动态动力：角度越大，推力越猛。
            // 使用一个较高的系数（如 2.0），让它在远距离时瞬间弹回
            float power = angle * _fwdRotMul; 
    
            // 限制最大动力，防止过冲
            _freeLook.m_XAxis.TouchInputValue = Mathf.Clamp(power, -100f, 100f);
        }
    }
}