using System;

namespace Tools.TimeScale.Receiver
{
    public interface ITimeScalable
    {
        string Id { get; }
        void TimeScaleChanged(float scale, TimeScaleType type);
    }
}