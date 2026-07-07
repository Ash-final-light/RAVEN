using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.Action.Data.Interact
{
    public class InteractAbility : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float interactRadius = 2.25f;
        [SerializeField] private LayerMask interactableLayer = ~0;

        public IInteractable CurrentInteractableObject { get; private set; }
        public string CurrentPrompt => CurrentInteractableObject?.InteractionPrompt ?? string.Empty;

        public event Action<string> InteractionPromptChanged;

        private string _lastPrompt = string.Empty;

        private void Update()
        {
            RefreshCurrentInteractable();
        }

        public void Interact()
        {
            RefreshCurrentInteractable();

            if (CurrentInteractableObject == null || !CurrentInteractableObject.CanInteract)
            {
                return;
            }

            CurrentInteractableObject.Interact();
            RefreshCurrentInteractable();
        }

        private void RefreshCurrentInteractable()
        {
            CurrentInteractableObject = FindNearestInteractable();
            var prompt = CurrentPrompt;

            if (prompt == _lastPrompt)
            {
                return;
            }

            _lastPrompt = prompt;
            InteractionPromptChanged?.Invoke(prompt);
        }

        private IInteractable FindNearestInteractable()
        {
            var colliders = Physics.OverlapSphere(transform.position, interactRadius, interactableLayer);
            var visited = new HashSet<IInteractable>();
            IInteractable nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var candidateCollider in colliders)
            {
                var behaviours = candidateCollider.GetComponentsInParent<MonoBehaviour>(true);
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is not IInteractable candidate || !candidate.CanInteract || !visited.Add(candidate))
                    {
                        continue;
                    }

                    var distance = (behaviour.transform.position - transform.position).sqrMagnitude;
                    if (distance >= nearestDistance)
                    {
                        continue;
                    }

                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
