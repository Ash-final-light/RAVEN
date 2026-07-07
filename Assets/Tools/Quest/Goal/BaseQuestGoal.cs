using System;
using Sirenix.OdinInspector;

namespace Tools.Quest.Goal
{
    [Serializable]
    public abstract class BaseQuestGoal
    {
        [InlineButton("ResetId")] public string id = Guid.NewGuid().ToString();
        [Title("任务过程动态描述"),ShowInInspector] public string description = "";
        [ShowInInspector, ReadOnly] public string preview = "";
        private void ResetId()
        {
            id = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 开始目标函数节点
        /// </summary>
        /// <param name="goal">目标运行时数据</param>
        public void Start(QuestGoal goal)
        {
            DeserializeFromState(goal.state);
            StartGoal(goal);
        }

        /// <summary>
        /// 更新目标函数节点
        /// </summary>
        /// <param name="goal">目标运行时数据</param>
        /// <param name="deltaTime">更新时间间隔</param>
        public void Update(QuestGoal goal, float deltaTime)
        {
            UpdateGoal(goal, deltaTime);
            goal.description = FormatDescription();
            goal.state = SerializeToState();
        }

        /// <summary>
        /// 完成目标函数节点
        /// </summary>
        /// <param name="goal">目标运行时数据</param>
        public void Complete(QuestGoal goal)
        {
            EndGoal(goal);
        }

        /// <summary>
        /// 中断目标函数节点
        /// </summary>
        /// <param name="goal">目标运行时数据</param>
        public void Interrupt(QuestGoal goal)
        {
            InterruptGoal(goal);
        }
        public abstract void StartGoal(QuestGoal goal);
        public abstract void UpdateGoal(QuestGoal goal,float deltaTime);
        public abstract void EndGoal(QuestGoal goal);
        public abstract void StopGoal(QuestGoal goal);
        public abstract void InterruptGoal(QuestGoal goal);
        protected abstract string FormatDescription(); // 格式化目标描述的函数
        
        protected abstract string SerializeToState(); // 将目标内部数据序列化为状态字符串的函数

        protected abstract void DeserializeFromState(string state); // 从状态字符串反序列化为目标内部数据的函数
    }
}