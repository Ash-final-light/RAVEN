using System.Collections.Generic;
using System.Linq;
using Tools.Blackboard.Data;
using UnityEngine;

namespace Tools.Action.Data
{
    [CreateAssetMenu(fileName = "ComboConfig", menuName = "Action/ComboConfig")]
    public class Input2ActionTable:ScriptableObject,IBlackBoardProvider
    {
        public string inputName;
        public string currentActionName;
        public string comboActionName;
        //数值越大越优先
        public int priority;
        public JudgeType judgeType = JudgeType.And;
        [SerializeField] private List<BlackboardCondition> conditions = new List<BlackboardCondition>();
#if UNITY_EDITOR
        public void SetParentBlackboard(Blackboard.Data.Blackboard parentBlackboard)
        {
            _blackboard = parentBlackboard;
        }
#endif
        /// <summary>
        /// 用于编辑器的黑板
        /// </summary>
        [SerializeField]private Blackboard.Data.Blackboard _blackboard;
        public Blackboard.Data.Blackboard Blackboard => _blackboard;
        public bool Judge( Blackboard.Data.Blackboard blackboard)
        {
            if(conditions.Count == 0)
                return true;

            if (blackboard == null)
                return false;

            //Debug.Log($"[Condition Check: ComboInfoContainer] Hash: {blackboard.GetHashCode()}");
            return judgeType == JudgeType.And ? 
                conditions.All(x => x.Satisfy(blackboard)) : 
                conditions.Any(x => x.Satisfy(blackboard));
        }
    }

    public enum JudgeType
    {
        And,
        Or,
    }
}
