using System;
using System.Collections.Generic;
using Package;
using Sirenix.OdinInspector;
using Tools.Quest;
using UnityEngine;
using VContainer;

namespace Tools.Dialog.Event
{
    [Serializable]
    public class DialogGiveObjectEvent : BaseDialogEvent
    {
        [Inject] private PackageObjectManager packageObjectManager;
        [Title("添加物品列表")] 
        public bool giveWhenEnter = true;
        [ShowIf("giveWhenEnter")]
        public List<GiveObjQuestRewardsData> givenObjectdata = new();
        public override void OnEnter()
        {
            Debug.Log($"OnEnter,giveobj");
            if (giveWhenEnter)
                givenObjectdata.ForEach(item => packageObjectManager.AddObjectById(item.id,item.num));
        }

        public override void OnExit()
        {
            Debug.Log($"OnExit,giveobj");
            if (!giveWhenEnter)
                givenObjectdata.ForEach(item => packageObjectManager.AddObjectById(item.id,item.num));
        }
    }
}