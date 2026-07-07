

namespace Tools.BehaviourTree.Nodes.CompositeNode
{
    [NodeItemMenu("Composite Node/Sequence")]
    public class SequenceNode : CompositeNode
    {
        private int current = 0;
        public override string NoticeInfo => "顺序执行子节点，逻辑与，直到有一个子节点失败或全部成功就退";

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
                    case State.Failure:
                        return State.Failure;
                    case State.Success:
                        current++;
                        break;
                }
            }
            return State.Success;
        }
    }
}