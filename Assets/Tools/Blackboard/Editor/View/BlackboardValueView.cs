using Tools.Blackboard.Data;
using UnityEditor;
using UnityEngine;

namespace Tools.Blackboard.Editor.View
{
    [CustomPropertyDrawer(typeof(BlackboardValue))]
    public class BlackboardValueView : PropertyDrawer
    {
        private float WidgetMinWidth = 60f;
        private float WidgetSpace = 15f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            
            var key = property.FindPropertyRelative("key");
            var dataType = property.FindPropertyRelative("dataType");
            var boolValue = property.FindPropertyRelative("boolValue");
            var intValue = property.FindPropertyRelative("intValue");
            var floatValue = property.FindPropertyRelative("floatValue");
            var stringValue = property.FindPropertyRelative("stringValue");
            
            // 绘制key
            var keyRect = new Rect(position.x, position.y, WidgetMinWidth, position.height);
            EditorGUI.PropertyField(keyRect, key, GUIContent.none);
            
            // 根据dataType绘制对应的值
            var dataTypeRect = new Rect(keyRect.xMax + WidgetSpace, position.y, WidgetMinWidth, position.height);
            
            switch (dataType.enumValueIndex)
            {
                case (int)DataType.Bool:
                    boolValue.boolValue =
                        EditorGUI.Toggle(dataTypeRect, boolValue.boolValue);
                    break;
                case (int)DataType.Int:
                    intValue.intValue =
                        EditorGUI.DelayedIntField(dataTypeRect, intValue.intValue);
                    break;
                case (int)DataType.Float:
                    floatValue.floatValue =
                        EditorGUI.DelayedFloatField(dataTypeRect, floatValue.floatValue);
                    break;
                case (int)DataType.String:
                    stringValue.stringValue = 
                        EditorGUI.DelayedTextField(dataTypeRect, stringValue.stringValue);
                    break;
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.EndProperty();
        }
    }
}