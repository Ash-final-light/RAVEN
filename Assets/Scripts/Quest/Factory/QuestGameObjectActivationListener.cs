using System.Collections.Generic;
using System.Linq;
using Tools.Quest.Event;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quest.Factory
{
    public sealed class QuestGameObjectActivationListener : MonoBehaviour
    {
        [SerializeField] private string eventName;
        [SerializeField] private List<GameObject> targets = new();
        [SerializeField] private List<string> targetPaths = new();
        [SerializeField] private bool activeOnEvent = true;
        [SerializeField] private bool unsubscribeAfterTriggered = true;
        private bool subscribed;

        public void Configure(string name, IEnumerable<GameObject> objects, bool active = true)
        {
            Unsubscribe();
            eventName = name;
            targets = new List<GameObject>(objects);
            targetPaths = targets.Select(BuildHierarchyPath).ToList();
            activeOnEvent = active;
            if (Application.isPlaying && isActiveAndEnabled)
                Subscribe();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
                Subscribe();
        }
        private void Start() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (subscribed || string.IsNullOrWhiteSpace(eventName))
                return;
            var hub = QuestEventHub.Instance;
            if (hub == null)
                return;
            hub.Subscribe(eventName, HandleEvent);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;
            if (QuestEventHub.TryGetInstance(out var hub))
                hub.Unsubscribe(eventName, HandleEvent);
            subscribed = false;
        }

        private void HandleEvent(QuestEventMessage message)
        {
            var count = Mathf.Max(targets.Count, targetPaths.Count);
            for (var index = 0; index < count; index++)
            {
                var target = index < targets.Count ? targets[index] : null;
                if (target == null && index < targetPaths.Count)
                    target = ResolveHierarchyPath(targetPaths[index]);
                if (target != null)
                    target.SetActive(activeOnEvent);
            }
            if (unsubscribeAfterTriggered)
                Unsubscribe();
        }

        private static string BuildHierarchyPath(GameObject target)
        {
            if (target == null)
                return string.Empty;
            var path = target.name;
            for (var parent = target.transform.parent; parent != null; parent = parent.parent)
                path = parent.name + "/" + path;
            return path;
        }

        private static GameObject ResolveHierarchyPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var separator = path.IndexOf('/');
            var rootName = separator < 0 ? path : path[..separator];
            var childPath = separator < 0 ? string.Empty : path[(separator + 1)..];
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name != rootName)
                    continue;
                if (string.IsNullOrEmpty(childPath))
                    return root;
                return root.transform.Find(childPath)?.gameObject;
            }
            return null;
        }
    }
}
