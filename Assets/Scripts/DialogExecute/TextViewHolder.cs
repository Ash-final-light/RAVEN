using NaiQiu.Framework.View;
using TMPro;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;

namespace DialogExecute
{
    public class TextViewHolder:ViewHolder
    {
        private TMP_Text nameText;
        private TMP_Text contentText;
        public override void FindUI()
        {
            nameText = transform.Find("NameText").GetComponent<TMP_Text>();
            contentText = transform.Find("ContentText").GetComponent<TMP_Text>();
        }

        public override void BindViewData<T>(T data)
        {
            if (data is DialogText dialogText)
            {
                nameText.text = dialogText.Speaker;
                contentText.text = dialogText.Content;
            }
        }
    }
    public class DialogText
    {
        public string Speaker;
        public string Content;
    }
}