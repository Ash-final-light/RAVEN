using UI.UIAction.SkillBallUI;
using UnityEditor;
using UnityEngine;

namespace UI.UIAction.SkillBallUI.Editor
{
    [CustomEditor(typeof(ActionUISkillBallDebugDriver))]
    public class ActionUISkillBallDebugDriverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Skill Ball Demo", EditorStyles.boldLabel);

            ActionUISkillBallDebugDriver driver = (ActionUISkillBallDebugDriver)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Bind"))
                {
                    driver.Bind();
                }

                if (GUILayout.Button("Reset"))
                {
                    driver.ResetTestBalls();
                }
            }

            if (GUILayout.Button("Add Configured Balls"))
            {
                driver.AddConfiguredBalls();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Consume Match At Index"))
                {
                    driver.ConsumeConfigured();
                }

                if (GUILayout.Button("Consume First 3"))
                {
                    driver.ConsumeFirstThree();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Matrix On"))
                {
                    driver.MatrixOn();
                }

                if (GUILayout.Button("Matrix Off"))
                {
                    driver.MatrixOff();
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(driver);
            }
        }
    }
}
