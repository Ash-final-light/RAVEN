using System;
using Sirenix.OdinInspector;
using Tools.Dialog.Nodes;
using Tools.Quest;
using UnityEngine;
using VContainer;

namespace Tools.Dialog.Option
{
    [Serializable]
    public class DialogGiveQuest:BaseDialogOptionAction
    {
        [Inject] private QuestManager _questManager;    
        [Title("任务ID")] public string questID = "";
        public override void Execute(OptionDialogNode optionDialogNode)
        {
            _questManager.UpdateQuestById(questID, 0);
        }
    }
}