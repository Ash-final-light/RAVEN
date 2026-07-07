namespace Tools.Camera
{
    public interface ICameraModel
    {
        CameraSceneData GetScene();
        CameraLockData GetLock();
    }
}