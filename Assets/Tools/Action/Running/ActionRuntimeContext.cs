namespace Tools.Action.Running
{
    /// <summary>
    /// 当前 ActionPlayer 正在播放的动作实例上下文。
    /// 技能球命令在动作 Start 阶段被消费后写入这里，动作切换或打断时随 ClearData 一起清空。
    /// </summary>
    public class ActionRuntimeContext
    {
        public Tools.Skill.SkillBallColor ConsumedSignalColor;
        public string ConsumedSignalInputName;
        public int ConsumedSignalCount;
        public string ConsumedSkillActionName;
        public int[] ConsumedSignalRuntimeIds;

        public static ActionRuntimeContext Empty()
        {
            return new ActionRuntimeContext();
        }
    }
}
