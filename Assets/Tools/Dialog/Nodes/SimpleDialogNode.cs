using System;
using System.Collections.Generic;
using Framework.Core.Attribute;
using Sirenix.OdinInspector;
using Tools.BehaviourTree.Nodes;
using Tools.Dialog.Event;
using Tools.Profile.Data;
using UnityEngine;

namespace Tools.Dialog.Nodes
{
    [Serializable]
    [DialogNodeMenu("SimpleDialogNode/Base")]
    public class SimpleDialogNode : BaseDialogNode,IProfilePoolProvider
    {
        [DisplayOnly] public BaseDialogNode child;
        [TextArea] public string speaker;
        [TextArea] public string content;
        [Title("开启profile动画")]
        public bool enableProfileAnimation;
        [Title("执行动画列表"),ShowIf("enableProfileAnimation")]
        public List<ProfileActionValue> profileActions;
        [Title("开启事件")]
        public bool enableEvent;
        [ShowIf("enableEvent"),SerializeReference]
        public List<BaseDialogEvent> dialogEvents = new();
        
        protected override void OnEnter()
        {
            Debug.Log($"OnEnter,{speaker}:{content}");
            if (dialogEvents == null)
            {
                return;
            }

            foreach (var dialogEvent in dialogEvents)
            {
                dialogEvent.OnEnter();
            }
        }

        protected override void OnProgress()
        {
            
        }

        public override bool AddChildNode(BaseDialogNode child)
        {
            if (this.child != null)
            {
                return false;
            }
            this.child = child;
            return true;
        }

        public override void Exit()
        {
            Debug.Log($"OnExit,{speaker}");
            if (dialogEvents == null)
            {
                return;
            }

            foreach (var dialogEvent in dialogEvents)
            {
                dialogEvent.OnExit();
            }
        }

        public override BaseDialogNode GetNextNode()
        {
            return child;
        }

        public ProfilePool ProfilePool => _profilePool;
    }
}
