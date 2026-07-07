using UnityEditor.Graphs;
using UnityEngine.UIElements;

namespace Tools.Action.Editor.Track
{
    public struct ActionTrackEditorUIData
    {
        public string Name;
        public float StartFrame;
        public float DurationFrames;
        public StyleColor NormalColor;
        public StyleColor SelectedColor;
    }
}