using System;

namespace BullyingGame.Quest
{
    [Serializable]
    public class QuestObjective
    {
        public string objectiveId;
        public string description;
        public int requiredAmount = 1;
        public int currentAmount;

        public bool IsCompleted => currentAmount >= requiredAmount;

        public void AddProgress(int amount = 1)
        {
            currentAmount = Math.Min(currentAmount + amount, requiredAmount);
        }

        public void Reset()
        {
            currentAmount = 0;
        }
    }
}
