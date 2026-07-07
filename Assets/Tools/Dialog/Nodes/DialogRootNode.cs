using Framework.Core.Attribute;

namespace Tools.Dialog.Nodes
{
    
    public class DialogRootNode:BaseDialogNode
    {
        [DisplayOnly] public BaseDialogNode child;
        protected override void OnEnter()
        {
            
        }

        protected override void OnProgress()
        {
            
        }

        public override void Exit()
        {
            
        }

        public override BaseDialogNode GetNextNode()
        {
            return child;
        }

        public override bool AddChildNode(BaseDialogNode child)
        {
            this.child = child;
            return true;
        }
        public override bool RemoveChildNode(BaseDialogNode child)
        {
            if (this.child == child)
            {
                this.child = null;
                return true;
            }

            return false;
        }
    }
}