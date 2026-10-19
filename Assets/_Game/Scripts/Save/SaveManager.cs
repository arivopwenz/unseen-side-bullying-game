using System;
using System.IO;
using System.Linq;
using UnityEngine;
using BullyingGame.Core;
using BullyingGame.Quest;

namespace BullyingGame.Save
{
    [Serializable] public class ReflectionCheckpoint
    {
        public string reflectionId;
        public bool correct, closed;
        public int firstAnswer = -1;
        public int[] retries = new int[0];
    }
    [Serializable] public class NarrativeChoiceCheckpoint { public string choiceId; public int optionIndex; }
    [Serializable] public class JournalEntryCheckpoint { public string entryId, caseId, text, status; }
    [Serializable] public class GameSaveData
    {
        public int version = 4;
        public string buildVersion = "PRD-1.1", chapterId = "CH01", sceneId = "Level_01", missionId = "M01", checkpointId = "CH01/start";
        public int currentChapter = 1;
        public POVType currentPOV = POVType.Protagonist;
        public int empathyScore;
        public QuestProgressRecord[] quests = new QuestProgressRecord[0];
        public string[] handledEvents = new string[0], storyFlags = new string[0], completedChapters = new string[0];
        public QuestItemCheckpoint[] items = new QuestItemCheckpoint[0];
        public bool hasPlayerPosition, campaignCompleted;
        public Vector3 playerPosition;
        public ReflectionCheckpoint[] reflections = new ReflectionCheckpoint[0];
        public NarrativeChoiceCheckpoint[] narrativeChoices = new NarrativeChoiceCheckpoint[0];
        public JournalEntryCheckpoint[] journalEntries = new JournalEntryCheckpoint[0];
    }
    [DefaultExecutionOrder(-90)]
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        public GameSaveData CurrentData { get; private set; } = new GameSaveData();
        public string LastError { get; private set; }
        public bool HasSave => !disableSaving && validSave;
        public bool CanSave => disableSaving || !blockedOverwrite;
        public event Action OnProgressChanged;
        [Header("Development")]
        [SerializeField] private bool disableSaving;
        private string saveFilePath;
        private bool validSave, blockedOverwrite;
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            // The previous multi-POV campaign has incompatible mission IDs. Its file is preserved.
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame_prd11.json");
#if UNITY_EDITOR
            string fixturePath = UnityEditor.SessionState.GetString("PRD.TestSavePath", "");
            if (!string.IsNullOrEmpty(fixturePath)) saveFilePath = fixturePath;
#endif
            LoadGame();
        }
        public void SaveGame() => TrySaveGame();
        public bool TrySaveGame()
        {
            if (disableSaving) return true;
            if (blockedOverwrite) return false;
            try
            {
                if (!Validate(CurrentData)) { LastError = "Progres belum valid. Kembali ke checkpoint atau mulai baru lewat menu."; return false; }
                Directory.CreateDirectory(Path.GetDirectoryName(saveFilePath));
                var temporary = saveFilePath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(CurrentData, true));
                if (!ReadValid(temporary, out _)) throw new IOException("Verifikasi file sementara gagal.");
                if (File.Exists(saveFilePath)) File.Replace(temporary, saveFilePath, saveFilePath + ".bak");
                else File.Move(temporary, saveFilePath);
                validSave = true; LastError = null; return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            { LastError = "Progres belum tersimpan. Coba lagi atau kembali ke menu; checkpoint sebelumnya tetap tersedia."; return false; }
        }
        public void LoadGame()
        {
            if (disableSaving) { CurrentData = new GameSaveData(); return; }
            validSave = false; blockedOverwrite = false;
            if (ReadValid(saveFilePath, out var data)) { CurrentData = data; validSave = true; LastError = null; }
            else if (ReadValid(saveFilePath + ".bak", out data))
            {
                CurrentData = data; validSave = true;
                // Preserve the good backup: move the corrupt primary out of the replacement path.
                try { if (File.Exists(saveFilePath)) File.Move(saveFilePath, saveFilePath + ".invalid-" + DateTime.UtcNow.Ticks); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { blockedOverwrite = true; }
                LastError = "Checkpoint cadangan dipulihkan. Progres terakhir yang valid dapat dilanjutkan.";
            }
            else
            {
                blockedOverwrite = File.Exists(saveFilePath) || File.Exists(saveFilePath + ".bak");
                CurrentData = new GameSaveData();
                LastError = blockedOverwrite ? "Save tidak dapat dipulihkan. Pilih Mulai baru dan konfirmasikan untuk membuat progres baru; file lama diarsipkan." : null;
            }
            OnProgressChanged?.Invoke();
        }
        private static bool ReadValid(string path, out GameSaveData data)
        {
            data = null;
            try
            {
                if (!File.Exists(path)) return false;
                var json = File.ReadAllText(path);
                if (!json.Contains("\"version\"") || !json.Contains("\"quests\"") || !json.TrimEnd().EndsWith("}")) return false;
                data = JsonUtility.FromJson<GameSaveData>(json);
                return Validate(data);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException) { return false; }
        }
        public static bool Validate(GameSaveData d)
        {
            if (d == null || d.version != 4 || d.buildVersion != "PRD-1.1" || d.currentChapter < 1 || d.currentChapter > 5 ||
                d.chapterId != "CH" + d.currentChapter.ToString("00") || d.sceneId != "Level_" + d.currentChapter.ToString("00") ||
                d.currentPOV != POVType.Protagonist || d.quests == null || d.reflections == null || d.narrativeChoices == null || d.journalEntries == null ||
                d.storyFlags == null || d.completedChapters == null || string.IsNullOrWhiteSpace(d.checkpointId)) return false;
            if (d.hasPlayerPosition && (!float.IsFinite(d.playerPosition.x) || !float.IsFinite(d.playerPosition.y) || !float.IsFinite(d.playerPosition.z))) return false;
            var questIds = new System.Collections.Generic.HashSet<string>();
            foreach (var q in d.quests)
                if (q == null || !questIds.Add(q.questId) || q.questId == null || q.questId.Length != 3 || q.questId[0] != 'M' ||
                    !int.TryParse(q.questId.Substring(1), out int n) || n < 1 || n > 15 || q.amounts == null || q.objectiveIds == null ||
                    q.amounts.Length != q.objectiveIds.Length || !Enum.IsDefined(typeof(QuestState), q.state) || q.amounts.Any(a => a < 0 || a > 1)) return false;
            return d.reflections.All(r => r != null && r.firstAnswer >= -1 && r.firstAnswer <= 2 && r.retries != null && r.retries.All(a=>a>=0&&a<=2)) &&
                d.narrativeChoices.All(c=>c!=null&&!string.IsNullOrWhiteSpace(c.choiceId)&&c.optionIndex>=0&&c.optionIndex<=2) &&
                d.journalEntries.All(e=>e!=null&&!string.IsNullOrWhiteSpace(e.entryId));
        }
        public bool TryResetProgress()
        {
            if (!disableSaving)
            {
                try
                {
                    foreach (var path in new[] {saveFilePath, saveFilePath + ".bak"})
                        if (File.Exists(path)) File.Copy(path, path + ".archive-" + DateTime.UtcNow.Ticks, false);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { LastError = "Progres lama belum dapat diarsipkan. Coba lagi."; return false; }
            }
            var previous = CurrentData;
            blockedOverwrite = false; CurrentData = new GameSaveData();
            if (TrySaveGame()) { OnProgressChanged?.Invoke(); return true; }
            CurrentData = previous; return false;
        }
        public void ResetProgress() => TryResetProgress();
        public void SetChapterCheckpoint(int number, string checkpoint)
        {
            CurrentData.currentChapter = number; CurrentData.currentPOV = POVType.Protagonist;
            CurrentData.chapterId = "CH" + number.ToString("00"); CurrentData.sceneId = "Level_" + number.ToString("00"); CurrentData.checkpointId = checkpoint;
        }
        public int GetNarrativeChoice(string id) => CurrentData.narrativeChoices.FirstOrDefault(c=>c.choiceId==id)?.optionIndex ?? -1;
        public void RecordNarrativeChoice(string id, int index)
        {
            if (string.IsNullOrWhiteSpace(id) || index < 0 || index > 2) return;
            var item = CurrentData.narrativeChoices.FirstOrDefault(c=>c.choiceId==id);
            if (item == null) CurrentData.narrativeChoices = CurrentData.narrativeChoices.Concat(new[]{new NarrativeChoiceCheckpoint{choiceId=id,optionIndex=index}}).ToArray();
            else item.optionIndex = index;
            OnProgressChanged?.Invoke();
        }
        public void AddJournalEntry(string id, string caseId, string text, string status)
        {
            if (string.IsNullOrWhiteSpace(id) || CurrentData.journalEntries.Any(e=>e.entryId==id)) return;
            CurrentData.journalEntries = CurrentData.journalEntries.Concat(new[]{new JournalEntryCheckpoint{entryId=id,caseId=caseId,text=text,status=status}}).ToArray();
            OnProgressChanged?.Invoke();
        }
        public bool HasStoryFlag(string id) => Array.IndexOf(CurrentData.storyFlags,id)>=0;
        public void SetStoryFlag(string id, bool value=true)
        {
            if (value && !HasStoryFlag(id)) CurrentData.storyFlags = CurrentData.storyFlags.Concat(new[]{id}).ToArray();
            else if (!value) CurrentData.storyFlags = CurrentData.storyFlags.Where(f=>f!=id).ToArray();
        }
        public void AddEmpathyScore(int amount) { } // No bravery/empathy score in the PRD campaign.
        public bool IsReflectionClosed(string id) => CurrentData.reflections.Any(r=>r.reflectionId==id&&r.closed);
        public void RecordReflectionAnswer(string id, bool correct) => RecordReflectionAnswer(id,-1,correct);
        public void RecordReflectionAnswer(string id, int answer, bool correct)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            var item = CurrentData.reflections.FirstOrDefault(r=>r.reflectionId==id);
            if (item == null) CurrentData.reflections = CurrentData.reflections.Concat(new[]{new ReflectionCheckpoint{reflectionId=id,firstAnswer=answer,correct=correct}}).ToArray();
            else if (!item.closed && answer >= 0) item.retries = item.retries.Concat(new[]{answer}).ToArray();
        }
        public void CloseReflection(string id) { var item=CurrentData.reflections.FirstOrDefault(r=>r.reflectionId==id); if(item!=null)item.closed=true; }
        private void OnDestroy() { if(Instance==this)Instance=null; }
    }
}
