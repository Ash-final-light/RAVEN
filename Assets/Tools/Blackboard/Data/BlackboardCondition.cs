using UnityEngine;

namespace Tools.Blackboard.Data
{
    public enum DataType
    {
        Int,
        Float,
        String,
        Bool,
    }

    public enum CompareEnum
    {
        Greater,
        Less,
        Equal,
        NotEqual,
    }
    [System.Serializable]
    public class BlackboardCondition
    {
        public string key;
        public DataType dataType;
        public CompareEnum compareType;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;
        public bool matchValueProperty;//used for gui
        
        public bool Satisfy(Blackboard blackBoard)
        {
            if(!blackBoard.ContainsKey(key))
            {
                Debug.LogError($"BlackboardCondition.{key} not found");
                return false;
            }
            var parameter = blackBoard.GetParameter(key);
            switch (dataType)
            {
                case DataType.Bool:
                    return parameter.boolValue == boolValue;
                case DataType.Int:
                    var intVal = parameter.intValue;
                    return compareType switch
                    {
                        CompareEnum.Equal => intVal == intValue,
                        CompareEnum.NotEqual => intVal != intValue,
                        CompareEnum.Greater => intVal > intValue,
                        CompareEnum.Less => intVal < intValue,
                        _ => false,
                    };
                case DataType.Float:
                    var floatVal = parameter.floatValue;
                    return compareType switch
                    {
                        CompareEnum.Equal => Mathf.Approximately(floatVal, floatValue),
                        CompareEnum.NotEqual => !Mathf.Approximately(floatVal, floatValue),
                        CompareEnum.Greater => floatVal > floatValue,
                        CompareEnum.Less => floatVal < floatValue,
                        _ => false,
                    };
                case DataType.String:
                    var stringVal = parameter.stringValue;
                    return compareType switch
                    {
                        CompareEnum.Equal => stringVal == stringValue,
                        CompareEnum.NotEqual => stringVal != stringValue,
                        _ => false,
                    };
            }
            return false;
        }
    }
}
