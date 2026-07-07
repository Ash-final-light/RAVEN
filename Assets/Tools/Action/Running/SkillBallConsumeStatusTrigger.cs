using Tools.Skill;
using UnityEngine;

namespace Tools.Action.Running
{
    [CreateAssetMenu(fileName = "SkillBallConsumeStatusTrigger", menuName = "Action/Status Trigger/Skill Ball Consume")]
    public class SkillBallConsumeStatusTrigger : StatusTrigger
    {
        [SerializeField] private bool matchAnyColor;
        [SerializeField] private SkillBallColor requiredColor = SkillBallColor.Blue;
        [SerializeField] private int requiredConsumeCount = 3;
        [SerializeField] private bool consumeCountOrGreater = true;
        [SerializeField] private string requiredSkillActionName;

        public override bool Satisfy(StatusTriggerContext context)
        {
            if (!context.HasSkillBallConsume)
            {
                return false;
            }

            if (!matchAnyColor && context.ConsumedSignalColor != requiredColor)
            {
                return false;
            }

            if (consumeCountOrGreater)
            {
                if (context.ConsumedSignalCount < requiredConsumeCount)
                {
                    return false;
                }
            }
            else if (context.ConsumedSignalCount != requiredConsumeCount)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredSkillActionName) &&
                context.ConsumedSkillActionName != requiredSkillActionName)
            {
                return false;
            }

            return true;
        }
    }
}
