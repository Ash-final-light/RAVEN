using UnityEngine.UIElements;

namespace Tools.BehaviourTree.Editor.View
{
    public class InspectorView:VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits>
        {
            
        }

        private UnityEditor.Editor _editor;
        public  InspectorView()
        {
            
        }
        public void UpdateView(NodeView nodeView)
        {
            Clear();
            
            UnityEngine.Object.DestroyImmediate(_editor);
            _editor = UnityEditor.Editor.CreateEditor(nodeView.node);
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                if (_editor.target)
                {
                    _editor.OnInspectorGUI();
                }
            });
            Add(container);
        }
    }
}