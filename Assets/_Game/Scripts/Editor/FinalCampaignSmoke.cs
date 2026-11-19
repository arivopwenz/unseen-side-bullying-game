using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using BullyingGame.Campaign;
using BullyingGame.Core;

namespace BullyingGame.EditorTools
{
    [InitializeOnLoad]
    public static class FinalCampaignSmoke
    {
        const string Key = "FinalCampaign.Smoke";
        const string Report = "Logs/FinalCampaignSmoke.txt";
        static string stepId;
        static double deadline;
        static int wait;
        static bool waitingForMenu;
        static FinalCampaignSmoke() => EditorApplication.update += Tick;
        public static void Begin()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Edit Mode required.");
            var path = Path.GetFullPath("Logs/FinalCampaignTest.json");
            File.WriteAllText(path, JsonUtility.ToJson(new CampaignProgress { activeChapter = 2, unlockedChapter = 2 }));
            SessionState.SetString("CampaignPRD.TestSave", path);
            SessionState.SetString("PRD.TestSavePath", Path.GetFullPath("Logs/FinalCampaignLegacyTest.json"));
            SessionState.SetBool(Key, true);
            File.WriteAllText(Report, "Final campaign interaction smoke\n");
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Levels/Level_02/Level02.unity");
            stepId = null;
            wait = 0;
            waitingForMenu = false;
            Application.runInBackground = true;
            EditorApplication.isPlaying = true;
        }
        static Button Button(string label) => UnityEngine.Object.FindObjectsByType<Button>()
            .FirstOrDefault(b => b.gameObject.activeInHierarchy && b.interactable && b.name == label);
        static void Click(Button button) { button.onClick.Invoke(); wait = 2; }
        static void Fail(Exception error)
        {
            File.AppendAllText(Report, "FAIL " + error + "\n");
            SessionState.SetBool(Key, false);
            EditorApplication.isPlaying = false;
        }
        static void Tick()
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (!Application.isPlaying) return;
            if (wait-- > 0) return;
            try
            {
                if (waitingForMenu)
                {
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "01_MainMenu")
                    {
                        File.AppendAllText(Report, "PASS ending to menu\nSUCCESS\n");
                        SessionState.SetBool(Key, false);
                        EditorApplication.isPlaying = false;
                    }
                    else if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Menu utama belum aktif.");
                    return;
                }
                var director = UnityEngine.Object.FindAnyObjectByType<CampaignChapterDirector>();
                if (director == null || !director.Ready) return;
                string id = director.Step?.id ?? "end-" + director.Content.chapter;
                if (id != stepId)
                {
                    if (stepId != null) File.AppendAllText(Report, "PASS " + stepId + "\n");
                    stepId = id;
                    deadline = EditorApplication.timeSinceStartup + 180;
                }
                if (EditorApplication.timeSinceStartup > deadline)
                    throw new Exception("Timed out at " + stepId + ". Focus=" +
                        UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.name);
                var skip = Button("Lewati adegan");
                if (skip != null) { Click(skip); return; }
                var next = Button("Lanjut");
                if (director.IsModal && next != null) { Click(next); return; }
                if (director.Complete)
                {
                    var proceed = Button("Lanjut chapter berikutnya");
                    if (proceed != null) { Click(proceed); return; }
                    var menu = Button("Menu utama");
                    if (director.Content.chapter == 5 && menu != null)
                    {
                        Click(menu);
                        waitingForMenu = true;
                        deadline = EditorApplication.timeSinceStartup + 90;
                    }
                    return;
                }
                if (!director.IsModal)
                {
                    var evade = director.GetComponent<CampaignEvade>();
                    if (evade != null && evade.Running) { Move(director, director.Target.position); return; }
                    foreach (var offset in new[] { Vector3.back, Vector3.left, Vector3.right, Vector3.forward, Vector3.zero })
                    {
                        Move(director, director.Target.position + offset);
                        if (!director.CanUse()) continue;
                        director.Use();
                        wait = 2;
                        return;
                    }
                    throw new Exception("Objective point blocked at " + stepId + " " + director.Target.position);
                }
                var step = director.Step;
                if (step.words != null)
                {
                    var slots = director.Progress.wordProgress.wordSlots;
                    if (slots.Length < 3)
                    {
                        var tile = Button(step.words.TextFor(step.words.Answer[slots.Length]));
                        if (tile != null) Click(tile);
                    }
                    else if (Button("Periksa") != null) Click(Button("Periksa"));
                    return;
                }
                if (step.kind == CampaignTaskKind.Order)
                {
                    var count = director.Progress.selectedOrder.Length;
                    if (count < step.order.Length && Button(step.order[count]) != null) Click(Button(step.order[count]));
                    return;
                }
                if (step.questions?.Length > 0)
                {
                    var question = step.questions[Mathf.Min(director.Progress.question, step.questions.Length - 1)];
                    var option = Button(question.options[step.kind == CampaignTaskKind.Choice ? 0 : question.answer]);
                    if (option != null) Click(option);
                    return;
                }
                if (Button("Mode mudah") != null) Click(Button("Mode mudah"));
            }
            catch (Exception error) { Fail(error); }
        }
        static void Move(CampaignChapterDirector director, Vector3 position)
        {
            var cc = director.Player.GetComponent<CharacterController>();
            cc.enabled = false;
            director.Player.transform.position = position;
            cc.enabled = true;
            Physics.SyncTransforms();
        }
    }
}
