using System;
using Tools.InputHandle;
using Tools.Skill;
using UnityEngine;

namespace Scripts.Action.Alpha
{
    [CreateAssetMenu(
        fileName = "AlphaSwordIntentSkillBallEffect",
        menuName = "Action/Alpha/Sword Intent Skill Ball Effect")]
    public class AlphaSwordIntentSkillBallEffect : ScriptableObject
    {
        [Header("Enter Sword Intent")]
        [SerializeField] private SkillBallColor enterColor = SkillBallColor.White;
        [SerializeField] private SkillBallConsumeMode enterConsumeMode = SkillBallConsumeMode.AlwaysSingle;
        [SerializeField] private string enterIconAddress;
        [SerializeField] private int extraEnterBallCount = 2;

        [Header("Exit Sword Intent")]
        [SerializeField] private SkillBallColor[] exitRandomColors =
        {
            SkillBallColor.Red,
            SkillBallColor.Yellow,
            SkillBallColor.Blue
        };
        [SerializeField] private SkillBallConsumeMode exitConsumeMode = SkillBallConsumeMode.AlwaysTripleCheck;
        [SerializeField] private ColorIconAddress[] exitIconAddresses =
        {
            new(SkillBallColor.Red, null),
            new(SkillBallColor.Yellow, null),
            new(SkillBallColor.Blue, null)
        };

        public void ApplyEnter(ActionStateController owner, int triggerConsumedBallCount = 0)
        {
            ReplaceAllBalls(
                owner,
                enterColor,
                enterConsumeMode,
                enterIconAddress,
                extraEnterBallCount + Mathf.Max(0, triggerConsumedBallCount));
        }

        public void ApplyExit(ActionStateController owner)
        {
            if (owner == null)
            {
                return;
            }

            int count = owner.SkillBallManager.GetBalls().Count;
            owner.SkillBallManager.ClearBalls();

            for (int i = 0; i < count; i++)
            {
                SkillBallColor color = GetRandomExitColor();
                owner.SkillBallPitcher.TryAppendBall(
                    color,
                    exitConsumeMode,
                    GetExitIconAddress(color));
            }
        }

        public static void ApplyDefaultEnter(ActionStateController owner, int triggerConsumedBallCount = 0)
        {
            ReplaceAllBalls(
                owner,
                SkillBallColor.White,
                SkillBallConsumeMode.AlwaysSingle,
                null,
                2 + Mathf.Max(0, triggerConsumedBallCount));
        }

        public static void ApplyDefaultExit(ActionStateController owner)
        {
            if (owner == null)
            {
                return;
            }

            int count = owner.SkillBallManager.GetBalls().Count;
            owner.SkillBallManager.ClearBalls();

            for (int i = 0; i < count; i++)
            {
                owner.SkillBallPitcher.TryAppendBall(
                    UnityEngine.Random.Range(0, 3) switch
                    {
                        0 => SkillBallColor.Red,
                        1 => SkillBallColor.Yellow,
                        _ => SkillBallColor.Blue
                    },
                    SkillBallConsumeMode.AlwaysTripleCheck);
            }
        }

        private static void ReplaceAllBalls(
            ActionStateController owner,
            SkillBallColor color,
            SkillBallConsumeMode consumeMode,
            string iconAddress,
            int extraBallCount)
        {
            if (owner == null)
            {
                return;
            }

            int count = owner.SkillBallManager.GetBalls().Count + Mathf.Max(0, extraBallCount);
            owner.SkillBallManager.ClearBalls();

            for (int i = 0; i < count; i++)
            {
                owner.SkillBallPitcher.TryAppendBall(color, consumeMode, iconAddress);
            }
        }

        private SkillBallColor GetRandomExitColor()
        {
            if (exitRandomColors == null || exitRandomColors.Length == 0)
            {
                return UnityEngine.Random.Range(0, 3) switch
                {
                    0 => SkillBallColor.Red,
                    1 => SkillBallColor.Yellow,
                    _ => SkillBallColor.Blue
                };
            }

            return exitRandomColors[UnityEngine.Random.Range(0, exitRandomColors.Length)];
        }

        private string GetExitIconAddress(SkillBallColor color)
        {
            if (exitIconAddresses == null)
            {
                return null;
            }

            foreach (ColorIconAddress iconAddress in exitIconAddresses)
            {
                if (iconAddress.Color == color)
                {
                    return iconAddress.IconAddress;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class ColorIconAddress
    {
        [SerializeField] private SkillBallColor color;
        [SerializeField] private string iconAddress;

        public SkillBallColor Color => color;
        public string IconAddress => iconAddress;

        public ColorIconAddress()
        {
        }

        public ColorIconAddress(SkillBallColor color, string iconAddress)
        {
            this.color = color;
            this.iconAddress = iconAddress;
        }
    }
}
