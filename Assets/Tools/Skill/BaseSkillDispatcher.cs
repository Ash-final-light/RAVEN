using System;
using System.Collections.Generic;

namespace Tools.Skill
{
    public enum SkillBallColor
    {
        Red,
        Yellow,
        Blue,
        White
    }

    public readonly struct SkillBallViewData
    {
        public readonly int RuntimeId;
        public readonly SkillBallColor Color;
        public readonly string IconAddress;
        public readonly int VisibleIndex;

        public SkillBallViewData(
            int runtimeId,
            SkillBallColor color,
            string iconAddress,
            int visibleIndex)
        {
            RuntimeId = runtimeId;
            Color = color;
            IconAddress = iconAddress;
            VisibleIndex = visibleIndex;
        }
    }

    public readonly struct SkillBallConsumeViewData
    {
        public readonly int[] RuntimeIds;
        public readonly int StartVisibleIndex;
        public readonly int ConsumeCount;
        public readonly SkillBallColor Color;

        public SkillBallConsumeViewData(
            int[] runtimeIds,
            int startVisibleIndex,
            int consumeCount,
            SkillBallColor color)
        {
            RuntimeIds = runtimeIds;
            StartVisibleIndex = startVisibleIndex;
            ConsumeCount = consumeCount;
            Color = color;
        }
    }

    public readonly struct CharacterSkillUISet
    {
        public readonly string CharacterId;
        public readonly string AttackIconAddress;
        public readonly string DodgeIconAddress;
        public readonly string UltimateIconAddress;
        public readonly string LockIconAddress;
        public readonly string[] SkillBallIconAddresses;

        public CharacterSkillUISet(
            string characterId,
            string attackIconAddress,
            string dodgeIconAddress,
            string ultimateIconAddress,
            string lockIconAddress,
            string[] skillBallIconAddresses)
        {
            CharacterId = characterId;
            AttackIconAddress = attackIconAddress;
            DodgeIconAddress = dodgeIconAddress;
            UltimateIconAddress = ultimateIconAddress;
            LockIconAddress = lockIconAddress;
            SkillBallIconAddresses = skillBallIconAddresses;
        }
    }

    public abstract class BaseSkillDispatcher
    {
        /// <summary>
        /// Tells UI which skill ball runtime ids should play consume animation.
        /// The final queue layout should still be synchronized by the next full visible-list refresh.
        /// </summary>
        public event Action<SkillBallConsumeViewData> SkillBallsConsumed;

        /// <summary>
        /// Full refresh of currently visible skill balls, not only newly added balls.
        /// UI should diff by RuntimeId to create, move, or remove visual entities.
        /// </summary>
        public event Action<IReadOnlyList<SkillBallViewData>> SkillBallsAdded;
        public event Action<CharacterSkillUISet> CharacterSkillUIChanged;
        public event Action<float, float> UltimateEnergyChanged;
        public event Action<bool> UltimateReadyChanged;
        public event Action<bool> MatrixModeChanged;

        public void NotifySkillBallsConsumed(SkillBallConsumeViewData data)
        {
            DispatchSkillBallsConsumed(data);
        }

        public void NotifySkillBallsAdded(IReadOnlyList<SkillBallViewData> balls)
        {
            DispatchSkillBallsAdded(balls);
        }

        public void NotifyCharacterSkillUIChanged(CharacterSkillUISet uiSet)
        {
            DispatchCharacterSkillUIChanged(uiSet);
        }

        public void NotifyUltimateEnergyChanged(float current, float max)
        {
            DispatchUltimateEnergyChanged(current, max);
        }

        public void NotifyUltimateReadyChanged(bool isReady)
        {
            DispatchUltimateReadyChanged(isReady);
        }

        public void NotifyMatrixModeChanged(bool isInMatrixMode)
        {
            DispatchMatrixModeChanged(isInMatrixMode);
        }

        protected void DispatchSkillBallsConsumed(SkillBallConsumeViewData data)
        {
            SkillBallsConsumed?.Invoke(data);
        }

        protected void DispatchSkillBallsAdded(IReadOnlyList<SkillBallViewData> balls)
        {
            SkillBallsAdded?.Invoke(balls);
        }

        protected void DispatchCharacterSkillUIChanged(CharacterSkillUISet uiSet)
        {
            CharacterSkillUIChanged?.Invoke(uiSet);
        }

        protected void DispatchUltimateEnergyChanged(float current, float max)
        {
            UltimateEnergyChanged?.Invoke(current, max);
            UltimateReadyChanged?.Invoke(max > 0f && current >= max);
        }

        protected void DispatchUltimateReadyChanged(bool isReady)
        {
            UltimateReadyChanged?.Invoke(isReady);
        }

        protected void DispatchMatrixModeChanged(bool isInMatrixMode)
        {
            MatrixModeChanged?.Invoke(isInMatrixMode);
        }
    }
}
