namespace Tools.Action.Running
{
    public interface IActionClipPlay
    {
        int CurrentTick { get; }
        float CurrentTime { get; }
        void Play();
        void Stop();
        void Tick(float deltaTime);
    }
}
