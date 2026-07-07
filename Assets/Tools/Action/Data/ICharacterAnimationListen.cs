using Animancer;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Action.Data
{
    public interface ICharacterAnimationListen
    {
        void HandleAnimationPlayed(ActionStateController character, AnimancerLayer animancerLayer,
            AnimancerState animancerState);

        void HandleAnimationStopped(ActionStateController character, AnimancerLayer animancerLayer,
            AnimancerState animancerState);
    }
}