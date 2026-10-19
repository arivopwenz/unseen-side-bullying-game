using System;
using System.Linq;
using UnityEngine;
using BullyingGame.Quest;
using BullyingGame.Save;

namespace BullyingGame.Core
{
    [Serializable] public struct MissionJournalBinding
    {
        public QuestData quest;
        public string objectiveId, caseId, status;
        [TextArea] public string text;
    }
    public class MissionJournal : MonoBehaviour
    {
        [Header("Public story facts only")]
        [SerializeField] private MissionJournalBinding[] entries;
        private QuestManager quests;
        private void Start()
        {
            quests=QuestManager.Instance;
            if(quests==null)return;
            quests.OnObjectiveProgress+=Progress;
        }
        private void Progress(QuestData quest,QuestObjective objective)
        {
            if(!objective.IsCompleted||entries==null)return;
            foreach(var entry in entries)
                if(entry.quest==quest&&entry.objectiveId==objective.objectiveId&&!string.IsNullOrWhiteSpace(entry.text))
                    SaveManager.Instance?.AddJournalEntry(quest.questId+"/"+entry.objectiveId,entry.caseId??"Sekolah",entry.text,string.IsNullOrEmpty(entry.status)?"Catatan fakta":entry.status);
        }
        private void OnDestroy(){if(quests!=null)quests.OnObjectiveProgress-=Progress;}
    }
}
