using System;
using System.IO;
using UnityEngine;
using BullyingGame.Quest;

namespace BullyingGame.LevelOne
{
    [Serializable]
    public class LevelOneSaveData
    {
        public int version = 1;
        public int step;
        public bool bottleFound, lunchFound, bottleTaken, lunchTaken, returned, permission, sikka;
        public string[] inspected = new string[0];
        public string[] journal = new string[0];
        public QuestProgressRecord[] quests;
        public Vector3 position;
        public bool hasPosition;
        public bool actionCardShown, wordCardShown, hasBimaPosition;
        public Vector3 bimaPosition;
        public string[] wordSlots = new string[0];
        public string[] wordOrder = new string[0];
        public int[] puzzleFailures = new int[3];
        public string[] externalCompleted = new string[0];
        public string pendingCutscene = "";
    }
    public static class LevelOneCheckpoint
    {
        // Separate schema: never overwrite the user's older five-chapter campaign save.
        public static bool UseDayTwo { get; set; }
        public static string PathName => DefaultPath(UseDayTwo);
        static string DefaultPath(bool dayTwo) => System.IO.Path.Combine(Application.persistentDataPath,
            dayTwo ? "unseen-level01-day-two.json" : "unseen-level01-cards-v1.json");
        public static string OverridePath { get; set; }
        static string ResolvePath(bool dayTwo)
        {
#if UNITY_EDITOR
            string test = UnityEditor.SessionState.GetString("LevelOnePRD.TestSave", "");
            if (!string.IsNullOrEmpty(test)) return test;
#endif
            return string.IsNullOrEmpty(OverridePath) ? DefaultPath(dayTwo) : OverridePath;
        }
        public static bool IsValid(LevelOneSaveData data, int count) => IsValid(data, count, UseDayTwo);
        static bool IsValid(LevelOneSaveData data, int count, bool dayTwo) =>
            data != null && data.version == (dayTwo ? 4 : 1) &&
            data.step >= 0 && data.step <= count && data.journal != null && data.inspected != null &&
            data.puzzleFailures != null && data.puzzleFailures.Length == 3 &&
            (!data.returned || (data.bottleTaken && (dayTwo || data.lunchTaken)));
        public static LevelOneSaveData Load(int count) => Load(count, UseDayTwo);
        static LevelOneSaveData Load(int count, bool dayTwo)
        {
            string primary = ResolvePath(dayTwo);
            foreach (string path in new[] { primary, primary + ".bak" })
            {
                try
                {
                    if (!File.Exists(path)) continue;
                    var data = JsonUtility.FromJson<LevelOneSaveData>(File.ReadAllText(path));
                    if (IsValid(data, count, dayTwo)) return data;
                }
                catch (Exception e) { Debug.LogWarning("Level01 checkpoint: " + e.Message); }
            }
            return NewProgress(dayTwo);
        }
        public static LevelOneSaveData NewProgress() => NewProgress(UseDayTwo);
        static LevelOneSaveData NewProgress(bool dayTwo) => new LevelOneSaveData { version = dayTwo ? 4 : 1 };
        public static bool HasDayTwoProgress => Load(int.MaxValue, true).step > 0;
        public static bool ResetDayTwo() => Write(NewProgress(true), true);
        public static bool Write(LevelOneSaveData data) => Write(data, UseDayTwo);
        static bool Write(LevelOneSaveData data, bool dayTwo)
        {
            string path = ResolvePath(dayTwo);
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path + ".tmp", json);
                if (!IsValid(JsonUtility.FromJson<LevelOneSaveData>(File.ReadAllText(path + ".tmp")), int.MaxValue, dayTwo)) return false;
                if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
                else File.Move(path + ".tmp", path);
                return true;
            }
            catch (Exception e) { Debug.LogWarning("Level01 save failed: " + e.Message); return false; }
        }
    }
}
