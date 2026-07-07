using DG.Tweening;
using NaiQiu.Framework.View;
using TMPro;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Quest
{
    /// <summary>
    /// 不要改名
    /// </summary>
    public class QuestGroupHolder:ViewHolder,IPointerEnterHandler,IPointerExitHandler
    {
        private TMP_Text questTitleText;
        
        public override void FindUI()
        {
            questTitleText = transform.Find("QuestTitle").GetComponent<TMP_Text>();
        }

        public override void BindViewData<T>(T data)
        {
            if (data is QuestViewData questViewData)
            {
                questTitleText.text = questViewData.title;
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            RectTransform.DOScale(Vector3.one * 1.1f, 0.2f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            RectTransform.DOScale(Vector3.one, 0.2f);
        }
    }
}