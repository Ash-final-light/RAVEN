using System;
using Tools.Dialog.Nodes;

namespace Tools.Dialog.Option
{
    [Serializable]
    public abstract class BaseDialogOptionAction
    {
        public abstract void Execute(OptionDialogNode optionDialogNode);
    }
}