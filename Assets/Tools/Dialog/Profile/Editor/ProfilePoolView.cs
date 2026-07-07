using Tools.Profile.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tools.Dialog.Editor.View
{
    public class ProfilePoolView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ProfilePoolView, VisualElement.UxmlTraits>
        {
        }

        private UnityEditor.Editor _blackboardEditor;

        public ProfilePoolView()
        {
            style.flexGrow = 1;          // 占满父容器剩余空间
            style.width = Length.Percent(100); // 宽度100%
            style.height = Length.Percent(100); // 高度100%
            style.backgroundColor = new Color(0.22f, 0.22f, 0.22f); // 适配编辑器深色主题
            
        }
        internal void UpdateView(ProfilePool profilePool)
        {
            Clear();

            if (_blackboardEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(_blackboardEditor);
                _blackboardEditor = null;
            }

            if (profilePool == null)
                return;

            var scrollView = new ScrollView
            {
                style = { flexGrow = 1 }
            };

            _blackboardEditor = UnityEditor.Editor.CreateEditor(profilePool);

            var inspector = _blackboardEditor.CreateInspectorGUI();

            if (inspector != null)
            {
                scrollView.Add(inspector);
            }
            else
            {
                scrollView.Add(new IMGUIContainer(() =>
                {
                    _blackboardEditor?.OnInspectorGUI();
                }));
            }

            Add(scrollView);
        }
    }
}