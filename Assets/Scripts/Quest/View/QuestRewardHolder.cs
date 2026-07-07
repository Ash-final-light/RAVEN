using Common;
using NaiQiu.Framework.View;
using TMPro;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;
using UnityEngine.UI;

namespace Quest
{
    public class QuestRewardHolder:ViewHolder
    {
        private Image rewardIcon;
        private Image rewardPriceBg;
        private TMP_Text rewardCountText;
        
        public override void FindUI()
        {
            rewardIcon = transform.Find("RewardIcon").GetComponent<Image>();
            rewardPriceBg = transform.Find("RewardPriceBg").GetComponent<Image>();
            rewardCountText = transform.Find("RewardCountText").GetComponent<TMP_Text>();
        }

        public override void BindViewData<T>(T data)
        {
            var rewardData = data as QuestRewardData;
            if(rewardData == null)
                return;
            rewardIcon.sprite = GameViewManager.SpriteManager.GetSprite(rewardData.thumbnail);
            rewardPriceBg.sprite = GameViewManager.SpriteManager.GetSprite("ItemIconAtlas",$"Background_Item_{rewardData.price}_Star");
            rewardCountText.text = rewardData.count.ToString();
        }
    }
}