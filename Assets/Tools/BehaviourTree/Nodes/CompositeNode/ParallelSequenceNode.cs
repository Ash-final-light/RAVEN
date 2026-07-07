using System.Collections.Generic;

namespace Tools.BehaviourTree.Nodes.CompositeNode
{
    [NodeItemMenu("Composite Node/ParallelSequence")]
    public class ParallelSequenceNode : CompositeNode
    {
        private HashSet<Node> completeNodes = new HashSet<Node>();
        private bool findFailure = false;
        
        public override string NoticeInfo => "并行序列器，一帧执行所有的节点，逻辑与，只要有一个子节点返回失败，就返回失败，并且打断其他节点执行";

        protected override void OnStart()
        {
            completeNodes.Clear();
            findFailure = false;
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            foreach (var child in childNodes)
            {
                if (completeNodes.Contains(child))
                {
                    continue;
                }
                
                switch (child.Tick(deltaTime))
                {
                    case State.Running:
                        break;
                    case State.Failure:
                        completeNodes.Add(child);
                        findFailure = true;
                        break;
                    case State.Success:
                        completeNodes.Add(child);
                        break;
                }
            }

            if (findFailure)
            {
                foreach (var node in childNodes)
                {
                    if (completeNodes.Contains(node))
                    {
                        continue;
                    }
                    node.Abort();
                }

                return State.Failure;
            }
            
            if(completeNodes.Count == childNodes.Count)
            {
                return State.Success;
            }

            return State.Running;
        }
    }
}