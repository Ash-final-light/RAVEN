namespace Tools.BehaviourTree.Nodes.CompositeNode
{
    [NodeItemMenu("Composite Node/Selector")]
    public class SelectorNode : CompositeNode
    {
        private int current = 0;
        public override string NoticeInfo => "顺序执行子节点，逻辑或，直到有一个子节点成功或全部失败就退";

        protected override void OnStart()
        {
            current = 0;
        }

        protected override void OnStop()
        {
           
        }

        protected override State OnTick(float deltaTime = 0)
        {
            while (current < childNodes.Count)
            {
                var child = childNodes[current];
                switch (child.Tick(deltaTime))
                {
                    case State.Running:
                        return State.Running;
                    case State.Success:
                        return State.Success;
                    case State.Failure:
                        current++;
                        break;
                }
            }
            return State.Failure;
        }
    }
}