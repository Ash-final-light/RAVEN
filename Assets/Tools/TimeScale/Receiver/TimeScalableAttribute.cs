using System;

namespace Tools.TimeScale.Receiver
{
    /// <summary>
    /// 用来告知分发器这个属性可以进行时间缩放
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TimeScalableAttribute : Attribute 
    {
        public TimeScaleType Category { get; set; }
    }
}