using System.Collections.Generic;
using Tools.Blackboard.Data;
using UnityEngine;

namespace Tools.BehaviourTree.Nodes.ActionNode
{
    [NodeItemMenu("Action Node/Blackboard Variable")]
    public class BlackboardVariableNode : ActionNode,IBlackBoardProvider
    {
        [SerializeField]
        private List<BlackboardValue> variables = new List<BlackboardValue>();
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            for(int i = 0; i < variables.Count; i++)
            {
                blackboard.SetParameter(variables[i]);
            }
            return State.Success;
        }
        public Tools.Blackboard.Data.Blackboard Blackboard => blackboard;
    }
}