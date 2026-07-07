using TimeScale;
using Tools.TimeScale.Receiver;

namespace Tools.TimeScale.Command
{
    public abstract class BaseTimeScaleCommand
    {
        public string Id { get; }
        public float Duration { get; }
        public float Time;
        // 该命令影响的细分类型
        public TimeScaleType AffectedType { get; }

        protected BaseTimeScaleCommand(string id, float duration, TimeScaleType affectedType = TimeScaleType.All)
        {
            Id = id;
            Duration = duration;
            AffectedType = affectedType;
            Time = 0;
        }

        public abstract void Execute(ITimeScaleEdit edit, ITimeScaleDispatcher target);
    }
}