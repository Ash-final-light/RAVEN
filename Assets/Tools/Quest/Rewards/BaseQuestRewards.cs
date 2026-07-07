using System;
using Sirenix.OdinInspector;

namespace Tools.Quest
{
    [Serializable]
    public abstract class BaseQuestRewards
    {
        [InlineButton("ResetId")]public string id = System.Guid.NewGuid().ToString();
        
        public abstract void GiveReward();

        private void ResetId()
        {
            id = System.Guid.NewGuid().ToString();
        }
    }
}