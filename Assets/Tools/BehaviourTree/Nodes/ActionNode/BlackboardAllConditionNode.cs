using System.Collections.Generic;
using System.Linq;
using Tools.Blackboard.Data;
using UnityEngine;

namespace Tools.BehaviourTree.Nodes.ActionNode
{
    [NodeItemMenu("Action Node/Blackboard All Condition")]
    public class BlackboardAllConditionNode : ActionNode,IBlackBoardProvider
    {
        public override string NoticeInfo =>"所有条件满足才返回正确";
        [SerializeField] private List<BlackboardCondition> conditions = new List<BlackboardCondition>();
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if (conditions.Count == 0 || conditions.All(condition => condition.Satisfy(blackboard)))
            {
                return State.Success;
            }

            return State.Failure;
        }

        public Tools.Blackboard.Data.Blackboard Blackboard => blackboard;
    }
}