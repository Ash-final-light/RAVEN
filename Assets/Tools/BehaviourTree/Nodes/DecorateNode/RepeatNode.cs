using UnityEngine;

namespace Tools.BehaviourTree.Nodes.DecorateNode
{
    [NodeItemMenu("Decorate Node/RepeatNode")]
    public class RepeatNode:DecorateNode
    {
        public int repeatTimes = 1;
        
        private int _currentTimes = 0;
        
        protected override void OnStart()
        {
            _currentTimes = 0;
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if (repeatTimes < 0)
            {
                return State.Success;
            }
            child.Tick(deltaTime);
            _currentTimes++;
            if (_currentTimes >= repeatTimes)
            {
                child.Abort();
                return State.Success;
            }
            return State.Running;
        }
    }
}