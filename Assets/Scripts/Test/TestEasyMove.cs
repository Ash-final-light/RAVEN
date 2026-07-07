using System;
using Tools.Action.Data.Interact;
using UnityEngine;

namespace Test
{
    public class TestEasyMove:MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        private InteractAbility _ability;

        private void Awake()
        {
            _ability = gameObject.GetComponent<InteractAbility>();
        }

        private void Update()
        {
            var movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (movement.sqrMagnitude > 1f)
                movement.Normalize();
            transform.position += movement * (moveSpeed * Time.deltaTime);
            if (Input.GetKeyDown(KeyCode.I))
            {
                _ability.Interact();
            }
        }


    }
}