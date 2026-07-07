using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Archive.Data;
using NaiQiu.Framework.View;
using Package;
using Quest;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.CustomObject.Data;
using Tools.Quest.Data;
using Tools.Quest.Event;
using Tools.Quest.Goal;
using Tools.Quest.Require;
using Tools.Quest.Step;
using Tools.RecycleViewFramework.View;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Tools.Quest
{
    public delegate void OnQuestReceived(Data.Quest quest);

    public delegate void OnQuestSubmitted(Data.Quest quest);
    
    public delegate void OnQuestUpdate(Data.Quest quest);

    
    
    //TODO: 后续改成继承IStarable
    public class QuestManager:MonoBehaviour,Archive.IArchivable
    {
        [FormerlySerializedAs("questPool")] [LabelText("任务池")] 
        [SerializeField] private QuestPool questPoolTemplate;
        private QuestPool _runningQuestPool;
        public QuestPool QuestPool => _runningQuestPool;

        [LabelText("调试")] [SerializeField] private bool debug;

        [Inject] private IObjectResolver _objectResolver;
        [Inject] private PackageObjectManager _objectManager;
        //测试用展示ui接口，正式项目删除
        [Inject] private UIManager questViewManager;

        [ShowInInspector, ReadOnly] private readonly Dictionary<string, Data.Quest> _quests = new(); // 全部任务列表
        [ShowInInspector, ReadOnly] private readonly HashSet<string> _playerQuestIds = new(); // 玩家任务id集合
        private readonly HashSet<string> _pendingQuestIds = new();
        private QuestEventHub _eventHub;
        
        public Data.Quest[] Quests => _quests.Values.ToArray();

        public Data.Quest[] PlayerQuests
        {
            get { return _playerQuestIds.Select(questId => _quests[questId]).ToArray(); }
        }

        public event OnQuestReceived OnQuestReceived;
        public event OnQuestSubmitted OnQuestSubmitted;
        public event OnQuestUpdate OnQuestUpdate;
        
        private void Start()
        {
            
            if (questPoolTemplate == null)
            {
                Debug.LogError("QuestManager requires a QuestPool template.");
                return;
            }

            _eventHub = QuestEventHub.Instance;
            _eventHub.Published += HandleQuestEventPublished;
            _runningQuestPool =questPoolTemplate.Clone();
            BuildQuests();
            EvaluateAllQuests();
        }

        private void LateUpdate()
        {
            if (_pendingQuestIds.Count == 0)
                return;

            var pending = _pendingQuestIds.ToArray();
            _pendingQuestIds.Clear();
            foreach (var questId in pending)
                UpdateQuestById(questId, 0f);
        }

        private void OnDestroy()
        {
            if (_eventHub != null)
                _eventHub.Published -= HandleQuestEventPublished;

            foreach (var quest in Quests)
            foreach (var step in quest.steps)
                if (step.triggered && !step.completed)
                    step.info.Interrupt?.Invoke(step);
        }

        public void SetQuestPoolTemplate(QuestPool pool)
        {
            questPoolTemplate = pool;
        }

        public void EvaluateAllQuests()
        {
            foreach (var quest in Quests)
                UpdateQuest(quest, 0f);
        }

        private void HandleQuestEventPublished(QuestEventMessage message)
        {
            foreach (var quest in Quests)
                if (!quest.state.IsQuestCompleted())
                    _pendingQuestIds.Add(quest.info.id);
        }

        [Button("背包窗口显示")]
        private void TestOpenPackageView(bool open = true)
        {
            if (!open)
            {
                questViewManager.Close("ObjectPackageView");
                return;
            }
            questViewManager.Open("ObjectPackageView");
        }
        
        [Button("任务窗口显示")]
        private void TestOpenQuestView(bool open = true)
        {
            if (!open)
            {
                questViewManager.Close("QuestView");
                return;
            }
            questViewManager.Open("QuestView");
        }
       
        private void BuildQuests()
        {
            // 将未完成的任务统一中断
            Quests.ForEach(quest =>
            {
                if (!quest.state.IsQuestInProgress())
                {
                    return;
                }

                // 中断正在执行的任务步骤
                quest.steps.ForEach(step =>
                {
                    if (step.triggered && !step.completed)
                    {
                        step.info.Interrupt?.Invoke(step);
                    }
                });
            });
            // 清空任务列表，并从预设任务池中重新构造列表
            _quests.Clear();
            _quests.AddRange(_runningQuestPool.QuestConfigs.ToDictionary(
                questConfig => questConfig.id,
                questConfig =>
                {
                    // 依赖注入
                    questConfig.preRequires.ForEach(requirement => _objectResolver.Inject(requirement));
                    questConfig.steps.ForEach(step =>
                    {
                        _objectResolver.Inject(step);
                        step.goals.ForEach(goal => _objectResolver.Inject(goal));
                    });
                    questConfig.rewards.ForEach(reward => _objectResolver.Inject(reward));

                    // SO数据=>静态数据=>运行时数据
                    var questInfo = questConfig.ToQuestInfo();
                    var quest = new Data.Quest
                    {
                        info = questInfo,
                        state = QuestState.RequirementNotMeet,
                        requirements = questInfo.requirements.Select(requirementInfo => new QuestRequire
                        {
                            info = requirementInfo,
                            description = "",
                            meet = false,
                        }).ToArray(),
                        steps = questInfo.steps.Select(stepInfo => new QuestStep
                        {
                            info = stepInfo,
                            triggered = false,
                            completed = false,
                            goals = stepInfo.goals.Select(goalInfo => new QuestGoal
                            {
                                info = goalInfo,
                                description = "",
                                triggered = false,
                                completed = false,
                                completionHandled = false,
                                state = "",
                            }).ToArray(),
                        }).ToArray(),
                        rewards = questInfo.rewards.Select(rewardInfo => new QuestReward
                        {
                            info = rewardInfo,
                            given = false,
                        }).ToArray(),
                    };
                    return quest;
                }));
        }
        public void SubmitQuest(string questId)
        {
            if (!_quests.TryGetValue(questId, out var quest))
            {
                Debug.LogError($"The Quest(id={questId}) is not found in the quest list");
                return;
            }

            if (quest.state != QuestState.AllStepsComplete)
            {
                if (debug)
                {
                    Debug.Log($"不满足任务《{quest.info.title}》({quest.info.id})的提交条件");
                }

                return;
            }

            if (debug)
            {
                Debug.Log($"提交任务《{quest.info.title}》({quest.info.id})");
            }

            quest.state = QuestState.Completed;
            _playerQuestIds.Add(questId);
            // 完成任务后立即更新任务
            UpdateQuest(quest, 0);
            // 调用事件
            OnQuestSubmitted?.Invoke(quest);
        }
        public void ReceiveQuest(string questId)
        {
            if (!_quests.TryGetValue(questId, out var quest))
            {
                Debug.LogError($"The Quest(id={questId}) is not found in the quest list");
                return;
            }
            if(quest.state.IsQuestInProgress())
            {
                return;
            }

            if (quest.state != QuestState.RequirementMeet)
            {
                if (debug)
                {
                    Debug.Log($"不满足任务《{quest.info.title}》({quest.info.id})的接受条件");
                }

                return;
            }

            if (debug)
            {
                Debug.Log($"接受任务《{quest.info.title}》({quest.info.id})");
            }

            quest.state = QuestState.InProgress;
            _playerQuestIds.Add(questId);
            // 接受任务后立即更新任务
            UpdateQuest(quest, 0);
            // 调用事件
            OnQuestReceived?.Invoke(quest);
        }

        /// <summary>
        /// 提供给ui显示器的任务组数据
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        public QuestViewData GetQuestGroupData(string questId)
        {
            if(!_quests.TryGetValue(questId, out var quest) || 
               quest.state.IsQuestCompleted() || 
               quest.state == QuestState.RequirementNotMeet)
            {
                return null;
            }

            return new QuestViewData
            {
                questId = quest.info.id,
                title = quest.info.title,
                fixedDescrption = quest.info.description,
                dynamicDescription = "",
                isExpand = false,
                templateName = "QuestGroupHolder"
            };
        }
        /// <summary>
        /// 提供给ui显示器的任务步骤数据
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        public QuestViewData GetQuestStepInfo(string questId)
        {
            if(!_quests.TryGetValue(questId, out var quest) || 
               quest.state.IsQuestCompleted() || 
               quest.state == QuestState.RequirementNotMeet)
            {
                return null;
            }

            if (!_quests.ContainsKey(questId) || quest.StepIndex < 0 || quest.StepIndex >= quest.steps.Length)
            {
                return null;
            }

            return new QuestViewData
            {
                questId = quest.info.id,
                title = quest.info.title,
                isExpand = false,
                fixedDescrption = quest.steps[quest.StepIndex].info.description,
                dynamicDescription = quest.CurrentStepShowGoal,
                templateName = "QuestItemHolder"
            };
        }
        public List<QuestRewardData> GetQuestRewardData(string questId)
        {
            var config = _runningQuestPool.QuestConfigList.Find(config => config.id == questId);
            if(config == null || config.rewards == null || config.rewards.Count == 0)
            {
                return null;
            }
            var rewardList = new List<QuestRewardData>();
            if (debug)
            {
                Debug.Log($"任务《{config.id}》奖励条目：{config.rewards.Count}");
            }

            // 这里根据奖励类型来添加不同的奖励数据
            foreach (var reward in config.rewards)
            {
                if (debug)
                {
                    Debug.Log($"奖励类型：{reward.GetType()}");
                }
                
                var rewardData = reward as GiveObjQuestRewards;
                if (rewardData != null)
                {
                    Debug.Log($"物品奖励条目：{rewardData.givenObjectdata.Count}");
                    foreach (var giveObjQuestReward in rewardData.givenObjectdata)
                    {
                        var objectInfo = _objectManager.GetObjectInfoById(giveObjQuestReward.id);
                        rewardList.Add(TransToQuestRewardData(objectInfo, giveObjQuestReward.num));
                        if (debug)
                        {
                            Debug.Log($"添加物品奖励图标《{objectInfo.Name}》({objectInfo.Id}) x {giveObjQuestReward.num}");
                        }
                    }

                    continue;
                }
                if (debug)
                {
                    Debug.LogError("The QuestRewardType is not GiveObjQuestRewards");
                }   
            }

            
            return rewardList;

            QuestRewardData TransToQuestRewardData(ObjectInfo objectInfo, int count)
            {
                return new QuestRewardData(objectInfo.Thumbnail, count, objectInfo.Price);
            }
        }
        
        [Button("更新任务")]
        public void UpdateQuestById(string questId = "e3829dad-ba59-4353-ae29-2b4e8e32091e",float deltaTime = 0)
        {
            if (!_quests.TryGetValue(questId, out var quest))
            {
                Debug.LogError($"The Quest(id={questId}) is not found in the quest list");
                return;
            }
            UpdateQuest(quest, deltaTime);
        }
        private void UpdateQuest
            (Data.Quest quest,
                float deltaTime,
                bool autoReceive = true,
                bool autoSubmit = true)
        {
            if (debug)
            {
                Debug.Log($"更新任务《{quest.info.title}》({quest.info.id})");
            }

            CheckQuestRequirement();

            if (autoReceive)
            {
                ReceiveQuest(quest.info.id);
            }

            UpdateQuestInProgress();

            if (autoSubmit)
            {
                SubmitQuest(quest.info.id);
            }

            // 调用事件
            OnQuestUpdate?.Invoke(quest);

            TryToGiveQuestReward();

            void CheckQuestRequirement()
            {
                if (quest.state.IsQuestInProgress()||quest.state.IsQuestCompleted())
                {
                    return;
                }

                foreach (var requirement in quest.requirements)
                {
                    if (debug)
                    {
                        Debug.Log($"检查任务需求《{quest.info.title}》({requirement.info.id})");
                    }
                    requirement.info.Update?.Invoke(requirement,deltaTime);
                }
                // 检查任务需求是否满足
                if (quest.requirements.Length == 0 || quest.requirements.All(requirement => requirement.meet))
                {
                    quest.state = QuestState.RequirementMeet;
                }
                else
                {
                    quest.state = QuestState.RequirementNotMeet;
                    if (debug)
                    {
                        Debug.Log($"任务需求《{quest.info.title}》({quest.info.id})未满足");
                    }
                }
            }
           void UpdateQuestInProgress()
            {
                // 跳过不处于流程的任务
                if (quest.state != QuestState.InProgress)
                {
                    return;
                }

                // 如果发现当前任务没有步骤，也推进任务状态
                if (quest.steps.Length == 0)
                {
                    if (debug)
                    {
                        Debug.Log($"任务《{quest.info.title}》({quest.info.id})完成所有步骤");
                    }

                    quest.state = QuestState.AllStepsComplete;
                    return;
                }

                // 依次遍历任务步骤执行未触发的步骤的对应节点函数
                for (var index = 0; index < quest.steps.Length; index++)
                {
                    var step = quest.steps[index];
                    // 如果步骤已经完成，跳过该任务步骤
                    var triggered = step.triggered;
                    if (step.completed)
                    {
                        // 未触发的任务步骤标记为触发
                        if (!triggered)
                        {
                            step.triggered = true;
                            step.goals.ForEach(goal => goal.triggered = true);
                        }

                        // 步骤完成就跳到下一个步骤
                        if (debug)
                        {
                            Debug.Log($"任务《{quest.info.title}》({quest.info.id})完成步骤({step.info.id})");
                        }

                        continue;
                    }

                    // 步骤未触发则需要执行步骤的开始节点函数
                    if (!triggered)
                    {
                        if (debug)
                        {
                            Debug.Log($"任务《{quest.info.title}》({quest.info.id})执行步骤({step.info.id})的开始函数");
                        }

                        step.info.Start?.Invoke(step);
                        step.triggered = true;
                    }

                    // 执行步骤的更新节点函数
                    if (debug)
                    {
                        Debug.Log($"任务《{quest.info.title}》({quest.info.id})执行步骤({step.info.id})的更新函数");
                    }

                    step.info.Update?.Invoke(step, triggered ? deltaTime : 0);
                    
                    // 在更新后判断步骤是否完成，如果未完成则直接返回，即任务卡在这一步骤了
                    if (!step.completed)
                    {
                        return;
                    }

                    // 完成了就执行步骤的完成节点函数，并跳到下一步骤
                    if (debug)
                    {
                        Debug.Log($"任务《{quest.info.title}》({quest.info.id})执行步骤({step.info.id})的完成函数");
                    }

                    step.info.Complete?.Invoke(step);
                }

                // 到达这里说明全部任务步骤已经完成，推进任务状态
                quest.state = QuestState.AllStepsComplete;
                if (debug)
                {
                    Debug.Log($"任务《{quest.info.title}》({quest.info.id})完成所有步骤");
                }
            }
            void TryToGiveQuestReward()
            {
                if (!quest.state.IsQuestCompleted())
                {
                    return;
                }
                foreach (var reward in quest.rewards)
                {
                    if (reward.given)
                    {
                        continue;
                    }
                    reward.given = true;
                    // 调用事件
                    reward.info.GiveReward?.Invoke();
                    quest.state = QuestState.RewardGiven;
                    if (debug)
                    {
                        Debug.Log($"任务《{quest.info.title}》({quest.info.id})奖励《{reward.info}》({reward.info.id})");
                    }
                }
            }
        }
        public void Save(ArchiveData archiveData)
        {
            throw new System.NotImplementedException();
        }

        public void Load(ArchiveData archiveData)
        {
            throw new System.NotImplementedException();
        }
    }
}
