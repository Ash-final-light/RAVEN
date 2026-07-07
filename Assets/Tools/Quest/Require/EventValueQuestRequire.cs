using System;
using Sirenix.OdinInspector;
using Tools.Quest.Event;
using UnityEngine;

namespace Tools.Quest.Require
{
    [Serializable]
    public class EventValueQuestRequire : BaseQuestRequire
    {
        [Title("事件前置条件")] public string eventName;
        public string sourceId;
        public QuestValueComparison comparison = QuestValueComparison.Exists;
        public QuestEventValue expectedValue = new() { type = QuestEventValueType.Boolean, boolValue = true };
        [TextArea] public string description = "{Event} {Comparison} {Expected}";

        public override void UpdateStatus(QuestRequire require, float deltaTime)
        {
            var actual = QuestEventValue.None;
            var exists = QuestEventHub.TryGetInstance(out var hub) &&
                         hub.TryGetRecordedValue(eventName, sourceId, out actual);
            require.meet = QuestEventComparisonUtility.Compare(exists, actual, comparison, expectedValue);
            require.description = description
                .Replace("{Event}", eventName ?? string.Empty)
                .Replace("{Comparison}", comparison.ToString())
                .Replace("{Expected}", expectedValue.ToString())
                .Replace("{Current}", exists ? actual.ToString() : "-");
        }
    }
}
