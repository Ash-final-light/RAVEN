namespace Tools.Damage
{
    public enum DamageType
    {
        #region 普通伤害，受到结算和无敌影响

        DirectDamage,
        PeriodDamage,

        #endregion

        #region 真实伤害，无视结算和无敌，可能会被Buff影响期望值

        TrueDamage,

        #endregion

        #region 治疗，无视无敌

        DirectHeal,
        PeriodHeal,

        #endregion
    }
}