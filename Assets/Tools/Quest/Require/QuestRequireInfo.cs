using System;

namespace Tools.Quest.Require
{
    [Serializable]
    public class QuestRequirementInfo
    {
        public string id; // 任务需求id
        [NonSerialized] public QuestRequirementUpdate Update; // 任务需求更新函数委托
    }

    public delegate void QuestRequirementUpdate(QuestRequire requirement, float deltaTime);
}