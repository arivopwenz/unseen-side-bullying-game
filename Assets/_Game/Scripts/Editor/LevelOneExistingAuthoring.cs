using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using BullyingGame.LevelOne;
using BullyingGame.NPC;
using BullyingGame.Events;
using BullyingGame.UI;

namespace BullyingGame.EditorTools
{
    public static class LevelOneExistingAuthoring
    {
        static void Edit(UnityEngine.Object o, Action<SerializedObject> f) { var s = new SerializedObject(o); f(s); s.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(o); }
        static void Ref(UnityEngine.Object o, string field, UnityEngine.Object value) => Edit(o, s => s.FindProperty(field).objectReferenceValue = value);
        [MenuItem("Tools/The Unseen Side/Level 1 PRD/Adapt to original zones and cutscenes")]
        public static void Apply()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.name != "Level01") throw new Exception("Existing Level01 in Edit Mode required");
            var root = GameObject.Find(LevelOnePRDAuthoring.RootName);
            var all = scene.GetRootGameObjects().Where(g => g != root).SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            Func<string, GameObject> get = name => all.First(t => t.name == name).gameObject;
            var keep = new[] { "GuideController", "WalkTalkDriver", "AmbushDialogueDriver", "BullyNPC", "BullyGroupController", "AmbushTrigger", "BimaConversationDriver", "LoadingScreen", "MissionBanner", "MissionTitleCard", "CompanionBar", "FadeController", "GroundMarker" };
            foreach (var b in all.SelectMany(t => t.GetComponents<MonoBehaviour>()).Where(b => b != null))
                if (keep.Contains(b.GetType().Name)) b.enabled = true;
            foreach (string name in new[] { "Bima_Guide", "Nakal", "Nakal1", "IbuGuru_Kelas", "WalkTalkCanvas" }) get(name).SetActive(true);
            foreach (var canvas in get("WalkTalkCanvas").GetComponentsInChildren<Canvas>(true)) canvas.enabled = true;
            foreach (var canvas in get("LoadingScreen").GetComponentsInChildren<Canvas>(true)) canvas.enabled = true;
            var director = root.GetComponent<LevelOneDirector>();
            Edit(director, s => { s.FindProperty("useExistingFlow").boolValue = true; s.FindProperty("staging").objectReferenceValue = null; });
            root.GetComponent<LevelOneStaging>().enabled = false;
            root.transform.Find("04 - Kamera Adegan PRD").gameObject.SetActive(false);
            var cast = root.transform.Find("03 - Pemeran PRD");
            foreach (Transform actor in cast) if (actor.name == "Bima" || actor.name == "Memet" || actor.name == "Raka" || actor.name == "Bu Ratna") actor.gameObject.SetActive(false);
            Edit(root.GetComponent<LevelOneView>(), s => s.FindProperty("useExistingHUD").boolValue = true);
            var route = get("GuideRoute").transform;
            var classZone = route.Find("Zone_PerkenalanKelas");
            if (classZone == null)
            {
                classZone = new GameObject("Zone_PerkenalanKelas").transform; classZone.SetParent(route);
                classZone.position = get("GroundMarker").transform.position;
                var col = classZone.gameObject.AddComponent<BoxCollider>(); col.isTrigger = true; col.center = Vector3.up; col.size = new Vector3(4, 3, 3);
            }
            var yellow = get("Canteen Cutscene Trigger").GetComponent<YellowCircleTrigger>(); yellow.enabled = true;
            Ref(yellow, "activationZone", classZone.GetComponent<Collider>());
            var bekal = get("Canteen Cutscene Trigger").GetComponent<BekalCutsceneTrigger>(); bekal.enabled = true;
            bekal.cutsceneSceneName = "BekalCutscene";
            var canteenMark = route.Find("BimaMark_Kantin");
            if (canteenMark == null) { canteenMark = new GameObject("BimaMark_Kantin").transform; canteenMark.SetParent(route); canteenMark.position = bekal.transform.position; }
            var flow = get("Script").GetComponent<LevelOneExistingFlow>() ?? get("Script").AddComponent<LevelOneExistingFlow>();
            Ref(flow, "quests", director); Ref(flow, "guide", get("Bima_Guide").GetComponent<GuideController>());
            Ref(flow, "ambush", get("AmbushTriggerZone").GetComponent<AmbushTrigger>()); Ref(flow, "conversation", get("BimaConversationDriver").GetComponent<BimaConversationDriver>());
            Ref(flow, "classroomReturn", classZone); Ref(flow, "canteenReturn", canteenMark); Ref(flow, "legacyObjective", get("ObjectiveDirector").GetComponent<ObjectiveDirector>());
            var points = root.GetComponentsInChildren<LevelOnePoint>(true);
            Ref(flow, "gardenBench", points.First(p => p.Id == "Bench").transform);
            Edit(flow, s => { var p = s.FindProperty("bimaPoints"); var selected = points.Where(x => x.Id == "MeetBima" || x.Id == "BimaCorridor" || x.Id == "BimaCanteen").ToArray(); p.arraySize = selected.Length; for (int i = 0; i < selected.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = selected[i]; });
            var settings = new SerializedObject(director); var controls = settings.FindProperty("modalControls");
            bool added = false; for (int i = 0; i < controls.arraySize; i++) if (controls.GetArrayElementAtIndex(i).objectReferenceValue == get("Bima_Guide").GetComponent<GuideController>()) added = true;
            if (!added) { int index = controls.arraySize++; controls.GetArrayElementAtIndex(index).objectReferenceValue = get("Bima_Guide").GetComponent<GuideController>(); settings.ApplyModifiedPropertiesWithoutUndo(); }
            foreach (var point in points.Where(p => new[] { "ClassFront", "Corridor", "Teacher", "Canteen" }.Contains(p.Id)))
                foreach (var renderer in point.GetComponentsInChildren<Renderer>()) renderer.enabled = false;
            get("GroundMarker").SetActive(true);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); Selection.activeGameObject = get("AmbushTriggerZone");
            Debug.Log("Level01 adapted to original GuideRoute, AmbushTriggerZone, YellowCircleTrigger and BekalCutsceneTrigger.");
        }
        public static void PrepareCutscenes()
        {
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var path in new[] { "Assets/_Game/Scenes/Levels/Level_01/Cutscene perkenalan kelas.unity", "Assets/_Game/Scenes/Levels/Level_01/BekalCutscene.unity" })
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                System.IO.Directory.CreateDirectory("Logs/BeforeLevelOneAdaptation");
                EditorSceneManager.SaveScene(scene, "Logs/BeforeLevelOneAdaptation/" + scene.name + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity", true);
                var directors = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>(true)).ToArray();
                var target = directors.First(d => scene.name == "BekalCutscene" ? d.name == "Timeline Canteen" : d.name == "Cinemachine");
                target.gameObject.SetActive(true); target.playOnAwake = false; target.extrapolationMode = UnityEngine.Playables.DirectorWrapMode.None;
                var auto = target.GetComponent<BullyingGame.Cinematics.CutsceneAutoReturn>();
                if (auto == null) auto = target.gameObject.AddComponent<BullyingGame.Cinematics.CutsceneAutoReturn>();
                Ref(auto, "authoredDirector", target);
                var conflicting = new[] { "StoryChapterSequence", "PlayablePOVController", "ChapterActorLayout", "SchoolStoryPresentation", "SchoolFieldGuide", "SceneCheckpoint", "StoryOpeningDirector", "VideoCutscenePlayer", "GuideController", "BullyGroupController", "AmbushTrigger", "YellowCircleTrigger", "BekalCutsceneTrigger", "BimaConversationDriver", "ObjectiveDirector", "DebugTeleport", "DeveloperMissionSelect", "PlayerInputHandler", "PlayerMovement", "PrototypeCharacterMotion", "PlayerInteraction", "GameplayModalGate", "DialogueCameraFraming" };
                foreach (var b in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MonoBehaviour>(true)).Where(b => b != null)) if (conflicting.Contains(b.GetType().Name)) b.enabled = false;
                foreach (var agent in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>(true))) agent.enabled = false;
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); EditorSceneManager.CloseScene(scene, true);
            }
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(active);
        }
    }
}
