using System;
using System.Collections.Generic;
using System.Linq;
using Tools.BehaviourTree.Nodes;
using Tools.BehaviourTree.Nodes.ActionNode;
using Tools.BehaviourTree.Nodes.CompositeNode;
using Tools.BehaviourTree.Nodes.DecorateNode;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = Tools.BehaviourTree.Nodes.Node;

namespace Tools.BehaviourTree.Editor.View
{
    public class BehaviourTreeView : GraphView
    {
        public Action<NodeView> OnNodeSelected;
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }
        public BehaviourTree tree { get; private set; }
         public BehaviourTreeView()
        {
            Insert(0,new GridBackground());
            
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Tools/BehaviourTree/Editor/BehaviourTreeEditor.uss");
            styleSheets.Add(styleSheet);
            
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }
        private void OnUndoRedoPerformed()
        {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }
         NodeView FindNodeView(Node node)
         {
             return GetNodeByGuid(node.guid) as NodeView;
         }
         internal void PopulateView(BehaviourTree tree)
         {
             this.tree = tree;

             tree.nodes.RemoveAll(n => n == null);

             graphViewChanged -= OnGraphViewChanged;
             DeleteElements(graphElements.ToList());
             graphViewChanged += OnGraphViewChanged;

             // 根据数据决定是否新建黑板
             if (tree.blackboard == null)
             {
                 tree.blackboard = tree.CreateBlackboard();
                 EditorUtility.SetDirty(tree.blackboard);
                 AssetDatabase.SaveAssets();
             }
             
             // 根据数据决定是否创建根节点
             if (tree.rootNode == null)
             {
                 tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                 EditorUtility.SetDirty(tree);
                 AssetDatabase.SaveAssets();
             }
             

             // 创建节点视图
             foreach (var node in tree.nodes)
             {
                 CreateNodeView(node);
             }

             // 创建边视图
             foreach (var node in tree.nodes)
             {
                 foreach (var child in tree.GetChildren(node))
                 {
                     NodeView parentView = FindNodeView(node);
                     NodeView childView = FindNodeView(child);

                     if (parentView != null && childView != null)
                     {
                         Edge edge = parentView.output.ConnectTo(childView.input);
                         AddElement(edge);
                     }
                 }
             }
         }


        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort => 
                endPort.direction != startPort.direction && 
                endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            Debug.Log($"OnGraphViewChanged {graphViewChange}");
            if(graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(element =>
                {
                    NodeView nodeView = element as NodeView;
                    if (nodeView != null)
                    {
                        tree.DeleteNode(nodeView.node);
                    }
                    
                    Edge edge = element as Edge;
                    if (edge != null)
                    {
                        NodeView parentNodeView = edge.output.node as NodeView;
                        NodeView childNodeView = edge.input.node as NodeView;
                        tree.RemoveChild(parentNodeView.node, childNodeView.node);
                    }
                });
            }
            if(graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentNodeView = edge.output.node as NodeView;
                    NodeView childNodeView = edge.input.node as NodeView;
                    tree.AddChild(parentNodeView.node, childNodeView.node);
                });
            }

            if (graphViewChange.movedElements != null)
            {
                nodes.ForEach((n) =>
                {
                    NodeView view = n as NodeView;
                    if (view != null) view.SortChildren();
                });
            }
            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var actionNodeTypes = TypeCache.GetTypesDerivedFrom<ActionNode>();
            AddMenuItem(actionNodeTypes);
            var compositeNodeTypes = TypeCache.GetTypesDerivedFrom<CompositeNode>();
            AddMenuItem(compositeNodeTypes);
            var decorateNodeTypes = TypeCache.GetTypesDerivedFrom<DecorateNode>();
            AddMenuItem(decorateNodeTypes);
            
            void AddMenuItem(TypeCache.TypeCollection types)
            {
                foreach (var type in types)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }
                    
                    if (type.IsDefined(typeof(NodeItemMenuAttribute), false))
                    {
                        var attributes = type.GetCustomAttributes(typeof(NodeItemMenuAttribute), false);
                        if (attributes.Length != 0)
                        {
                            var attribute = attributes[0] as NodeItemMenuAttribute;
                            if(attribute?.treeType != tree.treeType)
                                continue;
                            evt.menu.AppendAction(attribute._itemname, _ =>
                            {
                                var node = tree.CreateNode(type);
                                CreateNodeView(node);
                            });
                        }
                        else
                        {
                            evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", _ =>
                            {
                                var node = tree.CreateNode(type);
                                CreateNodeView(node);
                            });
                        }
                    }
                    else
                    {
                        evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", _ =>
                        {
                            var node = tree.CreateNode(type);
                            CreateNodeView(node);
                        });
                    }
                }
            }
        }

        void CreateNode(System.Type type)
        {
            Node node = tree.CreateNode(type);
            CreateNodeView(node);
        }
        void CreateNodeView(Node node)
        {
            NodeView nodeView = new NodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public void UpdateNodeStateView()
        {
            nodes.ForEach((n) =>
            {
                NodeView view = n as NodeView;
                if (view != null) view.UpdateStates();
            });
        }
    }
}
