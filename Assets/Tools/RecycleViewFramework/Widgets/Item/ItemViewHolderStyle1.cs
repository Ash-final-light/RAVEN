using Common;
using DG.Tweening;
using TMPro;
using Tools.CustomObject.Data;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tools.RecycleViewFramework.Widgets.Item
{
    public class ItemViewHolderStyle1 : ViewHolder, IPointerEnterHandler, IPointerExitHandler
    {
        private GameObject frame;
        private Image bgImage;
        private Image iconImage;
        private TMP_Text countText;

        public override void FindUI()
        {
            frame = transform.Find("Frame").gameObject;
            bgImage = transform.Find("BgImage").GetComponent<Image>();
            iconImage = transform.Find("IconImage").GetComponent<Image>();
            countText = transform.Find("CountText").GetComponent<TMP_Text>();
        }

        public override void BindViewData<T>(T data)
        {
            ObjectInfo objectInfo = data as ObjectInfo;
        
            bgImage.sprite = GameViewManager.SpriteManager.GetSprite($"Background_Item_Style_{objectInfo.Price}");
            iconImage.sprite = GameViewManager.SpriteManager.GetSprite(objectInfo.Thumbnail);
            countText.text = Random.Range(1, 10).ToString();
        }

        public override void BindChoiceState(bool state)
        {
            frame.SetActive(state);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RectTransform.DOScale(Vector3.one * 1.05f, 0.2f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            RectTransform.DOScale(Vector3.one, 0.2f);
        }
    }
}
