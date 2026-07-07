using Common;
using DG.Tweening;
using NaiQiu.Framework.View;
using TMPro;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Panel
{
    public class OpenViewHolder : ViewHolder,IPointerEnterHandler,IPointerExitHandler
    {
        private Image viewHolder;
        private TMP_Text nameText;

        private string spriteName;
        private string atlasName;
        
        public override void FindUI()
        {
            viewHolder = transform.GetComponent<Image>();
            nameText = transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        }

        public override void BindViewData<T>(T data)
        {
            var openViewHolderData = data as OpenViewHolderData;
            if (openViewHolderData == null)
                return;
            viewHolder.sprite = GameViewManager.SpriteManager.GetSprite(openViewHolderData.atlasName,$"{openViewHolderData.spriteName}_Unselected");
            nameText.text = openViewHolderData.name;
            spriteName = openViewHolderData.spriteName;
            atlasName = openViewHolderData.atlasName;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            viewHolder.sprite = GameViewManager.SpriteManager.GetSprite(atlasName,$"{spriteName}_Selected");
            RectTransform.DOScale(Vector3.one * 1.1f, 0.3f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            viewHolder.sprite = GameViewManager.SpriteManager.GetSprite(atlasName,$"{spriteName}_Unselected");
            RectTransform.DOScale(Vector3.one, 0.3f);
        }
    }
}