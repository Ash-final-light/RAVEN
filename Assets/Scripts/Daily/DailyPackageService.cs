using System.Collections.Generic;
using Package;
using Tools.CustomObject.Data;
using Tools.RecycleViewFramework.View;
using UnityEngine;

namespace Daily
{
    public sealed class DailyPackageService
    {
        private static readonly HashSet<int> SupportedObjectIds = new HashSet<int> { 1001, 1002 };

        private readonly PackageObjectManager packageObjectManager;
        private readonly UIManager uiManager;
        private bool pendingOpenPackageView;

        public DailyPackageService(PackageObjectManager packageObjectManager, UIManager uiManager)
        {
            this.packageObjectManager = packageObjectManager;
            this.uiManager = uiManager;
        }

        public void GrantRewards(rewardsInfo[] rewards)
        {
            if (rewards == null)
            {
                return;
            }

            for (int i = 0; i < rewards.Length; i++)
            {
                rewardsInfo reward = rewards[i];
                if (!SupportedObjectIds.Contains(reward.objectId))
                {
                    Debug.LogWarning($"[DailyPackageService] Unsupported object id: {reward.objectId}");
                    continue;
                }

                if (reward.amount <= 0)
                {
                    continue;
                }

                packageObjectManager.AddObjectById(reward.objectId, reward.amount);
            }
        }

        public PackageObjectArchiveData Capture()
        {
            return packageObjectManager.CaptureArchiveData();
        }

        public void Load(PackageObjectArchiveData archiveData)
        {
            packageObjectManager.LoadArchiveData(archiveData);
        }

        public void OpenPackageView()
        {
            if (uiManager.ResourcesLoaded)
            {
                uiManager.Open("ObjectPackageView");
                return;
            }

            if (pendingOpenPackageView)
            {
                return;
            }

            pendingOpenPackageView = true;
            uiManager.OnLoaded += OpenPackageViewAfterLoaded;
        }

        private void OpenPackageViewAfterLoaded()
        {
            uiManager.OnLoaded -= OpenPackageViewAfterLoaded;
            pendingOpenPackageView = false;
            uiManager.Open("ObjectPackageView");
        }
    }
}
