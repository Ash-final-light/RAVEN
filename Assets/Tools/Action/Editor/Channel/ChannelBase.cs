using Tools.Action.Editor.Track;
using UnityEngine.UIElements;

namespace Tools.Action.Editor.Channel
{
    public class ChannelBase
    {
        public string channelName;
        public ActionTrackType trackType;
        
        public ActionTrackUI trackUI;
        public ChannelBase(string channelName, ActionTrackType trackType,VisualElement trackParent,ActionTrackEditorUIData data)
        {
            this.channelName = channelName;
            this.trackType = trackType;
            trackUI = new ActionTrackUI();
            trackUI.CreateView(trackParent);
            trackUI.BindView(data);
            ActionEditorWindow.Instance.OnTracksRefreshed += RefreshUI;
        }
        ~ChannelBase()
        {
            ActionEditorWindow.Instance.OnTracksRefreshed -= RefreshUI;
        }
        private void RefreshUI()
        {
            trackUI.RefreshView();
        }
    }
}