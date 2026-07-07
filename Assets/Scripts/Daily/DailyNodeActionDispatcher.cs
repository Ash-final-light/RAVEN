using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Daily
{
    public class DailyNodeActionDispatcher : MonoBehaviour
    {
        private static DailyNodeActionDispatcher instance;

        [SerializeField] private DailyMapStepController stepController;
        [SerializeField] private DailyMissionPanelController missionPanelController;

        [Inject] private DailyPackageService dailyPackageService;

        public static void DispatchNodeAction(DailyMapNode node)
        {
            DailyNodeActionDispatcher dispatcher = GetOrCreateInstance();
            if (dispatcher != null)
            {
                dispatcher.DispatchAction(node);
            }
        }

        public void DispatchAction(global::Daily.DailyMapNode node)
        {
            if (node == null)
            {
                return;
            }

            switch (node.EventType)
            {
                case DailyMapNodeEventType.Battle1:
                case DailyMapNodeEventType.Battle2:
                case DailyMapNodeEventType.Battle3:
                    DispatchBattle(node);
                    break;
                case DailyMapNodeEventType.AdjustStep:
                    DispatchAdjustStep(node);
                    break;
                case DailyMapNodeEventType.ExternalSupport:
                    DispatchExternalSupport();
                    break;
                case DailyMapNodeEventType.ExtraAction:
                    DispatchExtraAction(node);
                    break;
                case DailyMapNodeEventType.PreciseRemoteSensing:
                    DispatchPreciseRemoteSensing(node);
                    break;
                case DailyMapNodeEventType.CrossValidation:
                    DispatchCrossValidation(node);
                    break;
                case DailyMapNodeEventType.Resource:
                    DispatchResource(node);
                    break;
                case DailyMapNodeEventType.AbandonedWarehouse:
                    DispatchAbandonedWarehouse(node);
                    break;
                default:
                    break;
            }
        }

        private void Awake() 
        {
            if (instance != null && instance != this)
            {
                return;
            }

            instance = this;
            CacheStepController();
            CacheMissionPanelController();
        }

        private void DispatchBattle(global::Daily.DailyMapNode node)
        {
            CacheStepController();
            
            if (stepController == null || missionPanelController == null) 
            {
                return;
            }

            if (!missionPanelController.IsCompleted(DailyWeeklyMissionType.Battle))
            {
                missionPanelController.AddProgress(DailyWeeklyMissionType.Battle);
            }
        }
        
        private void DispatchCrossValidation(DailyMapNode node)
        {
            CacheStepController();
            CacheMissionPanelController();
            
            if (stepController == null || stepController.crossValidationCount >= 2 || 
                missionPanelController == null) 
            {
                return;
            }
            
            stepController.crossValidationCount++;
            if (stepController.crossValidationCount == 1)
            {
                Debug.Log($"[DispatchNodeAction] CrossValidation: {stepController.crossValidationCount}");
            }
            else if (stepController.crossValidationCount == 2 && 
                     !missionPanelController.IsCompleted(DailyWeeklyMissionType.CrossValidation))
            {
                missionPanelController.AddProgress(DailyWeeklyMissionType.CrossValidation);
                Debug.Log($"[DispatchNodeAction] CrossValidation: {stepController.crossValidationCount}");
            }
        }

        private void DispatchAdjustStep(global::Daily.DailyMapNode node)
        {
            CacheStepController();

            if (stepController == null || node.Properties == null)
            {
                return;
            }

            int stepCount = Mathf.Max(1, node.Properties.AdjustStepAmount);
            StartCoroutine(MoveAfterCurrentNodeEvent(stepCount));
        }

        private System.Collections.IEnumerator MoveAfterCurrentNodeEvent(int stepCount)
        {
            yield return new WaitUntil(() => stepController.State == global::Daily.DailyMapMoveState.Idle);
            stepController.MoveByRewardSteps(stepCount);
        }
        private void DispatchResource(global::Daily.DailyMapNode node)
        {
            CacheStepController();
            CacheMissionPanelController();

            if (node.Properties == null || missionPanelController == null || 
                missionPanelController.IsCompleted(DailyWeeklyMissionType.SupportBox))
            {
                return;
            }
            
            missionPanelController.AddProgress(DailyWeeklyMissionType.SupportBox);
            if (dailyPackageService == null)
            {
                Debug.LogWarning("[DailyNodeActionDispatcher] DailyPackageService is not injected.");
                return;
            }

            dailyPackageService.GrantRewards(node.Properties.Rewards);
        }
        
        private void DispatchAbandonedWarehouse(global::Daily.DailyMapNode node)
        {
            CacheStepController();
            CacheMissionPanelController();

            if (node.Properties == null || missionPanelController == null || 
                missionPanelController.IsCompleted(DailyWeeklyMissionType.SupportBox))
            {
                return;
            }

            missionPanelController.AddProgress(DailyWeeklyMissionType.AbandonedWarehouse);
            if (dailyPackageService == null)
            {
                Debug.LogWarning("[DailyNodeActionDispatcher] DailyPackageService is not injected.");
                return;
            }

            dailyPackageService.GrantRewards(node.Properties.Rewards);
        }

        private void DispatchExternalSupport()
        {
            CacheMissionPanelController();

            if (missionPanelController == null || 
                missionPanelController.IsCompleted(DailyWeeklyMissionType.Battle))
            {
                return;
            }

            missionPanelController.AddProgress(DailyWeeklyMissionType.Battle);
        }

        private void DispatchExtraAction(DailyMapNode node)
        {
            CacheStepController();

            if (stepController == null || node.Properties == null)
            {
                return;
            }

            int extraActionCount = Mathf.Max(1, node.Properties.ExtraActionCount);
            stepController.SetDailyActionCount(
                stepController.DailyActionUsed,
                stepController.DailyActionLimit + extraActionCount
            );
        }

        private void DispatchPreciseRemoteSensing(DailyMapNode node)
        {
            CacheStepController();

            if (stepController == null || node.Properties == null)
            {
                return;
            }

            List<DailyMapNode> hiddenNodes = new List<DailyMapNode>();
            IReadOnlyList<DailyMapNode> allNodes = stepController.AllNodes;
            for (int i = 0; i < allNodes.Count; i++)
            {
                DailyMapNode mapNode = allNodes[i];
                if (mapNode != null && !mapNode.Revealed)
                {
                    hiddenNodes.Add(mapNode);
                }
            }

            if (hiddenNodes.Count == 0)
            {
                return;
            }

            int revealCount = Mathf.Min(Mathf.Max(1, node.Properties.RemoteSensingCount), hiddenNodes.Count);
            for (int i = 0; i < revealCount; i++)
            {
                int index = Random.Range(0, hiddenNodes.Count);
                DailyMapNode targetNode = hiddenNodes[index];
                hiddenNodes.RemoveAt(index);
                targetNode.SetRevealed(true);
            }
        }

        private void CacheStepController()
        {
            if (stepController == null)
            {
                stepController = FindObjectOfType<global::Daily.DailyMapStepController>(true);
            }
        }

        private void CacheMissionPanelController()
        {
            if (missionPanelController == null)
            {
                missionPanelController = FindObjectOfType<DailyMissionPanelController>(true);
            }
        }

        private static DailyNodeActionDispatcher GetOrCreateInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<DailyNodeActionDispatcher>(true);
            if (instance != null)
            {
                instance.CacheStepController();
                instance.CacheMissionPanelController();
                return instance;
            }

            GameObject mapRoot = GameObject.Find("DailyOperationMap");
            if (mapRoot == null)
            {
                DailyMapStepController controller = FindObjectOfType<DailyMapStepController>(true);
                mapRoot = controller != null ? controller.gameObject : null;
            }

            if (mapRoot == null)
            {
                return null;
            }

            instance = mapRoot.GetComponent<DailyNodeActionDispatcher>();
            if (instance == null)
            {
                instance = mapRoot.AddComponent<DailyNodeActionDispatcher>();
            }

            instance.CacheStepController();
            instance.CacheMissionPanelController();
            return instance;
        }
    }
}
