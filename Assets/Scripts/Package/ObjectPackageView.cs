using System.Collections.Generic;
using Common;
using NaiQiu.Framework.View;
using Sirenix.OdinInspector;
using TMPro;
using Tools.RecycleViewFramework.Load;
using TMPro;
using Tools.CustomObject;
using Tools.CustomObject.Data;
using Tools.CustomObject.Data.Extension;
using Tools.CustomObject.Interface;
using Tools.RecycleViewFramework.View;
using Tools.RecycleViewFramework.Widgets.RecyclerView;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Layout;
using Tools.ReddotTree;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Package
{
    public class ObjectPackageView : View
    {
        private TMP_Text nameText;
        private TMP_Text countText;
        private Button closeBtn;
        private RecyclerView typeListView;
        private RecyclerView itemListView;
        private ObjectDetailsView _objectDetailsView;

        private Adapter<BagData> typeAdapter;
        private Adapter<ObjectGroup> itemAdapter;

        [Inject] private PackageObjectManager packageObjectManager;

        public override void OnCreate(UIManager uiManager)
        {
            base.OnCreate(uiManager);

            nameText = transform.Find("TabView/NameText").GetComponent<TMP_Text>();
            countText = transform.Find("TabView/CountText").GetComponent<TMP_Text>();
            closeBtn = transform.Find("TabView/CloseBtn").GetComponent<Button>();
            typeListView = transform.Find("TabView/TypeListView").GetComponent<RecyclerView>();
            itemListView = transform.Find("ItemListView").GetComponent<RecyclerView>();
            _objectDetailsView = transform.Find("ItemDetailsView").GetComponent<ObjectDetailsView>();

            closeBtn.onClick.AddListener(() => { Close(); 
                //uiManager.Open("UIControlPanelView");
                });

            typeAdapter = new Adapter<BagData>(typeListView);
            typeListView.SetLayoutManager(new LinearLayoutManager());
            typeAdapter.SetOnItemClick(data =>
            {
                nameText.text = data.name;
                countText.text = $"{data.name} {itemAdapter.GetItemCount()}/{data.maxCount}";

                itemAdapter.SetList(packageObjectManager.GetSelectedObjectGroups(data.key));
                itemAdapter.Sort((a, b) => b.Data.Price.CompareTo(a.Data.Price));
                itemAdapter.ChoiceIndex = 0;
                _objectDetailsView.SetItemData(itemAdapter.GetData(0)?.ToObjectInfo());
            });

            itemAdapter = new Adapter<ObjectGroup>(itemListView);
            itemListView.SetLayoutManager(new GridLayoutManager(8));
            itemAdapter.SetOnItemClick(data => { _objectDetailsView.SetItemData(data.ToObjectInfo()); });

            packageObjectManager.OnObjectGroupAdded += RefreshListView;
            packageObjectManager.OnObjectGroupRemoved += RefreshListView;
            packageObjectManager.OnObjectGroupChanged += RefreshListView;
            
            FillData();
        }

        private void RefreshListView(string selectedTypeKey,ObjectGroup selectedObjectGroup)
        {
            typeAdapter.NotifyDataChanged();
            itemAdapter.NotifyDataChanged();
        }
        private void FillData()
        {
            if (typeAdapter == null)
            {
                Debug.LogError("FillData 失败：typeAdapter 未初始化！");
                return;
            }
            // 设置类型列表
            typeAdapter.SetList(packageObjectManager.typeList);
            typeAdapter.ChoiceIndex = 0;
            
            // 校验2：typeAdapter 获取第一条数据是否成功
            BagData firstTypeData = typeAdapter.GetData(0);
            if (firstTypeData == null)
            {
                Debug.LogError("FillData 失败：typeAdapter 第一条数据为null！");
                nameText.text = "未知类型";
                countText.text = "未知类型 0/0";
                return;
            }

            // 赋值标题文本
            nameText.text = firstTypeData.name;

            // 校验3：itemAdapter 是否初始化
            if (itemAdapter == null)
            {
                Debug.LogError("FillData 失败：itemAdapter 未初始化！");
                countText.text = $"{firstTypeData.name} 0/{firstTypeData.maxCount}";
                return;
            }

            // 校验4：PackageObjectManager 是否注入成功（VContainer 常见问题）
            if (packageObjectManager == null)
            {
                Debug.LogError("FillData 失败：packageObjectManager 未注入！请检查VContainer配置");
                countText.text = $"{firstTypeData.name} 0/{firstTypeData.maxCount}";
                // 兜底：设置空列表，避免崩溃
                itemAdapter.SetList(new List<ObjectGroup>());
                return;
            }

            // 校验5：调用 GetSelectedObjectGroups 并处理返回值
            List<ObjectGroup> objectGroups = packageObjectManager.GetSelectedObjectGroups(firstTypeData.key);
            if (objectGroups == null)
            {
                Debug.LogWarning($"FillData 警告：{firstTypeData.key} 对应的ObjectGroup列表为null，使用空列表兜底");
                objectGroups = new List<ObjectGroup>(); // 兜底空列表
            }

            // 设置物品列表（此时objectGroups一定非null）
            itemAdapter.SetList(objectGroups);
            itemAdapter.Sort((a, b) => b.Data.Price.CompareTo(a.Data.Price));

            // 校验6：物品列表是否有数据，再设置选中项
            if (itemAdapter.GetItemCount() > 0)
            {
                itemAdapter.ChoiceIndex = 0;
                _objectDetailsView.SetItemData(itemAdapter.GetData(0).ToObjectInfo());
            }
            else
            {
                Debug.LogWarning($"FillData 警告：{firstTypeData.key} 暂无物品数据");

            }

            // 最后更新计数文本（使用实际物品数量）
            countText.text = $"{firstTypeData.name} {itemAdapter.GetItemCount()}/{firstTypeData.maxCount}";


            ReddotTree reddotTree = GameViewManager.ReddotTree;
            if (reddotTree == null)
            {
                Debug.LogError("BindToReddotTree 失败：ReddotTree 组件未找到！");
                return;
            }

            List<ObjectGroup> dataGroups;
            //注册红点
            for (int i = 0; i < packageObjectManager.typeList.Count; i++)
            {
                reddotTree.RegisterReddotNode(packageObjectManager.typeList[i].id, true, 0);
                dataGroups = packageObjectManager.GetSelectedObjectGroups(packageObjectManager.typeList[i].key);
                for (int j = 0; j < dataGroups.Count; j++)
                {
                    reddotTree.RegisterReddotNode(dataGroups[j].GroupId, true, 0);
                    reddotTree.BindChildParent(dataGroups[j].GroupId, packageObjectManager.typeList[i].id);
                }
            }
        }

        [Button("模拟调用添加物品")]
        private void AddItem(int id, int num)
        {
            packageObjectManager.AddObjectById(id, num);
        }
        [Button("输出字典列表")]
        public void PrintDictionary()
        {
            foreach (var item in GameViewManager.ReddotTree.m_ChildParentDic)
            {
                Debug.Log($"Key: {item.Key}, Value: {item.Value}");
            }

            foreach (var item in GameViewManager.ReddotTree.m_ChildParentDic)
            {
                Debug.Log($"Key: {item.Key}, Value: {item.Value}");
            }
        }
    }

    public class BagData
    {
        public string key;
        public string name;
        public string icon;
        public int maxCount;
        public int count;
        public string id;



        public BagData(string key, string name, string icon, int maxCount, int count,string id)
        {
            this.key = key;
            this.name = name;
            this.icon = icon;
            this.maxCount = maxCount;
            this.count = count;
            this.id = id;
        }
    }

}