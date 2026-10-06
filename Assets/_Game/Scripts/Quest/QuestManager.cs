using System;
using System.Collections.Generic;
using UnityEngine;

namespace BullyingGame.Quest
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public event Action<QuestData, QuestState> OnQuestStateChanged;
        public event Action<QuestData, QuestObjective> OnObjectiveProgress;

        private readonly Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();
        private readonly Dictionary<string, QuestObjective[]> questProgress = new Dictionary<string, QuestObjective[]>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartQuest(QuestData quest)
        {
            if (quest == null || GetQuestState(quest) != QuestState.Available) return;

            var objectives = new QuestObjective[quest.objectives.Length];
            for (int i = 0; i < quest.objectives.Length; i++)
            {
                objectives[i] = new QuestObjective
                {
                    objectiveId = quest.objectives[i].objectiveId,
                    description = quest.objectives[i].description,
                    requiredAmount = quest.objectives[i].requiredAmount,
                    currentAmount = 0
                };
            }
            questProgress[quest.questId] = objectives;
            SetQuestState(quest, QuestState.Active);
        }

        public void UpdateObjective(QuestData quest, string objectiveId, int amount = 1)
        {
            if (GetQuestState(quest) != QuestState.Active) return;
            if (!questProgress.ContainsKey(quest.questId)) return;

            var objectives = questProgress[quest.questId];
            foreach (var objective in objectives)
            {
                if (objective.objectiveId == objectiveId)
                {
                    objective.AddProgress(amount);
                    OnObjectiveProgress?.Invoke(quest, objective);
                    break;
                }
            }

            CheckQuestCompletion(quest);
        }

        public QuestState GetQuestState(QuestData quest)
        {
            if (questStates.TryGetValue(quest.questId, out var state))
                return state;
            return QuestState.Locked;
        }

        public void MakeQuestAvailable(QuestData quest)
        {
            if (GetQuestState(quest) == QuestState.Locked)
                SetQuestState(quest, QuestState.Available);
        }

        private void SetQuestState(QuestData quest, QuestState newState)
        {
            questStates[quest.questId] = newState;
            OnQuestStateChanged?.Invoke(quest, newState);
        }

        private void CheckQuestCompletion(QuestData quest)
        {
            if (!questProgress.ContainsKey(quest.questId)) return;

            var objectives = questProgress[quest.questId];
            foreach (var objective in objectives)
            {
                if (!objective.IsCompleted) return;
            }

            SetQuestState(quest, QuestState.Completed);
        }

        public QuestObjective[] GetObjectives(QuestData quest)
        {
            if (questProgress.TryGetValue(quest.questId, out var objectives))
                return objectives;
            return null;
        }
    }
}
