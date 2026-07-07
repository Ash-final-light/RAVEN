using System.Collections.Generic;
using Tools.CustomObject.Data;
using Tools.RecycleViewFramework.View;
using UnityEngine.UI;

namespace Tools.RecycleViewFramework.Widgets.Item
{
    public class ItemGetView : View.View
    {
        private ItemListView itemListView;
        private Button maskBtn;

        public override void OnCreate(UIManager uiManager)
        {
            base.OnCreate(uiManager);

            itemListView = transform.Find("ItemListView").GetComponent<ItemListView>();
            maskBtn = transform.Find("MaskBtn").GetComponent<Button>();

            maskBtn.onClick.AddListener(() =>
            {
                Close();
            });
        }

        public void SetItemList(List<ObjectInfo> itemDatas)
        {
            itemListView.SetItemList(itemDatas);
        }
    }
}
