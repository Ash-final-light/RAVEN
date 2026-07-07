using System.Linq;
using Tools.Blackboard.Data;
using Tools.Profile.Data;
using UnityEditor;
using UnityEngine;

namespace Tools.Profile.Editor
{
    [CustomPropertyDrawer(typeof(ProfileActionValue))]
    public class ProfileActionValueDrawer : PropertyDrawer
    {
        private const float WidgetMinWidth = 60f;
        private const float WidgetSpace = 15f;
        private static readonly float SingleLineHeight = EditorGUIUtility.singleLineHeight;

        // 重写高度计算：根据不同Action类型返回对应高度
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var profileActionProperty = property.FindPropertyRelative("profileAction");
            switch (profileActionProperty.enumValueIndex)
            {
                case (int)ProfileAction.Move:
                    // Move需要两行高度（StartPos + EndPos）
                    return SingleLineHeight * 2;
                default:
                    return SingleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            // 获取所有序列化属性
            var keyProperty = property.FindPropertyRelative("key");
            var profileActionProperty = property.FindPropertyRelative("profileAction");
            var startPosProperty = property.FindPropertyRelative("startPos");
            var endPosProperty = property.FindPropertyRelative("endPos");
            var startScaleProperty = property.FindPropertyRelative("startScale");
            var endScaleProperty = property.FindPropertyRelative("endScale");
            var matchValueProperty = property.FindPropertyRelative("matchValueProperty");

            var sourceDataContainer = (property.serializedObject.targetObject as IProfilePoolProvider)?.ProfilePool;

            if (sourceDataContainer != null)
            {
                // 1. 计算基础宽度（三等分，最小不低于WidgetMinWidth）
                float totalWidth = position.width;
                float widgetWidth = Mathf.Max(totalWidth / 3, WidgetMinWidth);
                
                // ========== 第一步：绘制参数选择Popup ==========
                Rect paramPopupRect = new Rect(
                    position.x, 
                    position.y, 
                    widgetWidth, 
                    SingleLineHeight
                );

                // 查找对应参数索引
                int index = sourceDataContainer.sourceDatas.FindIndex(value => value.Name == keyProperty.stringValue);
                if (index < 0)
                {
                    if (matchValueProperty.boolValue && sourceDataContainer.sourceDatas.Count > 0)
                    {
                        keyProperty.stringValue = sourceDataContainer.sourceDatas[0].Name;
                        index = 0;
                        matchValueProperty.boolValue = true;
                    }
                }
                else
                {
                    matchValueProperty.boolValue = false;
                }

                // 绘制参数选择Popup
                if (index < 0)
                {
                    var selectContext = new string[sourceDataContainer.sourceDatas.Count + 1];
                    selectContext[0] = "";
                    for (int i = 0; i < sourceDataContainer.sourceDatas.Count; i++)
                    {
                        selectContext[i + 1] = sourceDataContainer.sourceDatas[i].Name;
                    }
                    index = EditorGUI.Popup(paramPopupRect, index + 1, selectContext) - 1;
                }
                else
                {
                    index = EditorGUI.Popup(
                        paramPopupRect, 
                        index, 
                        sourceDataContainer.sourceDatas.Select(value => value.Name).ToArray()
                    );
                }

                // 同步参数名
                if (index >= 0)
                {
                    keyProperty.stringValue = sourceDataContainer.sourceDatas[index].Name;
                }

                // ========== 第二步：绘制Action类型Popup ==========
                Rect actionPopupRect = new Rect(
                    paramPopupRect.xMax + WidgetSpace, 
                    position.y, 
                    widgetWidth, 
                    SingleLineHeight
                );

                if (index < 0)
                {
                    EditorGUI.LabelField(actionPopupRect, "no parameter found");
                }
                else
                {
                    // 绘制Action选择器
                    var actionArray = new[] { "Move", "Shake", "Hide", "Show", "PopUp", "ShutDown" };
                    profileActionProperty.enumValueIndex = EditorGUI.Popup(
                        actionPopupRect,
                        profileActionProperty.enumValueIndex,
                        actionArray
                    );

                    // ========== 第三步：绘制数值输入框 ==========
                    float valueAreaStartX = actionPopupRect.xMax + WidgetSpace;
                    float valueAreaTotalWidth = totalWidth - valueAreaStartX;

                    // 核心修复：根据Action类型动态调整y坐标
                    float currentY = position.y;

                    switch (profileActionProperty.enumValueIndex)
                    {
                        case (int)ProfileAction.Move:
                            // Move：两行显示，StartPos在第一行，EndPos在第二行
                            Rect startPosRect = new Rect(valueAreaStartX, currentY, valueAreaTotalWidth, SingleLineHeight);
                            startPosProperty.vector2Value = EditorGUI.Vector2Field(startPosRect,GUIContent.none, startPosProperty.vector2Value);

                            currentY += SingleLineHeight; // 切换到第二行
                            Rect endPosRect = new Rect(valueAreaStartX, currentY, valueAreaTotalWidth, SingleLineHeight);
                            endPosProperty.vector2Value = EditorGUI.Vector2Field(endPosRect, GUIContent.none, endPosProperty.vector2Value);
                            break;

                        case (int)ProfileAction.PopUp:
                        case (int)ProfileAction.ShutDown:
                            // PopUp/ShutDown：平分宽度，中间加间距
                            float singleFloatWidth = (valueAreaTotalWidth - WidgetSpace) / 2;
                            Rect startScaleRect = new Rect(valueAreaStartX, currentY, singleFloatWidth, SingleLineHeight);
                            Rect endScaleRect = new Rect(startScaleRect.xMax + WidgetSpace, currentY, singleFloatWidth, SingleLineHeight);
                            
                            startScaleProperty.floatValue = EditorGUI.DelayedFloatField(startScaleRect, GUIContent.none, startScaleProperty.floatValue);
                            endScaleProperty.floatValue = EditorGUI.DelayedFloatField(endScaleRect, GUIContent.none, endScaleProperty.floatValue);
                            break;

                        case (int)ProfileAction.Shake:
                        case (int)ProfileAction.Hide:
                        case (int)ProfileAction.Show:
                            break;
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, "no container found");
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}