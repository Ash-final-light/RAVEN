using System.Collections.Generic;
using System.Linq;
using Tools.Blackboard.Data;
using UnityEngine;

namespace Tools.BehaviourTree.Nodes.ActionNode
{

    [NodeItemMenu("Action Node/Blackboard One Condition")]
    public class BlackboardOneConditionNode : ActionNode, IBlackBoardProvider
    {
        public override string NoticeInfo => "任意一个条件满足就返回正确";
        [SerializeField] private List<BlackboardCondition> conditions = new List<BlackboardCondition>();

        protected override void OnStart()
        {

        }

        protected override void OnStop()
        {

        }

        protected override State OnTick(float deltaTime = 0)
        {
            if (conditions.Count == 0 || conditions.Any(condition => condition.Satisfy(blackboard)))
            {
                return State.Success;
            }

            return State.Failure;
        }

        public Tools.Blackboard.Data.Blackboard Blackboard => blackboard;
    }
}
