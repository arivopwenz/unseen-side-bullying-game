using System;
using System.IO;
using UnityEngine;
using BullyingGame.Core;
using BullyingGame.Quest;

namespace BullyingGame.Save
{
    [Serializable]
    public class ReflectionCheckpoint
    {
        public string reflectionId;
        public bool correct;
        public bool closed;
    }

    [Serializable]
    public class NarrativeChoiceCheckpoint
    {
        public string choiceId;
        public int optionIndex;
    }

    [Serializable]
    public class GameSaveData
    {
        public int version = 3;
        public int currentChapter = 1;
        public POVType currentPOV = POVType.Victim;
        public int empathyScore;
        public QuestProgressRecord[] quests = new QuestProgressRecord[0];
        public string[] handledEvents = new string[0];
        public QuestItemCheckpoint[] items = new QuestItemCheckpoint[0];
        public bool hasPlayerPosition;
        public Vector3 playerPosition;
        public bool campaignCompleted;
        public ReflectionCheckpoint[] reflections = new ReflectionCheckpoint[0];
        public NarrativeChoiceCheckpoint[] narrativeChoices = new NarrativeChoiceCheckpoint[0];
    }

    [DefaultExecutionOrder(-90)]
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        public GameSaveData CurrentData { get; private set; } = new GameSaveData();
        public string LastError { get; private set; }
        public bool HasSave => File.Exists(saveFilePath);
        private string saveFilePath;
        private void Awake()
        {
            if(Instance!=null && Instance!=this) { Destroy(gameObject);return; }
            Instance=this;DontDestroyOnLoad(gameObject);
            saveFilePath=Path.Combine(Application.persistentDataPath,"savegame.json");
            LoadGame();
        }
        public void SaveGame() => TrySaveGame();
        public bool TrySaveGame()
        {
            string temporary=saveFilePath+".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(saveFilePath));
                File.WriteAllText(temporary,JsonUtility.ToJson(CurrentData,true));
                if(File.Exists(saveFilePath)) File.Replace(temporary,saveFilePath,saveFilePath+".bak");
                else File.Move(temporary,saveFilePath);
                LastError=null;
                return true;
            }
            catch(Exception error) when(error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            {
                LastError="Progress belum tersimpan: "+error.Message;
                Debug.LogWarning(LastError,this);
                return false;
            }
        }
        public void LoadGame()
        {
            if(!TryLoad(saveFilePath) && !TryLoad(saveFilePath+".bak")) CurrentData=new GameSaveData();
        }
        private bool TryLoad(string path)
        {
            if(!File.Exists(path)) return false;
            try
            {
                string json=File.ReadAllText(path);
                if(string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{")) return false;
                var data=JsonUtility.FromJson<GameSaveData>(json);
                if(data==null || data.version>3 || data.currentChapter<1 ||
                    !Enum.IsDefined(typeof(POVType),data.currentPOV)) return false;
                data.currentChapter=Mathf.Clamp(data.currentChapter,1,5);
                data.version=3;
                data.empathyScore=Mathf.Max(0,data.empathyScore);
                data.quests=data.quests??new QuestProgressRecord[0];
                data.handledEvents=data.handledEvents??new string[0];
                data.items=data.items??new QuestItemCheckpoint[0];
                data.reflections=data.reflections??new ReflectionCheckpoint[0];
                data.narrativeChoices=data.narrativeChoices??new NarrativeChoiceCheckpoint[0];
                CurrentData=data;LastError=null;return true;
            }
            catch(Exception error) when(error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            {
                LastError="Save tidak dapat dibaca: "+error.Message;
                Debug.LogWarning(LastError,this);return false;
            }
        }
        public void ResetProgress() { CurrentData=new GameSaveData(); SaveGame(); }
        public int GetNarrativeChoice(string id)
        {
            if(string.IsNullOrWhiteSpace(id)) return -1;
            foreach(var record in CurrentData.narrativeChoices)
                if(record != null && record.choiceId == id) return record.optionIndex;
            return -1;
        }
        public void RecordNarrativeChoice(string id, int index)
        {
            if(string.IsNullOrWhiteSpace(id) || index < 0 || index > 2) return;
            foreach(var record in CurrentData.narrativeChoices)
                if(record != null && record.choiceId == id) { record.optionIndex = index; return; }
            var choices = CurrentData.narrativeChoices;
            Array.Resize(ref choices, choices.Length + 1);
            choices[choices.Length - 1] = new NarrativeChoiceCheckpoint { choiceId = id, optionIndex = index };
            CurrentData.narrativeChoices = choices;
        }
        public void AddEmpathyScore(int amount) { CurrentData.empathyScore=Mathf.Max(0,CurrentData.empathyScore+amount); }
        public bool IsReflectionClosed(string id)
        {
            foreach(var result in CurrentData.reflections)
                if(result!=null && result.reflectionId==id && result.closed) return true;
            return false;
        }
        public void RecordReflectionAnswer(string id, bool correct)
        {
            if(string.IsNullOrWhiteSpace(id)) return;
            foreach(var result in CurrentData.reflections)
                if(result!=null && result.reflectionId==id) return;
            var results=CurrentData.reflections;
            Array.Resize(ref results,results.Length+1);
            results[results.Length-1]=new ReflectionCheckpoint { reflectionId=id, correct=correct };
            CurrentData.reflections=results;
            if(correct) AddEmpathyScore(10);
        }
        public void CloseReflection(string id)
        {
            foreach(var result in CurrentData.reflections)
                if(result!=null && result.reflectionId==id) { result.closed=true;return; }
        }
        private void OnDestroy() { if(Instance==this) Instance=null; }
    }
}
