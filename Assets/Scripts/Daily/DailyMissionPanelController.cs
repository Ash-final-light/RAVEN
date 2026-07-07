using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Daily
{
    public enum DailyWeeklyMissionType
    {
        Battle,
        AbandonedWarehouse,
        SupportBox,
        CrossValidation
    }

    [Serializable]
    public class DailyWeeklyMissionProgressChangedEvent : UnityEvent<DailyWeeklyMissionType, int, int>
    {
    }

    [Serializable]
    public class DailyWeeklyMissionEntry
    {
        [SerializeField] private DailyWeeklyMissionType missionType;
        [SerializeField] private int maxValue = 1;
        [SerializeField] private int currentValue;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private GameObject completedMark;

        public DailyWeeklyMissionType MissionType { get { return missionType; } }
        public int MaxValue { get { return maxValue; } }
        public int CurrentValue { get { return currentValue; } }
        public TMP_Text ProgressText { get { return progressText; } }
        public GameObject CompletedMark { get { return completedMark; } }

        public DailyWeeklyMissionEntry()
        {
        }

        public DailyWeeklyMissionEntry(DailyWeeklyMissionType missionType, int maxValue)
        {
            this.missionType = missionType;
            SetMaxValue(maxValue);
            SetCurrentValue(0, true);
        }

        public void SetMaxValue(int value)
        {
            maxValue = Mathf.Max(1, value);
        }

        public void SetCurrentValue(int value, bool clampToMax)
        {
            currentValue = Mathf.Max(0, value);
            if (clampToMax)
            {
                currentValue = Mathf.Min(currentValue, MaxValue);
            }
        }

        public void SetProgressText(TMP_Text text)
        {
            progressText = text;
        }

        public void Refresh()
        {
            if (progressText != null)
            {
                progressText.text = string.Format("{0}/{1}", currentValue, MaxValue);
            }

            if (completedMark != null)
            {
                completedMark.SetActive(currentValue >= MaxValue);
            }
        }
        
    }

    public class DailyMissionPanelController : MonoBehaviour
    {
        [Header("Progress")]
        [SerializeField] private bool clampToMax = true;
        [SerializeField] private List<DailyWeeklyMissionEntry> missions = new List<DailyWeeklyMissionEntry>();

        [Header("Events")]
        [SerializeField] private DailyWeeklyMissionProgressChangedEvent onProgressChanged = new DailyWeeklyMissionProgressChangedEvent();
        [SerializeField] private UnityEvent onAllCompleted = new UnityEvent();

        public IReadOnlyList<DailyWeeklyMissionEntry> Missions { get { return missions; } }

        private void Reset()
        {
            EnsureDefaultEntries();
        }

        private void Awake()
        {
            EnsureDefaultEntries();
            NormalizeAll();
            RefreshAll();
        }

        private void OnValidate()
        {
            EnsureDefaultEntries();
            NormalizeAll();
            RefreshAll();
        }
    
        #region Process Add Test
        [Button("Add Fight Process")]
        private void AddFightProcess()
        {
            AddProgress(DailyWeeklyMissionType.Battle);
        }

        [Button("Add Abandoned Warehouse")]
        private void AddWareHouseProcess()
        {
            AddProgress(DailyWeeklyMissionType.AbandonedWarehouse);
        }
        [Button("Add Support Box")]
        private void AddSupportBoxProcess()
        {
            AddProgress(DailyWeeklyMissionType.SupportBox);
        }

        [Button("Add Cross Validation")]
        private void AddCrossValidationProcess()
        {
            AddProgress(DailyWeeklyMissionType.CrossValidation);
        }
        #endregion
    
        public void AddProgress(DailyWeeklyMissionType missionType)
        {
            AddProgress(missionType, 1);
        }

        public void AddProgress(DailyWeeklyMissionType missionType, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            DailyWeeklyMissionEntry entry = FindEntry(missionType);
            if (entry == null)
            {
                Debug.LogWarningFormat(this, "Mission type not configured: {0}", missionType);
                return;
            }

            int oldValue = entry.CurrentValue;
            entry.SetCurrentValue(oldValue + amount, clampToMax);
            entry.Refresh();

            if (oldValue != entry.CurrentValue)
            {
                onProgressChanged.Invoke(missionType, entry.CurrentValue, entry.MaxValue);
                if (IsAllCompleted())
                {
                    onAllCompleted.Invoke();
                }
            }
        }

        public void SetProgress(DailyWeeklyMissionType missionType, int value)
        {
            DailyWeeklyMissionEntry entry = FindEntry(missionType);
            if (entry == null)
            {
                Debug.LogWarningFormat(this, "Mission type not configured: {0}", missionType);
                return;
            }

            int oldValue = entry.CurrentValue;
            entry.SetCurrentValue(value, clampToMax);
            entry.Refresh();

            if (oldValue != entry.CurrentValue)
            {
                onProgressChanged.Invoke(missionType, entry.CurrentValue, entry.MaxValue);
                if (IsAllCompleted())
                {
                    onAllCompleted.Invoke();
                }
            }
        }

        public void SetMaxValue(DailyWeeklyMissionType missionType, int value)
        {
            DailyWeeklyMissionEntry entry = FindEntry(missionType);
            if (entry == null)
            {
                Debug.LogWarningFormat(this, "Mission type not configured: {0}", missionType);
                return;
            }

            entry.SetMaxValue(value);
            entry.SetCurrentValue(entry.CurrentValue, clampToMax);
            entry.Refresh();
            onProgressChanged.Invoke(missionType, entry.CurrentValue, entry.MaxValue);
        }

        public int GetProgress(DailyWeeklyMissionType missionType)
        {
            DailyWeeklyMissionEntry entry = FindEntry(missionType);
            return entry == null ? 0 : entry.CurrentValue;
        }

        public int GetMaxValue(DailyWeeklyMissionType missionType)
        {
            DailyWeeklyMissionEntry entry = FindEntry(missionType);
            return entry == null ? 0 : entry.MaxValue;
        }

        public bool IsCompleted(DailyWeeklyMissionType missionType)
        {
            DailyWeeklyMissionEntry entry = FindEntry(missionType);
            return entry != null && entry.CurrentValue >= entry.MaxValue;
        }

        public bool IsAllCompleted()
        {
            if (missions == null || missions.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < missions.Count; i++)
            {
                DailyWeeklyMissionEntry entry = missions[i];
                if (entry == null || entry.CurrentValue < entry.MaxValue)
                {
                    return false;
                }
            }

            return true;
        }

        public void ClearAllProgress()
        {
            for (int i = 0; i < missions.Count; i++)
            {
                DailyWeeklyMissionEntry entry = missions[i];
                if (entry == null)
                {
                    continue;
                }

                bool changed = entry.CurrentValue != 0;
                entry.SetCurrentValue(0, clampToMax);
                entry.Refresh();
                if (changed)
                {
                    onProgressChanged.Invoke(entry.MissionType, entry.CurrentValue, entry.MaxValue);
                }
            }
        }

        public void RefreshAll()
        {
            if (missions == null)
            {
                return;
            }

            for (int i = 0; i < missions.Count; i++)
            {
                if (missions[i] != null)
                {
                    missions[i].Refresh();
                }
            }
        }

        private DailyWeeklyMissionEntry FindEntry(DailyWeeklyMissionType missionType)
        {
            if (missions == null)
            {
                return null;
            }

            for (int i = 0; i < missions.Count; i++)
            {
                DailyWeeklyMissionEntry entry = missions[i];
                if (entry != null && entry.MissionType == missionType)
                {
                    return entry;
                }
            }

            return null;
        }

        private void NormalizeAll()
        {
            if (missions == null)
            {
                return;
            }

            for (int i = 0; i < missions.Count; i++)
            {
                DailyWeeklyMissionEntry entry = missions[i];
                if (entry != null)
                {
                    entry.SetMaxValue(entry.MaxValue);
                    entry.SetCurrentValue(entry.CurrentValue, clampToMax);
                }
            }
        }

        private void EnsureDefaultEntries()
        {
            if (missions == null)
            {
                missions = new List<DailyWeeklyMissionEntry>();
            }

            EnsureEntry(DailyWeeklyMissionType.Battle, 5);
            EnsureEntry(DailyWeeklyMissionType.SupportBox, 3);
            EnsureEntry(DailyWeeklyMissionType.CrossValidation, 1);
            EnsureEntry(DailyWeeklyMissionType.AbandonedWarehouse, 3);
        }

        private void EnsureEntry(DailyWeeklyMissionType missionType, int defaultMax)
        {
            if (FindEntry(missionType) != null)
            {
                return;
            }

            missions.Add(new DailyWeeklyMissionEntry(missionType, defaultMax));
        }
    }
}