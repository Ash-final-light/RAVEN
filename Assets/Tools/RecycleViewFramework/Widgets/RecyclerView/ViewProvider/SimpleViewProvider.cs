using System;
using Tools.RecycleViewFramework.ObjectPool;

namespace Tools.RecycleViewFramework.Widgets.RecyclerView.ViewProvider
{
    public sealed class SimpleViewProvider : ViewProvider
    {
        private readonly IObjectPool<ViewHolder.ViewHolder> objectPool;

        public SimpleViewProvider(RecyclerView recyclerView, ViewHolder.ViewHolder[] templates) : base(recyclerView, templates)
        {
            UnityComponentFactory<ViewHolder.ViewHolder> factory = new(GetTemplate(), recyclerView.Content);
            objectPool = new ObjectPool<ViewHolder.ViewHolder>(factory, 100);
        }

        public override ViewHolder.ViewHolder GetTemplate(string viewName = "")
        {
            if (templates == null || templates.Length == 0)
            {
                throw new NullReferenceException("ViewProvider templates can not null or empty.");
            }
            return templates[0];
        }

        public override ViewHolder.ViewHolder[] GetTemplates()
        {
            if (templates == null || templates.Length == 0)
            {
                throw new NullReferenceException("ViewProvider templates can not null or empty.");
            }
            return templates;
        }

        public override ViewHolder.ViewHolder Allocate(string viewName)
        {
            var viewHolder = objectPool.Allocate();
            viewHolder.gameObject.SetActive(true);
            return viewHolder;
        }

        public override void Free(string viewName, ViewHolder.ViewHolder viewHolder)
        {
            objectPool.Free(viewHolder);
        }

        public override void Reset()
        {
            Clear();
            objectPool.Dispose();
        }
    }
}
