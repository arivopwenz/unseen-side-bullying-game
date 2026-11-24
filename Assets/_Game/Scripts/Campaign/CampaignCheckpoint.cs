using System;
using System.IO;
using System.Linq;
using UnityEngine;
using BullyingGame.LevelOne;

namespace BullyingGame.Campaign
{
    [Serializable]
    public class CampaignChapterProgress
    {
        public int chapter;
        public int step, question, failures;
        public string stepId = "";
        public bool hasPosition, actionPresented;
        public Vector3 position;
        public string[] selectedOrder = Array.Empty<string>();
        public string[] orderBank = Array.Empty<string>();
        public LevelOneSaveData wordProgress = new LevelOneSaveData();
    }

    [Serializable]
    public class CampaignProgress
    {
        public int schemaVersion = 1;
        public string campaignId = "UNSEEN_SIDE";
        public int activeChapter = 1, unlockedChapter = 1;
        public bool complete;
        public string exhibitChoice = "UNSET", reportChoice = "UNSET";
        public string[] completedStepIds = Array.Empty<string>();
        public string[] journal = Array.Empty<string>();
        public CampaignChapterProgress[] chapters = Array.Empty<CampaignChapterProgress>();
    }

    public static class CampaignCheckpoint
    {
        public static string LastError { get; private set; }
        static bool blocked;
        public static string PathName
        {
            get
            {
#if UNITY_EDITOR
                var fixture = UnityEditor.SessionState.GetString("CampaignPRD.TestSave", "");
                if (!string.IsNullOrEmpty(fixture)) return fixture;
#endif
                return Path.Combine(Application.persistentDataPath, "unseen-campaign-final-v1.json");
            }
        }
        public static bool Valid(CampaignProgress data) => data != null && data.schemaVersion == 1 &&
            data.campaignId == "UNSEEN_SIDE" && data.activeChapter >= 1 && data.activeChapter <= 5 &&
            data.unlockedChapter >= data.activeChapter && data.unlockedChapter <= 5 &&
            data.chapters != null && data.chapters.All(p => p != null && p.chapter >= 2 && p.chapter <= 5 &&
                p.step >= 0 && p.wordProgress != null && p.selectedOrder != null && p.orderBank != null) &&
            data.chapters.Select(p => p.chapter).Distinct().Count() == data.chapters.Length &&
            data.completedStepIds != null && data.journal != null;

        public static CampaignProgress Load()
        {
            LastError = null;
            blocked = false;
            bool found = false;
            foreach (string path in new[] { PathName, PathName + ".bak" })
            {
                if (!File.Exists(path)) continue;
                found = true;
                try
                {
                    var data = JsonUtility.FromJson<CampaignProgress>(File.ReadAllText(path));
                    if (Valid(data)) { LastError = null; return data; }
                }
                catch (Exception e) when (e is IOException || e is ArgumentException || e is UnauthorizedAccessException)
                { LastError = e.Message; }
            }
            if (found)
            {
                blocked = true;
                LastError = "Checkpoint kampanye gagal dibaca. Pulihkan backup atau mulai cerita baru.";
            }
            return new CampaignProgress();
        }

        public static CampaignChapterProgress Chapter(CampaignProgress data, int chapter)
        {
            var result = data.chapters.FirstOrDefault(p => p.chapter == chapter);
            if (result != null) return result;
            result = new CampaignChapterProgress { chapter = chapter };
            data.chapters = data.chapters.Concat(new[] { result }).ToArray();
            return result;
        }

        public static bool Write(CampaignProgress data, bool reset = false)
        {
            if (!reset && blocked) return false;
            try
            {
                if (!Valid(data)) throw new ArgumentException("State kampanye tidak valid.");
                Directory.CreateDirectory(Path.GetDirectoryName(PathName));
                string temporary = PathName + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
                if (!Valid(JsonUtility.FromJson<CampaignProgress>(File.ReadAllText(temporary))))
                    throw new IOException("Verifikasi checkpoint gagal.");
                if (File.Exists(PathName)) File.Replace(temporary, PathName, PathName + ".bak");
                else File.Move(temporary, PathName);
                LastError = null;
                blocked = false;
                return true;
            }
            catch (Exception e) when (e is IOException || e is ArgumentException || e is UnauthorizedAccessException)
            { LastError = e.Message; return false; }
        }

        public static bool Reset() => Write(new CampaignProgress(), true);
        public static void AddJournal(CampaignProgress data, string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && !data.journal.Contains(text))
                data.journal = data.journal.Concat(new[] { text }).ToArray();
        }
        public static bool AcceptChapterOne()
        {
            var data = Load();
            if (LastError != null) return false;
            data.unlockedChapter = Mathf.Max(2, data.unlockedChapter);
            if (data.activeChapter == 1) data.activeChapter = 2;
            if (!data.completedStepIds.Contains("L01/report"))
                data.completedStepIds = data.completedStepIds.Concat(new[] { "L01/report" }).ToArray();
            AddJournal(data, "Bima • Observasi Aji: barang diambil di kantin. Botol dan bekal sudah dikembalikan.");
            return Write(data);
        }
    }
}
