using System;
using Animancer;
using Animancer.TransitionLibraries;
using UnityEngine;

namespace Tools.Action.Running
{
    public class TestPlayAnimation : MonoBehaviour
    {
        public ClipTransition idle;
        public ClipTransition attack;
        public TransitionAsset transitionLibrary;
        private AnimancerComponent animancer;
        void Start()
        {
            animancer = GetComponent<AnimancerComponent>();
            animancer.Play(idle,0.2f,FadeMode.NormalizedFromStart);
        }
        public void PlayAttack()
        {
            animancer.Play(attack,0.2f,FadeMode.NormalizedFromStart);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayAttack();
            }
        }
    }
}