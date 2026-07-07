using System;
using System.Collections.Generic;
using Framework.Core.Attribute;
using Tools.Dialog.Option;
using UnityEngine;

namespace Tools.Dialog.Nodes
{
    [Serializable]
    [DialogNodeMenu("OptionDialogNode/Base")]
    public class OptionDialogNode:BaseDialogNode
    {
        public List<OptionAction> options;
        [DisplayOnly] public List<BaseDialogNode> children;
        private int nextNodeIndexinChildren = -1;
        private int optionIndex = -1;
        protected override void OnEnter()
        {
            Debug.Log("OnEnter,option");
        }

        protected override void OnProgress()
        {
            Debug.Log($"OnProgress,option:{optionIndex}");
        }

        public override void Exit()
        {
            if (options == null || optionIndex < 0 || optionIndex >= options.Count)
            {
                throw new NullReferenceException("OptionDialogNode Exit: optionIndex out of range");
            }

            var actions = options[optionIndex].children;
            if (actions == null)
            {
                return;
            }

            foreach(BaseDialogOptionAction action in actions)
            {
                action.Execute(this);
            }
        }

        public override bool AddChildNode(BaseDialogNode child)
        {
            children.Add(child);
            return true;
        }

        public override BaseDialogNode GetNextNode()
        {
            if (nextNodeIndexinChildren < children.Count && nextNodeIndexinChildren != -1)
            {
                Debug.Log($"GetNextNode,nextNodeIndexinChildren:{nextNodeIndexinChildren}");
                return children[nextNodeIndexinChildren];
            }
            else
            {
                throw new NullReferenceException("OptionDialogNode GetNextNode: optionIndex out of range");
            }
        }

        public void SetNextNodeIndexInChildren(int index)
        {
            this.nextNodeIndexinChildren = index;
        }
        public void SetOptionIndex(int index)
        {
            optionIndex = index;
        }
    }
    [Serializable]
    public class OptionAction
    {
        public string text;
        [SerializeReference] public List<BaseDialogOptionAction> children = new();
    }
}
