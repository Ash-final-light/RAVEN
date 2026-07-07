using System;
using Sirenix.OdinInspector;
using Tools.Dialog.Nodes;
using UnityEngine;

namespace Tools.Dialog.Option
{
    [Serializable]
    public class DialogSwitchNode : BaseDialogOptionAction
    {
        [Title("切换子节点的坐标")] public int index = 0;

        public override void Execute(OptionDialogNode optionDialogNode)
        {
            Debug.Log($"DialogSwitchNode Execute,index:{index}");
            optionDialogNode.SetNextNodeIndexInChildren(index);
        }
    }
}