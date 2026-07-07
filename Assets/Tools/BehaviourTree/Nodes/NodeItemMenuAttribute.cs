namespace Tools.BehaviourTree.Nodes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class NodeItemMenuAttribute:System.Attribute
    {
        public string _itemname;
        public TreeType treeType;
        public NodeItemMenuAttribute(string itemname,TreeType type = TreeType.Base)
        {
            _itemname = itemname;
            treeType = type;
        }
    }
}