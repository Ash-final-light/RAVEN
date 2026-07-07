using System;
using Tools.Quest.Data;

namespace Quest
{
    public enum QuestType
    {
        Level,
        Collect,
        Kill
    }
    /// <summary>
    /// 任务视图数据
    /// isExpand 是否展开(不展开的项显示大任务和任务奖励,否则展示任务步骤）
    /// templateName 任务视图模板名称
    /// </summary>
    [Serializable]
    public class QuestViewData
    {
        public string questId;
        public string title;
        public string fixedDescrption;
        public string dynamicDescription;
        /*public QuestType questType;*/
        public bool isExpand = false;
        public string templateName;
        
        public QuestViewData(string questId, string templateName)
        {
            this.questId = questId;
            this.templateName = templateName;
        }
        public QuestViewData()
        {
            
        }
    }
}