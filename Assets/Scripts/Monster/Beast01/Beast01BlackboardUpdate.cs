using Tools.BehaviourTree;
using Tools.BehaviourTree.Nodes;
using Tools.Blackboard.Data;

namespace Monster.Beast01
{
    [NodeItemMenu("Action Node/Beast01 Blackboard Update",TreeType.Beast01)]
    public class Beast01BlackboardUpdate: BaseBlackboardUpdate
    {
        public Beast01BlackboardUpdate(Blackboard runningBlackboard)
            : base(runningBlackboard)
        {
            
        }
        public override void UpdateBlackBoardValue()
        {
            AiStatus.UpdateBlackboard();
        }
    }
}