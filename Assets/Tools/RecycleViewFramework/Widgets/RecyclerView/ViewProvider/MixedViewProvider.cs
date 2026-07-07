using System;
using System.Collections.Generic;
using System.Linq;
using Tools.RecycleViewFramework.ObjectPool;
using UnityEngine;

namespace Tools.RecycleViewFramework.Widgets.RecyclerView.ViewProvider
{
    public class MixedViewProvider : ViewProvider
    {
        [SerializeField] private ViewHolder.ViewHolder chatLeftViewHolder;
        [SerializeField] private ViewHolder.ViewHolder chatRightViewHolder;

        private IMixedObjectPool<ViewHolder.ViewHolder> objectPool;
        private Dictionary<string, ViewHolder.ViewHolder> dict = new();

        public MixedViewProvider(RecyclerView recyclerView, ViewHolder.ViewHolder[] templates) : base(recyclerView, templates)
        {
            foreach (var template in templates)
            {
                dict[template.name] = template;
            }

            UnityMixedComponentFactory<ViewHolder.ViewHolder> factory = new(dict, recyclerView.Content);
            objectPool = new MixedObjectPool<ViewHolder.ViewHolder>(factory);
        }

        public override ViewHolder.ViewHolder GetTemplate(string viewName)
        {
            if (templates == null || templates.Length == 0)
            {
                throw new NullReferenceException("ViewProvider templates can not null or empty.");
            }
            return dict[viewName];
        }

        public override ViewHolder.ViewHolder[] GetTemplates()
        {
            if (templates == null || templates.Length == 0)
            {
                throw new NullReferenceException("ViewProvider templates can not null or empty.");
            }
            return dict.Values.ToArray();
        }

        public override ViewHolder.ViewHolder Allocate(string viewName)
        {
            var viewHolder = objectPool.Allocate(viewName);
            viewHolder.gameObject.SetActive(true);
            return viewHolder;
        }

        public override void Free(string viewName, ViewHolder.ViewHolder viewHolder)
        {
            objectPool.Free(viewName, viewHolder);
        }

        public override void Reset()
        {
            Clear();
            objectPool.Dispose();
        }
    }
}
