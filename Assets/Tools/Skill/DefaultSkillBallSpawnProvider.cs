using Tools.InputHandle;
using UnityEngine;

namespace Tools.Skill
{
    /// <summary>
    /// Default character skill-ball spawn rule.
    /// Normal pool is red/yellow/blue; white can be enabled for tests or special characters.
    /// </summary>
    public class DefaultSkillBallSpawnProvider : MonoBehaviour, ISkillBallSpawnProvider
    {
        [SerializeField] private bool includeWhite;
        [SerializeField] private SkillBallConsumeMode defaultConsumeMode = SkillBallConsumeMode.AlwaysTripleCheck;

        [Header("Blackboard State")]
        [SerializeField] private bool useBlackboardState = true;
        [SerializeField]
        private string[] forceWhiteStateKeys =
        {
            SkillBallBlackboardKeys.ForceWhiteSpawn,
            SkillBallBlackboardKeys.CoreStateActive,
            SkillBallBlackboardKeys.SwordIntentActive
        };

        [Header("Icon Address")]
        [SerializeField] private string redIconAddress;
        [SerializeField] private string yellowIconAddress;
        [SerializeField] private string blueIconAddress;
        [SerializeField] private string whiteIconAddress;

        public bool TryGetNextBall(
            ActionStateController owner,
            SkillBallEnergySourceContext context,
            out SkillBallSpawnResult result)
        {
            SkillBallColor color = ShouldForceWhite(owner)
                ? SkillBallColor.White
                : RollColor();

            result = new SkillBallSpawnResult(
                color,
                defaultConsumeMode,
                GetIconAddress(color));
            return true;
        }

        private bool ShouldForceWhite(ActionStateController owner)
        {
            if (!useBlackboardState ||
                owner == null ||
                owner.RunningBlackboard == null ||
                forceWhiteStateKeys == null)
            {
                return false;
            }

            foreach (string key in forceWhiteStateKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (owner.RunningBlackboard.ContainsKey(key) &&
                    owner.RunningBlackboard.GetBoolParameter(key))
                {
                    return true;
                }
            }

            return false;
        }

        private SkillBallColor RollColor()
        {
            int count = includeWhite ? 4 : 3;
            int index = Random.Range(0, count);

            return index switch
            {
                0 => SkillBallColor.Red,
                1 => SkillBallColor.Yellow,
                2 => SkillBallColor.Blue,
                _ => SkillBallColor.White
            };
        }

        private string GetIconAddress(SkillBallColor color)
        {
            return color switch
            {
                SkillBallColor.Red => redIconAddress,
                SkillBallColor.Yellow => yellowIconAddress,
                SkillBallColor.Blue => blueIconAddress,
                SkillBallColor.White => whiteIconAddress,
                _ => null
            };
        }
    }
}
