namespace Tools.Quest.Step
{
    /// <summary>
    /// 完成所有任务中的一个即可（不限制顺序）
    /// </summary>
    public class ParallelOrQuestStep:BaseQuestStep
    {
        public override void StartStep(QuestStep step) { }

        public override void UpdateStep(QuestStep step, float deltaTime)
        {
            if (step.goals.Length == 0)
            {
                step.completed = true;
                return;
            }

            Tools.Quest.Goal.QuestGoal completedGoal = null;
            foreach (var goal in step.goals)
            {
                if (!goal.triggered)
                {
                    goal.info.Start?.Invoke(goal);
                    goal.triggered = true;
                }

                if (!goal.completed)
                    goal.info.Update?.Invoke(goal, deltaTime);

                if (goal.completed)
                {
                    completedGoal = goal;
                    if (!goal.completionHandled)
                    {
                        goal.info.Complete?.Invoke(goal);
                        goal.completionHandled = true;
                    }
                    break;
                }
            }

            if (completedGoal == null)
                return;

            foreach (var goal in step.goals)
                if (goal != completedGoal && goal.triggered && !goal.completed)
                    goal.info.Interrupt?.Invoke(goal);
            step.completed = true;
        }

        public override void EndStep(QuestStep step)
        {
        }

        public override void StopStep(QuestStep step)
        {
            InterruptActiveGoals(step);
        }

        public override void InterruptStep(QuestStep step)
        {
            InterruptActiveGoals(step);
        }

        private static void InterruptActiveGoals(QuestStep step)
        {
            foreach (var goal in step.goals)
                if (goal.triggered && !goal.completed)
                    goal.info.Interrupt?.Invoke(goal);
        }
    }
}
