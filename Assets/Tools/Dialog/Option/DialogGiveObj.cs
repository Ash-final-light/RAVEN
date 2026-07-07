using System.Collections.Generic;
using System.Runtime.Serialization;
using Package;
using Sirenix.OdinInspector;
using Tools.Dialog.Nodes;
using Tools.Quest;
using VContainer;

namespace Tools.Dialog.Option
{
    public class DialogGiveObj:BaseDialogOptionAction
    {
        [Inject] private PackageObjectManager _objectManager;  
        [Title("物品ID")]public List<GiveObjQuestRewardsData> giveObjQuestRewardsDatas = new();
        public override void Execute(OptionDialogNode optionDialogNode)
        {
            foreach(GiveObjQuestRewardsData data in giveObjQuestRewardsDatas)
            {
                _objectManager.AddObjectById(data.id,data.num);
            }
        }
    }
}