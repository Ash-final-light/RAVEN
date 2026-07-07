using System;

namespace Tools.Dialog.Event
{
    [Serializable]
    public abstract class BaseDialogEvent
    {
        public abstract void OnEnter();
        public abstract void OnExit();
    }
}