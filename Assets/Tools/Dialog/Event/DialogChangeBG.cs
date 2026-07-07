using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Dialog.Event
{
    [Serializable]
    public class DialogChangeBG : BaseDialogEvent
    {
        [Title("背景图片")] 
        public string spriteName;
        public string atlasName;
        public override void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        public override void OnExit()
        {
            throw new System.NotImplementedException();
        }
    }
}