using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using BullyingGame.LevelOne;
using BullyingGame.Quest;

namespace BullyingGame.EditorTools
{
    [InitializeOnLoad]
    public static class LevelOnePRDSmoke
    {
        const string Stage = "LevelOnePRD.SmokeStage";
        static double next;
        static System.Collections.IEnumerator routine;
        static LevelOnePRDSmoke() { EditorApplication.update += Update; }
        static string Report => "Logs/LevelOnePRD/Smoke-" + SessionState.GetInt("LevelOnePRD.Variant", 0) + ".txt";
        static void Log(string text) { Directory.CreateDirectory("Logs/LevelOnePRD"); File.AppendAllText(Report, text + "\n"); }
        static void Check(bool result, string message) { if (!result) throw new Exception(message); Log("PASS " + message); }
        public static void Begin(int variant)
        {
            if (EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Level01") throw new Exception("Level01 Edit Mode required.");
            SessionState.SetInt("LevelOnePRD.Variant", variant);
            Directory.CreateDirectory("Logs/LevelOnePRD"); File.WriteAllText(Report, "Level01 PRD integration — variant " + variant + "\n");
            string save = Path.GetFullPath("Logs/LevelOnePRD/Checkpoint-" + variant + ".json");
            File.WriteAllText(save, JsonUtility.ToJson(new LevelOneSaveData()));
            SessionState.SetString("LevelOnePRD.TestSave", save); SessionState.SetInt(Stage, 1);
            EditorApplication.isPlaying = true;
        }
        static void Update()
        {
            int stage = SessionState.GetInt(Stage, 0);
            if (stage == 0 || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.timeSinceStartup < next) return;
            next = EditorApplication.timeSinceStartup + .25;
            try
            {
                if (!EditorApplication.isPlaying)
                {
                    if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                    if (stage == 2) { SessionState.SetInt(Stage, 3); EditorApplication.isPlaying = true; }
                    else if (stage == 4) { SessionState.EraseString("LevelOnePRD.TestSave"); SessionState.EraseInt(Stage); }
                    return;
                }
                var d = UnityEngine.Object.FindAnyObjectByType<LevelOneDirector>();
                if (d == null || !d.Ready) return;
                if (stage == 1)
                {
                    if (routine == null) routine = FirstHalf(d);
                    if (!routine.MoveNext()) routine = null;
                }
                else if (stage == 3) SecondHalf(d);
            }
            catch (Exception e)
            {
                Log("FAIL " + e); Debug.LogError("[Level01 PRD smoke] " + e.Message);
                SessionState.SetInt(Stage, 4); EditorApplication.isPlaying = false;
            }
        }
        static LevelOnePoint Point(string id) => UnityEngine.Object.FindObjectsByType<LevelOnePoint>().Single(p => p.Id == id);
        static void Teleport(LevelOneDirector d, LevelOnePoint p)
        {
            var c = d.Player.GetComponent<CharacterController>(); c.enabled = false;
            // Find an accessible side of the marker, rather than intersecting the NPC or furniture.
            var candidates = new[] { new Vector3(0,0,-1.8f), new Vector3(1.8f,0,0), new Vector3(-1.8f,0,0), new Vector3(0,0,1.8f), Vector3.zero };
            bool found = false;
            foreach (var offset in candidates)
            {
                d.Player.transform.position = p.transform.position + offset; Physics.SyncTransforms();
                if (d.IsReachable(p)) { found = true; break; }
            }
            c.enabled = true; Physics.SyncTransforms();
            Check(found, "line of sight and interaction distance for " + p.Id);
        }
        static Button Button(string label) => UnityEngine.Object.FindAnyObjectByType<LevelOneView>().GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == label && b.gameObject.activeInHierarchy);
        static void Click(string label)
        {
            var b = Button(label); if (b == null || !b.interactable) throw new Exception("Button unavailable: " + label); b.onClick.Invoke();
        }
        static void Drain()
        {
            int guard = 0;
            while (Button("Lanjut") != null && Button("Lanjut").interactable) { if (++guard > 40) throw new Exception("Dialogue deadlock"); Click("Lanjut"); }
        }
        static void Use(LevelOneDirector d, string id)
        {
            var p = Point(id); Teleport(d, p); Check(p.CanInteract(), "context available " + id);
            d.Use(p); Drain();
        }
        static void Solve(LevelOneDirector d, bool exerciseRetry)
        {
            var step = d.Step; var data = step.puzzle; int index = d.StepIndex;
            Check(data != null, "word puzzle active at " + step.id);
            if (exerciseRetry)
            {
                Click(data.TextFor(data.Answer[1])); Click(data.TextFor(data.Answer[0])); Click(data.TextFor(data.Answer[2]));
                for (int i = 0; i < 4; i++) Click("Ucapkan");
                Check(d.StepIndex == index, "wrong ordered phrases do not progress " + data.Id);
                Check(UnityEngine.Object.FindAnyObjectByType<LevelOneView>().GetComponentsInChildren<TMPro.TMP_Text>().Any(t => t.text.Contains("Petunjuk:")), "hint after repeated errors");
                Check(Button("Bantu susun") != null, "auto arrangement after four errors"); Click("Bantu susun");
            }
            else foreach (var id in data.Answer) Click(data.TextFor(id));
            Click("Ucapkan"); Drain(); Check(d.StepIndex == index + 1, "word card accepted exactly once " + data.Id);
        }
        static System.Collections.IEnumerator FirstHalf(LevelOneDirector d)
        {
            Check(d.StepIndex == 0, "fresh isolated checkpoint");
            Check(!Point("P3").CanInteract(), "item cannot be taken before its quest");
            int start = d.StepIndex; d.Use(Point("MeetBima")); Check(d.StepIndex == start, "out of range input cannot advance");
            Use(d, "MeetBima"); Teleport(d, Point("ClassFront"));
            while (!Point("ClassFront").CanInteract()) yield return null;
            Use(d, "ClassFront");
            Check(!Point("BimaCorridor").CanInteract(), "K-01 locked before confrontation leaves");
            Use(d, "Corridor");
            while (Button("Lanjut") == null) yield return null;
            Drain();
            while (d.IsModal) yield return null;
            var memet = GameObject.Find(LevelOnePRDAuthoring.RootName).transform.Find("03 - Pemeran PRD/Memet");
            Check(!memet.gameObject.activeSelf, "Memet leaves before K-01");
            Use(d, "BimaCorridor"); Solve(d, true);
            Use(d, "Teacher"); Use(d, "Sikka"); Check(d.Progress.sikka, "Sikka information flag recorded");
            Use(d, "Canteen");
            while (Button("Lanjut") == null) yield return null;
            Drain();
            while (d.IsModal) yield return null;
            Use(d, "BimaCanteen"); Solve(d, false); Use(d, "Fatimah");
            Check(d.Searching && d.HeldCount == 0, "search starts at 0/2");
            Use(d, "P2"); Check(d.HeldCount == 0, "other student's bag never enters inventory");
            Use(d, "P1"); Use(d, "P4");
            bool reverse = SessionState.GetInt("LevelOnePRD.Variant", 0) == 1;
            if (!reverse)
            {
                Use(d, "P3"); Use(d, "P3"); Check(!d.Progress.bottleTaken, "bottle blocked without staff permission");
                Use(d, "Staff"); Use(d, "P3");
            }
            else { Use(d, "P5"); Use(d, "P5"); }
            Check(d.HeldCount == 1 && d.Searching, "either item order produces 1/2");
            Check(!Point("Bench").CanInteract(), "cannot return before both items collected");
            d.Save(); var loaded = LevelOneCheckpoint.Load(13);
            Check(loaded.bottleTaken == d.Progress.bottleTaken && loaded.lunchTaken == d.Progress.lunchTaken, "checkpoint round trip preserves unique item IDs");
            SessionState.SetInt(Stage, 2); Log("Restarting Play Mode to verify actual resume at 1/2."); EditorApplication.isPlaying = false;
        }
        static void SecondHalf(LevelOneDirector d)
        {
            Check(d.Searching && d.HeldCount == 1, "actual Play Mode reload restores 1/2 and search quest");
            bool reverse = SessionState.GetInt("LevelOnePRD.Variant", 0) == 1;
            if (reverse) { Use(d, "P3"); Use(d, "Staff"); Use(d, "P3"); }
            else { Use(d, "P5"); Use(d, "P5"); }
            Check(d.HeldCount == 2 && d.StepIndex == 10, "both collection orders reach 2/2 exactly once");
            Use(d, "Bench"); Check(d.Progress.returned && d.HeldCount == 0, "2/2 items transferred to owner");
            Check(!d.Complete, "returning items alone does not complete level");
            Use(d, "Bench"); Solve(d, false); Check(!d.Complete, "K-03 requires incident record before finish");
            Use(d, "Bench");
            Check(d.Complete, "all five quests complete");
            var records = QuestManager.Instance.CaptureProgress();
            Check(records.Length == 5 && records.All(q => q.state == QuestState.Completed), "five real QuestManager records completed");
            Check(d.JournalText.Contains("Candaan") && d.JournalText.Contains("barang pribadi") && d.JournalText.Contains("Dukungan korban"), "three required learning ideas retained in journal");
            Check(LevelOneCheckpoint.Load(13).step == 13, "ending checkpoint persisted");
            Log("SUCCESS: full campaign, puzzle retry and true save/resume verified.");
            SessionState.SetInt(Stage, 4); EditorApplication.isPlaying = false;
        }
    }
}
