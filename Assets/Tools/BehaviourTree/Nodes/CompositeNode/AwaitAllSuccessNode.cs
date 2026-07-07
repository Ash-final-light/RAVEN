namespace Tools.BehaviourTree.Nodes.CompositeNode
{
    [NodeItemMenu("Composite Node/AwaitAllSuccess")]
    public class AwaitAllSuccessNode : CompositeNode
    {
        private int current = 0;
        public override string NoticeInfo => "若子节点有非成功状态的，后续子节点不执行，所有子节点成功后才返回成功";
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
                        return State.Running;
                        break;
                    case State.Success:
                        current++;
                        break;
                }
            }

            return State.Success;
        }
    }
}