using System;
using Sirenix.OdinInspector;

namespace Tools.Quest.Require
{
    [Serializable]
    public abstract class BaseQuestRequire
    {
        [InlineButton("ResetId")] public string id = Guid.NewGuid().ToString();
        public abstract void UpdateStatus(QuestRequire require,float deltaTime);
        private void ResetId()
        {
            id = Guid.NewGuid().ToString();
        }
    }
}