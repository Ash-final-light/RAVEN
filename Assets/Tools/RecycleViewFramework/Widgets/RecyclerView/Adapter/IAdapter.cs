namespace Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter
{
    public interface IAdapter
    {
        int GetItemCount();

        int GetRealCount();

        string GetViewName(int index);

        void OnBindViewHolder(ViewHolder.ViewHolder viewHolder, int index);

        void NotifyDataChanged();
    }
}
