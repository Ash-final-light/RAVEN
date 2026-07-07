using Common;
using NaiQiu.Framework.View;
using Package;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;
using UnityEngine.UI;

namespace Quest
{
    public class TypeViewHolder:ViewHolder
    {
        private Image iconImage;
        private Image indicatorImage;

        private QuestTypeData bagData;

        public override void FindUI()
        {
            iconImage = transform.Find("IconImage").GetComponent<Image>();
            indicatorImage = transform.Find("SelectImage").GetComponent<Image>();
        }

        public override void BindViewData<T>(T data)
        {
            bagData = data as QuestTypeData;
            if (bagData == null)
                return;
            iconImage.sprite = GameViewManager.SpriteManager.GetSprite(bagData.atlasName,$"{bagData.spriteName}_Unselected");
        }

        public override void BindChoiceState(bool state)
        {
            indicatorImage.gameObject.SetActive(state);
            string icon = state ? $"{bagData.spriteName}_Selected" : $"{bagData.spriteName}_Unselected";
            iconImage.sprite = GameViewManager.SpriteManager.GetSprite(bagData.atlasName,icon);
        }
    }
}