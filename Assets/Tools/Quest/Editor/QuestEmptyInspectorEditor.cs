using Tools.Quest.Data;
using UnityEditor;

namespace Tools.Quest.Editor
{
    [CustomEditor(typeof(QuestEmptyInspector))]
    public class QuestEmptyInspectorEditor:UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("No Quest Selected", MessageType.Info);
        }
    }
}