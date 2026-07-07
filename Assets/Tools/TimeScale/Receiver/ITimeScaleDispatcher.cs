using TimeScale;

namespace Tools.TimeScale.Receiver
{
    public interface ITimeScaleDispatcher
    {
        string InstanceId { get; }  
        // 外部根据类型下发缩放值
        void ApplyScale(TimeScaleType type, float scale);
    }
}