using Tools.AI;
using UnityEngine;

namespace Tools.BehaviourTree.Nodes
{
    public abstract class Node:ScriptableObject
    {
        public enum State
        {
            Running,
            Success,
            Failure
        }
        
        public string guid;
        public Vector2 position;
        
        [SerializeField] public BehaviourTree tree;
        [SerializeField] public Blackboard.Data.Blackboard blackboard;
        [TextArea] public string description;
        [SerializeField] public bool stopWhenAbort = true;
        [SerializeField] public bool debugLifecycle;
        [SerializeField] public bool debugTime;

        public virtual string NoticeInfo => "基本节点";
        
        [HideInInspector] public AiStatus AiStatus;
        
        public State state = State.Running;
        
        private bool started;
        private bool aborted;
        
        public State Tick(float deltaTime = 0f)
        {
            var startTime = Time.realtimeSinceStartup;

            var tickDeltaTime = deltaTime;
            
            if (aborted)
            {
                aborted = false;
                if(debugLifecycle)
                {
                    Debug.Log($"Node {guid} OnAbort");
                }
                OnResume();
            }
            
            if (!started)
            {
                started = true;
                if (debugLifecycle)
                {
                    Debug.Log($"Node {guid} OnStart");
                }
                OnStart();
            }

            if (debugLifecycle)
            {
                Debug.Log($"Node {guid} OnTick");
            }
            
            state = OnTick(deltaTime);
            if(state == State.Failure || state == State.Success)
            {
                OnStop();
                started = false;
                aborted = false;
                if(debugLifecycle)
                {
                    Debug.Log($"Node {guid} OnStop");
                }
            }
            
            var endTime = Time.realtimeSinceStartup;
            if(debugTime)
            {
                Debug.Log($"Node {guid} TickTime: {(endTime - startTime) * 1000}ms");
            }
            
            return state;
        }

        public void Abort()
        {
            if (!started)
            {
                return;
            }

            if (aborted)
            {
                if(debugLifecycle)
                {
                    Debug.Log($"Node {guid} Abort: Already Aborted");
                }
                return;
            }
            
            aborted = true;
            if(debugLifecycle)
            {
                Debug.Log($"Node {guid} Abort");
            }
            OnAbort();
            
            if(stopWhenAbort)
            {
                started = false;
                aborted = false;
                state = State.Failure;
                if(debugLifecycle)
                {
                    Debug.Log($"Node {guid} Abort: Stop When Abort");
                }
                OnStop();
            }
        }
        public virtual Node Clone()
        {
            return Instantiate(this);
        }
        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnTick(float deltaTime = 0f);
        /// <summary>
        /// 被打断后再次调用Tick时，会使用其进行复位
        /// </summary>
        protected virtual void OnResume(){}
        protected virtual void OnAbort(){}
        
        public bool isStarted => started;
        public bool isAborted => aborted;
    }
}