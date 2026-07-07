using Tools.BehaviourTree.Editor.View;
using Tools.Blackboard.Editor.View;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tools.BehaviourTree.Editor
{
    public class BehaviourTreeEditor : EditorWindow
    {
        InspectorView inspectorView;
        BlackboardView blackboardView;
        BehaviourTreeView treeView;
        private ObjectField _ofBehaviourTree;
    
        [MenuItem("Window/UI Toolkit/BehaviourTreeEditor")]
        public static void ShowExample()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if(Selection.activeObject is BehaviourTree tree)
            {
                ShowExample();
                return true;
            }
            return false;
        }
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            //import uxml 
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Tools/BehaviourTree/Editor/BehaviourTreeEditor.uxml");
            visualTree.CloneTree(root);

            //add uss
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Tools/BehaviourTree/Editor/BehaviourTreeEditor.uss");
            root.styleSheets.Add(styleSheet);
        
            treeView = root.Q<BehaviourTreeView>();
            inspectorView = root.Q<InspectorView>();
            blackboardView = root.Q<BlackboardView>();
        
            _ofBehaviourTree = root.Q<ObjectField>("OfBehaviourTree");
        
            treeView.OnNodeSelected += OnNodeSelectionChanged;
        
            OnSelectionChange();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange(); 
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }
        /// <summary>
        /// 需要预先打开BehaviourTreeEditor窗口时，避免报错
        /// </summary>
        /// <returns></returns>
        private BehaviourTree TryToGetTree()
        {
            if(_ofBehaviourTree == null)
                return null;
            // 当项目选中行为树文件且能够打开行为树时获取行为树
            if (Selection.activeObject is BehaviourTree behaviourTree)
            {
                _ofBehaviourTree.value = behaviourTree;
                return behaviourTree;
            }

            // 当项目选中游戏对象且存在行为树运行组件且树不为空时获取行为树
            if (Selection.activeGameObject)
            {
                Debug.Log($"Selected GameObject: {Selection.activeGameObject.name}");
                var behaviourTreeExecutor = Selection.activeGameObject.GetComponent<BehaviourRunner>();
                if (behaviourTreeExecutor && behaviourTreeExecutor.RunningTree)
                {
                    _ofBehaviourTree.value = behaviourTreeExecutor.RunningTree;
                    return behaviourTreeExecutor.RunningTree;
                }
            }
            return null;
        }

        private void OnSelectionChange()
        {
        
            BehaviourTree tree = TryToGetTree();
        
            if (tree != null)
            {
                if (Application.isPlaying)
                {
                    // 运行时直接刷新TreeView
                    treeView.PopulateView(tree);
                }
                else
                {
                    // 非运行时需检查是否可在编辑器中打开
                    if (AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
                    {
                        treeView.PopulateView(tree);
                    }
                }
            
                //刷新blackboardView
                blackboardView.UpdateView(tree.blackboard);
            }
        }
        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            inspectorView.UpdateView(nodeView);
        }

        private void OnInspectorUpdate()
        {
            treeView?.UpdateNodeStateView();
        }
    }
}
