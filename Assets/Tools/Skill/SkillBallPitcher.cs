using System;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Skill
{
    /// <summary>
    /// Utility facade for adding, removing, and energy-generating skill balls.
    /// It does not own buff timing or character-specific spawn rules.
    /// </summary>
    public class SkillBallPitcher
    {
        private ActionStateController owner;
        private SkillBallManager skillBallManager;
        private ISkillBallSpawnProvider spawnProvider;

        private float currentEnergy;
        private float energyPerBall = 100f;
        private int nextRuntimeId = 1;

        public float CurrentEnergy => currentEnergy;
        public float EnergyPerBall => energyPerBall;

        public event Action<float, float> EnergyChanged;
        public event Action<SkillBallData> BallAdded;
        public event Action<int> BallRemoved;

        public void Initialize(
            ActionStateController owner,
            SkillBallManager skillBallManager,
            ISkillBallSpawnProvider spawnProvider,
            float energyPerBall)
        {
            this.owner = owner;
            this.skillBallManager = skillBallManager;
            this.spawnProvider = spawnProvider;
            this.energyPerBall = Mathf.Max(1f, energyPerBall);
            EnsureNextRuntimeId();
        }

        public void ResetEnergy(float energy = 0f)
        {
            currentEnergy = Mathf.Max(0f, energy);
            EnergyChanged?.Invoke(currentEnergy, energyPerBall);
        }

        public void AddEnergy(float amount, SkillBallEnergySourceContext context)
        {
            if (amount <= 0f)
            {
                return;
            }

            if (!HasStoreCapacity())
            {
                DiscardEnergy();
                return;
            }

            currentEnergy += amount;
            EnergyChanged?.Invoke(currentEnergy, energyPerBall);
            TryPitchByEnergy(context);
        }

        public bool TryPitchByRule(SkillBallEnergySourceContext context)
        {
            if (!CanPitchByRule())
            {
                return false;
            }

            if (!spawnProvider.TryGetNextBall(owner, context, out SkillBallSpawnResult result))
            {
                return false;
            }

            return TryAppendBall(result.Color, result.ConsumeMode, result.IconAddress);
        }

        public bool TryAppendBall(
            SkillBallColor color,
            SkillBallConsumeMode consumeMode = SkillBallConsumeMode.AlwaysTripleCheck,
            string iconAddress = null)
        {
            if (skillBallManager == null)
            {
                return false;
            }

            SkillBallData ball = new SkillBallData(
                nextRuntimeId++,
                color,
                consumeMode,
                iconAddress);

            if (!skillBallManager.TryAppendBall(ball))
            {
                return false;
            }

            BallAdded?.Invoke(ball);
            return true;
        }

        public int AppendBalls(
            SkillBallColor color,
            int count,
            SkillBallConsumeMode consumeMode = SkillBallConsumeMode.AlwaysTripleCheck,
            string iconAddress = null)
        {
            int added = 0;
            int safeCount = Mathf.Max(0, count);

            for (int i = 0; i < safeCount; i++)
            {
                if (!TryAppendBall(color, consumeMode, iconAddress))
                {
                    break;
                }

                added++;
            }

            return added;
        }

        public int AppendTriple(
            SkillBallColor color,
            SkillBallConsumeMode consumeMode = SkillBallConsumeMode.AlwaysTripleCheck,
            string iconAddress = null)
        {
            return AppendBalls(color, 3, consumeMode, iconAddress);
        }

        public bool TryRemoveAt(int visibleIndex)
        {
            if (skillBallManager == null)
            {
                return false;
            }

            if (!skillBallManager.TryRemoveAt(visibleIndex, out SkillBallData removedBall))
            {
                return false;
            }

            BallRemoved?.Invoke(removedBall.RuntimeId);
            return true;
        }

        public int RemoveRange(int startIndex, int count)
        {
            if (skillBallManager == null)
            {
                return 0;
            }

            int removed = skillBallManager.RemoveRange(startIndex, count);
            if (removed > 0)
            {
                BallRemoved?.Invoke(-1);
            }

            return removed;
        }

        public int ClearBalls()
        {
            if (skillBallManager == null)
            {
                return 0;
            }

            int removed = skillBallManager.ClearBalls();
            if (removed > 0)
            {
                BallRemoved?.Invoke(-1);
            }

            return removed;
        }

        private void TryPitchByEnergy(SkillBallEnergySourceContext context)
        {
            while (currentEnergy >= energyPerBall)
            {
                if (!HasStoreCapacity())
                {
                    DiscardEnergy();
                    return;
                }

                if (!TryPitchByRule(context))
                {
                    if (!HasStoreCapacity())
                    {
                        DiscardEnergy();
                    }

                    return;
                }

                currentEnergy -= energyPerBall;
                EnergyChanged?.Invoke(currentEnergy, energyPerBall);
            }
        }

        private bool HasStoreCapacity()
        {
            return skillBallManager != null &&
                   skillBallManager.GetBalls().Count < SkillBallManager.MaxStoreCount;
        }

        private void DiscardEnergy()
        {
            if (currentEnergy <= 0f)
            {
                return;
            }

            currentEnergy = 0f;
            EnergyChanged?.Invoke(currentEnergy, energyPerBall);
        }

        private bool CanPitchByRule()
        {
            return owner != null &&
                   skillBallManager != null &&
                   spawnProvider != null;
        }

        private void EnsureNextRuntimeId()
        {
            if (skillBallManager == null)
            {
                return;
            }

            foreach (SkillBallData ball in skillBallManager.GetBalls())
            {
                nextRuntimeId = Mathf.Max(nextRuntimeId, ball.RuntimeId + 1);
            }
        }
    }
}
