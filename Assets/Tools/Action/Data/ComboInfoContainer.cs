using System.Collections.Generic;
using Common.Constant;
using Sirenix.OdinInspector;
using Tools.Blackboard.Data;
using Tools.Skill;
using UnityEngine;

namespace Tools.Action.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "Action/ComboInfoContainer")]
    public class ComboInfoContainer:ScriptableObject
    {
        [SerializeField] private Blackboard.Data.Blackboard blackboard;

        [Header("Fallback Actions")]
        [SerializeField] private string defaultActionName;
        [SerializeField] private string defaultAttackActionName;
        [SerializeField] private string defaultUltimateActionName;
        [SerializeField] private string defaultSkillBallActionName;
        [SerializeField] private string defaultRedSkillBallActionName;
        [SerializeField] private string defaultYellowSkillBallActionName;
        [SerializeField] private string defaultBlueSkillBallActionName;
        [SerializeField] private string defaultWhiteSkillBallActionName;

        public List<Input2ActionTable> inputTables;

        public string DefaultActionName => defaultActionName;
        public string DefaultAttackActionName => defaultAttackActionName;
        public string DefaultUltimateActionName => defaultUltimateActionName;
        public string DefaultSkillBallActionName => defaultSkillBallActionName;
        public string DefaultRedSkillBallActionName => defaultRedSkillBallActionName;
        public string DefaultYellowSkillBallActionName => defaultYellowSkillBallActionName;
        public string DefaultBlueSkillBallActionName => defaultBlueSkillBallActionName;
        public string DefaultWhiteSkillBallActionName => defaultWhiteSkillBallActionName;

        public string GetNextActionOrDefault(
            string currentActionName,
            string inputName,
            Blackboard.Data.Blackboard runtimeBlackboard)
        {
            string configuredAction = GetConfiguredNextAction(
                currentActionName,
                inputName,
                runtimeBlackboard);

            return string.IsNullOrEmpty(configuredAction)
                ? GetDefaultAction(inputName)
                : configuredAction;
        }

        private string GetConfiguredNextAction(
            string currentActionName,
            string inputName,
            Blackboard.Data.Blackboard runtimeBlackboard)
        {
            if (inputTables == null)
            {
                return null;
            }

            int priority = int.MinValue;
            string result = null;

            foreach (Input2ActionTable comboConfig in inputTables)
            {
                if (comboConfig == null)
                {
                    continue;
                }

                if (comboConfig.currentActionName == currentActionName &&
                    comboConfig.inputName == inputName &&
                    comboConfig.Judge(runtimeBlackboard))
                {
                    if (comboConfig.priority >= priority)
                    {
                        priority = comboConfig.priority;
                        result = comboConfig.comboActionName;
                    }
                }
            }

            return result;
        }

        private string GetDefaultAction(string inputName)
        {
            if (inputName == InputConstants.PlayerAttack)
            {
                return FirstValid(defaultAttackActionName, defaultActionName);
            }

            if (inputName == InputConstants.PlayerUltimate)
            {
                return FirstValid(defaultUltimateActionName, defaultActionName);
            }

            if (IsSkillBallInput(inputName))
            {
                return FirstValid(
                    GetColorSkillBallDefault(inputName),
                    defaultSkillBallActionName,
                    defaultActionName);
            }

            return defaultActionName;
        }

        private string GetColorSkillBallDefault(string inputName)
        {
            if (inputName == SkillBallInputNames.Red1 ||
                inputName == SkillBallInputNames.Red2 ||
                inputName == SkillBallInputNames.Red3)
            {
                return defaultRedSkillBallActionName;
            }

            if (inputName == SkillBallInputNames.Yellow1 ||
                inputName == SkillBallInputNames.Yellow2 ||
                inputName == SkillBallInputNames.Yellow3)
            {
                return defaultYellowSkillBallActionName;
            }

            if (inputName == SkillBallInputNames.Blue1 ||
                inputName == SkillBallInputNames.Blue2 ||
                inputName == SkillBallInputNames.Blue3)
            {
                return defaultBlueSkillBallActionName;
            }

            if (inputName == SkillBallInputNames.White1)
            {
                return defaultWhiteSkillBallActionName;
            }

            return null;
        }

        private static bool IsSkillBallInput(string inputName)
        {
            return !string.IsNullOrEmpty(inputName) &&
                   inputName.StartsWith("SkillBall");
        }

        private static string FirstValid(params string[] actionNames)
        {
            if (actionNames == null)
            {
                return null;
            }

            for (int i = 0; i < actionNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(actionNames[i]))
                {
                    return actionNames[i];
                }
            }

            return null;
        }

        #if UNITY_EDITOR
        private void OnEnable()
        {
            if (inputTables == null)
            {
                return;
            }

            foreach (var item in inputTables)
            {
                if (item == null)
                {
                    continue;
                }

                item.SetParentBlackboard(blackboard);
            }
        }
        [Button("Set Same Blackboard")]
        public void RefreshBlackboard()
        {
            OnEnable();
        }
        #endif
    }
}
