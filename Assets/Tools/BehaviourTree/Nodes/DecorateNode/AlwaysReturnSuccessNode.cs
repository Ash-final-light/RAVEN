namespace Tools.BehaviourTree.Nodes.DecorateNode
{
    [NodeItemMenu("Decorate Node/AlwaysReturnSuccessNode")]
    public class AlwaysReturnSuccessNode:DecorateNode
    {
        public override string NoticeInfo => "总是返回成功节点";
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
            return State.Success;
        }
    }
}