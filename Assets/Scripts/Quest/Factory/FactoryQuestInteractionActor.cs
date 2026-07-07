using System.Collections.Generic;
using UnityEngine;

namespace Quest.Factory
{
    [DisallowMultipleComponent]
    public sealed class FactoryQuestInteractionActor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float interactionRadius = 2.25f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private KeyCode interactionKey = KeyCode.I;
        [SerializeField, Min(0f)] private float moveSpeed = 4f;

        public string CurrentPrompt { get; private set; }

        private void Update()
        {
            var movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (movement.sqrMagnitude > 1f)
                movement.Normalize();
            transform.position += movement * (moveSpeed * Time.deltaTime);

            var target = FindNearestInteractable();
            CurrentPrompt = target?.InteractionPrompt ?? string.Empty;
            if (target != null && Input.GetKeyDown(interactionKey))
                target.Interact(this);
        }

        private IFactoryQuestInteractable FindNearestInteractable()
        {
            var colliders = Physics.OverlapSphere(transform.position, interactionRadius, interactionMask, QueryTriggerInteraction.Collide);
            var visited = new HashSet<IFactoryQuestInteractable>();
            IFactoryQuestInteractable nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var candidateCollider in colliders)
            {
                var behaviours = candidateCollider.GetComponentsInParent<MonoBehaviour>(true);
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is not IFactoryQuestInteractable candidate || !candidate.CanInteract || !visited.Add(candidate))
                        continue;

                    var distance = (behaviour.transform.position - transform.position).sqrMagnitude;
                    if (distance >= nearestDistance)
                        continue;

                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
