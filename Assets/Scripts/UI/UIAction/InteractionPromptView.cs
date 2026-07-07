using TMPro;
using Tools.Action.Data.Interact;
using UnityEngine;

namespace Scripts.UI.UIAction
{
    public class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private InteractAbility interactAbility;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptText;

        private void Awake()
        {
            if (promptText == null)
            {
                promptText = GetComponentInChildren<TMP_Text>(true);
            }

            if (promptRoot == null && promptText != null && promptText.gameObject != gameObject)
            {
                promptRoot = promptText.gameObject;
            }
        }

        private void OnEnable()
        {
            if (interactAbility != null)
            {
                interactAbility.InteractionPromptChanged += SetPrompt;
                SetPrompt(interactAbility.CurrentPrompt);
                return;
            }

            SetPrompt(string.Empty);
        }

        private void OnDisable()
        {
            if (interactAbility != null)
            {
                interactAbility.InteractionPromptChanged -= SetPrompt;
            }
        }

        public void SetInteractAbility(InteractAbility ability)
        {
            if (interactAbility == ability)
            {
                SetPrompt(interactAbility != null ? interactAbility.CurrentPrompt : string.Empty);
                return;
            }

            if (isActiveAndEnabled && interactAbility != null)
            {
                interactAbility.InteractionPromptChanged -= SetPrompt;
            }

            interactAbility = ability;

            if (isActiveAndEnabled && interactAbility != null)
            {
                interactAbility.InteractionPromptChanged += SetPrompt;
            }

            SetPrompt(interactAbility != null ? interactAbility.CurrentPrompt : string.Empty);
        }

        private void SetPrompt(string prompt)
        {
            var hasPrompt = !string.IsNullOrWhiteSpace(prompt);

            if (promptRoot != null && promptRoot != gameObject)
            {
                promptRoot.SetActive(hasPrompt);
            }
            else if (promptText != null)
            {
                promptText.enabled = hasPrompt;
            }

            if (promptText != null && hasPrompt)
            {
                promptText.text = prompt;
            }
        }
    }
}
