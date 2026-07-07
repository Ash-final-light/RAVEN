using System;
using System.Collections.Generic;

namespace Tools.Blackboard.Data
{
    [Serializable]
    public class BlackboardConditionChecker:IBlackBoardProvider
    {
        public List<BlackboardValue> blackBoardValues = new List<BlackboardValue>();
        
        public Blackboard blackboard;


        public Blackboard Blackboard => blackboard;
    }
}