using System;
using Tools.BehaviourTree.Nodes.ActionNode;
using Tools.BehaviourTree.Nodes.CompositeNode;
using Tools.BehaviourTree.Nodes.DecorateNode;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = Tools.BehaviourTree.Nodes.Node;

namespace Tools.BehaviourTree.Editor.View
{
    public class NodeView:UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public Port output;
        
        public NodeView(Node node):base("Assets/Tools/BehaviourTree/Editor/NodeVIew.uxml")
        {
            this.node = node;
            this.title = node.name;
            this.viewDataKey = node.guid;
            
            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            
            
            Label noticeInfoLabel = this.Q<Label>("NoticeInfo");// => .uxml
            noticeInfoLabel.text = node.NoticeInfo;
            
            
            Label descriptionLabel = this.Q<Label>("description");// => .uxml
            descriptionLabel.bindingPath = "description";// => node.description;
            descriptionLabel.Bind(new SerializedObject(node));
        }

        private void SetupClasses()
        {
            if (node is ActionNode)
            {
                AddToClassList("action-node");
            }
            else if (node is CompositeNode)
            {
                AddToClassList("composite-node");
            }
            else if (node is DecorateNode)
            {
                AddToClassList("decorate-node");
            }else if (node is RootNode)
            {
                AddToClassList("root-node");
            }
        }
        // 创建输出端口
        private void CreateOutputPorts()
        {
            if (node is ActionNode)
            {
                
            }
            else if (node is CompositeNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(object));
            }
            else if (node is DecorateNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(object));
            }else if (node is RootNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(object));
            }



            if (output != null)
            {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }
        
        // 创建输入端口
        private void CreateInputPorts()
        {
            if (node is ActionNode)
            {
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(object));
            }
            else if (node is CompositeNode)
            {
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(object));
            }
            else if (node is DecorateNode)
            {
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(object));
            }else if (node is RootNode)
            {
               
            }

            if (input != null)
            {
                input.portName = "";
                input.style.flexDirection = FlexDirection.ColumnReverse;
                inputContainer.Add(input);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Behaviour Tree (Set Position");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }
        }
        public void SortChildren()
        {
            CompositeNode composite = node as CompositeNode;
            if(composite)
            {
                composite.childNodes.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(Node left, Node right)
        {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateStates()
        {
            RemoveFromClassList("success-mode");
            RemoveFromClassList("failure-mode");
            RemoveFromClassList("running-mode");
            
            if (Application.isPlaying)
            {
                switch (node.state)
                {
                    case Node.State.Running:
                        if(node.isStarted)
                        {
                            AddToClassList("running-mode");
                        }
                        break;
                    case Node.State.Success:
                        AddToClassList("success-mode");
                        break;
                    case Node.State.Failure:
                        AddToClassList("failure-mode");
                        break;
                }
            }

        }
    }
}