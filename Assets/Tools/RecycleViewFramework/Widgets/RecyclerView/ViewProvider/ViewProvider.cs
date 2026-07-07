using System.Collections.Generic;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Layout;
using UnityEngine;

namespace Tools.RecycleViewFramework.Widgets.RecyclerView.ViewProvider
{
    /// <summary>
    /// 提供和管理 ViewHolder
    /// </summary>
    public abstract class ViewProvider
    {
        private readonly List<ViewHolder.ViewHolder> viewHolders = new();

        public IAdapter Adapter { get; set; }
        public LayoutManager LayoutManager { get; set; }

        public List<ViewHolder.ViewHolder> ViewHolders => viewHolders;

        protected RecyclerView recyclerView;
        protected ViewHolder.ViewHolder[] templates;

        public ViewProvider(RecyclerView recyclerView, ViewHolder.ViewHolder[] templates)
        {
            this.recyclerView = recyclerView;
            this.templates = templates;
        }

        public abstract ViewHolder.ViewHolder GetTemplate(string viewName);

        public abstract ViewHolder.ViewHolder[] GetTemplates();

        public abstract ViewHolder.ViewHolder Allocate(string viewName);

        public abstract void Free(string viewName, ViewHolder.ViewHolder viewHolder);

        public abstract void Reset();

        public void CreateViewHolder(int index)
        {
            for (int i = index; i < index + LayoutManager.Unit; i++)
            {
                if (i > Adapter.GetItemCount() - 1) break;

                string viewName = Adapter.GetViewName(i);
                var viewHolder = Allocate(viewName);
                viewHolder.OnStart();
                viewHolder.Name = viewName;
                viewHolder.Index = i;
                viewHolders.Add(viewHolder);

                LayoutManager.Layout(viewHolder, i);
                Adapter.OnBindViewHolder(viewHolder, i);
            }
        }

        public void RemoveViewHolder(int index)
        {
            for (int i = index; i < index + LayoutManager.Unit; i++)
            {
                if (i > Adapter.GetItemCount() - 1) break;

                int viewHolderIndex = GetViewHolderIndex(i);

                if (viewHolderIndex < 0 || viewHolderIndex >= viewHolders.Count) return;

                var viewHolder = viewHolders[viewHolderIndex];
                viewHolders.RemoveAt(viewHolderIndex);
                viewHolder.OnStop();
                Free(viewHolder.Name, viewHolder);
            }
        }

        /// <summary>
        /// 根据数据的下标获取对应的 ViewHolder
        /// </summary>
        /// <param name="index">数据的下标</param>
        /// <returns></returns>
        public ViewHolder.ViewHolder GetViewHolder(int index)
        {
            foreach (var viewHolder in viewHolders)
            {
                if (viewHolder.Index == index)
                {
                    return viewHolder;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据数据的下标获取 ViewHolder 的下标
        /// </summary>
        /// <param name="index">数据的下标</param>
        /// <returns></returns>
        public int GetViewHolderIndex(int index)
        {
            for (int i = 0; i < viewHolders.Count; i++)
            {
                if (viewHolders[i].Index == index)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Clear()
        {
            foreach (var viewHolder in viewHolders)
            {
                Free(viewHolder.Name, viewHolder);
            }
            viewHolders.Clear();
        }

        /// <summary>
        /// 计算 ViewHolder 的尺寸
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector2 CalculateViewSize(int index)
        {
            Vector2 size = GetTemplate(Adapter.GetViewName(index)).SizeDelta;
            return size;
        }

        public int GetItemCount()
        {
            return Adapter == null ? 0 : Adapter.GetItemCount();
        }
    }
}
