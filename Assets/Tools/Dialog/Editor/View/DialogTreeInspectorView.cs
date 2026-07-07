using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tools.Dialog.Editor.View
{
    public class DialogTreeInspectorView:VisualElement
    {
        public new class UxmlFactory : UxmlFactory<DialogTreeInspectorView, GraphView.UxmlTraits> { }
        private UnityEditor.Editor _nodeEditor;
        internal void HandleNodeSelected(DialogNodeView nodeView)
        {
            ClearNode();
            // 使用节点Editor渲染内容
            var scrollView = new ScrollView();
            _nodeEditor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            var container = new IMGUIContainer(() =>
            {
                // EditorGUILayout.ObjectField(nodeView.Node, typeof(Node.Node));
                if (_nodeEditor != null && _nodeEditor.target)
                {
                    _nodeEditor.OnInspectorGUI();
                }
            });
            scrollView.Add(container);
            Add(scrollView);
        }

        internal void HandleNodeUnselected(DialogNodeView nodeView)
        {
            if (_nodeEditor.target == nodeView.Node)
            {
                ClearNode();
            }
        }

        private void ClearNode()
        {
            Clear();
            if (_nodeEditor)
            {
                GameObject.DestroyImmediate(_nodeEditor);
                _nodeEditor = null;
            }
        }
    }
}