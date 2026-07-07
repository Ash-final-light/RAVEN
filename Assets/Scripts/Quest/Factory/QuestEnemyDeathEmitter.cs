using Tools.Action.Data.Interact;
using Tools.Quest.Event;
using UnityEngine;

namespace Quest.Factory
{
    [DisallowMultipleComponent]
    public sealed class QuestEnemyDeathEmitter : MonoBehaviour, IInteractable
    {
        [SerializeField] private string deathEventName;
        [SerializeField] private string enemyId;
        [SerializeField] private bool emitOnce = true;
        [SerializeField] private bool allowKeyboardDefeatForTest = true;
        [SerializeField] private string interactionPrompt = "按 I 清除威胁";
        private bool emitted;

        public bool CanInteract => allowKeyboardDefeatForTest && isActiveAndEnabled && !emitted;
        public string InteractionPrompt => interactionPrompt;

        public void Configure(string eventName, string uniqueEnemyId, bool keyboardTest = true)
        {
            deathEventName = eventName;
            enemyId = uniqueEnemyId;
            allowKeyboardDefeatForTest = keyboardTest;
        }

        public void Interact() => NotifyDeath();

        public void NotifyDeath()
        {
            if ((emitOnce && emitted) || string.IsNullOrWhiteSpace(deathEventName))
                return;

            emitted = true;
            QuestEventHub.Instance.Publish(new QuestEventMessage(
                deathEventName,
                QuestEventValue.FromInt(1),
                QuestEventRecordMode.Pulse,
                enemyId,
                gameObject));
            gameObject.SetActive(false);
        }
    }
}
