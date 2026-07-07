using Tools.AI;
using Tools.BehaviourTree;
using Tools.BehaviourTree.Nodes;
using Tools.BehaviourTree.Nodes.ActionNode;
using UnityEngine;

namespace Monster
{
    [NodeItemMenu("Action Node/Move To Position",TreeType.Base)]
    public class MoveToPosition:ActionNode
    {
        public override string NoticeInfo => "移动到指定位置";
        [SerializeField] private Vector3 targetPosition;
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if(AiStatus.MoveTo(targetPosition))
                return State.Success;
            return State.Running;
        }
    }
}