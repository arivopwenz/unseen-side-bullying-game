using System;
using System.Collections.Generic;
using UnityEngine;

namespace BullyingGame.Quest
{
    [Serializable]
    public class QuestProgressRecord
    {
        public string questId;
        public QuestState state;
        public int[] amounts;
        public string[] objectiveIds;
    }

    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }
        public event Action<QuestData, QuestState> OnQuestStateChanged;
        public event Action<QuestData, QuestObjective> OnObjectiveProgress;
        private readonly Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();
        private readonly Dictionary<string, QuestObjective[]> questProgress = new Dictionary<string, QuestObjective[]>();
        private readonly Dictionary<string, QuestData> definitions = new Dictionary<string, QuestData>();
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
        public static bool IsValid(QuestData quest)
        {
            if (quest == null || string.IsNullOrWhiteSpace(quest.questId) || quest.objectives == null || quest.objectives.Length == 0) return false;
            var ids = new HashSet<string>();
            foreach (var objective in quest.objectives)
                if (objective == null || string.IsNullOrWhiteSpace(objective.objectiveId) ||
                    objective.requiredAmount <= 0 || !ids.Add(objective.objectiveId)) return false;
            return true;
        }
        public bool PrerequisitesCompleted(QuestData quest)
        {
            if (!IsValid(quest)) return false;
            if (quest.prerequisites != null)
                foreach (var prerequisite in quest.prerequisites)
                    if (prerequisite == null || prerequisite == quest || GetQuestState(prerequisite) != QuestState.Completed) return false;
            return true;
        }
        public void StartQuest(QuestData quest) => TryStartQuest(quest);
        public bool TryStartQuest(QuestData quest)
        {
            if (!IsValid(quest) || GetQuestState(quest) != QuestState.Available || !PrerequisitesCompleted(quest)) return false;
            if (definitions.TryGetValue(quest.questId, out var known) && known != quest) return false;
            definitions[quest.questId] = quest;
            if (!questProgress.ContainsKey(quest.questId)) questProgress[quest.questId] = CopyObjectives(quest);
            SetQuestState(quest, QuestState.Active);
            return true;
        }
        private static QuestObjective[] CopyObjectives(QuestData quest)
        {
            var objectives = new QuestObjective[quest.objectives.Length];
            for (int i=0; i<objectives.Length; i++)
                objectives[i] = new QuestObjective { objectiveId=quest.objectives[i].objectiveId,
                    description=quest.objectives[i].description, requiredAmount=quest.objectives[i].requiredAmount, currentAmount=0 };
            return objectives;
        }
        public void UpdateObjective(QuestData quest, string objectiveId, int amount=1)
        {
            if (amount<=0 || GetQuestState(quest)!=QuestState.Active || !questProgress.TryGetValue(quest.questId,out var objectives)) return;
            foreach (var objective in objectives)
            {
                if (objective.objectiveId != objectiveId || objective.IsCompleted) continue;
                objective.AddProgress(amount);
                OnObjectiveProgress?.Invoke(quest,objective);
                break;
            }
            foreach (var objective in objectives) if (!objective.IsCompleted) return;
            SetQuestState(quest,QuestState.Completed);
        }
        public QuestState GetQuestState(QuestData quest) => quest != null && !string.IsNullOrWhiteSpace(quest.questId) &&
            questStates.TryGetValue(quest.questId,out var state) ? state : QuestState.Locked;
        public void MakeQuestAvailable(QuestData quest)
        {
            if (!IsValid(quest) || !PrerequisitesCompleted(quest) || GetQuestState(quest)!=QuestState.Locked) return;
            if (definitions.TryGetValue(quest.questId,out var known) && known != quest) return;
            definitions[quest.questId]=quest;
            SetQuestState(quest,QuestState.Available);
        }
        public QuestObjective[] GetObjectives(QuestData quest) => quest != null && !string.IsNullOrWhiteSpace(quest.questId) &&
            questProgress.TryGetValue(quest.questId,out var objectives) ? objectives : null;
        public bool IsObjectiveCompleted(QuestData quest,string objectiveId)
        {
            var objectives=GetObjectives(quest);
            if(objectives!=null) foreach(var objective in objectives)
                if(objective.objectiveId==objectiveId) return objective.IsCompleted;
            return false;
        }
        public QuestProgressRecord[] CaptureProgress()
        {
            var records = new List<QuestProgressRecord>();
            foreach (var entry in definitions)
            {
                var objectives=GetObjectives(entry.Value);
                var amounts=objectives==null ? new int[0] : new int[objectives.Length];
                var ids=new string[amounts.Length];
                for(int i=0;i<amounts.Length;i++) { amounts[i]=objectives[i].currentAmount; ids[i]=objectives[i].objectiveId; }
                records.Add(new QuestProgressRecord { questId=entry.Key,state=GetQuestState(entry.Value),amounts=amounts,objectiveIds=ids });
            }
            return records.ToArray();
        }
        public void RestoreProgress(QuestProgressRecord[] records,QuestData[] knownQuests)
        {
            questStates.Clear();questProgress.Clear();definitions.Clear();
            if(records==null || knownQuests==null) return;
            var map=new Dictionary<string,QuestData>();
            foreach(var quest in knownQuests) if(IsValid(quest) && !map.ContainsKey(quest.questId)) map.Add(quest.questId,quest);
            foreach(var record in records)
            {
                if(record==null || record.questId==null || !map.TryGetValue(record.questId,out var quest) ||
                    !Enum.IsDefined(typeof(QuestState),record.state)) continue;
                definitions[quest.questId]=quest;
                var objectives=CopyObjectives(quest);
                if(record.amounts!=null)
                    for(int i=0;i<record.amounts.Length;i++)
                    {
                        int destination=i;
                        if(record.objectiveIds!=null && i<record.objectiveIds.Length)
                            destination=Array.FindIndex(objectives,o=>o.objectiveId==record.objectiveIds[i]);
                        if(destination>=0 && destination<objectives.Length)
                            objectives[destination].currentAmount=Mathf.Clamp(record.amounts[i],0,objectives[destination].requiredAmount);
                    }
                questProgress[quest.questId]=objectives;
                bool complete=true;foreach(var objective in objectives) complete &= objective.IsCompleted;
                questStates[quest.questId]=complete && (record.state==QuestState.Active || record.state==QuestState.Completed) ? QuestState.Completed :
                    record.state==QuestState.Completed ? QuestState.Active : record.state;
            }
        }
        private void SetQuestState(QuestData quest,QuestState state)
        {
            questStates[quest.questId]=state;OnQuestStateChanged?.Invoke(quest,state);
        }
        private void OnDestroy() { if(Instance==this) Instance=null; }
    }
}
