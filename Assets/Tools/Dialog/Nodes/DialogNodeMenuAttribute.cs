namespace Tools.Dialog.Nodes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DialogNodeMenuAttribute : System.Attribute
    {
        public string _itemName;
        public DialogNodeMenuAttribute(string itemName)
        {
            _itemName = itemName;
        }
    }
}