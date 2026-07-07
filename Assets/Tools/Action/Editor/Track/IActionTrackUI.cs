using UnityEngine.UIElements;

namespace Tools.Action.Editor.Track
{
    public interface IActionTrackUI
    {
        // UI属性
        VisualElement Root { get; }
        VisualElement Parent { get; }
        ActionTrackEditorUIData Data { get; }
        
        // UI类生命周期函数
        void CreateView(VisualElement parent);
        void BindView(ActionTrackEditorUIData data);
        void DestroyView();

        // UI类辅助函数
        void RefreshView();
    }
}