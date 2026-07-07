using Tools.InputHandle;
using Tools.Quest.Event;
using UnityEngine;

namespace Quest.GamePlay
{
    /// <summary>
    /// Emits a quest event when the attached ActionStateController reports a real character death.
    /// </summary>
    [RequireComponent(typeof(ActionStateController))]
    [DisallowMultipleComponent]
    public class QuestEnemyDeathEmitter : MonoBehaviour
    {
        [SerializeField] private string deathEventName;
        [SerializeField] private string enemyId;
        [SerializeField] private bool emitOnce = true;
        [SerializeField] private bool deactivateAfterDeath = true;

        private ActionStateController stateController;
        private bool emitted;
        private bool subscribed;

        public void Configure(string eventName, string uniqueEnemyId)
        {
            deathEventName = eventName;
            enemyId = uniqueEnemyId;
        }

        private void Awake()
        {
            CacheStateController();
            FillDefaultEnemyId();
        }

        private void OnEnable()
        {
            CacheStateController();

            if (stateController != null && !subscribed)
            {
                stateController.OnCharacterDieAction += NotifyDeath;
                subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (stateController != null && subscribed)
            {
                stateController.OnCharacterDieAction -= NotifyDeath;
                subscribed = false;
            }
        }

        public void NotifyDeath()
        {
            if ((emitOnce && emitted) || string.IsNullOrWhiteSpace(deathEventName))
            {
                return;
            }

            FillDefaultEnemyId();

            if (!QuestEventHub.TryGetInstance(out var hub))
            {
                Debug.LogWarning($"[{nameof(QuestEnemyDeathEmitter)}] QuestEventHub is missing. Event '{deathEventName}' was not published.", this);
                return;
            }

            emitted = true;
            hub.Publish(new QuestEventMessage(
                deathEventName,
                QuestEventValue.FromInt(1),
                QuestEventRecordMode.Pulse,
                enemyId,
                gameObject));

            if (deactivateAfterDeath)
            {
                gameObject.SetActive(false);
            }
        }

        public void ResetEmitter()
        {
            emitted = false;
        }

        private void CacheStateController()
        {
            if (stateController == null)
            {
                stateController = GetComponent<ActionStateController>();
            }
        }

        private void FillDefaultEnemyId()
        {
            if (!string.IsNullOrWhiteSpace(enemyId) || stateController == null)
            {
                return;
            }

            enemyId = stateController.characterId;
        }
    }
}
