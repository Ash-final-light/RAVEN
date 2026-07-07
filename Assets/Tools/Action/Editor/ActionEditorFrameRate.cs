using System;
using System.Collections.Generic;
using Animancer;
using Animancer.TransitionLibraries;
using Sirenix.OdinInspector;
using Tools.Audio;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Tools.Action.Editor
{
    public enum ActionEditorFrameRate : int
    {
        FPS24 = 24,
        FPS30 = 30,
        FPS60 = 60,
    }
}