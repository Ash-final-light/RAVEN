using Tools.Action.Data;

namespace Tools.Action.Editor
{
    public enum ActionStage
    {
        Idle,
        Start,
        Anticipation,
        Judgment,
        Recovery,
        End,
        Stop,
    }
    public interface IActionClipPlay
    {
        int CurrentTick { get; }
        float CurrentTime { get; }
        
        void Start();
        void Tick(float deltaTime);
        void Stop();
        void RegisterEventListener(System.Action<string, ActionEventParameter, object> listener);
        void UnregisterEventListener(System.Action<string, ActionEventParameter, object> listener);
#if UNITY_EDITOR
        public void ChangeActionClip(ActionClip actionClip);
        public void StartAt(int tick);
        public void PlayAt(int tick);
#endif
    }
}