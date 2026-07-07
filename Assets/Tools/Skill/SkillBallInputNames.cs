using UnityEngine;

namespace Tools.Skill
{
    /// <summary>
    /// 技能球消除结果对应的连招输入名。这里不是 Input System 的按键 Action 名。
    /// 槽位按键仍然使用 SkillBall0/1/2/3，连招表使用 SkillBallRed1、SkillBallRed3 等固定字符串。
    /// </summary>
    public static class SkillBallInputNames
    {
        public const string Red1 = "SkillBallRed1";
        public const string Red2 = "SkillBallRed2";
        public const string Red3 = "SkillBallRed3";
        public const string Yellow1 = "SkillBallYellow1";
        public const string Yellow2 = "SkillBallYellow2";
        public const string Yellow3 = "SkillBallYellow3";
        public const string Blue1 = "SkillBallBlue1";
        public const string Blue2 = "SkillBallBlue2";
        public const string Blue3 = "SkillBallBlue3";
        public const string White1 = "SkillBallWhite1";

        public static string FromColorAndCount(SkillBallColor color, int consumeCount)
        {
            int count = color == SkillBallColor.White
                ? 1
                : Mathf.Clamp(consumeCount, 1, 3);

            return color switch
            {
                SkillBallColor.Red => $"SkillBallRed{count}",
                SkillBallColor.Yellow => $"SkillBallYellow{count}",
                SkillBallColor.Blue => $"SkillBallBlue{count}",
                SkillBallColor.White => White1,
                _ => null
            };
        }
    }
}
