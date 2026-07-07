namespace Tools.Action.Data.Event
{
    public enum ActionEventEnum
    {
        Blackboard,
        AddBuff,
        Debug,
        FireBullet,
        SlideJudge,
        CharacterPropertyFlag,
    }

    public enum SlideJudgeOp
    {
        Open,
        Close,
    }

    public enum CharacterPropertyFlagType
    {
        Immune,
        Endure,
        Unbreakable,
    }

    public enum BlackboardOp
    {
        Add,
        Set,
    }

    public enum BlackboardValueType
    {
        Int,
        Float,
        Bool,
        String,
    }
}
