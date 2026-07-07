
using UnityEngine.UIElements;

namespace Tools.Editor
{
    public class SplitView:TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits>
        {
            
        }
    }
}