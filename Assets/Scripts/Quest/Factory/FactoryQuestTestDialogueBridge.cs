using System;
using System.Collections.Generic;
using Common;
using DialogExecute;
using Tools.Dialog;
using UnityEngine;

namespace Quest.Factory
{
    public sealed class FactoryQuestTestDialogueBridge : MonoBehaviour, IFactoryQuestDialogueGateway
    {
        [Serializable]
        private struct DialogueBinding
        {
            public string dialogueId;
            public DialogTree dialogTree;
        }

        [SerializeField] private QuestDialogueEventListener dialogueListener;
        [SerializeField] private DialogTree defaultDialogTree;
        [SerializeField] private List<DialogueBinding> dialogues = new();

        private void OnEnable()
        {
            if (dialogueListener == null)
                dialogueListener = GetComponent<QuestDialogueEventListener>();
            if (dialogueListener != null)
                dialogueListener.SetDialogueGateway(this);
        }

        private void OnDisable()
        {
            if (dialogueListener != null)
                dialogueListener.SetDialogueGateway(null);
        }

        public void Play(string dialogueId, Action onCompleted)
        {
            var dialogTree = ResolveDialogTree(dialogueId);
            if (dialogTree == null)
            {
                Debug.LogError($"[FactoryQuestTestDialogueBridge] No dialog tree configured for dialogue id '{dialogueId}'.");
                onCompleted?.Invoke();
                return;
            }

            var uiManager = GameViewManager.UIManager;
            if (uiManager == null)
            {
                Debug.LogError("[FactoryQuestTestDialogueBridge] Cannot play dialogue because UIManager is not ready.");
                onCompleted?.Invoke();
                return;
            }

            if (!uiManager.Open("DialogView", out DialogUIView dialogView) || dialogView == null)
            {
                Debug.LogError("[FactoryQuestTestDialogueBridge] Cannot open DialogView.");
                onCompleted?.Invoke();
                return;
            }

            dialogView.Play(dialogTree, onCompleted);
        }

        private DialogTree ResolveDialogTree(string dialogueId)
        {
            foreach (var dialogue in dialogues)
            {
                if (dialogue.dialogueId == dialogueId)
                {
                    return dialogue.dialogTree;
                }
            }

            return defaultDialogTree;
        }
    }
}
