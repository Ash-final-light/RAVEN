using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Daily
{
    public enum DailyClickCloseInfoPanelKind
    {
        Action,// 节点操作信息
        Object,// 获得物品信息
    }

    public class DailyClickCloseInfoPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private DailyClickCloseInfoPanelKind panelKind;
        [SerializeField] private GameObject panelRoot;

        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            SetPanelRaycasts(false);
            panelRoot.SetActive(false);
        }

        private void OnDisable()
        {
            DailyInfoPanelClickBlocker.Hide(panelRoot);
        }

        public static void ShowAction(DailyMapNode node)
        {
            DailyClickCloseInfoPanel panel = FindPanel(DailyClickCloseInfoPanelKind.Action);
            if (panel != null)
            {
                panel.Show(node);
            }
        }

        public static void ShowDescribe(DailyMapNode node)
        {
            DailyClickCloseInfoPanel panel = FindPanel(DailyClickCloseInfoPanelKind.Object);
            if (panel != null)
            {
                panel.Show(node);
            }
        }

        public void Show(DailyMapNode node)
        {
            ApplyNodeText(node);

            panelRoot.SetActive(true);
            DailyInfoPanelClickBlocker.Show(panelRoot, Close);
            DailyInfoPanelClickBlocker.PlacePanelAboveBlocker(panelRoot);
        }

        public void Close()
        {
            DailyInfoPanelClickBlocker.Hide(panelRoot);
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private static DailyClickCloseInfoPanel FindPanel(DailyClickCloseInfoPanelKind kind)
        {
            DailyClickCloseInfoPanel[] panels = Resources.FindObjectsOfTypeAll<DailyClickCloseInfoPanel>();
            for (int i = 0; i < panels.Length; i++)
            {
                DailyClickCloseInfoPanel panel = panels[i];
                if (panel != null && panel.panelKind == kind && panel.gameObject.scene.IsValid())
                {
                    return panel;
                }
            }

            return null;
        }

        private void ApplyNodeText(DailyMapNode node)
        {
            string title = panelKind == DailyClickCloseInfoPanelKind.Action ? "行动结果" : "节点情报";
            string content = "未知节点";

            if (node != null)
            {
                title = panelKind == DailyClickCloseInfoPanelKind.Action
                    ? GetArrivalTitle(node.EventType)
                    : GetDescriptionTitle(node.EventType);
                content = panelKind == DailyClickCloseInfoPanelKind.Action
                    ? GetArrivalContent(node)
                    : GetDescriptionContent(node);
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (contentText != null)
            {
                contentText.text = content;
            }
        }

        private static string GetArrivalTitle(DailyMapNodeEventType type)
        {
            switch (type)
            {
                case DailyMapNodeEventType.Battle1:
                case DailyMapNodeEventType.Battle2:
                case DailyMapNodeEventType.Battle3:
                    return "遭遇战斗";
                case DailyMapNodeEventType.Resource:
                    return "获得资源";
                case DailyMapNodeEventType.ExtraAction:
                    return "额外行动";
                case DailyMapNodeEventType.ExternalSupport:
                    return "外部支援";
                case DailyMapNodeEventType.AdjustStep:
                    return "调动步数";
                case DailyMapNodeEventType.PreciseRemoteSensing:
                    return "精确遥感";
                default:
                    return "行动结果";
            }
        }

        private static string GetDescriptionTitle(DailyMapNodeEventType type)
        {
            switch (type)
            {
                case DailyMapNodeEventType.Battle1:
                    return "战斗1";
                case DailyMapNodeEventType.Battle2:
                    return "战斗2";
                case DailyMapNodeEventType.Battle3:
                    return "战斗3";
                case DailyMapNodeEventType.Resource:
                    return "资源";
                case DailyMapNodeEventType.ExtraAction:
                    return "额外行动";
                case DailyMapNodeEventType.ExternalSupport:
                    return "外部支援";
                case DailyMapNodeEventType.AdjustStep:
                    return "调动步数";
                case DailyMapNodeEventType.PreciseRemoteSensing:
                    return "精确遥感";
                default:
                    return "节点情报";
            }
        }

        private static string GetArrivalContent(DailyMapNode node)
        {
            DailyMapNodeProperties properties = node.Properties;
            switch (node.EventType)
            {
                case DailyMapNodeEventType.Resource:
                    return string.Format("已抵达资源节点，获得资源一次");
                case DailyMapNodeEventType.ExtraAction:
                    return string.Format("已获得额外行动次数 {0}。", properties.ExtraActionCount);
                case DailyMapNodeEventType.ExternalSupport:
                    return "已触发外部支援任务，请完成一次战斗任务。";
                case DailyMapNodeEventType.AdjustStep:
                    return string.Format("已获得调动步数 +{0}。", properties.AdjustStepAmount);
                case DailyMapNodeEventType.PreciseRemoteSensing:
                    return string.Format("已获得精确遥感次数 {0}。", properties.RemoteSensingCount);
                default:
                    return "已抵达该节点。";
            }
        }

        private static string GetDescriptionContent(DailyMapNode node)
        {
            DailyMapNodeProperties properties = node.Properties;
            switch (node.EventType)
            {
                case DailyMapNodeEventType.Battle1:
                    return "低强度战斗节点，抵达后打开战斗确认面板。";
                case DailyMapNodeEventType.Battle2:
                    return "中强度战斗节点，抵达后打开战斗确认面板。";
                case DailyMapNodeEventType.Battle3:
                    return "高强度战斗节点，抵达后打开战斗确认面板。";
                case DailyMapNodeEventType.Resource:
                    return string.Format("资源节点，抵达后获得资源一次");
                case DailyMapNodeEventType.ExtraAction:
                    return string.Format("额外行动节点，抵达后增加行动次数 {0}。", properties.ExtraActionCount);
                case DailyMapNodeEventType.ExternalSupport:
                    return "外部支援节点，抵达后要求完成一次战斗任务。";
                case DailyMapNodeEventType.AdjustStep:
                    return string.Format("调动步数节点，抵达后获得可配置步数 +{0}。", properties.AdjustStepAmount);
                case DailyMapNodeEventType.PreciseRemoteSensing:
                    return string.Format("精确遥感节点，抵达后获得遥感次数 {0}。", properties.RemoteSensingCount);
                default:
                    return "该节点尚未配置说明。";
            }
        }

        private void SetPanelRaycasts(bool enabled)
        {
            if (panelRoot == null)
            {
                return;
            }

            Graphic[] graphics = panelRoot.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] != null)
                {
                    graphics[i].raycastTarget = enabled;
                }
            }
        }
    }

    internal static class DailyInfoPanelClickBlocker
    {
        private static readonly List<GameObject> visiblePanels = new List<GameObject>();
        private static readonly List<ClickCloseOwner> clickCloseOwners = new List<ClickCloseOwner>();
        private static GameObject blocker;
        private static Button blockerButton;

        public static void Show(GameObject panelRoot, Action clickCloseAction)
        {
            if (panelRoot == null)
            {
                return;
            }

            EnsureBlocker(panelRoot.transform);
            AddVisiblePanel(panelRoot);
            AddClickCloseOwner(panelRoot, clickCloseAction);
            RefreshClickHandler();
            blocker.SetActive(true);
        }

        public static void ShowWithoutClickClose(GameObject panelRoot)
        {
            if (panelRoot == null)
            {
                return;
            }

            EnsureBlocker(panelRoot.transform);
            AddVisiblePanel(panelRoot);
            RemoveClickCloseOwner(panelRoot);
            RefreshClickHandler();
            blocker.SetActive(true);
        }

        public static void Hide(GameObject panelRoot)
        {
            if (panelRoot == null)
            {
                return;
            }

            visiblePanels.Remove(panelRoot);
            RemoveClickCloseOwner(panelRoot);
            RefreshClickHandler();

            if (blocker != null && visiblePanels.Count == 0)
            {
                blocker.SetActive(false);
            }
        }

        public static void PlacePanelAboveBlocker(GameObject panelRoot)
        {
            if (blocker == null || panelRoot == null)
            {
                return;
            }

            blocker.transform.SetAsLastSibling();
            panelRoot.transform.SetAsLastSibling();
        }

        private static void EnsureBlocker(Transform panelTransform)
        {
            if (blocker != null)
            {
                return;
            }

            Canvas canvas = panelTransform.GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : panelTransform.parent;
            blocker = new GameObject("DailyInfoPanelClickBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            blocker.transform.SetParent(parent, false);

            RectTransform rect = blocker.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = blocker.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;

            blockerButton = blocker.GetComponent<Button>();
            blockerButton.transition = Selectable.Transition.None;
            blocker.SetActive(false);
        }

        private static void AddVisiblePanel(GameObject panelRoot)
        {
            visiblePanels.Remove(panelRoot);
            visiblePanels.Add(panelRoot);
        }

        private static void AddClickCloseOwner(GameObject panelRoot, Action closeAction)
        {
            RemoveClickCloseOwner(panelRoot);
            if (closeAction != null)
            {
                clickCloseOwners.Add(new ClickCloseOwner(panelRoot, closeAction));
            }
        }

        private static void RemoveClickCloseOwner(GameObject panelRoot)
        {
            for (int i = clickCloseOwners.Count - 1; i >= 0; i--)
            {
                if (clickCloseOwners[i].PanelRoot == panelRoot)
                {
                    clickCloseOwners.RemoveAt(i);
                }
            }
        }

        private static void RefreshClickHandler()
        {
            if (blockerButton == null)
            {
                return;
            }

            blockerButton.onClick.RemoveAllListeners();
            blockerButton.onClick.AddListener(OnBlockerClicked);
        }

        private static void OnBlockerClicked()
        {
            if (clickCloseOwners.Count == 0 || visiblePanels.Count == 0)
            {
                return;
            }

            GameObject topPanel = visiblePanels[visiblePanels.Count - 1];
            for (int i = clickCloseOwners.Count - 1; i >= 0; i--)
            {
                ClickCloseOwner owner = clickCloseOwners[i];
                if (owner.PanelRoot == topPanel)
                {
                    owner.CloseAction.Invoke();
                    return;
                }
            }
        }

        private struct ClickCloseOwner
        {
            public readonly GameObject PanelRoot;
            public readonly Action CloseAction;

            public ClickCloseOwner(GameObject panelRoot, Action closeAction)
            {
                PanelRoot = panelRoot;
                CloseAction = closeAction;
            }
        }
    }
}