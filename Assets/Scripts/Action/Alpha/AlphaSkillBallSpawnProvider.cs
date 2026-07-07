using Tools.InputHandle;
using Tools.Skill;
using UnityEngine;

namespace Scripts.Action.Alpha
{
    /// <summary>
    /// Alpha spawn rule:
    /// normal state generates red/yellow/blue triple-check balls,
    /// sword-intent state generates white single-consume balls only.
    /// </summary>
    public class AlphaSkillBallSpawnProvider : MonoBehaviour, ISkillBallSpawnProvider
    {
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
            SkillBallColor color = IsSwordIntentActive(owner)
                ? SkillBallColor.White
                : RollNormalColor();

            result = new SkillBallSpawnResult(
                color,
                GetConsumeMode(color),
                GetIconAddress(color));
            return true;
        }

        private static bool IsSwordIntentActive(ActionStateController owner)
        {
            return owner != null &&
                   owner.RunningBlackboard != null &&
                   owner.RunningBlackboard.ContainsKey(SkillBallBlackboardKeys.SwordIntentActive) &&
                   owner.RunningBlackboard.GetBoolParameter(SkillBallBlackboardKeys.SwordIntentActive);
        }

        private static SkillBallColor RollNormalColor()
        {
            return Random.Range(0, 3) switch
            {
                0 => SkillBallColor.Red,
                1 => SkillBallColor.Yellow,
                _ => SkillBallColor.Blue
            };
        }

        private static SkillBallConsumeMode GetConsumeMode(SkillBallColor color)
        {
            return color == SkillBallColor.White
                ? SkillBallConsumeMode.AlwaysSingle
                : SkillBallConsumeMode.AlwaysTripleCheck;
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
