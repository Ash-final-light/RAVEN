using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.Quest.Event
{
    public enum QuestEventValueType
    {
        None,
        Integer,
        Float,
        Boolean,
        String,
    }

    public enum QuestEventRecordMode
    {
        Pulse,
        LatestValue,
        Accumulate,
        Latch,
    }

    public enum QuestValueComparison
    {
        Exists,
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        CountAtLeast,
    }

    [Serializable]
    public struct QuestEventValue
    {
        public QuestEventValueType type;
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;

        public static QuestEventValue None => new() { type = QuestEventValueType.None };
        public static QuestEventValue FromInt(int value) => new() { type = QuestEventValueType.Integer, intValue = value };
        public static QuestEventValue FromFloat(float value) => new() { type = QuestEventValueType.Float, floatValue = value };
        public static QuestEventValue FromBool(bool value) => new() { type = QuestEventValueType.Boolean, boolValue = value };
        public static QuestEventValue FromString(string value) => new() { type = QuestEventValueType.String, stringValue = value };

        public int AsInt(int defaultValue = 0)
        {
            return type switch
            {
                QuestEventValueType.Integer => intValue,
                QuestEventValueType.Float => Mathf.RoundToInt(floatValue),
                QuestEventValueType.Boolean => boolValue ? 1 : 0,
                QuestEventValueType.None => defaultValue,
                _ => int.TryParse(stringValue, out var value) ? value : defaultValue,
            };
        }

        public float AsFloat(float defaultValue = 0f)
        {
            return type switch
            {
                QuestEventValueType.Integer => intValue,
                QuestEventValueType.Float => floatValue,
                QuestEventValueType.Boolean => boolValue ? 1f : 0f,
                QuestEventValueType.None => defaultValue,
                _ => float.TryParse(stringValue, out var value) ? value : defaultValue,
            };
        }

        public override string ToString()
        {
            return type switch
            {
                QuestEventValueType.Integer => intValue.ToString(),
                QuestEventValueType.Float => floatValue.ToString("0.###"),
                QuestEventValueType.Boolean => boolValue.ToString(),
                QuestEventValueType.String => stringValue ?? string.Empty,
                _ => "Signal",
            };
        }
    }

    [Serializable]
    public struct QuestEventMessage
    {
        public string eventName;
        public string sourceId;
        public QuestEventValue value;
        public QuestEventRecordMode recordMode;
        public GameObject source;

        public QuestEventMessage(
            string eventName,
            QuestEventValue value,
            QuestEventRecordMode recordMode = QuestEventRecordMode.Pulse,
            string sourceId = "",
            GameObject source = null)
        {
            this.eventName = eventName;
            this.sourceId = sourceId;
            this.value = value;
            this.recordMode = recordMode;
            this.source = source;
        }
    }

    [Serializable]
    public class QuestEventDefinition
    {
        public string eventName;
        public string displayName;
        [TextArea] public string description;
        public QuestEventValueType valueType;
        public QuestEventRecordMode recordMode;
    }

    [CreateAssetMenu(menuName = "Quest/Event Catalog", fileName = "QuestEventCatalog")]
    public class QuestEventCatalog : ScriptableObject
    {
        public List<QuestEventDefinition> events = new();

        public bool TryGet(string eventName, out QuestEventDefinition definition)
        {
            definition = events.Find(item => item != null && item.eventName == eventName);
            return definition != null;
        }
    }

    public static class QuestEventComparisonUtility
    {
        public static bool Compare(bool exists, QuestEventValue actual, QuestValueComparison comparison, QuestEventValue expected)
        {
            if (comparison == QuestValueComparison.Exists)
                return exists;
            if (!exists)
                return false;

            if (actual.type == QuestEventValueType.String || expected.type == QuestEventValueType.String)
            {
                var equal = string.Equals(actual.ToString(), expected.ToString(), StringComparison.Ordinal);
                return comparison == QuestValueComparison.NotEqual ? !equal : equal;
            }

            if (actual.type == QuestEventValueType.Boolean || expected.type == QuestEventValueType.Boolean)
            {
                var equal = actual.AsInt() == expected.AsInt();
                return comparison == QuestValueComparison.NotEqual ? !equal : equal;
            }

            var left = actual.AsFloat();
            var right = expected.AsFloat();
            return comparison switch
            {
                QuestValueComparison.Equal => Mathf.Approximately(left, right),
                QuestValueComparison.NotEqual => !Mathf.Approximately(left, right),
                QuestValueComparison.Greater => left > right,
                QuestValueComparison.GreaterOrEqual => left >= right,
                QuestValueComparison.Less => left < right,
                QuestValueComparison.LessOrEqual => left <= right,
                QuestValueComparison.CountAtLeast => left >= right,
                _ => false,
            };
        }
    }
}
