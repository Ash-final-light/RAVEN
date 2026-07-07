using System;
using UnityEngine;
using UnityEngine.AI;
namespace Tools.AI
{
    public class DebugNavMeshAgent:MonoBehaviour
    {
        public bool velocity;
        public bool desiredVelocity;
        public bool path;
        NavMeshAgent agent;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void OnDrawGizmos()
        {
            if (velocity)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + agent.velocity);
                
            }
            if (desiredVelocity)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + agent.desiredVelocity);
            }
            if (path)
            { 
                Gizmos.color = Color.black;
               var agentpath = agent.path;
               Vector3 prevCorner = transform.position;
               foreach (var corner in agentpath.corners)
               {
                   Gizmos.DrawLine(prevCorner, corner);
                   Gizmos.DrawSphere(corner, 0.5f);
                   
                   prevCorner = corner;
               }
            }
        }
    }
}