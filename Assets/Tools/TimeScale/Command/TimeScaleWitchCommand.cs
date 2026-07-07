
using Tools.TimeScale.Command;
using Tools.TimeScale.Receiver;

namespace TimeScale.Command
{
    /// <summary>
    /// 魔女时间命令：
    /// 1. 场景中除发动者外的所有对象大幅减速（或停止）。
    /// 2. 发动者保持原速或获得加速加成。
    /// </summary>
    public class TimeScaleWitchCommand : BaseTimeScaleCommand
    {
        private readonly float _slowDownScale;  // 其他人受到的减速倍率
        private readonly float _hurryUpScale;   // 发动者受到的加速倍率
        private readonly string _witchInstanceId;  // 发动者的唯一ID（从宿主脚本获取）

        /// <summary>
        /// 构造魔女时间指令
        /// </summary>
        /// <param name="id">命令唯一标识</param>
        /// <param name="duration">持续时间</param>
        /// <param name="slow">环境减速值 (例: 0.1f)</param>
        /// <param name="hurry">主角加速值 (例: 1.0f 或 1.2f)</param>
        /// <param name="witchId">发动者的ID</param>
        /// <param name="affectedType">受影响的组件类型，默认全部受影响</param>
        public TimeScaleWitchCommand(
            string id, 
            float duration, 
            float slow, 
            float hurry, 
            string witchId, 
            TimeScaleType affectedType = TimeScaleType.All) 
            : base(id, duration, affectedType)
        {
            _slowDownScale = slow;
            _hurryUpScale = hurry;
            _witchInstanceId = witchId;
        }

        /// <summary>
        /// 执行逻辑：由 TimeScaleMode 遍历所有注册的 Dispatcher 时调用
        /// </summary>
        public override void Execute(ITimeScaleEdit edit, ITimeScaleDispatcher dispatcher)
        {
            // 通过 InstanceId 判断当前正在处理的是否为魔女本人
            if (dispatcher.InstanceId == _witchInstanceId)
            {
                // 如果是发动者，应用加速/正常缩放
                edit.SetTimeScale(dispatcher, AffectedType, _hurryUpScale);
            }
            else
            {
                // 如果是其他人，应用环境减速缩放
                edit.SetTimeScale(dispatcher, AffectedType, _slowDownScale);
            }
        }
    }
}