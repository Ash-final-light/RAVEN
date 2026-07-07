using UnityEngine;
using UnityEngine.UIElements;

namespace Tools.Blackboard.Editor.View
{
    public class BlackboardView: VisualElement
    {
        public new class UxmlFactory : UxmlFactory<BlackboardView, VisualElement.UxmlTraits>
        {
            
        }

        private UnityEditor.Editor _editor;
        
        public void UpdateView(Data.Blackboard blackboard)
        {
            // 清除UI内容
            Clear();
            if (_editor)
            {
                GameObject.DestroyImmediate(_editor);
                _editor = null;
            }

            if (!blackboard)
            {
                return;
            }

            // 使用黑板Editor渲染内容
            var scrollView = new ScrollView();
            _editor = UnityEditor.Editor.CreateEditor(blackboard);
            scrollView.Add(_editor.CreateInspectorGUI());
            Add(scrollView);
        }
    }
}