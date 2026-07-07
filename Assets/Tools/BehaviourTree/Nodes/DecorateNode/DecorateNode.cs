namespace Tools.BehaviourTree.Nodes.DecorateNode
{
    public abstract class DecorateNode:Node
    {
        public Node child;
        protected override void OnAbort()
        {
            base.OnAbort();
            child?.Abort();
        }

        public override Node Clone()
        {
            DecorateNode decorateNode = Instantiate(this);
            decorateNode.child = child.Clone();
            return decorateNode;
        }
    }
}