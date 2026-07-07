using System;
using Sirenix.OdinInspector;
using Tools.AI;
using UnityEngine;

namespace Tools.BehaviourTree
{
    public abstract class BehaviourRunner:MonoBehaviour
    {
        public BehaviourTree sourceTree;
        
        public abstract BehaviourTree RunningTree { get; }
    }
}