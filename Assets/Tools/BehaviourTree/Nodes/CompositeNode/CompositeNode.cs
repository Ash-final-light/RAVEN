using System.Collections.Generic;

namespace Tools.BehaviourTree.Nodes.CompositeNode
{
    public abstract class CompositeNode:Node
    {
        public List<Node> childNodes = new List<Node>();
        public override string NoticeInfo => "组合节点，操作多个子节点";
        
        public override Node Clone()
        {
            CompositeNode compositeNode = Instantiate(this);
            compositeNode.childNodes = childNodes.ConvertAll(n => n.Clone());
            return compositeNode;
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            childNodes.ForEach(n => n.Abort());
        }
    }
}