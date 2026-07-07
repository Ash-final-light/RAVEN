namespace Tools.BehaviourTree.Nodes.DecorateNode
{
    [NodeItemMenu("Decorate Node/LoopNode")]
    public class LoopNode:DecorateNode
    {
        public override string NoticeInfo => "循环节点，永远循环执行子节点，返回子节点的状态";
        protected override void OnStart()
        {
           
        }

        protected override void OnStop()
        {
           
        }

        protected override State OnTick(float deltaTime = 0)
        {
            child.Tick(deltaTime);
            return State.Running;
        }
    }
}