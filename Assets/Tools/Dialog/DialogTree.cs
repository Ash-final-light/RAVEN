using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Core.Attribute;
using Sirenix.OdinInspector;
using Tools.Dialog.Nodes;
using Tools.Profile.Data;
using UnityEditor;
using UnityEngine;

namespace Tools.Dialog
{
    [CreateAssetMenu()]
    public class DialogTree:ScriptableObject
    {
        public BaseDialogNode rootNode;
        [DisplayOnly]public List<BaseDialogNode> nodes = new List<BaseDialogNode>();
        [DisplayOnly] public ProfilePool ProfilePool;
        #if UNITY_EDITOR
        public virtual void CreateProfilePool()
        {
            var blackboard = ScriptableObject.CreateInstance<ProfilePool>();
            blackboard.name = "ProfilePool";

            if (!Application.isPlaying && AssetDatabase.Contains(this))
            {
                AssetDatabase.AddObjectToAsset(blackboard, this);
            }

            this.ProfilePool = blackboard;
            Dfs(rootNode, node => node._profilePool = blackboard);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public virtual BaseDialogNode CreateRootNode()
        {
            return CreateNode(typeof(DialogRootNode));
        }
#endif
        public void Dfs(BaseDialogNode node, Action<BaseDialogNode> visitor)
        {
            visitor.Invoke(node);
            GetChildNodes(node).ForEach(childNode => Dfs(childNode, visitor));
        }
        
        public List<BaseDialogNode> GetChildNodes(BaseDialogNode parent)
        {
            var childNodes = new List<BaseDialogNode>();
            switch (parent)
            {
                case DialogRootNode rootNode:
                    if (rootNode.child)
                    {
                        childNodes.Add(rootNode.child);
                    }

                    return childNodes;
                case SimpleDialogNode normalDialogNode:
                    if (normalDialogNode.child)
                    {
                        childNodes.Add(normalDialogNode.child);
                    }

                    return childNodes;
                case OptionDialogNode optionDialogNode:
                    return optionDialogNode.children.Where(child => child != null).ToList();

                default:
                    return childNodes;
            }
        }
        #if UNITY_EDITOR
        public BaseDialogNode CreateNode(Type type)
        {
            Undo.RecordObject(this, "Dialog Tree(Create Node)");

            var node = ScriptableObject.CreateInstance(type) as BaseDialogNode;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node._profilePool = ProfilePool;

            nodes.Add(node);

            Debug.Log(AssetDatabase.Contains(this));
            // 关键判断
            if (!Application.isPlaying && AssetDatabase.Contains(this))
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Dialog Tree(Create Node)");

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            return node;
        }
        #endif
        /// <summary>
        /// 克隆函数，用于多个组件同时执行同一文件的行为树时克隆数据
        /// 注意，这里仅克隆行为树流程的节点，那些不处于Root树中的节点存在不被克隆的情况
        /// </summary>
        /// <returns></returns>
        public DialogTree Clone()
        {
            var dialogTree = Instantiate(this);
            dialogTree.ProfilePool = Instantiate(ProfilePool);
            // 树节点克隆在这里从根节点递归克隆节点
            dialogTree.rootNode = rootNode.Clone() as DialogRootNode;
            // 这里节点列表采用DFS遍历获取流程的克隆节点
            var cloneNodes = new List<BaseDialogNode>();
            Dfs(dialogTree.rootNode, node =>
            {
                node._profilePool = dialogTree.ProfilePool;
                cloneNodes.Add(node);
            });
            dialogTree.nodes = cloneNodes;
            return dialogTree;
        }

        BaseDialogNode GetNodeByGuid(string guid)
        {
            return nodes.FirstOrDefault(node => node.guid == guid);
        }
        /// <summary>
        /// 销毁函数，用于彻底销毁自身及自身关联的节点资源
        /// </summary>
        public void Destroy()
        {
            // 销毁自身
            GameObject.Destroy(this);
            // 销毁黑板
            GameObject.Destroy(ProfilePool);
            // 销毁内部节点
            nodes.ForEach(GameObject.Destroy);
            nodes.Clear();
        }
        #if UNITY_EDITOR
        public void DeleteNode(BaseDialogNode node)
        {
            Undo.RecordObject(this, "Dialog Tree(Delete Node)");
            nodes.Remove(node);
            if (rootNode == node)
            {
                rootNode = null;
            }

            // AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        public bool RemoveChildNode(BaseDialogNode parent, BaseDialogNode child)
        {
            Undo.RecordObject(parent, "Dialog Tree(Remove Child Node)");
            var result = parent.RemoveChildNode(child);
            EditorUtility.SetDirty(parent);
            return result;
        }
        public bool AddChildNode(BaseDialogNode parent, BaseDialogNode child)
        {
            Debug.Log($"AddChildNode,{parent.name},{child.name}");
            Undo.RecordObject(parent, "Dialog Tree(Add Child Node)");
            var result = parent.AddChildNode(child);
            EditorUtility.SetDirty(parent);
            return result;
        }
        #endif
    }
}