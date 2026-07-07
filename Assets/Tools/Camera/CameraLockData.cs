using System;
using JetBrains.Annotations;
using Tools.InputHandle;

namespace Tools.Camera
{
    [Serializable]
    public struct CameraLockData
    {
        public bool @lock;
        [CanBeNull] public ActionStateController lockTarget;
    }
}