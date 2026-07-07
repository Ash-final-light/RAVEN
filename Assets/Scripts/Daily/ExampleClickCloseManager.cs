using System.Collections.Generic;
using Daily.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Daily
{
    public class ExampleClickCloseManager : MonoBehaviour
    {
        [Header("Text Config")]
        [SerializeField] private TextDic descriptionDic;
        [SerializeField] private TextDic popupDic;

        [Header("Panels")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private GameObject objectPanel;
        [SerializeField] private GameObject explainPanel;
        [SerializeField] private GameObject clickBlocker;

        [Header("Mission")]
        [SerializeField] private DailyMissionPanelController missionPanelController;

        private PopupPanel popupPanelController;
        private ObjectPanel objectPanelController;
        private ExplainPanel explainPanelController;
        private Button clickBlockerButton;
        private readonly List<GameObject> showingPanels = new List<GameObject>();

        private void Awake()
        {
            BindControllers();
            EnsureClickBlocker();
            HideAll();
        }

        public void ShowPopup(DailyMapNode node)
        {
            BindControllers();
            CacheMissionPanelController();
            if (popupPanelController == null || node == null)
            {
                return;
            }

            string content = GetPopupContent(node);
            popupPanelController.SetInfo(content);
            ShowClickClosePanel(popupPanel);
        }

        /// <summary>
        /// TIPS: only show content.
        /// </summary>
        public void ShowPopup(string title, string content)
        {
            BindControllers();
            if (popupPanelController == null)
            {
                return;
            }

            popupPanelController.SetInfo(content);
            ShowClickClosePanel(popupPanel);
        }

        public void ShowDescribe(DailyMapNode node)
        {
            ShowExplain(node);
        }

        public void ShowExplain(DailyMapNode node)
        {
            BindControllers();
            if (explainPanelController == null || node == null)
            {
                return;
            }

            string content = descriptionDic == null ? string.Empty : descriptionDic.GetDescription(node);
            explainPanelController.SetInfo(GetNodeTitle(node), content);
            ShowClickClosePanel(explainPanel);
        }

        public void ShowObject(string title, List<rewardsInfo> rewards)
        {
            BindControllers();
            if (objectPanelController == null)
            {
                return;
            }

            objectPanelController.SetTitle(title);
            objectPanelController.SetContent(rewards);
            ShowClickClosePanel(objectPanel);
        }

        public void HideAll()
        {
            HidePanel(popupPanel);
            HidePanel(objectPanel);
            HidePanel(explainPanel);
            showingPanels.Clear();

            if (clickBlocker != null)
            {
                clickBlocker.SetActive(false);
            }
        }

        public void HideTopPanel()
        {
            if (showingPanels.Count == 0)
            {
                HideClickBlockerIfEmpty();
                return;
            }

            GameObject topPanel = showingPanels[showingPanels.Count - 1];
            HidePanel(topPanel);
        }

        public void HidePanel(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            showingPanels.Remove(panel);
            panel.SetActive(false);
            HideClickBlockerIfEmpty();
        }

        private void ShowClickClosePanel(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            EnsureClickBlocker();
            showingPanels.Remove(panel);
            showingPanels.Add(panel);

            if (clickBlocker != null)
            {
                clickBlocker.SetActive(true);
                clickBlocker.transform.SetAsLastSibling();
            }

            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
        }

        private void BindControllers()
        {
            if (popupPanelController == null && popupPanel != null)
            {
                popupPanelController = popupPanel.GetComponent<PopupPanel>();
            }

            if (objectPanelController == null && objectPanel != null)
            {
                objectPanelController = objectPanel.GetComponent<ObjectPanel>();
            }

            if (explainPanelController == null && explainPanel != null)
            {
                explainPanelController = explainPanel.GetComponent<ExplainPanel>();
            }
        }

        private void CacheMissionPanelController()
        {
            if (missionPanelController == null)
            {
                missionPanelController = FindObjectOfType<DailyMissionPanelController>(true);
            }
        }

        private string GetPopupContent(DailyMapNode node)
        {
            if (IsMissionCompletedNode(node, out DailyWeeklyMissionType missionType))
            {
                return GetMissionCompletedText(missionType);
            }

            return popupDic == null ? string.Empty : popupDic.GetDescription(node);
        }

        private bool IsMissionCompletedNode(DailyMapNode node, out DailyWeeklyMissionType missionType)
        {
            missionType = default;

            if (node == null || missionPanelController == null)
            {
                return false;
            }

            if (!TryGetMissionType(node.EventType, out missionType))
            {
                return false;
            }

            return missionPanelController.IsCompleted(missionType);
        }

        private static bool TryGetMissionType(DailyMapNodeEventType eventType, out DailyWeeklyMissionType missionType)
        {
            missionType = default;

            switch (eventType)
            {
                case DailyMapNodeEventType.Battle1:
                case DailyMapNodeEventType.Battle2:
                case DailyMapNodeEventType.Battle3:
                case DailyMapNodeEventType.ExternalSupport:
                    missionType = DailyWeeklyMissionType.Battle;
                    return true;

                case DailyMapNodeEventType.Resource:
                    missionType = DailyWeeklyMissionType.SupportBox;
                    return true;

                case DailyMapNodeEventType.CrossValidation:
                    missionType = DailyWeeklyMissionType.CrossValidation;
                    return true;

                case DailyMapNodeEventType.AbandonedWarehouse:
                    missionType = DailyWeeklyMissionType.AbandonedWarehouse;
                    return true;

                default:
                    return false;
            }
        }

        private static string GetMissionCompletedText(DailyWeeklyMissionType missionType)
        {
            switch (missionType)
            {
                case DailyWeeklyMissionType.Battle:
                    return "战斗任务已完成。";

                case DailyWeeklyMissionType.SupportBox:
                    return "支援箱任务已完成。";

                case DailyWeeklyMissionType.CrossValidation:
                    return "交叉验证任务已完成。";

                case DailyWeeklyMissionType.AbandonedWarehouse:
                    return "废弃仓库任务已完成。";

                default:
                    return "任务已完成。";
            }
        }

        private void EnsureClickBlocker()
        {
            if (clickBlocker == null)
            {
                Transform foundBlocker = transform.root.Find("DailyOperationMapCanvas/DailyInfoPanelClickBlocker");
                if (foundBlocker != null)
                {
                    clickBlocker = foundBlocker.gameObject;
                }
            }

            if (clickBlocker == null)
            {
                Debug.LogWarning("[ExampleClickCloseManager] Missing click blocker reference.");
                return;
            }

            BindClickBlockerButton();
        }

        private void BindClickBlockerButton()
        {
            Image image = clickBlocker.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0f, 0f, 0f, 0f);
                image.raycastTarget = true;
            }

            if (clickBlockerButton == null)
            {
                clickBlockerButton = clickBlocker.GetComponent<Button>();
            }

            if (clickBlockerButton == null)
            {
                return;
            }

            clickBlockerButton.transition = Selectable.Transition.None;
            clickBlockerButton.onClick.RemoveListener(HideTopPanel);
            clickBlockerButton.onClick.AddListener(HideTopPanel);
            clickBlocker.SetActive(false);
        }

        private void HideClickBlockerIfEmpty()
        {
            if (clickBlocker != null && showingPanels.Count == 0)
            {
                clickBlocker.SetActive(false);
            }
        }

        private static string GetNodeTitle(DailyMapNode node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            if (node.Properties != null && !string.IsNullOrEmpty(node.Properties.EventName))
            {
                return node.Properties.EventName;
            }

            return node.EventType.ToString();
        }
    }
}
