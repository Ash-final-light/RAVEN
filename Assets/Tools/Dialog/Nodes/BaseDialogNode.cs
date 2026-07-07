using System;
using Framework.Core.Attribute;
using Tools.Profile.Data;
using UnityEngine;

namespace Tools.Dialog.Nodes
{
    public enum DialogNodeStatus
    {
        Running,
        Success,
        UnStarted
    }
    [Serializable]
    public abstract class BaseDialogNode:ScriptableObject
    {
        [DisplayOnly]public string guid;
        [HideInInspector]public Vector2 position;
        public DialogNodeStatus State = DialogNodeStatus.Running;
        [DisplayOnly]public ProfilePool _profilePool;
        private bool isEntered = false;

        public void ExecuteNode()
        {
            OnEnter();
            OnProgress();
        }
        protected abstract void OnEnter();
        protected abstract void OnProgress();
        public abstract void Exit();
        public abstract BaseDialogNode GetNextNode();
        
        /// <summary>
        /// 删除子节点
        /// </summary>
        public virtual bool RemoveChildNode(BaseDialogNode child)
        {
            return false;
        }
        /// <summary>
        /// 添加子节点
        /// </summary>
        public virtual bool AddChildNode(BaseDialogNode child)
        {
            return false;
        }

        /// <summary>
        /// 克隆节点
        /// </summary>
        /// <returns></returns>
        public virtual BaseDialogNode Clone()
        {
            return Instantiate(this);
        }
    }
}