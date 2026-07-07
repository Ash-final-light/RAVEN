using System.Collections.Generic;
using NaiQiu.Framework.View;
using Sirenix.OdinInspector;
using TMPro;
using Tools.RecycleViewFramework.Widgets.RecyclerView;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Adapter;
using Tools.RecycleViewFramework.Widgets.RecyclerView.Layout;
using UnityEngine;

namespace Quest
{
    public class QuestRewardDetailsView:MonoBehaviour
    {
        private RecyclerView rewardView;
        private Adapter<QuestRewardData> rewardAdapter;
        [ShowInInspector]private List<QuestRewardData> rewardList = new ();

        public void FindUI()
        {
            rewardView = transform.Find("TaskList").GetComponent<RecyclerView>();
            rewardAdapter = new Adapter<QuestRewardData>(rewardView);
            rewardView.SetLayoutManager(new LinearLayoutManager());
            rewardAdapter.SetList(rewardList);
        }

        public void SetQuestDetails(List<QuestRewardData> questData)
        {
            rewardList.Clear(); // 清空旧数据
            if (questData != null)
            {
                rewardList.AddRange(questData); // 添加新数据
            }
            rewardAdapter.NotifyDataChanged();
        }
    }
    
}