using System;
using System.Collections.Generic;
using System.Linq;
using Tools.Quest.Data;
using Tools.Quest.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private VisualTreeAsset m_QuestListItemView = default;
    
    private ListView LvQuestList;
    private IMGUIContainer ImcQuestConfig;
    
    private Button BtnAddQuest;
    private Button BtnRemoveQuest;
    private Button BtnApply;
    
    private ObjectField OfQuestPool;
    
    private QuestPool questPool;
    private QuestConfig selectedQuestConfig;
    private QuestConfigListInspector _questConfigListInspector;
    
    
    private QuestEmptyInspector emptyInspector;
    private UnityEditor.Editor selectedQuestConfigEditor;
    [MenuItem("Window/UI Toolkit/QuestEditor")]
    public static void ShowExample()
    {
        QuestEditor wnd = GetWindow<QuestEditor>();
        wnd.titleContent = new GUIContent("QuestEditor");
    }
    private void HandleApplyClicked()
    {
        Debug.Log("Apply");
        if (OfQuestPool.value && OfQuestPool.value is QuestPool questPool)
        {
            Debug.Log(OfQuestPool.value.name);
            Undo.ClearAll();
            new SerializedObject(questPool).ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            RefreshQuestConfig();
        }
    }
    
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        //import uxml 
        var visualTree =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Tools/Quest/Editor/QuestEditor.uxml");
        visualTree.CloneTree(root);
        
        m_QuestListItemView = 
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Tools/Quest/Editor/QuestItem.uxml");

        //add uss
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Tools/Quest/Editor/QuestEditor.uss");
        root.styleSheets.Add(styleSheet);
        
        LvQuestList = root.Q<ListView>("SvQuestList");
        ImcQuestConfig = root.Q<IMGUIContainer>("ImcQuestConfig");
        BtnAddQuest = root.Q<Button>("BtnAddQuest");
        BtnRemoveQuest = root.Q<Button>("BtnRemoveQuest");
        BtnApply = root.Q<Button>("BtnApply");
        OfQuestPool = root.Q<ObjectField>("OfQuestPool");
        emptyInspector = ScriptableObject.CreateInstance<QuestEmptyInspector>();
        
        // 设定事件绑定
        LvQuestList.makeItem = CreateQuestListItemView;
        LvQuestList.bindItem += BindQuestListItemView;
        LvQuestList.selectionChanged += HandleQuestListSelectionChanged;
        LvQuestList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        
        BtnAddQuest.clicked += CreateQuestItem;
        BtnRemoveQuest.clicked += RemoveQuestItem;
        BtnApply.clicked += HandleApplyClicked;
        OfQuestPool.RegisterValueChangedCallback(HandleQuestPoolValueChanged);
        ImcQuestConfig.onGUIHandler += DrawQuestConfigInspector;
        
        //设置选项过滤
        OfQuestPool.objectType = typeof(QuestPool);
        OfQuestPool.allowSceneObjects = false;
    }
    
    private void OnDestroy()
    {
        if (selectedQuestConfigEditor != null)
        {
            DestroyImmediate(selectedQuestConfigEditor);
            selectedQuestConfigEditor = null;
        }
        LvQuestList.bindItem -= BindQuestListItemView;
        LvQuestList.selectionChanged -= HandleQuestListSelectionChanged;
        BtnAddQuest.clicked -= CreateQuestItem;
        BtnRemoveQuest.clicked -= RemoveQuestItem;
        OfQuestPool.UnregisterValueChangedCallback(HandleQuestPoolValueChanged);
        ImcQuestConfig.onGUIHandler -= DrawQuestConfigInspector;
    }

    private void DrawQuestConfigInspector()
    {
        if (selectedQuestConfigEditor == null)
        {
            if (selectedQuestConfig == null)
            {
                selectedQuestConfigEditor = Editor.CreateEditor(emptyInspector);
            }
            else
            {
                selectedQuestConfigEditor = Editor.CreateEditor(selectedQuestConfig);
            }
        }

        var editor = selectedQuestConfigEditor;

        if (editor == null)
            return;

        // ⭐关键：更新序列化数据
        editor.serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        editor.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            // ⭐关键1：先应用序列化对象的修改（核心步骤）
            editor.serializedObject.ApplyModifiedProperties();
            
            // 刷新列表显示最新数据
            LvQuestList.RefreshItems();
        }
    }

    private void BindQuestListItemView(VisualElement element, int index)
    {
        var data = LvQuestList.itemsSource[index] as QuestConfig;

        var idLabel = element.Q<Label>("IdLabel");
        var titleLabel = element.Q<Label>("TitleLabel");
        var descLabel = element.Q<Label>("DescLabel");

        idLabel.text = index.ToString();
        titleLabel.text = data.title;
        descLabel.text = data.description;
    }
    private void RemoveQuestItem()
    {
        if (questPool == null) return;

        if (selectedQuestConfig == null)
        {
            return;
        }

        // 先获取要删除项的索引
        int removeIndex = questPool.QuestConfigList.IndexOf(selectedQuestConfig);
        if (removeIndex >= 0)
        {
            // 移除指定的任务配置
            questPool.RemoveQuestConfig(selectedQuestConfig);
        
            // 删除后处理选中逻辑：如果列表还有数据，选中相邻项；否则清空选中
            if (questPool.QuestConfigList.Count > 0)
            {
                // 选中上一项（如果是第一项则选中最后一项）
                int newSelectedIndex = Mathf.Max(0, removeIndex - 1);
                if (newSelectedIndex >= questPool.QuestConfigList.Count)
                {
                    newSelectedIndex = questPool.QuestConfigList.Count - 1;
                }
                LvQuestList.selectedIndex = newSelectedIndex;
                // 更新选中的配置项
                selectedQuestConfig = questPool.QuestConfigList[newSelectedIndex];
            }
            else
            {
                // 列表为空，清空选中状态
                LvQuestList.selectedIndex = -1;
                selectedQuestConfig = null;
            }
        
            // 刷新列表显示
            RefreshQuestConfig();
            
        }
    }
    private void CreateQuestItem()
    {
        if (questPool == null) return;
    
        var questConfig = questPool.AddQuestConfig();
        // 确保新创建的QuestConfig有默认值（避免Label显示空）
        questConfig.id = Guid.NewGuid().ToString();
        questConfig.title = "新任务";
        questConfig.description = "新任务描述";
    
        if (selectedQuestConfig == null)
        {
            selectedQuestConfig = questConfig;
        }
    
        RefreshQuestConfig();
    
        // 选中新增的项
        int newIndex = questPool.QuestConfigList.IndexOf(questConfig);
        if (newIndex >= 0)
        {
            LvQuestList.selectedIndex = newIndex;
        }
    }

    private VisualElement CreateQuestListItemView()
    {
        var item = m_QuestListItemView.CloneTree();
        return item;
    }
    
    private void HandleQuestPoolValueChanged(ChangeEvent<UnityEngine.Object> evt)
    {
        if (selectedQuestConfigEditor != null)
        {
            DestroyImmediate(selectedQuestConfigEditor);
            selectedQuestConfigEditor = null;
        }

        questPool = OfQuestPool.value as QuestPool;

        if (questPool == null)
        {
            LvQuestList.itemsSource = null;
            selectedQuestConfig = null;
            return;
        }

        LvQuestList.itemsSource = questPool.QuestConfigList;
        LvQuestList.Rebuild();
        LvQuestList.RefreshItems();

        Debug.Log($"Created quest pool : {questPool.QuestConfigList.Count}");
    }

    private void RefreshQuestConfig()
    {
        if (questPool == null)
        {
            LvQuestList.itemsSource = null;
            LvQuestList.RefreshItems();
            LvQuestList.Rebuild();  // 确保列表重新构建
            return;
        }

        // 重新赋值并触发完整刷新
        LvQuestList.itemsSource = questPool.QuestConfigList; // 新建List触发变更检测
        LvQuestList.Rebuild(); // 重建列表
        LvQuestList.RefreshItems(); // 刷新所有项
    }

    private void HandleQuestListSelectionChanged(IEnumerable<object> objs)
    {
        selectedQuestConfig = objs.FirstOrDefault() as QuestConfig;

        if (selectedQuestConfig == null)
        {
            if(selectedQuestConfigEditor != null)
            {
                DestroyImmediate(selectedQuestConfigEditor);
                selectedQuestConfigEditor = null;
            }
            return;
        }
        
        selectedQuestConfigEditor = UnityEditor.Editor.CreateEditor(selectedQuestConfig);
    }
}
