using System;
using Sirenix.OdinInspector;
using Tools.Blackboard.Data;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.AI
{
    public enum aiAgentType
    {
        Bat
    } 
    [Serializable]
    public abstract class AiStatus
    {
        //public Blackboard.Data.Blackboard blackboard;
        [SerializeField]    
        protected Transform selfTransform;
        protected ActionStateController stateController;
        
        public AiStatus(Transform selfTransform, ActionStateController stateController)
        {
            this.stateController = stateController ?? throw new ArgumentNullException(nameof(stateController));
            this.selfTransform = selfTransform ?? throw new ArgumentNullException(nameof(selfTransform));
        }
        public abstract void UpdateBlackboard();
        public abstract bool TryToPlayAction(string actionName,bool loop = false);
        
        public bool MoveTo(Vector3 position)
        {
           if(Vector3.Distance(selfTransform.position, position) <= 0.1f)
                return true;
           Debug.Log($"AiAgent.MoveTo {position}");
           return false;
        }
    }
}