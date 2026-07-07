using DialogExecute;
using Tools.Dialog;
using Tools.Dialog.Editor.View;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogTreeEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    
    private ObjectField _ofDialogTree;
    private ToolbarButton _tbRevert;
    private ToolbarButton _tbApply;
    
    private DialogTreeView _dialogTreeView;
    private DialogTreeInspectorView _dialogTreeInspectorView;
    private ProfilePoolView _profilePoolView;
    
    [MenuItem("Window/UI Toolkit/DialogTreeEditor")]
    public static void ShowExample()
    {
        DialogTreeEditor wnd = GetWindow<DialogTreeEditor>();
        wnd.titleContent = new GUIContent("DialogTreeEditor");
    }
    
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        if (Selection.activeObject is DialogTree dialogTree && AssetDatabase.CanOpenAssetInEditor(instanceID))
        {
            ShowExample();
            return true;
        }

        return false;
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        // 引用UXML
        var visualTree =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Tools/Dialog/Editor/DialogTreeEditor.uxml");
        visualTree.CloneTree(root);
        // 添加USS
        var styleSheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Tools/Dialog/Editor/DialogTreeEditor.uss");
        root.styleSheets.Add(styleSheet);
        
        //绑定元素
        _ofDialogTree = root.Q<ObjectField>("OfDialogTree");
        _tbRevert = root.Q<ToolbarButton>("TbRevert");
        _tbApply = root.Q<ToolbarButton>("TbApply");
        
        _dialogTreeView = root.Q<DialogTreeView>("DialogTreeView");
        _dialogTreeInspectorView = root.Q<DialogTreeInspectorView>("DialogTreeInspectorView");
        _profilePoolView = root.Q<ProfilePoolView>("ProfilePoolView");

        // 设置节点选择回调
        _dialogTreeView.OnNodeSelected = _dialogTreeInspectorView.HandleNodeSelected;
        _dialogTreeView.OnNodeUnSelected = _dialogTreeInspectorView.HandleNodeUnselected;

        // 监听运行/编辑模式
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;

        // 监听行为树对象变化
        _ofDialogTree.RegisterValueChangedCallback(HandleDialogTreeChanged);
        

        // 撤销操作和保存操作
        _tbRevert.clicked += HandleRevertClicked;
        _tbApply.clicked += HandleApplyClicked;

        //设置选项过滤
        _ofDialogTree.objectType = typeof(DialogTree);
        _ofDialogTree.allowSceneObjects = false;
        
        // 获取行为树
        TryToGetTree();
        TryToUpdateView();
    }
    private void OnGUI()
    {
        if (_dialogTreeView != null && Event.current != null)
        {
            _dialogTreeView.MousePosition = Event.current.mousePosition;
        }
    }
    #if UNITY_EDITOR
    private void HandleRevertClicked()
    {
        Undo.PerformUndo();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    private void HandleApplyClicked()
    {
        if (_ofDialogTree.value && _ofDialogTree.value is DialogTree tree)
        {
            Undo.ClearAll();
            new SerializedObject(tree).ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
    #endif
    private void HandlePlayModeStateChanged(PlayModeStateChange playModeStateChange)
    {
        switch (playModeStateChange)
        {
            case PlayModeStateChange.EnteredEditMode:
                // 获取行为树
                TryToGetTree();
                TryToUpdateView();
                break;
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                // 获取行为树
                TryToGetTree();
                TryToUpdateView();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                break;
        }
    }
    private void HandleDialogTreeChanged(ChangeEvent<Object> evt)
    {
        // 更新树UI
        TryToUpdateView();
    }
    /// <summary>
    /// 项目资源选中改变节点回调
    /// </summary>
    private void OnSelectionChange()
    {
        // 获取行为树
        TryToGetTree();
    }
    private void TryToUpdateView()
    {
        var tree = _ofDialogTree.value as DialogTree;
        _dialogTreeView?.UpdateView(tree);
        _profilePoolView?.UpdateView(tree?.ProfilePool);
    }
    private void TryToGetTree()
    {
        // 当项目选中对话树文件且能够打开对话树时获取对话树
        if (Selection.activeObject is DialogTree dialogTree)
        {
            _ofDialogTree.value = dialogTree;
            return;
        }

        // 当项目选中游戏对象且存在对话树运行组件且树不为空时获取对话树
        if (Selection.activeGameObject)
        {
            var dialogUIView = Selection.activeGameObject.GetComponent<DialogUIView>();
            if (dialogUIView && dialogUIView.executor?.RuntimeTree != null)
            {
                _ofDialogTree.value = dialogUIView.executor.RuntimeTree;
            }
        }
    }
}
