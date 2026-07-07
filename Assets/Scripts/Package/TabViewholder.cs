using Common;
using Tools.CustomObject.Data;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;

namespace Package
{
    using NaiQiu.Framework.View;
    using UnityEngine.UI;

    public class TabViewHolder : ViewHolder
    {
        private Image iconImage;
        private Image indicatorImage;

        private BagData bagData;

        public override void FindUI()
        {
            iconImage = transform.Find("IconImage").GetComponent<Image>();
            indicatorImage = transform.Find("IndicatorImage").GetComponent<Image>();
        }

        public override void BindViewData<T>(T data)
        {
            bagData = data as BagData;
            if (bagData == null)
                return;
            iconImage.sprite = GameViewManager.SpriteManager.GetSprite($"{bagData.icon}_Unselected");
            UpdateReddotState(bagData);
        }

        public override void BindChoiceState(bool state)
        {
            indicatorImage.gameObject.SetActive(state);
            string icon = state ? $"{bagData.icon}_Selected" : $"{bagData.icon}_Unselected";
            iconImage.sprite = GameViewManager.SpriteManager.GetSprite(icon);
        }

        public override void UpdateReddotState<T>(T data)
        {
            Debug.Log($"UpdateReddotState type = {data?.GetType()}");
            BagData bagData = data as BagData;
            if (bagData == null)
            {
                Debug.LogError($"UpdateReddotState() {bagData.id} bagData is null");
                return;
            }
                
            var reddotNode = GameViewManager.ReddotTree.GetReddotNode(bagData.id);
            if (reddotNode == null)
            {
                Debug.LogError($"UpdateReddotState() {bagData.id} reddotNode is null");
                return;
            }
                
            Debug.Log($"UpdateReddotState() {bagData.id} {reddotNode.ShowCount} {reddotNode.ReddotCount}");
        }
    }

}