using UnityEngine;

namespace Tools.BehaviourTree.Nodes.ActionNode
{
    [NodeItemMenu("Action Node/Wait")]
    public class WaitNode : ActionNode
    {
        public float duration = 1f;
        float startTime;


        protected override void OnStart()
        {
            startTime = Time.time;
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnTick(float deltaTime = 0)
        {
            if(Time.time - startTime > duration)
            {
                return State.Success;
            }
            return State.Running;
        }
    }
}