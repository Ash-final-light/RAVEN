using System;

namespace Tools.Blackboard.Data
{
    [Serializable]
    public class BlackboardValue
    {
        public string key;
        public DataType dataType;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;

        public BlackboardValue Clone()
        {
            return new BlackboardValue
            {
                key = key,
                dataType = dataType,
                boolValue = boolValue,
                intValue = intValue,
                floatValue = floatValue,
                stringValue = stringValue,
            };
        }
    }
}