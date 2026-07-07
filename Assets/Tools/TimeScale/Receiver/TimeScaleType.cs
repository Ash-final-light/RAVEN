using System;

namespace Tools.TimeScale.Receiver
{
    [Flags]
    public enum TimeScaleType
    {
        None   = 0,
        Bullet = 1 << 0,
        Effect = 1 << 1,
        Audio  = 1 << 2,
        // 倾向于角色本地的时间缩放
        Logic  = 1 << 3,
        All    = Bullet | Effect | Audio | Logic
    }
}