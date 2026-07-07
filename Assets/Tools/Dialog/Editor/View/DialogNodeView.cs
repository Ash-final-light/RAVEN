using Tools.BehaviourTree;
using Tools.BehaviourTree.Nodes.CompositeNode;
using Tools.Dialog.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tools.Dialog.Editor.View
{
    public class DialogNodeView : UnityEditor.Experimental.GraphView.Node
    {
        public readonly BaseDialogNode Node;
        public readonly Port Input;
        public readonly Port Output;

        public System.Action<DialogNodeView> OnNodeSelected;
        public System.Action<DialogNodeView> OnNodeUnSelected;

        public DialogNodeView(BaseDialogNode node) : base(
            "Assets/Tools/Dialog/Editor/DialogNodeView.uxml")
        {
            Node = node;
            title = node.name;
            viewDataKey = node.guid;
            style.left = node.position.x;
            style.top = node.position.y;

            // 双向绑定
            var labelSpeaker = mainContainer.Q<Label>("Lbspeaker");
            var labelContent = mainContainer.Q<Label>("Lbcontent");


            switch (Node)
            {
                case DialogRootNode:
                    labelSpeaker.style.display = DisplayStyle.None;
                    labelContent.style.display = DisplayStyle.None;
                    Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single,
                        typeof(bool));
                    break;
                case SimpleDialogNode:
                    var normalDialogNode = Node as SimpleDialogNode;
                    labelSpeaker.bindingPath = "speaker";
                    labelSpeaker.Bind(new SerializedObject(normalDialogNode));
                    labelContent.bindingPath = "content";
                    labelContent.Bind(new SerializedObject(normalDialogNode));
                    Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single,
                        typeof(bool));
                    break;
                case OptionDialogNode:
                    labelSpeaker.style.display = DisplayStyle.None;
                    labelContent.style.display = DisplayStyle.None;
                    Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi,
                        typeof(bool));
                    break;
                default:
                    Debug.LogError($"未处理的节点类型: {Node.GetType()}");
                    break;
            }


            if (Input != null)
            {
                Input.portName = "";
                Input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(Input);
            }

            if (Output != null)
            {
                Output.portName = "";
                Output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(Output);
            }


            SetupClasses();
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(Node, "Behaviour Tree Node(Set Position)");
            Node.position = new Vector2(newPos.xMin, newPos.yMin);
            EditorUtility.SetDirty(Node);
        }
        
        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnNodeUnSelected?.Invoke(this);
        }
        internal void UpdateState()
        {
            if (Application.isPlaying)
            {
                RemoveFromClassList("running");
                RemoveFromClassList("success");
                RemoveFromClassList("unstarted");

                switch (Node.State)
                {
                    case DialogNodeStatus.Running:
                        AddToClassList("running");
                        break;
                    case DialogNodeStatus.Success:
                        AddToClassList("success");
                        break;
                    case DialogNodeStatus.UnStarted:
                        AddToClassList("unstarted");
                        break;
                }
            }
        }
        private void SetupClasses()
        {
            switch (Node)
            {
                case DialogRootNode:
                    AddToClassList("root");
                    break;
                case SimpleDialogNode:
                    AddToClassList("simple");
                    break;
                case OptionDialogNode:
                    AddToClassList("option");
                    break;
            }
        }
    }
}