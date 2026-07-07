using System.Linq;
using Tools.Blackboard.Data;
using UnityEditor;
using UnityEngine;

namespace Tools.Blackboard.Editor.View
{
    [CustomPropertyDrawer(typeof(BlackboardCondition))]
    public class BlackboardConditionView : PropertyDrawer
    {
        private float WidgetMinWidth = 60f;
        private float WidgetSpace = 15f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            
            var keyProperty = property.FindPropertyRelative("key");
            var dataTypeProperty = property.FindPropertyRelative("dataType");
            var compareTypeProperty = property.FindPropertyRelative("compareType");
            var boolValueProperty = property.FindPropertyRelative("boolValue");
            var intValueProperty = property.FindPropertyRelative("intValue");
            var floatValueProperty = property.FindPropertyRelative("floatValue");
            var stringValueProperty = property.FindPropertyRelative("stringValue");
            var matchValueProperty = property.FindPropertyRelative("matchValueProperty");
            
            var sourceDataContainer = (property.serializedObject.targetObject as IBlackBoardProvider)?.Blackboard;
            
            
            if (sourceDataContainer != null)
            {
                var widgetWidth = Mathf.Max(position.width / 3, WidgetMinWidth);
                // 遍历sourceDatas数组，绘制每个SourceData的属性
                
                // 查找是否有对应参数
                int index = sourceDataContainer.sourceDatas.FindIndex(sourceData => sourceData.key == keyProperty.stringValue);
                if (index == -1)
                {
                    // 参数已经消失，使用第一个默认的有效参数
                    if (matchValueProperty.boolValue)
                    {
                        if (sourceDataContainer.sourceDatas.Count > 0)
                        {
                            keyProperty.stringValue = sourceDataContainer.sourceDatas[0].key;
                            dataTypeProperty.enumValueIndex = (int)sourceDataContainer.sourceDatas[0].dataType;
                            index = 0;
                            matchValueProperty.boolValue = true;
                        }
                    }

                }
                else
                {
                    matchValueProperty.boolValue = false;
                }
                
                // 绘制参数选择ui
                var recTrans = new Rect(position.x, position.y, widgetWidth, position.height);
                if (index < 0)
                {
                    index++;
                    var selectContex = new string[sourceDataContainer.sourceDatas.Count + 1];
                    selectContex[0] = "";
                    for (int i = 0; i < sourceDataContainer.sourceDatas.Count; i++)
                    {
                        selectContex[i + 1] = sourceDataContainer.sourceDatas[i].key;
                    }
                    index = EditorGUI.Popup(recTrans, index, selectContex) - 1;
                }
                else
                {   
                   index = EditorGUI.Popup(recTrans, index, sourceDataContainer.sourceDatas.Select
                        (value => value.key).ToArray());
                }
                
                //如果参数有效，同步数据
                if (index >= 0)
                {
                    keyProperty.stringValue = sourceDataContainer.sourceDatas[index].key;
                    dataTypeProperty.enumValueIndex = (int)sourceDataContainer.sourceDatas[index].dataType;
                }
                
                position.x +=  WidgetSpace + widgetWidth;
                if(index < 0)
                {
                    EditorGUI.LabelField(position, "no parameter found");
                }
                else
                {
                    widgetWidth =  Mathf.Max(position.width / 3 - WidgetSpace, WidgetMinWidth);
                    var variable = sourceDataContainer.sourceDatas[index];
                    if(variable.dataType == DataType.Bool)
                    {
                        // 绘制条件值UI
                        position.x += widgetWidth + WidgetSpace;
                        var conditionRect = new Rect(position.x, position.y, widgetWidth, position.height);
                        boolValueProperty.boolValue =
                            EditorGUI.Toggle(conditionRect, boolValueProperty.boolValue);
                    }
                    else {
                        //绘制选择器
                        var comprareRect = new Rect(position.x, position.y, widgetWidth, position.height);
                        if (variable.dataType == DataType.String)
                        {
                            // 不符合规定的比较类型
                            if (compareTypeProperty.enumValueIndex != (int)CompareEnum.Equal && 
                                compareTypeProperty.enumValueIndex != (int)CompareEnum.NotEqual)
                            {
                                compareTypeProperty.enumValueIndex = (int)CompareEnum.Equal;
                            }
                            var comparisonArray = new[]
                            {
                                "Equal",
                                "NotEqual",
                            };
                            compareTypeProperty.enumValueIndex = 
                                EditorGUI.Popup(comprareRect, 
                                    compareTypeProperty.enumValueIndex - (int)CompareEnum.Equal, 
                                    comparisonArray)
                                + (int)CompareEnum.Equal;
                        }
                        else
                        {
                            var comparisonArray = new[]
                            {
                                "Greater",
                                "Less",
                                "Equal",
                                "NotEqual",
                            };
                            compareTypeProperty.enumValueIndex = 
                                EditorGUI.Popup(comprareRect, 
                                    compareTypeProperty.enumValueIndex, comparisonArray);
                        }
                        
                        //绘制数值ui
                        position.x += widgetWidth + WidgetSpace;
                        var valueRect = new Rect(position.x, position.y, widgetWidth, position.height);
                        switch (variable.dataType)
                        {
                            case DataType.String:
                                stringValueProperty.stringValue = 
                                    EditorGUI.DelayedTextField(valueRect, stringValueProperty.stringValue);
                                break;
                            case DataType.Int:
                                intValueProperty.intValue =
                                    EditorGUI.DelayedIntField(valueRect, intValueProperty.intValue);
                                break;
                            case DataType.Float:
                                floatValueProperty.floatValue =
                                    EditorGUI.DelayedFloatField(valueRect, floatValueProperty.floatValue);
                                break;
                        }
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position,"no container found");
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            // 绘制比较类型
            
            EditorGUI.EndProperty();
        }
    }
}