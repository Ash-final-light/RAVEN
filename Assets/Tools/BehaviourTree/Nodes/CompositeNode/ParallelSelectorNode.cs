using System.Collections.Generic;

namespace Tools.BehaviourTree.Nodes.CompositeNode
{
    [NodeItemMenu("Composite Node/ParallelSelector")]
    public class ParallelSelectorNode : CompositeNode
    {
        private HashSet<Node> completeNodes = new HashSet<Node>();
        private bool findSuccess = false;
        
        public override string NoticeInfo => "并行选择器，一帧执行所有的节点，逻辑或，只要有一个子节点返回成功，就返回成功，并且打断其他节点执行";

        protected override void OnStart()
        {
            completeNodes.Clear();
            findSuccess = false;
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
                        break;
                    case State.Success:
                        completeNodes.Add(child);
                        findSuccess = true;
                        break;
                }
            }

            if (findSuccess)
            {
                foreach (var node in childNodes)
                {
                    if (completeNodes.Contains(node))
                    {
                        continue;
                    }
                    node.Abort();
                }

                return State.Success;
            }
            
            if(completeNodes.Count == childNodes.Count)
            {
                return State.Success;
            }

            return State.Running;
        }
    }
}