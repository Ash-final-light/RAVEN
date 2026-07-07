using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Daily
{
    [Serializable]
    public enum FightScene{
        Battle1,
        Battle2,
        Battle3,
    }
    public class DailyFightInfoPanelController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text rewardsText;

        [Header("Buttons")]
        [SerializeField] private Button avoidButton;
        [SerializeField] private Button acceptButton;

        [Header("Events")]
        [SerializeField] private UnityEvent<DailyMapNode> onAvoid = new UnityEvent<DailyMapNode>();
        [SerializeField] private UnityEvent<DailyMapNode> onAccept = new UnityEvent<DailyMapNode>();

        private DailyMapNode currentNode;

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            SetPassiveRaycasts(false);
            if (avoidButton != null)
            {
                avoidButton.onClick.AddListener(Avoid);
            }

            if (acceptButton != null)
            {
                acceptButton.onClick.AddListener(Accept);
            }

            panelRoot.SetActive(false);
        }

        private void OnDisable()
        {
            DailyInfoPanelClickBlocker.Hide(panelRoot);
        }

        public static void ShowFight(DailyMapNode node)
        {
            DailyFightInfoPanelController panel = FindPanel();
            if (panel != null)
            {
                panel.Show(node);
            }
        }

        public void Show(DailyMapNode node)
        {
            currentNode = node;
            ApplyNodeText(node);

            panelRoot.SetActive(true);
            DailyInfoPanelClickBlocker.ShowWithoutClickClose(panelRoot);
            DailyInfoPanelClickBlocker.PlacePanelAboveBlocker(panelRoot);
        }

        public void Avoid()
        {
            onAvoid.Invoke(currentNode);
            Close();
        }

        public void Accept()
        {
            onAccept.Invoke(currentNode);
            //TODO: 这里加载战斗场景并保存当前场景的数据信息
            Debug.Log("Accept battle node");
            Close();
        }

        public void Close()
        {
            DailyInfoPanelClickBlocker.Hide(panelRoot);
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private static DailyFightInfoPanelController FindPanel()
        {
            DailyFightInfoPanelController[] panels = Resources.FindObjectsOfTypeAll<DailyFightInfoPanelController>();
            for (int i = 0; i < panels.Length; i++)
            {
                DailyFightInfoPanelController panel = panels[i];
                if (panel != null && panel.gameObject.scene.IsValid())
                {
                    return panel;
                }
            }

            return null;
        }

        private void ApplyNodeText(DailyMapNode node)
        {
            string title = "战斗确认";
            string content = "是否执行本次战斗任务？";
            string rewards = "完成战斗后获得对应行动奖励。";

            if (node != null)
            {
                DailyMapNodeProperties properties = node.Properties;
               
                switch (node.EventType)
                {
                    case DailyMapNodeEventType.Battle1:
                        title = "战斗1";
                        content = "低强度战斗节点，确认后进入战斗任务。";
                        break;
                    case DailyMapNodeEventType.Battle2:
                        title = "战斗2";
                        content = "中强度战斗节点，确认后进入战斗任务。";
                        break;
                    case DailyMapNodeEventType.Battle3:
                        title = "战斗3";
                        content = "高强度战斗节点，确认后进入战斗任务。";
                        break;
                    default:
                        content = "该节点将按战斗流程处理。";
                        break;
                }

                rewards = string.Format("战斗等级奖励等待补充后续逻辑：{0}", node.EventType.ToString());
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (contentText != null)
            {
                contentText.text = content;
            }

            if (rewardsText != null)
            {
                rewardsText.text = rewards;
            }
        }

        private void SetPassiveRaycasts(bool enabled)
        {
            if (panelRoot == null)
            {
                return;
            }

            Graphic[] graphics = panelRoot.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null)
                {
                    continue;
                }

                Button button = graphic.GetComponentInParent<Button>(true);
                graphic.raycastTarget = button != null || enabled;
            }
        }
    }
}
