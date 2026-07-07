using Tools.BehaviourTree.Nodes;

namespace Tools.BehaviourTree
{
    public class RootNode:Node
    {
        public Node child;
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if (!child)
            {
                return State.Running;
            }
            return child.Tick(deltaTime);
        }

        public override Node Clone()
        {
            RootNode rootNode = Instantiate(this);
            if(rootNode.child != null)
                rootNode.child = child.Clone();
            return rootNode;
        }
    }
}