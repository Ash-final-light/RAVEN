using Tools.TimeScale.Command;
using Tools.TimeScale.Receiver;

namespace TimeScale.Command
{
    /// <summary>
    /// 全局时间缩放命令：
    /// 对场景中所有对象（Dispatcher）应用统一的缩放值。
    /// 常用于：全局大招特写、菜单打开时的半暂停、整体游戏节奏调整。
    /// </summary>
    public class TimeScaleGlobalCommand : BaseTimeScaleCommand
    {
        private readonly float _targetScale;

        /// <summary>
        /// 构造全局缩放指令
        /// </summary>
        /// <param name="id">命令唯一标识</param>
        /// <param name="duration">持续时间</param>
        /// <param name="scale">缩放倍率</param>
        /// <param name="affectedType">受影响的组件类型（位掩码）</param>
        public TimeScaleGlobalCommand(
            string id, 
            float duration, 
            float scale, 
            TimeScaleType affectedType = TimeScaleType.All) 
            : base(id, duration, affectedType)
        {
            _targetScale = scale;
        }

        /// <summary>
        /// 执行逻辑：对传入的 target（由 Mode 遍历提供）应用统一缩放
        /// </summary>
        public override void Execute(ITimeScaleEdit edit, ITimeScaleDispatcher target)
        {
            // 无论 target 是谁，都应用同样的缩放值
            // 细颗粒度控制：只影响 AffectedType 中定义的槽位
            if(edit.GetTimeScale(target, AffectedType) <= _targetScale)
            {
                return;
            }
            edit.SetTimeScale(target, AffectedType, _targetScale);
        }
    }
}