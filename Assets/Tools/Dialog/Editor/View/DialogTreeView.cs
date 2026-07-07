using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Tools.Dialog.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Tools.Dialog.Editor.View
{
    public class DialogTreeView:GraphView
    {
        public Action<DialogNodeView> OnNodeSelected;
        public Action<DialogNodeView> OnNodeUnSelected;
        public new class UxmlFactory : UxmlFactory<DialogTreeView, GraphView.UxmlTraits> { }
        
        public DialogTree _tree{ get; private set; }
        private readonly List<Edge> _reconnectEdges = new();
        private readonly List<Edge> _disconnectEdges = new();
        public UnityEngine.Vector2 MousePosition;
        public DialogTreeView()
        {
            // 1. 创建 GridBackground 并设置全屏样式
            var gridBackground = new GridBackground();
            // 关键：设置 GridBackground 占满整个 GraphView
            gridBackground.style.position = Position.Absolute;
            gridBackground.style.top = 0;
            gridBackground.style.left = 0;
            gridBackground.style.width = Length.Percent(100);
            gridBackground.style.height = Length.Percent(100);
            Insert(0, gridBackground); // 插入到最底层，避免遮挡其他元素
            
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Tools/Dialog/Editor/DialogTreeEditor.uss");
            styleSheets.Add(styleSheet);
            
            this.style.flexGrow = 1; // 占满父容器
            this.style.width = Length.Percent(100);
            this.style.height = Length.Percent(100);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_tree == null)
            {
                return;
            }

            var simpleDialogNodeTypes = TypeCache.GetTypesDerivedFrom<SimpleDialogNode>().ToList();
            simpleDialogNodeTypes.Add(typeof(SimpleDialogNode));
            AddMenuItem(simpleDialogNodeTypes);
            var optionDialogNodeTypes = TypeCache.GetTypesDerivedFrom<OptionDialogNode>().ToList();
            optionDialogNodeTypes.Add(typeof(OptionDialogNode));
            AddMenuItem(optionDialogNodeTypes);
            

            void AddMenuItem(List<Type> types)
            {
                foreach (var type in types)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }
                    
                    if (type.IsDefined(typeof(DialogNodeMenuAttribute), false))
                    {
                        var attributes = type.GetCustomAttributes(typeof(DialogNodeMenuAttribute), false);
                        if (attributes.Length != 0)
                        {
                            var attribute = attributes[0] as DialogNodeMenuAttribute;
                            evt.menu.AppendAction(attribute._itemName, _ =>
                            {
                                var node = _tree.CreateNode(type);
                                CreateNodeView(node, true);
                            });
                        }
                        else
                        {
                            evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", _ =>
                            {
                                var node = _tree.CreateNode(type);
                                CreateNodeView(node, true);
                            });
                        }
                    }
                    else
                    {
                        evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", _ =>
                        {
                            var node = _tree.CreateNode(type);
                            CreateNodeView(node, true);
                        });
                    }
                }
            }
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // 这里是获取在某个端口连接时的满足条件的端口（满足条件是端口输入对输出、且端口对应的节点不同）
            return ports.ToList()
                .Where(endPort => startPort.direction != endPort.direction && startPort.node != endPort.node).ToList();
        }
        /// <summary>
        /// 提供给编辑器的更新UI函数
        /// </summary>
        /// <param name="tree"></param>
        internal void UpdateView(DialogTree tree)
        {
            _tree = tree;
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            if (!tree)
            {
                return;
            }

            graphViewChanged += OnGraphViewChanged;
            focusable = true;

            // 如果没有根节点就创建根节点
            if (!tree.rootNode)
            {
                tree.rootNode = tree.CreateRootNode();
            }
            // 如果没有ProfilePool就创建ProfilePool
            if (!tree.ProfilePool)
            {
                tree.CreateProfilePool();
            }

            // 创建节点UI
            tree.nodes.ForEach(node => CreateNodeView(node, false));

            // 创建图连接
            tree.nodes.ForEach(node =>
            {
                var parentNodeView = FindNodeView(node);
                tree.GetChildNodes(node).ForEach(childNode =>
                {
                    var childNodeView = FindNodeView(childNode);
                    var edge = parentNodeView.Output.ConnectTo(childNodeView.Input);
                    AddElement(edge);
                });
            });

            // 节点重排序
            /*SortAllNodeViews();*/
        }
        internal void UpdateNodeStates()
        {
            nodes.ForEach(node =>
            {
                if (node is DialogNodeView treeNodeView)
                {
                    treeNodeView.UpdateState();
                }
            });
        }
        private DialogNodeView FindNodeView(BaseDialogNode node)
        {
            return GetNodeByGuid(node.guid) as DialogNodeView;
        }
        private DialogNodeView CreateNodeView(BaseDialogNode node, bool isNewNode)
        {
            var nodeView = new DialogNodeView(node)
            {
                OnNodeSelected = OnNodeSelected,
                OnNodeUnSelected = OnNodeUnSelected
            };
            AddElement(nodeView);
            if (isNewNode)
            {
                // 设置节点位置为鼠标位置
                var position = contentViewContainer.WorldToLocal(MousePosition);
                var newPosition = nodeView.GetPosition();
                newPosition.x = position.x;
                newPosition.y = position.y;
                nodeView.SetPosition(newPosition);
            }

            return nodeView;
        }

        /// <summary>
        /// 图内容变化节点函数，这里处理图数据的改变
        /// </summary>
        /// <param name="graphViewChange"></param>
        /// <returns></returns>
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            HandleElementsRemoved();
            HandleEdgesCreated();
            HandleElementsMoved();

            return graphViewChange;

            void HandleElementsRemoved()
            {
                if (graphViewChange.elementsToRemove == null)
                {
                    return;
                }

                foreach (var graphElement in graphViewChange.elementsToRemove)
                {
                    // 图节点删除时同步删除对话树数据的节点
                    if (graphElement is DialogNodeView nodeView)
                    {
                        _tree?.DeleteNode(nodeView.Node);
                    }

                    // 图边删除时删除节点的子节点
                    if (graphElement is Edge edge)
                    {
                        var parentNodeView = edge.output.node as DialogNodeView;
                        var childNodeView = edge.input.node as DialogNodeView;
                        if (!_tree || !_tree.RemoveChildNode(parentNodeView!.Node, childNodeView!.Node))
                        {
                            _reconnectEdges.Add(edge);
                        }
                    }

                    // 图节点或边删除时所有子节点重新排序
                    /*SortAllNodeViews();*/
                }
            }

            void HandleEdgesCreated()
            {
                if (graphViewChange.edgesToCreate == null)
                {
                    return;
                }

                // 图边创建时新增节点的子节点
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var parentNodeView = edge.output.node as DialogNodeView;
                    var childNodeView = edge.input.node as DialogNodeView;
                    if (!_tree || !_tree.AddChildNode(parentNodeView!.Node, childNodeView!.Node))
                    {
                        _disconnectEdges.Add(edge);
                    }
                }

                // 图边创建时所有子节点重新排序
                /*SortAllNodeViews();*/
            }

            void HandleElementsMoved()
            {
                if (graphViewChange.movedElements == null)
                {
                    return;
                }

                // 图节点移动时对所有子节点重新排序
                /*SortAllNodeViews();*/
            }
        }
        private void HandleRedoPerformed()
        {
            if (!_tree)
            {
                return;
            }

            UpdateView(_tree);
            AssetDatabase.SaveAssets();
        }
        private void HandleEdgeReconnectedOrDisconnected()
        {
            _reconnectEdges.ForEach(edge =>
            {
                edge.output?.Connect(edge);
                edge.input?.Connect(edge);
                AddElement(edge);
            });
            _reconnectEdges.Clear();
            _disconnectEdges.ForEach(edge =>
            {
                edge.output?.Disconnect(edge);
                edge.input?.Disconnect(edge);
                RemoveElement(edge);
            });
            _disconnectEdges.Clear();
        }
    }
}