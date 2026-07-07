using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.InputHandle.Data
{
    [Serializable]
    public enum CharacterStateType
    {
        Idle,
        NormalAttack,
        Skill,
        Slide,
        Hurt,
        Move,
        Interact,
        Ultimate
    }
    [System.Serializable]
    public class CharacterStateConfig
    {
        public CharacterStateType StateType;

        [Tooltip("数值越大，优先级越高")]
        public int Priority;

        [Tooltip("是否可以被低优先级状态打断")]
        public bool CanBeInterrupted;
        [ShowIf("CanBeInterrupted")][Tooltip("允许低优先级打断的列表")]
        public List<CharacterStateType> AllowedInterruptTypes = new ();
    }
    [CreateAssetMenu(
        fileName = "CharacterStateConfig",
        menuName = "Action/StateConfig")]
    public class CharacterStateConfigSo : ScriptableObject
    {
        public CharacterStateConfig[] StateConfigs;

        public CharacterStateConfig GetConfig(CharacterStateType type)
        {
            foreach (var config in StateConfigs)
            {
                if (config.StateType == type)
                    return config;
            }
            return null;
        }
    }
}