using System;
using Test;
using Tools.InputHandle;
using UnityEngine;

namespace Monster.Simple
{
    [RequireComponent(typeof(ActionStateController))]
    public class HideAfterDeath : MonoBehaviour
    {
        private ActionStateController stateController;
        private void Awake()
        {
            stateController = GetComponent<ActionStateController>();
        }

        private void OnEnable()
        {
            if (stateController != null) 
                stateController.OnCharacterDieAction += OnCharacterDie;
        }
        private void OnDisable()
        {
            if (stateController != null) 
                stateController.OnCharacterDieAction -= OnCharacterDie;
        }

        private void OnCharacterDie()
        {
            gameObject.SetActive(false);
        }
    }
}