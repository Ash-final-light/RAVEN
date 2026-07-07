using Tools.Damage;
using Tools.InputHandle;

namespace Tools.Skill
{
    /// <summary>
    /// Describes why skill-ball energy was gained.
    /// Providers can inspect this context to choose the next generated ball.
    /// </summary>
    public readonly struct SkillBallEnergySourceContext
    {
        public readonly DamageInfo DamageInfo;
        public readonly int ConsumedSignalCount;
        public readonly SkillBallColor ConsumedSignalColor;
        public readonly string ActionName;

        public SkillBallEnergySourceContext(
            DamageInfo damageInfo,
            int consumedSignalCount = 0,
            SkillBallColor consumedSignalColor = default,
            string actionName = null)
        {
            DamageInfo = damageInfo;
            ConsumedSignalCount = consumedSignalCount;
            ConsumedSignalColor = consumedSignalColor;
            ActionName = actionName;
        }
    }

    /// <summary>
    /// Result returned by a character-specific provider when the pitcher asks for the next ball.
    /// </summary>
    public readonly struct SkillBallSpawnResult
    {
        public readonly SkillBallColor Color;
        public readonly SkillBallConsumeMode ConsumeMode;
        public readonly string IconAddress;

        public SkillBallSpawnResult(
            SkillBallColor color,
            SkillBallConsumeMode consumeMode,
            string iconAddress)
        {
            Color = color;
            ConsumeMode = consumeMode;
            IconAddress = iconAddress;
        }
    }

    /// <summary>
    /// Character-specific skill-ball generation rule.
    /// Implementations may query PlayerParameters, RuntimeContext, or RunningBlackboard.
    /// </summary>
    public interface ISkillBallSpawnProvider
    {
        bool TryGetNextBall(
            ActionStateController owner,
            SkillBallEnergySourceContext context,
            out SkillBallSpawnResult result);
    }
}
