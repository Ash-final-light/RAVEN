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
    public class QuestItemHolder:ViewHolder ,IPointerEnterHandler, IPointerExitHandler
    {
        private TMP_Text stepTitleText;
        private string viewTemplate;
        
        public override void FindUI()
        {
            stepTitleText = transform.Find("StepTitle").GetComponent<TMP_Text>();
            
        }

        public override void BindViewData<T>(T data)
        {
            if (data is QuestViewData questViewData)
            {
                stepTitleText.text = questViewData.title;
                viewTemplate = questViewData.templateName;
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (viewTemplate == "QuestItemHolder")
            {
                RectTransform.DOScale(Vector3.one * 1.1f, 0.2f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (viewTemplate == "QuestItemHolder")
            {
                RectTransform.DOScale(Vector3.one, 0.2f);
            }
        }
    }
}