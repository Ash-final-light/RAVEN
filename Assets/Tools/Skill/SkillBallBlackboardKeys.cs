namespace Tools.Skill
{
    /// <summary>
    /// Blackboard keys used by the skill-ball spawning and state systems.
    /// Character-specific actions can write these keys through ActionClip blackboard events.
    /// </summary>
    public static class SkillBallBlackboardKeys
    {
        /// <summary>
        /// Generic switch: when true, the default spawn provider generates only white balls.
        /// </summary>
        public const string ForceWhiteSpawn = "skill_ball_force_white_spawn";

        /// <summary>
        /// Generic core-state flag. Different characters may enter it through different rules.
        /// </summary>
        public const string CoreStateActive = "skill_ball_core_state_active";

        /// <summary>
        /// Crimson Abyss style sword-intent flag. Kept separate so character rules can stay readable.
        /// </summary>
        public const string SwordIntentActive = "skill_ball_sword_intent_active";
    }
}
