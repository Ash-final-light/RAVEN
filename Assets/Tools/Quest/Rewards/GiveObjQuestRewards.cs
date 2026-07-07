using System;
using System.Collections.Generic;
using Package;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Tools.Quest
{
    public class GiveObjQuestRewards:BaseQuestRewards
    {
        [Inject] PackageObjectManager packageObjectManager;
        [Title("物品奖励")]
        public List<GiveObjQuestRewardsData> givenObjectdata = new();
        public override void GiveReward()
        {
            Debug.Log("GiveObjQuestRewards");
            for (int i = 0; i < givenObjectdata.Count; i++)
            {
                packageObjectManager.AddObjectById(givenObjectdata[i].id, givenObjectdata[i].num);
            }
        }
    }
    [Serializable]
    public struct GiveObjQuestRewardsData
    {
        public int id;
        public int num;
    }
}