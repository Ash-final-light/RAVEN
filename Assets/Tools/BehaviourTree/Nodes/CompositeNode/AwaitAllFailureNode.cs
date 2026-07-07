namespace Tools.BehaviourTree.Nodes.CompositeNode
{
    [NodeItemMenu("Composite Node/AwaitAllFailure")]
    public class AwaitAllFailureNode : CompositeNode
    {
        private int current = 0;
        public override string NoticeInfo => "若子节点有非失败状态的，后续子节点不执行，所有子节点失败后才返回失败";
        protected override void OnStart()
        {
            current = 0;
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if (current < childNodes.Count)
            {
                var child = childNodes[current];
                switch (child.Tick(deltaTime))
                {
                    case State.Running:
                        return State.Running;
                    case State.Failure:
                        current++;
                        break;
                    case State.Success:
                        return State.Running;
                }
            }

            return State.Failure;
        }
    }
}