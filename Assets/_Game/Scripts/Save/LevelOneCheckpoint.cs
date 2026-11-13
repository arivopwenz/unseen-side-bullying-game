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
        public int[] puzzleFailures = new int[3];
        public string[] externalCompleted = new string[0];
        public string pendingCutscene = "";
    }
    public static class LevelOneCheckpoint
    {
        // Separate schema: never overwrite the user's older five-chapter campaign save.
        public static string PathName => System.IO.Path.Combine(Application.persistentDataPath, "unseen-level01-cards-v1.json");
        public static string OverridePath { get; set; }
        static string Path
        {
            get
            {
#if UNITY_EDITOR
                string test = UnityEditor.SessionState.GetString("LevelOnePRD.TestSave", "");
                if (!string.IsNullOrEmpty(test)) return test;
#endif
                return string.IsNullOrEmpty(OverridePath) ? PathName : OverridePath;
            }
        }
        public static bool IsValid(LevelOneSaveData data, int count) => data != null && data.version == 1 &&
            data.step >= 0 && data.step <= count && data.journal != null && data.inspected != null &&
            data.puzzleFailures != null && data.puzzleFailures.Length == 3 &&
            (!data.returned || (data.bottleTaken && data.lunchTaken));
        public static LevelOneSaveData Load(int count)
        {
            foreach (string path in new[] { Path, Path + ".bak" })
            {
                try
                {
                    if (!File.Exists(path)) continue;
                    var data = JsonUtility.FromJson<LevelOneSaveData>(File.ReadAllText(path));
                    if (IsValid(data, count)) return data;
                }
                catch (Exception e) { Debug.LogWarning("Level01 checkpoint: " + e.Message); }
            }
            return new LevelOneSaveData();
        }
        public static bool Write(LevelOneSaveData data)
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(Path + ".tmp", json);
                if (!IsValid(JsonUtility.FromJson<LevelOneSaveData>(File.ReadAllText(Path + ".tmp")), int.MaxValue)) return false;
                if (File.Exists(Path)) File.Replace(Path + ".tmp", Path, Path + ".bak");
                else File.Move(Path + ".tmp", Path);
                return true;
            }
            catch (Exception e) { Debug.LogWarning("Level01 save failed: " + e.Message); return false; }
        }
    }
}
