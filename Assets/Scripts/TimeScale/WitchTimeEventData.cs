using Tools.TimeScale.Receiver;

namespace Game.TimeScale
{
    public enum WitchTimePlayPolicy
    {
        IgnoreIfActive,
        Restart,
        RefreshDuration,
        ExtendDuration
    }

    public struct WitchTimeRequest
    {
        public string commandId;
        public string sourceId;
        public string casterId;
        public float duration;
        public float worldScale;
        public float selfScale;
        public TimeScaleType affectedType;
        public WitchTimePlayPolicy playPolicy;

        public bool IsValid => !string.IsNullOrEmpty(casterId) && duration > 0f;
    }

    public readonly struct WitchTimeRuntimeInfo
    {
        public readonly string commandId;
        public readonly string sourceId;
        public readonly string casterId;
        public readonly float duration;
        public readonly float remainingTime;
        public readonly float progress;

        public WitchTimeRuntimeInfo(
            string commandId,
            string sourceId,
            string casterId,
            float duration,
            float remainingTime)
        {
            this.commandId = commandId;
            this.sourceId = sourceId;
            this.casterId = casterId;
            this.duration = duration;
            this.remainingTime = remainingTime;
            progress = duration > 0f ? 1f - remainingTime / duration : 1f;
        }
    }
}
