using Common;
using DG.Tweening;
using TMPro;
using Tools.CustomObject.Data;
using Tools.RecycleViewFramework.Load;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NaiQiu.Framework.View
{
    public class ObjectViewHolder : ViewHolder, IPointerEnterHandler, IPointerExitHandler
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
            ObjectGroup objectGroup = data as ObjectGroup;

            if(objectGroup == null)
                return;
            
            bgImage.sprite = GameViewManager.SpriteManager.GetSprite("ItemIconAtlas",$"Background_Item_{objectGroup.Data.Price}_Star");
            iconImage.sprite = GameViewManager.SpriteManager.GetSprite(objectGroup.Data.ThumbnailAtlas, objectGroup.Data.ThumbnailName);
            countText.text = objectGroup.Number.ToString();
            
            UpdateReddotState(data);
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
        public override void UpdateReddotState<T>(T data)
        {
            Debug.Log($"UpdateReddotState type = {data?.GetType()}");
            ObjectGroup objectGroup = data as ObjectGroup;
            if (objectGroup == null)
            {
                Debug.LogError($"UpdateReddotState() {objectGroup.GroupId} objectGroup is null");
                return;
            }
                
            var reddotNode = GameViewManager.ReddotTree.GetReddotNode(objectGroup.GroupId);
            if (reddotNode == null)
            {
                Debug.LogError($"UpdateReddotState() {objectGroup.GroupId} reddotNode is null");
                return;
            }
                
            Debug.Log($"UpdateReddotState() {objectGroup.GroupId} {reddotNode.ShowCount} {reddotNode.ReddotCount}");
        }
    }
}
