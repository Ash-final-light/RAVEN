using TimeScale;
using Tools.TimeScale.Receiver;

namespace Tools.TimeScale.Command
{
    public class TimeScaleSingleCommand:BaseTimeScaleCommand
    {
        private readonly float _targetScale;
        private readonly string _targetInstanceId;  // 发动者的唯一ID（从宿主脚本获取）
        
        public TimeScaleSingleCommand(
            string id, 
            float duration, 
            float targetScale, 
            string targetInstanceId,
            TimeScaleType affectedType = TimeScaleType.All)
            : base(id, duration, affectedType)
        {
            _targetScale = targetScale;
            _targetInstanceId = targetInstanceId;
        }

        public override void Execute(ITimeScaleEdit edit, ITimeScaleDispatcher target)
        {
            if (target.InstanceId != _targetInstanceId)
            {
                return;
            }

            if(edit.GetTimeScale(target, AffectedType) <= _targetScale)
            {
                return;
            }
            edit.SetTimeScale(target, AffectedType, _targetScale);
        }
    }
}
