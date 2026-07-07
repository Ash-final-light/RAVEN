using Tools.Action.Data;
using UnityEngine;

namespace Tools.InputHandle
{
    /// <summary>
    /// Resolves the next action from the combo table and its configured fallback actions.
    /// </summary>
    public class PlayerCharacterEvaluator
    {
        private readonly ComboInfoContainer _comboInfoContainer;
        private readonly Blackboard.Data.Blackboard _blackboard;

        public PlayerCharacterEvaluator(
            ComboInfoContainer comboInfoContainer,
            Blackboard.Data.Blackboard blackboard)
        {
            _comboInfoContainer = comboInfoContainer;
            _blackboard = blackboard;
        }

        public string GetNextAction(string currentAction, string inputName)
        {
            if (_comboInfoContainer == null)
            {
                return null;
            }

            string result = _comboInfoContainer.GetNextActionOrDefault(
                currentAction,
                inputName,
                _blackboard);
            
            return result;
        }
    }
}
