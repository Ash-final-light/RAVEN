using Tools.CustomObject.Data;
using Tools.RecycleViewFramework.View;

namespace Tools.RecycleViewFramework.Widgets.Item
{
    public class ItemDetailsDialog : Dialog.Dialog
    {
        private ObjectDetailsView _objectDetailsView;

        public override void OnCreate(UIManager uiManager)
        {
            base.OnCreate(uiManager);

            _objectDetailsView = transform.Find("ItemDetailsView").GetComponent<ObjectDetailsView>();

            canClickBlock = true;
        }
        public void SetItemData(ObjectInfo objectInfo)
        {
            _objectDetailsView.SetItemData(objectInfo);
        }
    }
}
