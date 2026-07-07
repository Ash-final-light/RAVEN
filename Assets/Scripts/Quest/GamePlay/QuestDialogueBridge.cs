using System;
using System.Collections.Generic;
using Common;
using DialogExecute;
using Tools.Dialog;
using UnityEngine;

namespace Quest.GamePlay
{
    public sealed class QuestDialogueBridge : MonoBehaviour, IQuestDialogueGateway
    {
        [Serializable]
        private struct DialogueBinding
        {
            public string dialogueId;
            public DialogTree dialogTree;
        }

        [SerializeField] private QuestDialogueEventListener dialogueListener;
        [SerializeField] private string dialogViewName = "DialogView";
        [SerializeField] private DialogTree defaultDialogTree;
        [SerializeField] private List<DialogueBinding> dialogues = new();

        private void OnEnable()
        {
            if (dialogueListener == null)
            {
                dialogueListener = GetComponent<QuestDialogueEventListener>();
            }

            if (dialogueListener != null)
            {
                dialogueListener.SetDialogueGateway(this);
            }
        }

        private void OnDisable()
        {
            if (dialogueListener != null)
            {
                dialogueListener.SetDialogueGateway(null);
            }
        }

        public void Play(string dialogueId, Action onCompleted)
        {
            var dialogTree = ResolveDialogTree(dialogueId);
            if (dialogTree == null)
            {
                Debug.LogError($"[{nameof(QuestDialogueBridge)}] No dialog tree configured for dialogue id '{dialogueId}'.", this);
                onCompleted?.Invoke();
                return;
            }

            var uiManager = GameViewManager.UIManager;
            if (uiManager == null)
            {
                Debug.LogError($"[{nameof(QuestDialogueBridge)}] Cannot play dialogue because UIManager is not ready.", this);
                onCompleted?.Invoke();
                return;
            }

            if (!uiManager.Open(dialogViewName, out DialogUIView dialogView) || dialogView == null)
            {
                Debug.LogError($"[{nameof(QuestDialogueBridge)}] Cannot open '{dialogViewName}'.", this);
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
