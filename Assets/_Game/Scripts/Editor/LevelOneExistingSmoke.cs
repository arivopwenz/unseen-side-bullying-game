using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.AI;
using UnityEditor;
using BullyingGame.LevelOne;
using BullyingGame.Events;
using BullyingGame.NPC;

namespace BullyingGame.EditorTools
{
    [InitializeOnLoad]
    public static class LevelOneExistingSmoke
    {
        const string Key = "LevelOneExisting.Smoke";
        const string Report = "Logs/LevelOnePRD/ExistingFlow-Smoke.txt";
        static IEnumerator routine;
        static double deadline;
        static LevelOneExistingSmoke() { EditorApplication.update += Update; }
        public static void Begin()
        {
            if (EditorApplication.isPlaying) throw new Exception("Edit Mode required");
            Directory.CreateDirectory("Logs/LevelOnePRD"); File.WriteAllText(Report, "Original zone and authored Timeline integration\n");
            string save = Path.GetFullPath("Logs/LevelOnePRD/ExistingFlow-Test.json"); File.WriteAllText(save, JsonUtility.ToJson(new LevelOneSaveData()));
            SessionState.SetString("LevelOnePRD.TestSave", save); SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
            SessionState.SetBool("LevelOneExisting.OwnsSaveOverride", true);
        }
        static void Update()
        {
            if (!SessionState.GetBool(Key, false))
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && SessionState.GetBool("LevelOneExisting.OwnsSaveOverride", false))
                {
                    SessionState.EraseString("LevelOnePRD.TestSave"); SessionState.EraseBool("LevelOneExisting.OwnsSaveOverride"); routine = null;
                }
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                SessionState.EraseBool(Key); SessionState.EraseString("LevelOnePRD.TestSave"); routine = null; return;
            }
            try
            {
                if (routine == null) routine = Run();
                if (!routine.MoveNext()) { File.AppendAllText(Report, "SUCCESS\n"); SessionState.EraseBool(Key); EditorApplication.isPlaying = false; }
            }
            catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); SessionState.EraseBool(Key); EditorApplication.isPlaying = false; }
        }
        static void Check(bool condition, string message) { if (!condition) throw new Exception(message); File.AppendAllText(Report, "PASS " + message + "\n"); }
        static LevelOneDirector Director => UnityEngine.Object.FindAnyObjectByType<LevelOneDirector>();
        static LevelOnePoint Point(string id) => UnityEngine.Object.FindObjectsByType<LevelOnePoint>().Single(p => p.Id == id);
        static void MovePlayer(Vector3 position)
        {
            var player = Director.Player; var c = player.GetComponent<CharacterController>(); c.enabled = false; player.transform.position = position; c.enabled = true; Physics.SyncTransforms();
        }
        static Button Button(string label) => UnityEngine.Object.FindObjectsByType<Button>().FirstOrDefault(b => b.name == label && b.gameObject.activeInHierarchy && b.interactable);
        static void Click(string label) { var b = Button(label); Check(b != null, "button " + label); b.onClick.Invoke(); }
        static void Drain() { int i = 0; while (Button("Lanjut") != null) { if (i++ > 30) throw new Exception("dialogue loop"); Button("Lanjut").onClick.Invoke(); } }
        static void Use(string id)
        {
            var p = Point(id); MovePlayer(p.transform.position + Vector3.back * 1.7f); Check(Director.IsReachable(p), "reachable " + id); Director.Use(p); Drain();
        }
        static void Solve(string id)
        {
            var data = AssetDatabase.LoadAssetAtPath<WordCardData>("Assets/_Game/Data/LevelOnePRD/" + id + ".asset");
            foreach (string answer in data.Answer) Click(data.TextFor(answer)); Click("Ucapkan"); Drain();
        }
        static void Timeout(double seconds) { deadline = EditorApplication.timeSinceStartup + seconds; }
        static void Tick() { if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timed out waiting for existing flow at " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + " step=" + (Director == null ? -1 : Director.StepIndex)); }
        static IEnumerator Run()
        {
            Application.runInBackground = true;
            Timeout(180); while (Director == null || !Director.Ready || LevelOneExistingFlow.Current == null || Director.StepIndex == 0) { Tick(); yield return null; }
            Check(Director.UsesExistingFlow, "original flow adapter active");
            var guide = GameObject.Find("Bima_Guide").GetComponent<GuideController>();
            Check(guide.isActiveAndEnabled, "original Bima_Guide running");
            Check(!GameObject.Find(LevelOnePRDAuthoring.RootName).transform.Find("03 - Pemeran PRD/Bima").gameObject.activeSelf, "duplicate Bima disabled");
            Check(!LevelOneExistingFlow.Current.TryClassroomZone(), "class zone locked before ambush and K-01");
            guide.GetComponent<NavMeshAgent>().Warp(new Vector3(-16.1f, .1f, -15.1f)); guide.PauseGuide();
            MovePlayer(new Vector3(-16.1f, .1f, -13.5f));
            Timeout(35);
            while (!Director.IsModal)
            {
                Tick(); var dialogue = UnityEngine.Object.FindAnyObjectByType<AmbushDialogueDriver>(); if (dialogue != null) dialogue.RequestAdvance(); yield return null;
            }
            Check(Director.ExternalDone("corridor"), "original AmbushTrigger completed and fired card hook");
            Solve("K-01"); Check(Director.ExternalDone("check_bima"), "K-01 recorded after original ambush");
            MovePlayer(GameObject.Find("GuideRoute/Zone_PerkenalanKelas").transform.position);
            Timeout(180); while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Cutscene perkenalan kelas") { Tick(); yield return null; }
            var classDirector = UnityEngine.Object.FindObjectsByType<PlayableDirector>().First(d => d.name == "Cinemachine");
            Check(classDirector.playableAsset.name == "MasukKelas", "existing MasukKelas asset used");
            while (classDirector.state != PlayState.Playing) { Tick(); yield return null; }
            classDirector.time = classDirector.duration - 1; classDirector.Evaluate();
            File.AppendAllText(Report, "NOTE: test seeks to final second to verify natural Timeline completion; this is not a full-duration playback review.\n");
            Timeout(180); while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Level01") { Tick(); yield return null; }
            while (Director == null || !Director.Ready || Director.StepIndex != 5) { Tick(); yield return null; }
            Check(Director.ExternalDone("introduction"), "class Timeline natural completion restores Level01 at chair inspection");
            var conversation = UnityEngine.Object.FindAnyObjectByType<BimaConversationDriver>(); conversation.autoAdvance = true;
            Use("Sikka"); Check(Director.StepIndex == 6, "chair inspection continues original canteen conversation");
            Timeout(90); while (!BimaConversationDriver.ConversationDone) { Tick(); yield return null; }
            MovePlayer(GameObject.Find("Canteen Cutscene Trigger").transform.position);
            Timeout(180); while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "BekalCutscene") { Tick(); yield return null; }
            var canteen = UnityEngine.Object.FindObjectsByType<PlayableDirector>().First(d => d.name == "Timeline Canteen");
            Check(canteen.playableAsset.name == "Canteen", "existing Canteen asset used");
            while (canteen.state != PlayState.Playing) { Tick(); yield return null; }
            canteen.time = canteen.duration - 1; canteen.Evaluate();
            Timeout(180); while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Level01") { Tick(); yield return null; }
            while (Director == null || !Director.Ready || Director.StepIndex != 7) { Tick(); yield return null; }
            Check(Director.ExternalDone("canteen"), "canteen Timeline naturally returns at K-02 without a replay loop");
            Use("BimaCanteen"); Solve("K-02"); Use("Fatimah"); Check(Director.Searching, "PRD search follows original cutscenes");
            Use("P3"); Use("Staff"); Use("P3"); Use("P5"); Use("P5"); Check(Director.HeldCount == 2, "both PRD items preserved after scene transitions");
            // Navigation of the original Bima to the return bench is verified as a complete path.
            var native = GameObject.Find("Bima_Guide").GetComponent<NavMeshAgent>(); var bench = Point("Bench");
            var path = new NavMeshPath(); Check(native.CalculatePath(bench.transform.position, path) && path.status == NavMeshPathStatus.PathComplete, "original Bima can reach garden bench");
            Use("Bench"); Use("Bench"); Solve("K-03"); Use("Bench");
            Check(Director.Complete, "complete Level 1 using original zones and authored Timelines");
            Check(!LevelOneExistingFlow.Current.TryClassroomZone() && !LevelOneExistingFlow.Current.TryCanteenZone("BekalCutscene"), "completed zones cannot replay quests");
        }
    }
}
