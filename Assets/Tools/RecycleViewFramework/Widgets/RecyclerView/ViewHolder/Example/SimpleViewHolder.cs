using TMPro;

namespace Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder.Example
{
    public sealed class SimpleViewHolder : ViewHolder
    {
        private TMP_Text simpleText;

        public override void FindUI()
        {
            simpleText = transform.Find("SimpleText").GetComponent<TMP_Text>();
        }

        public override void BindViewData<T>(T data)
        {
        
            string text = data as string;

            simpleText.text = text;
        }
    }
}
