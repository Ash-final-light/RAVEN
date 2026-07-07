using System;
using System.Collections.Generic;
using NaiQiu.Framework.View;
using Tools.RecycleViewFramework.Widgets.RecyclerView;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter;
using Tools.RecycleViewFramework.Widgets.RecyclerView.ViewHolder;
using UnityEngine;

namespace Quest
{
    public class QuestListAdapater : Adapter<QuestViewData>
    {
        private readonly List<QuestViewData> showList = new();
        private string templateName;

        public QuestListAdapater(RecyclerView recyclerView, string templateName) : base(recyclerView)
        {
            this.templateName = templateName;
        }

        public QuestListAdapater(RecyclerView recyclerView, List<QuestViewData> list) : base(recyclerView, list)
        {
        }

        public QuestListAdapater(RecyclerView recyclerView, List<QuestViewData> list, Action<QuestViewData> onItemClick)
            : base(recyclerView, list, onItemClick)
        {
        }

        public override int GetItemCount()
        {
            return showList.Count;
        }

        public override string GetViewName(int index)
        {
            if (index < 0 || index >= showList.Count)
            {
                return templateName;
            }

            return showList[index].templateName;
        }

        public override void OnBindViewHolder(ViewHolder viewHolder, int index)
        {
            if (index < 0 || index >= GetItemCount()) return;

            var data = showList[index];

            viewHolder.BindViewData(data);

            viewHolder.BindItemClick(data, t =>
            {
                if (data.templateName == templateName)
                {
                    data.isExpand = !data.isExpand;
                    NotifyDataChanged();
                }
                else
                {
                    SetChoiceIndex(index);
                    onItemClick?.Invoke(data);
                }
            });
        }

        public override void SetList(List<QuestViewData> list)
        {
            this.list = list;

            BuildShowList();

            recyclerView.Reset();
            NotifyDataChanged();
        }

        public override void NotifyDataChanged()
        {
            BuildShowList();

            recyclerView.RequestLayout();
            recyclerView.Refresh();
        }

        private void BuildShowList()
        {
            showList.Clear();

            foreach (var data in list)
            {
                if (data.templateName == templateName)
                {
                    showList.Add(data);

                    if (data.isExpand)
                    {
                        AddChildren(data.questId);
                    }
                }
            }
        }

        private void AddChildren(string questId)
        {
            foreach (var data in list)
            {
                if (data.questId == questId && data.templateName != templateName)
                {
                    showList.Add(data);
                }
            }
        }
    }
}