using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Daily
{
    public enum DailyMapNodeEventType
    {
        Battle1,
        Battle2,
        Battle3,
        Resource,//支援箱
        ExtraAction,
        ExternalSupport,
        AdjustStep,
        PreciseRemoteSensing,
        CrossValidation,//交叉验证
        AbandonedWarehouse,//废弃仓库
    }

    [Serializable]
    public struct rewardsInfo
    {
        public int objectId;
        public int amount;
    }

    [Serializable]
    public class DailyMapNodeProperties
    {
        [SerializeField] private string eventName;
        [SerializeField] private FightScene fightScene;
        [SerializeField] private rewardsInfo[] rewards;
        
        [SerializeField] private int extraActionCount = 1;
        [SerializeField] private int adjustStepAmount = 6;
        
        [SerializeField] private int remoteSensingCount = 1;

        public string EventName { get { return eventName; } }
        public FightScene FightScene { get { return fightScene; } }
        public rewardsInfo[] Rewards { get { return rewards; } }
        public int ExtraActionCount { get { return extraActionCount; } }
        public int AdjustStepAmount { get { return adjustStepAmount; } }
        public int RemoteSensingCount { get { return remoteSensingCount; } }
    }

    [Serializable]
    public class DailyMapNodeTriggeredEvent : UnityEvent<DailyMapNode>
    {
    }

    public class DailyMapNode : MonoBehaviour, IPointerClickHandler
    {
        [Header("Data")]
        [SerializeField] private string nodeId;
        [SerializeField] private DailyMapNodeEventType eventType;
        [SerializeField] private DailyMapNodeProperties properties = new DailyMapNodeProperties();
        [SerializeField] private DailyMapNode nextNode;
        [SerializeField] private bool revealed = true;
        [SerializeField] private bool isVisited = false;

        [Header("Visuals")]
        [SerializeField] private GameObject currentGlow;
        [SerializeField] private GameObject previewGlow;
        [SerializeField] private TMP_Text iconText;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private string hiddenIcon = "?";

        [Header("Events")]
        [SerializeField] private DailyMapNodeTriggeredEvent onNodeEventTriggered = new DailyMapNodeTriggeredEvent();

        public string NodeId { get { return nodeId; } }
        public DailyMapNodeEventType EventType { get { return eventType; } }
        public DailyMapNodeProperties Properties { get { return properties; } }
        public DailyMapNode NextNode { get { return nextNode; } }
        public bool Revealed { get { return revealed; } }
        public bool IsVisited { get { return isVisited; } }
        public RectTransform RectTransform { get { return transform as RectTransform; } }

        private string revealedIcon;
        private ExampleClickCloseManager clickCloseManager;
        
        //节点按钮的点击
        private Button nodeButton;

        private ExampleClickCloseManager ClickCloseManager
        {
            get
            {
                if (clickCloseManager == null)
                {
                    clickCloseManager = FindObjectOfType<ExampleClickCloseManager>(true);
                }

                return clickCloseManager;
            }
        }

        private void Awake()
        {
            CacheRevealedIcon();
            RefreshRevealVisual();
            EnableNodeClickRaycasts();
            SetCurrent(false);
            SetPreview(false);
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                RefreshRevealVisual();
            }
        }

        /// <summary>
        /// 用于外部初始化调用设置下一个节点的引用
        /// </summary>
        /// <param name="node"></param>
        public void SetNextNode(DailyMapNode node)
        {
            nextNode = node;
        }

        public void SetCurrent(bool active)
        {
            if (currentGlow != null)
            {
                currentGlow.SetActive(active);
            }
        }

        public void SetPreview(bool active)
        {
            if (previewGlow != null)
            {
                previewGlow.SetActive(active);
            }
        }

        public void Arrive()
        {
            revealed = true;
            isVisited = true;
            RefreshRevealVisual();
        }

        public void TriggerStayEvent(DailyMapStepController controller)
        {
            onNodeEventTriggered.Invoke(this);
            DailyNodeActionDispatcher.DispatchNodeAction(this);

            if (IsBattleNode())
            {
                DailyFightInfoPanelController.ShowFight(this);
                return;
            }

            if (ClickCloseManager != null)
            {
                ClickCloseManager.ShowPopup(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ClickCloseManager != null)
            {
                ClickCloseManager.ShowDescribe(this);
            }
        }

        public void SetRevealed(bool value)
        {
            revealed = value;
            RefreshRevealVisual();
        }
        
        private bool IsBattleNode()
        {
            return eventType == DailyMapNodeEventType.Battle1
                   || eventType == DailyMapNodeEventType.Battle2
                   || eventType == DailyMapNodeEventType.Battle3;
        }

        private void CacheRevealedIcon()
        {
            if (iconText != null && !string.IsNullOrEmpty(iconText.text) && iconText.text != hiddenIcon)
            {
                revealedIcon = iconText.text;
            }
        }

        private void RefreshRevealVisual()
        {
            if (iconText != null && string.IsNullOrEmpty(revealedIcon) && iconText.text != hiddenIcon)
            {
                revealedIcon = iconText.text;
            }

            if (iconText != null)
            {
                iconText.text = revealed ? revealedIcon : hiddenIcon;
            }

            if (labelText != null)
            {
                labelText.text = revealed ? GetNodeTitle() : string.Empty;
                labelText.gameObject.SetActive(revealed);
            }
        }

        private string GetNodeTitle()
        {
            if (properties != null && !string.IsNullOrWhiteSpace(properties.EventName))
            {
                return properties.EventName;
            }

            return eventType.ToString();
        }

        private void EnableNodeClickRaycasts()
        {
            Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] != null && (graphics[i].name == "Ring" || graphics[i].name == "Icon"))
                {
                    graphics[i].raycastTarget = true;
                }
            }
        }
    }
}
