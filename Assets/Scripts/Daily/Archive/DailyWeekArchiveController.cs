using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Archive;
using Archive.Data;
using Common;
using Daily.Config;
using Tools.CustomObject.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Daily.Archive
{
    public class DailyWeekArchiveController : MonoBehaviour, IArchivable
    {
        [Header("UI")]
        [SerializeField] private TMP_Text weekText;
        [SerializeField] private Button nextDayButton;
        [SerializeField] private Button saveListButton;
        [SerializeField] private Button closeSaveListButton;
        [SerializeField] private GameObject saveListPanel;
        [SerializeField] private List<DailyArchiveSlotView> slotViews = new List<DailyArchiveSlotView>();

        [Header("Daily Controllers")]
        [SerializeField] private DailyMissionPanelController missionPanelController;
        [SerializeField] private DailyMapStepController stepController;
        [SerializeField] private DailyStepCardHand cardHand;
        [SerializeField] private List<DailyMapNode> nodes = new List<DailyMapNode>();

        [Header("Archive")]
        [SerializeField] private string saveFileName = "daily_week_save.json";
        [SerializeField] private int maxDayCount = 7;
        [SerializeField] private bool loadOnStart = true;

        private static readonly string[] DefaultWeekNames =
        {
            "星期一",
            "星期二",
            "星期三",
            "星期四",
            "星期五",
            "星期六",
            "星期日"
        };

        private DailyWeekSaveData weekSaveData = new DailyWeekSaveData();

        [Inject] private DailyPackageService dailyPackageService;

        private string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, saveFileName); }
        }

        private void Awake()
        {
            BindSceneReferences();
            BindButtons();
        }

        private void OnEnable()
        {
            GameApplication.Instance?.ArchiveManager.Register(this);
        }

        private void OnDisable()
        {
            GameApplication.Instance?.ArchiveManager.Unregister(this);
        }

        private void Start()
        {
            if (loadOnStart)
            {
                ArchiveData newestArchive = GameApplication.Instance?.ArchiveManager.GetNewestArchive();
                if (newestArchive != null && newestArchive.daily?.week?.days != null &&
                    newestArchive.daily.week.days.Count > 0)
                {
                    GameApplication.Instance.ArchiveManager.NotifyNewestLoad();
                }
                else
                {
                    weekSaveData.currentDayIndex = 0;
                    SaveCurrentDaySnapshot();
                    GameApplication.Instance?.ArchiveManager.NotifySave(true);
                }
            }

            RefreshWeekText();
            RefreshSlotViews();
            if (saveListPanel != null)
            {
                saveListPanel.SetActive(false);
            }
        }

        public void EnterNextDay()
        {
            if (!CanChangeArchiveState())
            {
                return;
            }

            SaveCurrentDaySnapshot();
            GameApplication.Instance?.ArchiveManager.NotifySave(true);

            int lastDayIndex = Mathf.Max(0, maxDayCount - 1);
            if (weekSaveData.currentDayIndex >= lastDayIndex)
            {
                RefreshWeekText();
                RefreshSlotViews();
                return;
            }

            weekSaveData.currentDayIndex++;
            if (stepController != null)
            {
                stepController.ClearActionCounter();
            }

            SaveCurrentDaySnapshot();
            GameApplication.Instance?.ArchiveManager.NotifySave(true);
            RefreshWeekText();
            RefreshSlotViews();
        }

        public void ShowSaveList()
        {
            RefreshSlotViews();
            if (saveListPanel != null)
            {
                saveListPanel.SetActive(true);
            }
        }

        public void HideSaveList()
        {
            if (saveListPanel != null)
            {
                saveListPanel.SetActive(false);
            }
        }

        public void LoadDay(int dayIndex)
        {
            if (!CanChangeArchiveState())
            {
                return;
            }

            DailyDaySaveData daySave = FindDaySave(dayIndex);
            if (daySave == null || daySave.daily == null)
            {
                return;
            }

            if (daySave.archiveId >= 0)
            {
                GameApplication.Instance?.ArchiveManager.NotifyLoad(daySave.archiveId);
            }
            else
            {
                ApplyDailySave(daySave.daily);
                weekSaveData.currentDayIndex = dayIndex;
                weekSaveData.days.RemoveAll(day => day != null && day.dayIndex > dayIndex);
                RefreshWeekText();
                RefreshSlotViews();
            }

            HideSaveList();
        }

        public void Save(ArchiveData archiveData)
        {
            if (archiveData == null)
            {
                return;
            }

            SaveCurrentDaySnapshot();
            DailyDaySaveData currentDay = FindDaySave(weekSaveData.currentDayIndex);
            if (currentDay != null)
            {
                currentDay.archiveId = archiveData.id;
            }

            archiveData.daily ??= new DailyArchiveData();
            archiveData.daily.week = weekSaveData;
            archiveData.daily.package = dailyPackageService == null
                ? new PackageObjectArchiveData()
                : dailyPackageService.Capture();
        }

        public void Load(ArchiveData archiveData)
        {
            if (archiveData?.daily == null)
            {
                return;
            }

            weekSaveData = archiveData.daily.week ?? new DailyWeekSaveData();
            if (weekSaveData.days == null)
            {
                weekSaveData.days = new List<DailyDaySaveData>();
            }

            dailyPackageService?.Load(archiveData.daily.package);

            DailyDaySaveData daySave = FindDaySave(weekSaveData.currentDayIndex);
            if (daySave != null)
            {
                ApplyDailySave(daySave.daily);
            }

            RefreshWeekText();
            RefreshSlotViews();
            if (saveListPanel != null)
            {
                saveListPanel.SetActive(false);
            }
        }

        public void SaveCurrentDaySnapshot()
        {
            if (weekSaveData == null)
            {
                weekSaveData = new DailyWeekSaveData();
            }

            weekSaveData.currentDayIndex = Mathf.Clamp(weekSaveData.currentDayIndex, 0, Mathf.Max(0, maxDayCount - 1));
            DailyDaySaveData daySave = FindDaySave(weekSaveData.currentDayIndex);
            if (daySave == null)
            {
                daySave = new DailyDaySaveData();
                weekSaveData.days.Add(daySave);
            }

            daySave.dayIndex = weekSaveData.currentDayIndex;
            daySave.weekName = GetWeekName(daySave.dayIndex);
            daySave.daily = CaptureDailySave();
        }

        private DailySaveData CaptureDailySave()
        {
            DailySaveData saveData = new DailySaveData();
            CaptureMission(saveData.mission);
            CaptureStep(saveData.step);
            CaptureNodes(saveData.nodes);
            CaptureCards(saveData.cards);
            return saveData;
        }

        private void ApplyDailySave(DailySaveData saveData)
        {
            if (saveData == null)
            {
                return;
            }

            ApplyNodes(saveData.nodes);
            ApplyStep(saveData.step);
            ApplyMission(saveData.mission);
            ApplyCards(saveData.cards);
        }

        private void CaptureMission(DailyMissionSaveData saveData)
        {
            saveData.progresses.Clear();
            if (missionPanelController == null || missionPanelController.Missions == null)
            {
                return;
            }

            for (int i = 0; i < missionPanelController.Missions.Count; i++)
            {
                DailyWeeklyMissionEntry entry = missionPanelController.Missions[i];
                if (entry == null)
                {
                    continue;
                }

                saveData.progresses.Add(new DailyMissionProgressSaveData
                {
                    missionType = entry.MissionType,
                    currentValue = entry.CurrentValue
                });
            }
        }

        private void ApplyMission(DailyMissionSaveData saveData)
        {
            if (missionPanelController == null || saveData == null || saveData.progresses == null)
            {
                return;
            }

            for (int i = 0; i < saveData.progresses.Count; i++)
            {
                DailyMissionProgressSaveData progress = saveData.progresses[i];
                if (progress != null)
                {
                    missionPanelController.SetProgress(progress.missionType, progress.currentValue);
                }
            }

            missionPanelController.RefreshAll();
        }

        private void CaptureStep(DailyStepSaveData saveData)
        {
            if (stepController == null)
            {
                return;
            }

            saveData.currentNodeId = stepController.CurrentNode == null ? string.Empty : stepController.CurrentNode.NodeId;
            saveData.dailyActionUsed = stepController.DailyActionUsed;
            saveData.dailyActionLimit = stepController.DailyActionLimit;
            saveData.crossValidationCount = stepController.crossValidationCount;
        }

        private void ApplyStep(DailyStepSaveData saveData)
        {
            if (stepController == null || saveData == null)
            {
                return;
            }

            stepController.ClearPreview();
            SetPrivateField(stepController, "currentNode", FindNode(saveData.currentNodeId));
            SetPrivateField(stepController, "state", DailyMapMoveState.Idle);
            stepController.crossValidationCount = saveData.crossValidationCount;
            stepController.SetDailyActionCount(saveData.dailyActionUsed, saveData.dailyActionLimit);
            InvokePrivateMethod(stepController, "ClearAllPreview");
            InvokePrivateMethod(stepController, "SetOnlyCurrentNodeLit");
        }

        private void CaptureNodes(List<DailyNodeSaveData> saveData)
        {
            saveData.Clear();
            EnsureNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                DailyMapNode node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                saveData.Add(new DailyNodeSaveData
                {
                    nodeId = node.NodeId,
                    revealed = node.Revealed,
                    isVisited = node.IsVisited
                });
            }
        }

        private void ApplyNodes(List<DailyNodeSaveData> saveData)
        {
            if (saveData == null)
            {
                return;
            }

            EnsureNodes();
            for (int i = 0; i < saveData.Count; i++)
            {
                DailyNodeSaveData nodeSave = saveData[i];
                if (nodeSave == null)
                {
                    continue;
                }

                DailyMapNode node = FindNode(nodeSave.nodeId);
                if (node == null)
                {
                    continue;
                }

                node.SetRevealed(nodeSave.revealed);
                SetPrivateField(node, "isVisited", nodeSave.isVisited);
                node.SetPreview(false);
                node.SetCurrent(false);
            }
        }

        private void CaptureCards(DailyCardHandSaveData saveData)
        {
            saveData.stepCounts.Clear();
            List<DailyStepCardButton> cards = GetCardList();
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                DailyStepCardButton card = cards[i];
                if (card != null && card.gameObject.activeSelf)
                {
                    saveData.stepCounts.Add(card.StepCount);
                }
            }
        }

        private void ApplyCards(DailyCardHandSaveData saveData)
        {
            if (cardHand == null || saveData == null || saveData.stepCounts == null)
            {
                return;
            }

            List<DailyStepCardButton> cards = GetCardList();
            if (cards == null || cards.Count == 0)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                DailyStepCardButton card = cards[i];
                if (card != null)
                {
                    card.gameObject.SetActive(i < saveData.stepCounts.Count);
                    if (i < saveData.stepCounts.Count)
                    {
                        card.SetController(stepController);
                        card.SetStepCount(saveData.stepCounts[i]);
                        card.SetInteractable(true);
                        RectTransform rect = card.RectTransform;
                        if (rect != null)
                        {
                            rect.localScale = Vector3.one;
                            rect.anchoredPosition = GetCardPosition(i);
                        }
                    }
                }
            }

            cardHand.SetCardsInteractable(true);
        }

        private Vector2 GetCardPosition(int index)
        {
            if (cardHand == null)
            {
                return Vector2.zero;
            }

            Vector2 firstCardPosition = GetPrivateField(cardHand, "firstCardPosition", Vector2.zero);
            float cardSpacing = GetPrivateField(cardHand, "cardSpacing", 0f);
            return firstCardPosition + new Vector2(cardSpacing * index, 0f);
        }

        private bool CanChangeArchiveState()
        {
            return stepController == null || stepController.State != DailyMapMoveState.Moving;
        }

        private void RefreshWeekText()
        {
            if (weekText != null)
            {
                weekText.text = GetWeekName(weekSaveData.currentDayIndex);
            }
        }

        private void RefreshSlotViews()
        {
            BindSceneReferences();
            for (int i = 0; i < slotViews.Count; i++)
            {
                DailyArchiveSlotView slot = slotViews[i];
                if (slot == null)
                {
                    continue;
                }

                bool hasSave = FindDaySave(i) != null;
                bool isCurrent = i == weekSaveData.currentDayIndex;
                slot.SetInfo(i, GetWeekName(i), hasSave, isCurrent, LoadDay);
            }
        }

        private string GetWeekName(int dayIndex)
        {
            if (dayIndex >= 0 && dayIndex < DefaultWeekNames.Length)
            {
                return DefaultWeekNames[dayIndex];
            }

            return string.Format("第{0}天", dayIndex + 1);
        }

        private DailyDaySaveData FindDaySave(int dayIndex)
        {
            if (weekSaveData == null || weekSaveData.days == null)
            {
                return null;
            }

            for (int i = 0; i < weekSaveData.days.Count; i++)
            {
                DailyDaySaveData day = weekSaveData.days[i];
                if (day != null && day.dayIndex == dayIndex)
                {
                    return day;
                }
            }

            return null;
        }

        private DailyMapNode FindNode(string nodeId)
        {
            EnsureNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                DailyMapNode node = nodes[i];
                if (node != null && node.NodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        private void LoadArchiveFile()
        {
            if (!File.Exists(SavePath))
            {
                weekSaveData = new DailyWeekSaveData();
                return;
            }

            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrEmpty(json))
            {
                weekSaveData = new DailyWeekSaveData();
                return;
            }

            weekSaveData = JsonUtility.FromJson<DailyWeekSaveData>(json);
            if (weekSaveData == null)
            {
                weekSaveData = new DailyWeekSaveData();
            }

            if (weekSaveData.days == null)
            {
                weekSaveData.days = new List<DailyDaySaveData>();
            }
        }

        private void WriteArchiveFile()
        {
            string json = JsonUtility.ToJson(weekSaveData, true);
            File.WriteAllText(SavePath, json);
        }

        private void BindButtons()
        {
            if (nextDayButton != null)
            {
                nextDayButton.onClick.RemoveListener(EnterNextDay);
                nextDayButton.onClick.AddListener(EnterNextDay);
            }

            if (saveListButton != null)
            {
                saveListButton.onClick.RemoveListener(ShowSaveList);
                saveListButton.onClick.AddListener(ShowSaveList);
            }

            if (closeSaveListButton != null)
            {
                closeSaveListButton.onClick.RemoveListener(HideSaveList);
                closeSaveListButton.onClick.AddListener(HideSaveList);
            }
        }

        private void BindSceneReferences()
        {
            if (weekText == null)
            {
                weekText = FindText("DailyOperationMapCanvas/DailyOperationMap/TitleBlock/WeekText");
            }

            if (nextDayButton == null)
            {
                nextDayButton = FindButton("DailyOperationMapCanvas/DailyOperationMap/TitleBlock/Btn_NextDay");
            }

            if (saveListButton == null)
            {
                saveListButton = FindButton("DailyOperationMapCanvas/DailyOperationMap/TitleBlock/Btn_SaveList");
            }

            if (saveListPanel == null)
            {
                Transform found = FindTransform("DailyOperationMapCanvas/DailyOperationMap/SaveListPanel");
                if (found != null)
                {
                    saveListPanel = found.gameObject;
                }
            }

            if (closeSaveListButton == null)
            {
                closeSaveListButton = FindButton("DailyOperationMapCanvas/DailyOperationMap/SaveListPanel/CloseButton");
            }

            if (missionPanelController == null)
            {
                missionPanelController = FindObjectOfType<DailyMissionPanelController>(true);
            }

            if (stepController == null)
            {
                stepController = FindObjectOfType<DailyMapStepController>(true);
            }

            if (cardHand == null)
            {
                cardHand = FindObjectOfType<DailyStepCardHand>(true);
            }

            EnsureNodes();
            EnsureSlotViews();
        }

        private void EnsureNodes()
        {
            if (nodes == null)
            {
                nodes = new List<DailyMapNode>();
            }

            nodes.RemoveAll(node => node == null);
            if (nodes.Count == 0)
            {
                nodes.AddRange(FindObjectsOfType<DailyMapNode>(true));
            }
        }

        private void EnsureSlotViews()
        {
            if (slotViews == null)
            {
                slotViews = new List<DailyArchiveSlotView>();
            }

            slotViews.RemoveAll(slot => slot == null);
            if (slotViews.Count > 0)
            {
                return;
            }

            Transform slotRoot = FindTransform("DailyOperationMapCanvas/DailyOperationMap/SaveListPanel/SlotScrollView/Viewport/SlotRoot");
            if (slotRoot == null)
            {
                return;
            }

            for (int i = 0; i < slotRoot.childCount; i++)
            {
                DailyArchiveSlotView view = slotRoot.GetChild(i).GetComponent<DailyArchiveSlotView>();
                if (view != null)
                {
                    slotViews.Add(view);
                }
            }
        }

        private List<DailyStepCardButton> GetCardList()
        {
            if (cardHand == null)
            {
                return null;
            }

            FieldInfo field = typeof(DailyStepCardHand).GetField("cards", BindingFlags.Instance | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(cardHand) as List<DailyStepCardButton>;
        }

        private static TMP_Text FindText(string path)
        {
            Transform target = FindTransform(path);
            return target == null ? null : target.GetComponentInChildren<TMP_Text>(true);
        }

        private static Button FindButton(string path)
        {
            Transform target = FindTransform(path);
            return target == null ? null : target.GetComponent<Button>();
        }

        private static Transform FindTransform(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string[] parts = path.Split('/');
            if (parts.Length == 0)
            {
                return null;
            }

            Transform current = null;
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject go = allObjects[i];
                if (go == null || go.name != parts[0] || go.transform.parent != null)
                {
                    continue;
                }

                current = go.transform;
                break;
            }

            for (int i = 1; i < parts.Length && current != null; i++)
            {
                current = FindDirectChild(current, parts[i]);
            }

            return current;
        }

        private static Transform FindDirectChild(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                return;
            }

            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static T GetPrivateField<T>(object target, string fieldName, T fallback)
        {
            if (target == null)
            {
                return fallback;
            }

            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                return fallback;
            }

            object value = field.GetValue(target);
            return value is T ? (T)value : fallback;
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            if (target == null)
            {
                return;
            }

            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null && method.GetParameters().Length == 0)
            {
                method.Invoke(target, null);
            }
        }
    }
}
