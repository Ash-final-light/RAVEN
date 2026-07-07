using System;
using System.Collections.Generic;
using NaiQiu.Framework.View;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tools.Quest;
using Tools.Quest.Data;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.View;
using Tools.RecycleViewFramework.Widgets.RecyclerView;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Layout;
using VContainer;

namespace Quest
{
    public class QuestView:View
    {
        private Button closeBtn;
        private RecyclerView typeListView;
        private RecyclerView taskListView;
        private TMP_Text nameText;
        
        
        private TMP_Text questTitle;
        private TMP_Text questFixedDescription;
        private TMP_Text questDynamicDescription;

        private Adapter<QuestTypeData> typeAdapter;
        private QuestListAdapater taskGroupAdapter;
        
        [ShowInInspector]private List<QuestViewData> runningQuestViewDatas = new();
        private int lastIndex;
        private string selectedQuestId;
        private string selectedTemplateName;
        [Inject] private QuestManager _questManager;
        private QuestRewardDetailsView _questRewardDetails;

        public override void OnCreate(UIManager uiManager)
        {
            base.OnCreate(uiManager);
            
            typeListView = transform.Find("TypeListView").GetComponent<RecyclerView>();
            taskListView = transform.Find("QuestListView").GetComponent<RecyclerView>();
            nameText = transform.Find("Topbar/NameText").GetComponent<TextMeshProUGUI>();
            questTitle = transform.Find("QuestDetails/QuestTitle").GetComponent<TextMeshProUGUI>();
            questFixedDescription = transform.Find("QuestDetails/QuestFixedDescription").GetComponent<TextMeshProUGUI>();
            questDynamicDescription = transform.Find("QuestDetails/QuestDynamicDescription").GetComponent<TextMeshProUGUI>();
            
            closeBtn = transform.Find("Topbar/CloseBtn").GetComponent<Button>();
            _questRewardDetails = transform.Find("QuestDetails").GetComponent<QuestRewardDetailsView>();
            
            closeBtn.onClick.AddListener(() =>
            {
                uiManager.Close("QuestView");
                //uiManager.Open("UIControlPanelView");
            });
            
            
            typeAdapter = new Adapter<QuestTypeData>(typeListView);
            
            typeListView.SetLayoutManager(new LinearLayoutManager()); 
            typeAdapter.SetOnItemClick((data) =>
                {
                    Debug.Log(data.name);
                }
            );
    
            
            taskGroupAdapter = new QuestListAdapater(taskListView,"QuestGroupHolder");
           
            taskListView.SetLayoutManager(new MixedLayoutManager());
            // 混合布局
            taskGroupAdapter.SetList(runningQuestViewDatas);
            // 点击任务组
            taskGroupAdapter.SetOnItemClick(data =>
            {
                SetTaskDetails(data);
            });
            
            // 初始化数据
            FillData();
            _questRewardDetails.FindUI();
            
            // Bind quest receive, submit, and update events.
            _questManager.OnQuestReceived += AddRunningQuest;
            _questManager.OnQuestSubmitted += RemoveRunningQuest;
            _questManager.OnQuestUpdate += UpdateRunningQuest;

            foreach (var quest in _questManager.PlayerQuests)
                AddRunningQuest(quest);
        }

        public override void OnDestroy()
        {
            if (_questManager != null)
            {
                _questManager.OnQuestReceived -= AddRunningQuest;
                _questManager.OnQuestSubmitted -= RemoveRunningQuest;
                _questManager.OnQuestUpdate -= UpdateRunningQuest;
            }
            base.OnDestroy();
        }

        private void AddRunningQuest(Tools.Quest.Data.Quest quest)
        {
            Debug.Log($"AddRunningQuest {quest.info.id}");
            var wasExpanded = runningQuestViewDatas.Exists(x =>
                x.questId == quest.info.id && x.templateName == "QuestGroupHolder" && x.isExpand);
            runningQuestViewDatas.RemoveAll(x => x.questId == quest.info.id);

            if (!quest.state.IsQuestInProgress())
            {
                taskGroupAdapter.NotifyDataChanged();
                RefreshSelectedTaskDetails(quest.info.id);
                return;
            }

            var questGroup = _questManager.GetQuestGroupData(quest.info.id);
            var questStep = _questManager.GetQuestStepInfo(quest.info.id);
            // 初始化数据组
            if(questGroup != null)
            {
                questGroup.isExpand = wasExpanded;
                runningQuestViewDatas.Add(questGroup);
            }
            else
                Debug.LogError($"AddRunningQuest {quest.info.id} has no quest group.");
            // 初始化数据步骤
            if(questStep != null)
                runningQuestViewDatas.Add(questStep);
            
            taskGroupAdapter.NotifyDataChanged();
            RefreshSelectedTaskDetails(quest.info.id);
        }
        private void RemoveRunningQuest(Tools.Quest.Data.Quest quest)
        {
            Debug.Log($"RemoveRunningQuest {quest.info.id}");
            runningQuestViewDatas.RemoveAll(x => x.questId == quest.info.id);
            taskGroupAdapter.NotifyDataChanged();
            RefreshSelectedTaskDetails(quest.info.id);
        }
        private void UpdateRunningQuest(Tools.Quest.Data.Quest quest)
        {
            Debug.Log($"UpdateRunningQuest {quest.info.id}");
            if (quest.state.IsQuestInProgress())
                AddRunningQuest(quest);
            else
                RemoveRunningQuest(quest);
        }
        private void SetTaskDetails(QuestViewData data)
        {
            selectedQuestId = data.questId;
            selectedTemplateName = data.templateName;
            questTitle.text = data.title;
            questFixedDescription.text = data.fixedDescrption;
            questDynamicDescription.text = data.dynamicDescription;
            // 初始化奖励详情
            var rewardList = _questManager.GetQuestRewardData(data.questId);
            _questRewardDetails.SetQuestDetails(rewardList);
        }



        private void RefreshSelectedTaskDetails(string changedQuestId)
        {
            if (string.IsNullOrEmpty(selectedQuestId) || selectedQuestId != changedQuestId)
                return;

            var latestData = runningQuestViewDatas.Find(x =>
                x.questId == selectedQuestId && x.templateName == selectedTemplateName);
            if (latestData != null)
            {
                SetTaskDetails(latestData);
                return;
            }

            selectedQuestId = null;
            selectedTemplateName = null;
            questTitle.text = string.Empty;
            questFixedDescription.text = string.Empty;
            questDynamicDescription.text = string.Empty;
            _questRewardDetails.SetQuestDetails(null);
        }

        private void FillData()
        {
            List<QuestTypeData> questTypeDatas =
            new List<QuestTypeData>()
            {
                new QuestTypeData("Icon_Task_All", "所有任务", "Icon_Task_All", "ItemIconAtlas", 0),
                new QuestTypeData("Icon_Task_Daily", "每日任务", "Icon_Task_Daily", "ItemIconAtlas", 0),
                new QuestTypeData("Icon_Task_World", "世界任务", "Icon_Task_World", "ItemIconAtlas", 0),
                new QuestTypeData("Icon_Task_Legend", "传说任务", "Icon_Task_Legend", "ItemIconAtlas", 0),
            };
            typeAdapter.SetList(questTypeDatas);
            typeAdapter.NotifyDataChanged();
        }
    }
    [Serializable]
    public class QuestRewardData
    {
        public string thumbnail;
        public int count;
        public int price;
        
        public QuestRewardData(string thumbnail, int count, int price)
        {
            this.thumbnail = thumbnail;
            this.count = count;
            this.price = price;
        }
    }
    public class QuestTypeData
    {
        public string key;
        public string name;
        public string spriteName;
        public string atlasName;
        public int maxCount;
        
        public QuestTypeData(string key, string name, string spriteName, string atlasName, int maxCount)
        {
            this.key = key;
            this.name = name;
            this.spriteName = spriteName;
            this.atlasName = atlasName;
            this.maxCount = maxCount;
        }
    }
}
