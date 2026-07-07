namespace Tools.Quest.Step
{
    /// <summary>
    /// 完成所有任务中的所有任务（不限制顺序）
    /// </summary>
    public class ParallelAndQuestStep:BaseQuestStep
    {
        public override void StartStep(QuestStep step) { }

        public override void UpdateStep(QuestStep step, float deltaTime)
        {
            if (step.goals.Length == 0)
            {
                step.completed = true;
                return;
            }

            foreach (var goal in step.goals)
            {
                if (!goal.triggered)
                {
                    goal.info.Start?.Invoke(goal);
                    goal.triggered = true;
                }

                if (!goal.completed)
                    goal.info.Update?.Invoke(goal, deltaTime);

                if (goal.completed && !goal.completionHandled)
                {
                    goal.info.Complete?.Invoke(goal);
                    goal.completionHandled = true;
                }
            }

            step.completed = System.Array.TrueForAll(step.goals, goal => goal.completed);
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
