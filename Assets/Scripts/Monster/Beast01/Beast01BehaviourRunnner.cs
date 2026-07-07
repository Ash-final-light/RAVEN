using Sirenix.OdinInspector;
using Test;
using Tools.AI;
using Tools.BehaviourTree;
using Tools.InputHandle;
using UnityEngine;

namespace Monster.Beast01
{
    public class Beast01BehaviourRunnner : BehaviourRunner
    {
        private ActionStateController stateController;
        [SerializeField] private Transform target;
        [ShowInInspector][ReadOnly] private AiStatus aistatus;
        private BehaviourTree runningTree;
        private bool initialized;

        private void Awake()
        {
            stateController = gameObject.GetComponent<ActionStateController>();
            if (stateController == null)
            {
                Debug.LogError($"{nameof(Beast01BehaviourRunnner)} requires an {nameof(ActionStateController)} on {name}.", this);
                enabled = false;
                return;
            }

            if (sourceTree == null)
            {
                Debug.LogError($"{nameof(Beast01BehaviourRunnner)} requires a source behaviour tree on {name}.", this);
                enabled = false;
                return;
            }

            runningTree = sourceTree.Clone();
            if (runningTree == null)
            {
                Debug.LogError($"{nameof(Beast01BehaviourRunnner)} failed to clone source tree on {name}.", this);
                enabled = false;
                return;
            }

            aistatus = new Beast01Status(runningTree, transform, stateController, target);
            runningTree.Bind(aistatus);
            initialized = true;
        }

        private void Update()
        {
            if (!initialized || runningTree == null || aistatus == null)
            {
                return;
            }

            //TODO: Drive this from a dispatcher later.
            aistatus.UpdateBlackboard();
            float behaviourTreeScale = stateController is TestEnemyStateController testEnemy
                ? testEnemy.BehaviourTreeScale
                : 1f;
            runningTree.Tick(Time.deltaTime * behaviourTreeScale);
        }

        private void LateUpdate()
        {
            if (aistatus is Beast01Status beast01Status)
            {
                beast01Status.ApplyLateFaceTarget();
            }
        }

        public override BehaviourTree RunningTree => runningTree;
    }
}
