using UnityEngine;

namespace Tools.BehaviourTree.Nodes.ActionNode
{
    [NodeItemMenu("Action Node/Debug")]
    public class DebugNode:ActionNode
    {
        public string debugString;
        public override string NoticeInfo => "调试节点";

        protected override void OnStart()
        {
           
        }

        protected override void OnStop()
        {
                   
        }

        protected override State OnTick(float deltaTime = 0)
        { 
            //Debug.Log(debugString);
           return State.Success;
        }
    }
}
