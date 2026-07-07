using Tools.BehaviourTree.Nodes.ActionNode;

namespace Tools.BehaviourTree
{
    public abstract class BaseBlackboardUpdate:ActionNode
    {
        protected Blackboard.Data.Blackboard runningBlackboard;
        public abstract void UpdateBlackBoardValue();

        protected BaseBlackboardUpdate(Blackboard.Data.Blackboard runningBlackboard)
        {
            this.runningBlackboard = runningBlackboard;
        }
        protected override void OnStart()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnStop()
        {
            throw new System.NotImplementedException();
        }

        protected override State OnTick(float deltaTime = 0)
        {
            UpdateBlackBoardValue();
            return State.Success;
        }
    }
}