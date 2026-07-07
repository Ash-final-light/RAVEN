using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.AI;
using Tools.BehaviourTree.Nodes;
using Tools.BehaviourTree.Nodes.CompositeNode;
using Tools.BehaviourTree.Nodes.DecorateNode;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Tools.BehaviourTree
{
    /// <summary>
    /// 枚举所有行为树类型，决定创建面板显示的节点种类
    /// </summary>
    public enum TreeType
    {
        Base,
        Bat,
        Beast01
    }
    [CreateAssetMenu()]
    public class BehaviourTree:ScriptableObject
    {
        [ShowInInspector] public TreeType treeType { get; private set; }
        public Node rootNode;
        public Node.State treestate = Node.State.Running;
        public List<Node> nodes = new List<Node>();

        public Blackboard.Data.Blackboard blackboard;
        
        private float _time = 0f;
        public float Time => _time;
        
        public Node.State Tick(float deltaTime = 0f)
        {
            if(rootNode.state == Node.State.Running)
            {
                _time += deltaTime;
                treestate = rootNode.Tick(deltaTime);
            }
            return treestate;
        }
#if UNITY_EDITOR
        public Blackboard.Data.Blackboard CreateBlackboard()
        {
            Blackboard.Data.Blackboard data = ScriptableObject.CreateInstance(typeof(Blackboard.Data.Blackboard)) as Blackboard.Data.Blackboard;
            
            EditorUtility.SetDirty(data);
            AssetDatabase.AddObjectToAsset(data,this);
            AssetDatabase.SaveAssets();
            return data;
        }
        public Node CreateNode(System.Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.tree = this;
            node.blackboard = blackboard;
            
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node,this);
            }
            
            
            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (Create Node)");
            Undo.RecordObject(this,"Behaviour Tree (Create Node)");
            
            nodes.Add(node);
            
            EditorUtility.SetDirty(this);
            
            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            Debug.Log($"DeleteNode {node.name}");
            Undo.RecordObject(this,"Behaviour Tree (Delete Node)");
            nodes.Remove(node);
            
            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
            
            // delete rootNode
            if(rootNode == node)
            {
                rootNode = null;
            }
        }
        
        public void AddChild(Node parent,Node child)
        {
            DecorateNode decorator = parent as DecorateNode;
            if(decorator)
            {
                Debug.Log($"AddChild {parent.name},{child.name}");
                Undo.RecordObject(decorator, "Behaviour Tree (Add Child");
                decorator.child = child;
                EditorUtility.SetDirty(decorator);
            }
            
            CompositeNode composite = parent as CompositeNode;
            if(composite)
            {
                Debug.Log($"AddChild {parent.name},{child.name}");
                Undo.RecordObject(composite, "Behaviour Tree (Add Child");
                composite.childNodes.Add(child);
                EditorUtility.SetDirty(composite);
            }
            
            RootNode rootNode = parent as RootNode;
            if(rootNode)
            {
                Debug.Log($"AddChild {parent.name},{child.name}");
                Undo.RecordObject(rootNode, "Behaviour Tree (Add Child");
                rootNode.child = child;
                EditorUtility.SetDirty(rootNode);
            }
        }
        public void RemoveChild(Node parent, Node child)
        {
            DecorateNode decorator = parent as DecorateNode;
            if(decorator)
            {
                Debug.Log($"RemoveChild {parent.name},{child.name}");
                Undo.RecordObject(decorator, "Behaviour Tree (Remove Child");
                decorator.child = null;
                EditorUtility.SetDirty(decorator);
            }
            
            CompositeNode composite = parent as CompositeNode;
            if(composite)
            {
                Debug.Log($"RemoveChild {parent.name},{child.name}");
                Undo.RecordObject(composite, "Behaviour Tree (Remove Child");
                composite.childNodes.Remove(child);
                EditorUtility.SetDirty(composite);
            }
            
            RootNode rootNode = parent as RootNode;
            if(rootNode)
            {
                Debug.Log($"RemoveChild {parent.name},{child.name}");
                Undo.RecordObject(rootNode, "Behaviour Tree (Remove Child");
                rootNode.child = null;
                EditorUtility.SetDirty(rootNode);
            }
        }
#endif
        public List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();
            
            DecorateNode decorator = parent as DecorateNode;
            if(decorator && decorator.child != null)
            {
                children.Add(decorator.child);
            }
            RootNode root = parent as RootNode;
            if(root && root.child != null)
            {
                children.Add(root.child);
            }
            CompositeNode composite = parent as CompositeNode;
            if(composite)
            {
                return composite.childNodes;
            }
            
            return children;
        }

        public void Tranverse(Node node, System.Action<Node> action)
        {
            if (node)
            {
                action.Invoke(node);
                var children = GetChildren(node);
                children.ForEach(child => Tranverse(child, action));
            }
        }
        public BehaviourTree Clone()
        {
            BehaviourTree behaviourTree = Instantiate(this);
            Blackboard.Data.Blackboard blackboard = Instantiate(this.blackboard);
            behaviourTree.blackboard = blackboard;
            behaviourTree.rootNode = rootNode.Clone();
            behaviourTree.nodes = new List<Node>();
            Tranverse(behaviourTree.rootNode, node => behaviourTree.nodes.Add(node));
            return behaviourTree;
        }

        public void Bind(AiStatus status)
        {
            Tranverse(rootNode, node =>
            {
                node.AiStatus = status;
                node.blackboard = blackboard;
            });
        }
    }
}