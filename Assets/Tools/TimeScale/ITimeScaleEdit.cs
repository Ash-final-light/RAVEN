using Tools.InputHandle;
using Tools.TimeScale.Receiver;

namespace TimeScale
{
    public interface ITimeScaleEdit
    {
        float GetTimeScale(ITimeScaleDispatcher target, TimeScaleType type);
        void SetTimeScale(ITimeScaleDispatcher target, TimeScaleType type, float scale);
    }
}
