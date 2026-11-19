using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using BullyingGame.Campaign;

namespace BullyingGame.EditorTools
{
    [InitializeOnLoad]
    internal static class CampaignPolishSessionQA
    {
        const string Report = "Logs/FinalCampaignPolishQA.txt";
        const string Ticket = "Temp/CampaignPolishQA.request";
        static CampaignPolishSessionQA() => EditorApplication.update += Tick;
        static void Tick()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SessionState.GetBool("PolishQA.Restore", false))
            {
                SessionState.SetBool("PolishQA.Restore", false);
                SessionState.EraseString("CampaignPRD.TestSave");
                SessionState.EraseString("PRD.TestSavePath");
                EditorSceneManager.OpenScene(SessionState.GetString("PolishQA.Scene", "Assets/_Game/Scenes/Levels/Level_01/Level01.unity"));
                File.AppendAllText(Report, "Returned to edit mode.\n");
                File.WriteAllText("Temp/PrologueMapQA.request", "test");
                return;
            }
            if (!File.Exists(Ticket)) return;
            bool resume = File.ReadAllText(Ticket).Trim() == "resume";
            File.Delete(Ticket);
            try
            {
                var scene = SceneManager.GetActiveScene();
                if (SceneManager.sceneCount != 1 || scene.isDirty || string.IsNullOrEmpty(scene.path))
                    throw new InvalidOperationException("Scene setup has unsaved or additive work.");
                SessionState.SetString("PolishQA.Scene", scene.path);
                if (resume)
                {
                    var path = Path.GetFullPath("Logs/FinalCampaignTransitionTest.json");
                    var seed = new CampaignProgress { activeChapter = 4, unlockedChapter = 4 };
                    CampaignCheckpoint.Chapter(seed,4).stepId = "L04-Q08";
                    File.WriteAllText(path, JsonUtility.ToJson(seed));
                    SessionState.SetString("CampaignPRD.TestSave", path);
                    SessionState.SetString("PRD.TestSavePath", Path.GetFullPath("Logs/FinalCampaignLegacyTest.json"));
                    File.AppendAllText("Logs/FinalCampaignSmoke.txt", "Retest chapter transition after load guard\n");
                    SessionState.SetBool("FinalCampaign.Smoke", true);
                    SessionState.SetBool("PolishQA.Restore", true);
                    EditorSceneManager.OpenScene("Assets/_Game/Scenes/Levels/Level_04/Level04.unity");
                    EditorApplication.isPlaying = true;
                    return;
                }
                File.WriteAllText(Report, "Apply scene polish\n");
                FinalCampaignAuthoring.PolishChapters();
                var original = EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    for (int n = 2; n <= 5; n++)
                    {
                        var hub = EditorSceneManager.OpenScene($"Assets/_Game/Scenes/Levels/Level_{n:00}/Level{n:00}.unity");
                        var d = UnityEngine.Object.FindFirstObjectByType<CampaignChapterDirector>();
                        var points = new SerializedObject(d).FindProperty("points");
                        var previous = d.Content.spawn;
                        var route = new NavMeshPath();
                        for (int i = 0; i < points.arraySize; i++)
                        {
                            var dest = ((Transform)points.GetArrayElementAtIndex(i).objectReferenceValue).position;
                            if (!NavMesh.CalculatePath(previous, dest, NavMesh.AllAreas, route) || route.status != NavMeshPathStatus.PathComplete)
                                throw new InvalidOperationException("Unreachable " + d.Content.steps[i].id);
                            previous = dest;
                        }
                        File.AppendAllText(Report, hub.name + " " + points.arraySize + " reachable\n");
                    }
                }
                finally { EditorSceneManager.RestoreSceneManagerSetup(original); }
                SessionState.SetBool("PolishQA.Restore", true);
                FinalCampaignSmoke.Begin();
            }
            catch (Exception e) { File.AppendAllText(Report, e + "\n"); }
        }
    }
}
