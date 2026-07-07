using Framework.Core.Attribute;
using Sirenix.OdinInspector;
using Tools.Dialog.Nodes;
using UnityEngine;
using VContainer;

namespace Tools.Dialog
{
    public class DialogTreeExecutor
    {
        [Inject] private IObjectResolver _objectResolver;
        [DisplayOnly] private BaseDialogNode currentNode;
        //可以扩展读档功能，从指定guid开始执行
        public BaseDialogNode CurrentNode{get{return currentNode;}}
        public DialogTree RuntimeTree;
        
        public event System.Action<string,BaseDialogNode> OnNodeAdded;
        /// <summary>
        /// 给定初始值引用，通过克隆开始执行对话树
        /// </summary>
        public void StartDialog(DialogTree tree)
        {
            RuntimeTree = tree.Clone();
            //对所有节点进行注入
            RuntimeTree.nodes.ForEach(InjectNode);
            currentNode = RuntimeTree.rootNode;
            bool isEnd = false;
            ExecuteNode(out isEnd);
            if (isEnd)
            {
                Debug.LogError("对话为空");
            }
        }
        private void InjectNode(BaseDialogNode node)
        {
            if (node == null || _objectResolver == null)
            {
                return;
            }
            
            _objectResolver.Inject(node);
            
            SimpleDialogNode simple = node as SimpleDialogNode;
            if (simple != null)
            {
                if (simple.dialogEvents == null)
                {
                    return;
                }

                foreach (var dialogEvent in simple.dialogEvents)
                {
                    _objectResolver.Inject(dialogEvent);
                }
            }
            OptionDialogNode optionNode = node as OptionDialogNode;
            if (optionNode != null)
            {
                if (optionNode.options == null)
                {
                    return;
                }

                foreach (var option in optionNode.options)
                {
                    if (option.children == null)
                    {
                        continue;
                    }

                    foreach (var child in option.children)
                    {
                        _objectResolver.Inject(child);
                    }
                }
            }
        }
        /// <summary>
        /// 执行下一个节点
        /// </summary>
        public void ExecuteNode(out bool isEnd)
        {
            isEnd = false;
            if (currentNode == null)
            {
                // 根节点为空，说明树为空
                isEnd = true;
            }
            else
            {
                // 执行当前节点
                currentNode.Exit();
                currentNode = currentNode.GetNextNode();
                if (currentNode == null)
                {
                    isEnd = true;
                    return;
                }
                currentNode.ExecuteNode();
                OnNodeAdded?.Invoke(currentNode.guid,currentNode);
            }
        }

        public void ExecuteSelectedNode(BaseDialogNode node)
        {
            currentNode = node;
            ExecuteNode(out bool isEnd);
            if (isEnd)
            {
                Debug.LogError("对话为空");
            }
        }
    }
}
