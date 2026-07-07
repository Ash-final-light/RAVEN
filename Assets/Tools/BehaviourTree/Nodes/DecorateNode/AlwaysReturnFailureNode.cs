namespace Tools.BehaviourTree.Nodes.DecorateNode
{
    [NodeItemMenu("Decorate Node/AlwaysReturnFailureNode")]
    public class AlwaysReturnFailureNode:DecorateNode
    {
        public override string NoticeInfo => "总是返回失败节点";
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            child.Tick(deltaTime);
            child.Abort();
            return State.Failure;
        }
    }
}