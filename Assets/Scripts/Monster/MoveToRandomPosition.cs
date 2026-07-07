using Tools.AI;
using Tools.BehaviourTree;
using Tools.BehaviourTree.Nodes;
using Tools.BehaviourTree.Nodes.ActionNode;
using UnityEngine;

namespace Monster
{
    [NodeItemMenu("Action Node/Move To Random Position",TreeType.Base)]
    public class MoveToRandomPosition:ActionNode
    {
        public override string NoticeInfo => "移动到随机位置";
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 minPos;
        [SerializeField] private Vector3 maxPos;
        protected override void OnStart()
        {
            targetPosition = new Vector3(
                Random.Range(minPos.x, maxPos.x),
                Random.Range(minPos.y, maxPos.y),
                Random.Range(minPos.z, maxPos.z)
            );
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