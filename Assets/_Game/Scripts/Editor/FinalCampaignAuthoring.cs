using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Unity.Cinemachine;
using BullyingGame.Campaign;
using BullyingGame.LevelOne;
using BullyingGame.Quest;
using BullyingGame.Core;
using TMPro;

namespace BullyingGame.EditorTools
{
    public static class FinalCampaignAuthoring
    {
        const string DataFolder = "Assets/_Game/Data/CampaignPRD/";
        const string Hub = "Assets/_Game/Scenes/Levels/Level_01/Level01.unity";
        const string School = "Assets/_Game/Prefabs/Environment/AreaSekolah.prefab";
        [Serializable] class Source { public ChapterSource[] chapters; public PuzzleSource[] puzzles; }
        [Serializable] class ChapterSource
        {
            public int chapter;
            public string title, reflection, nextScene;
            public float[] spawn;
            public StepSource[] steps;
        }
        [Serializable] class StepSource
        {
            public string id, title, objective, actor, pointId, insight, scenePath, kind, choiceKey, wordId;
            public string[] lines, afterLines, order;
            public Vector3 position;
            public CampaignQuestion[] questions;
            public bool actionCard;
        }
        [Serializable] class PuzzleSource
        {
            public string id, title, hint, feedback, response;
            public WordCardPhrase[] phrases;
            public string[] answer;
        }
        static void Edit(UnityEngine.Object target, Action<SerializedObject> edit)
        {
            var data = new SerializedObject(target);
            edit(data);
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
        static void Link(UnityEngine.Object target, string name, UnityEngine.Object value) =>
            Edit(target, data => data.FindProperty(name).objectReferenceValue = value);
        static void References(UnityEngine.Object target, string name, UnityEngine.Object[] values) =>
            Edit(target, data =>
            {
                var list = data.FindProperty(name);
                list.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            });
        static T Asset<T>(string name) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(DataFolder + name + ".asset");
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, DataFolder + name + ".asset");
            return asset;
        }
        static Vector3 Ground(Vector3 position)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = 0, areaMask = NavMesh.AllAreas };
            if (!NavMesh.SamplePosition(position, out var hit, 3f, filter))
                throw new InvalidOperationException("No walkable anchor near " + position);
            return hit.position;
        }
        static GameObject Child(Transform parent, string name, Vector3 position)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.position = position;
            return obj;
        }
        static GameObject Clone(GameObject source, Scene scene)
        {
            var copy = UnityEngine.Object.Instantiate(source);
            copy.name = source.name;
            SceneManager.MoveGameObjectToScene(copy, scene);
            return copy;
        }
        static void Strip(GameObject actor)
        {
            foreach (var script in actor.GetComponentsInChildren<MonoBehaviour>(true).Reverse())
                if (script != null) UnityEngine.Object.DestroyImmediate(script);
            foreach (var agent in actor.GetComponentsInChildren<NavMeshAgent>(true)) UnityEngine.Object.DestroyImmediate(agent);
            foreach (var collider in actor.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
            foreach (var rb in actor.GetComponentsInChildren<Rigidbody>(true)) UnityEngine.Object.DestroyImmediate(rb);
        }
        static GameObject Model(string name, Transform parent, Vector3 point)
        {
            string baseFolder = "Assets/_Game/Art/Characters/";
            var models = new Dictionary<string, string>
            {
                ["Laura"] = "Animation/Laura/KarakterCewek_MIXAMO@Idle.fbx",
                ["Fatimah"] = "Model/Fatimah/Fatimah_Mixamo (1)@Happy Idle + With Skin.fbx",
                ["Bu Lilis"] = "Animation/Laura/KarakterCewek_MIXAMO@Idle.fbx",
                ["Pak Jaya"] = "Model/Pak Beni/Meshy_AI_Khaki_Guardian_20261030_texture_fbx/Meshy_AI_Khaki_Guardian_20261030_texture.fbx",
                ["Pak Damar"] = "Model/Pak Arif/Meshy_AI_Stern_Officer_in_T_Pose_20261030_texture_fbx/Meshy_AI_Stern_Officer_in_T_Pose_20261030_texture.fbx"
            };
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(baseFolder + models[name]);
            if (source == null) throw new InvalidOperationException("Character model missing: " + name);
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            obj.name = name;
            Strip(obj);
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("Model without visible mesh: " + name);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            if (bounds.size.y < .05f) throw new InvalidOperationException("Invalid character scale: " + name);
            obj.transform.localScale *= (name.StartsWith("Pak") || name.StartsWith("Bu") ? 1.85f : 1.7f) / bounds.size.y;
            obj.transform.position = point - Vector3.up * ((bounds.min.y - obj.transform.position.y) * obj.transform.localScale.x);
            var animator = obj.GetComponentInChildren<Animator>();
            if (animator != null && animator.isHuman)
            {
                var path = DataFolder + "Idle_" + name.Replace(" ", "") + ".controller";
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (controller == null)
                {
                    controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                    var clip = AssetDatabase.FindAssets("Idle t:AnimationClip", new[] { baseFolder })
                        .Select(AssetDatabase.GUIDToAssetPath).SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                        .OfType<AnimationClip>().FirstOrDefault(c => c.humanMotion && !c.name.StartsWith("__"));
                    if (clip != null) controller.AddMotion(clip);
                }
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
            }
            return obj;
        }

        [MenuItem("Tools/UNSEEN SIDE/Final Campaign/Apply chapters 1 to ending")]
        public static void Apply()
        {
            var hub = SceneManager.GetActiveScene();
            if (Application.isPlaying || hub.path != Hub || hub.isDirty)
                throw new InvalidOperationException("Open and save Level01 in Edit Mode first.");
            var source = JsonUtility.FromJson<Source>(File.ReadAllText(DataFolder + "CampaignContent.json"));
            if (hub.GetRootGameObjects().Any(go => go.GetComponent<ChapterOneCampaignBridge>() != null)
                && Enumerable.Range(2, 4).All(i => AssetDatabase.LoadAssetAtPath<CampaignChapterData>(DataFolder + "Chapter" + i + ".asset") != null))
                throw new InvalidOperationException("Kampanye sudah terpasang. Gunakan menu Rapikan gameplay untuk memperbarui scene.");
            Directory.CreateDirectory("Logs/BeforeFinalCampaign");
            var root = hub.GetRootGameObjects().Single(g => g.name == "questLevel01");
            var roots = hub.GetRootGameObjects().ToDictionary(g => g.name);
            if (root.GetComponent<ChapterOneCampaignBridge>() == null) root.AddComponent<ChapterOneCampaignBridge>();
            EditorSceneManager.MarkSceneDirty(hub);
            EditorSceneManager.SaveScene(hub);
            var words = BuildWords(source.puzzles);
            var templates = new Dictionary<string, GameObject>
            {
                ["Bima"] = roots["Bima_Guide"], ["Radit"] = roots["Radit"], ["Memet"] = roots["Memet"],
                ["Raka"] = roots["Raka"], ["Bu Ratna"] = roots["IbuGuru_Kelas"],
                ["Vivi"] = roots.Values.First(g => g.name.StartsWith("Vivi_")),
                ["Nando"] = roots.Values.First(g => g.name.StartsWith("Nando_")),
                ["Sahrul"] = roots.Values.First(g => g.name.StartsWith("Sahrul_")),
                ["Sikka"] = roots.Values.First(g => g.name.StartsWith("Sikka-"))
            };
            var newScenes = new List<string>();
            foreach (var chapter in source.chapters)
            {
                string path = "Assets/_Game/Scenes/Levels/Level_" + chapter.chapter.ToString("00") +
                    "/Level" + chapter.chapter.ToString("00") + ".unity";
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    EditorSceneManager.SaveScene(scene, "Logs/BeforeFinalCampaign/Level" + chapter.chapter + ".unity", true);
                    // Preserve all original scene authoring as an inactive archive.
                    var legacy = new GameObject("Legacy authoring (inactive)");
                    SceneManager.MoveGameObjectToScene(legacy, scene);
                    var original = scene.GetRootGameObjects().Where(g => g != legacy).ToArray();
                    foreach (var obj in original) obj.transform.SetParent(legacy.transform, true);
                    legacy.SetActive(false);
                    var environment = original.FirstOrDefault(g => g.name == "AreaSekolah");
                    if (environment != null)
                    {
                        environment.transform.SetParent(null, true);
                        environment.SetActive(true);
                    }
                    else environment = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(School), scene);
                    if (chapter.chapter > 2) environment.transform.position = roots["AreaSekolah"].transform.position;
                    SceneManager.SetActiveScene(scene);
                    var player = Clone(roots["Player"], scene);
                    foreach (var script in player.GetComponentsInChildren<MonoBehaviour>(true))
                        if (script != null && !(script is BullyingGame.Player.PlayerMovement) &&
                            !(script is BullyingGame.Player.PlayerInputHandler) && !(script is TMP_Text)) script.enabled = false;
                    var cameras = Clone(roots["Cameras"], scene);
                    var camera = cameras.GetComponentsInChildren<CinemachineCamera>(true).Single(c => c.name == "CinemachineCamera");
                    foreach (var other in cameras.GetComponentsInChildren<CinemachineCamera>(true))
                        if (other != camera) other.gameObject.SetActive(false);
                    camera.Follow = player.transform;
                    camera.LookAt = player.transform;
                    camera.Priority = 20;
                    var main = cameras.GetComponentInChildren<UnityEngine.Camera>();
                    main.enabled = true;
                    main.GetComponent<CinemachineBrain>().enabled = true;
                    if (main.TryGetComponent<AudioListener>(out var listener)) listener.enabled = true;
                    var system = new GameObject("Campaign Level " + chapter.chapter, typeof(LevelGameplayEntry), typeof(QuestManager));
                    SceneManager.MoveGameObjectToScene(system, scene);
                    var owner = system.AddComponent<CampaignChapterDirector>();
                    var view = owner.GetComponent<LevelOneView>();
                    var state = system.AddComponent<CampaignWorldState>();
                    var cast = Child(system.transform, "Characters", Vector3.zero).transform;
                    var points = Child(system.transform, "Quest Points", Vector3.zero).transform;
                    var actors = new Dictionary<string, GameObject>();
                    var content = Asset<CampaignChapterData>("Chapter" + chapter.chapter);
                    content.chapter = chapter.chapter;
                    content.title = chapter.title;
                    content.reflection = chapter.reflection;
                    content.nextScene = chapter.nextScene;
                    content.spawn = Ground(new Vector3(chapter.spawn[0], chapter.spawn[1], chapter.spawn[2]));
                    player.transform.position = content.spawn;
                    var markers = new List<Transform>();
                    content.steps = chapter.steps.Select(step =>
                    {
                        var anchor = Child(points, step.id + " - " + step.title, Ground(step.position)).transform;
                        markers.Add(anchor);
                        if (!string.IsNullOrEmpty(step.actor) && step.actor != "Aji" && !actors.ContainsKey(step.actor))
                        {
                            GameObject actor;
                            var position = anchor.position + Vector3.right * 1.3f;
                            if (templates.TryGetValue(step.actor, out var template))
                            {
                                actor = UnityEngine.Object.Instantiate(template, cast);
                                actor.name = step.actor;
                                Strip(actor);
                                actor.SetActive(true);
                                actor.transform.position = position;
                            }
                            else actor = Model(step.actor, cast, position);
                            actor.transform.rotation = Quaternion.Euler(0, 180, 0);
                            actors.Add(step.actor, actor);
                        }
                        var quest = Asset<QuestData>(step.id);
                        quest.questId = step.id;
                        quest.questTitle = step.title;
                        quest.questDescription = step.objective;
                        quest.objectives = new[] { new QuestObjective { objectiveId = step.id, description = step.objective, requiredAmount = 1 } };
                        quest.prerequisites = Array.Empty<QuestData>();
                        EditorUtility.SetDirty(quest);
                        return new CampaignStep
                        {
                            id = step.id, title = step.title, pointId = step.pointId, actor = step.actor,
                            objective = step.objective, position = anchor.position, kind = (CampaignTaskKind)Enum.Parse(typeof(CampaignTaskKind), step.kind),
                            lines = step.lines, afterLines = step.afterLines, insight = step.insight,
                            order = step.order, questions = step.questions, scenePath = step.scenePath,
                            choiceKey = step.choiceKey, actionCard = step.actionCard, quest = quest,
                            words = string.IsNullOrEmpty(step.wordId) ? null : words[step.wordId]
                        };
                    }).ToArray();
                    if (chapter.chapter == 5)
                    {
                        foreach (var name in new[] { "Bima", "Radit", "Nando", "Sahrul", "Memet", "Raka" })
                        {
                            if (actors.ContainsKey(name)) continue;
                            var actor = UnityEngine.Object.Instantiate(templates[name], cast);
                            actor.name = name;
                            Strip(actor);
                            actor.SetActive(true);
                            actor.transform.position = Ground(new Vector3(20 + actors.Count % 4 * 2, 0, -20 - actors.Count / 4 * 2));
                            actors.Add(name, actor);
                        }
                    }
                    EditorUtility.SetDirty(content);
                    Link(owner, "content", content); Link(owner, "player", player); Link(owner, "world", state);
                    References(owner, "points", markers.Cast<UnityEngine.Object>().ToArray());
                    References(owner, "modalControls", player.GetComponents<Behaviour>()
                        .Where(c => c is BullyingGame.Player.PlayerMovement || c is BullyingGame.Player.PlayerInputHandler)
                        .Concat(camera.GetComponents<CinemachineInputAxisController>().Cast<Behaviour>())
                        .Cast<UnityEngine.Object>().ToArray());
                    var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                    SceneManager.MoveGameObjectToScene(events, scene);
                    if (root.transform.Find("Canvas_KartuAksi") != null)
                    {
                        var card = UnityEngine.Object.Instantiate(root.transform.Find("Canvas_KartuAksi").gameObject, system.transform);
                        card.name = "Canvas_KartuAksi"; card.SetActive(false);
                        Link(view, "actionCardArtwork", card.GetComponent<CanvasGroup>());
                        Link(view, "actionCardImage", card.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(image => image.sprite != null));
                    }
                    if (root.transform.Find("Canvas_KartuKata") != null)
                    {
                        var card = UnityEngine.Object.Instantiate(root.transform.Find("Canvas_KartuKata").gameObject, system.transform);
                        card.name = "Canvas_KartuKata"; card.SetActive(false);
                    }
                    ConfigurePropsAndStations(chapter.chapter, owner, state, actors, player, system.transform, content);
                    if (chapter.chapter == 2)
                    {
                        var evade = system.AddComponent<CampaignEvade>();
                        Link(evade, "player", player.transform); Link(evade, "memet", actors["Memet"].transform);
                        Link(evade, "safeZone", markers[Array.FindIndex(content.steps, step => step.kind == CampaignTaskKind.Evade)]);
                        Link(evade, "startPoint", Child(system.transform, "Evade checkpoint", Ground(new Vector3(32, 0, -20))).transform);
                        Link(owner, "evade", evade);
                        actors["Memet"].SetActive(false);
                    }
                    foreach (var step in content.steps.Where(step => !string.IsNullOrEmpty(step.scenePath)))
                    {
                        BuildCinematic(step, environment, actors, player);
                        newScenes.Add(step.scenePath);
                    }
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Cannot save " + path);
                }
                finally
                {
                    SceneManager.SetActiveScene(hub);
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            var build = EditorBuildSettings.scenes.ToList();
            foreach (var path in newScenes)
                if (!build.Any(entry => entry.path == path)) build.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = build.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[FinalCampaign] Chapters 2–5, cinematic scenes, data and Chapter 1 bridge authored.");
        }

        [MenuItem("Tools/UNSEEN SIDE/Final Campaign/Rapikan gameplay")]
        public static void PolishChapters()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Gunakan Edit Mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Simpan scene terbuka dahulu.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var source = JsonUtility.FromJson<Source>(File.ReadAllText(DataFolder + "CampaignContent.json"));
            try
            {
                var hub = EditorSceneManager.OpenScene(Hub);
                SetDialogueFonts(hub.GetRootGameObjects().Single(go => go.name == "questLevel01").GetComponent<LevelOneView>());
                EditorSceneManager.SaveScene(hub);
                foreach (var chapter in source.chapters)
                {
                    var scene = EditorSceneManager.OpenScene($"Assets/_Game/Scenes/Levels/Level_{chapter.chapter:00}/Level{chapter.chapter:00}.unity");
                    var owner = UnityEngine.Object.FindFirstObjectByType<CampaignChapterDirector>();
                    var content = owner.Content;
                    var points = new SerializedObject(owner).FindProperty("points");
                    var pointRoot = owner.transform.Find("Quest Points");
                    var world = owner.GetComponent<CampaignWorldState>();
                    var props = new SerializedObject(world).FindProperty("props");
                    for (int i = 0; i < props.arraySize; i++)
                    {
                        var parent = Group(owner.transform, "Properti misi");
                        var prop = props.GetArrayElementAtIndex(i);
                        var visual = (GameObject)prop.FindPropertyRelative("visual").objectReferenceValue;
                        if (visual.transform.parent == owner.transform) visual.transform.SetParent(parent, true);
                        var grip = (Transform)prop.FindPropertyRelative("returnGrip").objectReferenceValue;
                        if (grip != null && grip.parent == owner.transform) grip.SetParent(parent, true);
                    }
                    for (int i = 0; i < content.steps.Length; i++)
                    {
                        var step = content.steps[i];
                        var written = chapter.steps.Single(entry => entry.id == step.id);
                        step.title = written.title;
                        step.objective = written.objective;
                        step.insight = written.insight;
                        step.questions = written.questions;
                        step.quest.questTitle = step.title;
                        step.quest.questDescription = step.objective;
                        step.quest.objectives[0].description = step.objective;
                        EditorUtility.SetDirty(step.quest);
                        var point = (Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;
                        var groupId = string.Join("-", step.id.Split('-').Take(2));
                        point.SetParent(Group(pointRoot, groupId), true);
                        point.name = step.id + " - " + step.title;
                    }
                    if (owner.GetComponentInChildren<BullyingGame.UI.GroundMarker>(true) == null)
                        Group(owner.transform, "Penanda tujuan").gameObject.AddComponent<BullyingGame.UI.GroundMarker>();
                    SetDialogueFonts(owner.GetComponent<LevelOneView>());
                    var env = scene.GetRootGameObjects().Single(go => go.name == "AreaSekolah");
                    var surface = env.GetComponent<Unity.AI.Navigation.NavMeshSurface>();
                    if (surface == null) surface = env.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
                    if (surface.navMeshData == null)
                    {
                        surface.collectObjects = Unity.AI.Navigation.CollectObjects.Children;
                        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                        surface.BuildNavMesh();
                        if (surface.navMeshData == null) throw new InvalidOperationException("Bake gagal di " + scene.name);
                        AssetDatabase.CreateAsset(surface.navMeshData, Path.GetDirectoryName(scene.path).Replace('\\','/') + "/NavMesh-" + scene.name + ".asset");
                        PrefabUtility.RecordPrefabInstancePropertyModifications(surface);
                    }
                    PlaceConnectedObjectives(owner, points);
                    if (chapter.chapter == 3)
                    {
                        string[] pickups = { "L03-Q04-bahan-pola", "L03-Q04-bahan-pita", "L03-Q04-bahan-aksesori" };
                        string[] materials = { "Pola kain", "Pita", "Aksesori" };
                        for (int i = 0; i < props.arraySize; i++)
                        {
                            var prop = props.GetArrayElementAtIndex(i);
                            int material = Array.IndexOf(materials, prop.FindPropertyRelative("id").stringValue);
                            if (material < 0) continue;
                            string pickup = pickups[material];
                            var item = (GameObject)prop.FindPropertyRelative("visual").objectReferenceValue;
                            item.transform.position = content.steps.Single(step => step.id == pickup).position + Vector3.up * .8f;
                            prop.FindPropertyRelative("returnAfter").stringValue = pickup;
                        }
                        props.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }
                    EditorUtility.SetDirty(content);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Gagal menyimpan " + scene.name);
                    AssetDatabase.SaveAssets();
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        static Transform Group(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing;
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        static void PlaceConnectedObjectives(CampaignChapterDirector owner, SerializedProperty points)
        {
            // The gate belongs to the campus walking route. Some classroom floors are separate NavMesh islands.
            if (!NavMesh.SamplePosition(new Vector3(-6, 0, -48), out var gate, 4, NavMesh.AllAreas))
                throw new InvalidOperationException("Gerbang tidak terhubung ke NavMesh sekolah.");
            var path = new NavMeshPath();
            Vector3 Approach(Vector3 desired)
            {
                float best = float.PositiveInfinity;
                Vector3 point = desired;
                for (int ring = 0; ring <= 9; ring++)
                {
                    int samples = ring == 0 ? 1 : 24;
                    for (int i = 0; i < samples; i++)
                    {
                        float angle = i * Mathf.PI * 2 / samples;
                        var offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * ring;
                        if (!NavMesh.SamplePosition(desired + offset, out var hit, 1.5f, NavMesh.AllAreas) ||
                            Mathf.Abs(hit.position.y - desired.y) > 1.45f) continue;
                        float distance = Vector3.Distance(hit.position, desired);
                        if (distance >= best || !NavMesh.CalculatePath(gate.position, hit.position, NavMesh.AllAreas, path)
                            || path.status != NavMeshPathStatus.PathComplete) continue;
                        point = hit.position;
                        best = distance;
                    }
                    if (best < ring + .5f) break;
                }
                if (float.IsInfinity(best)) throw new InvalidOperationException("Tujuan di luar jalur sekolah: " + desired);
                return point;
            }
            var data = owner.Content;
            data.spawn = Approach(data.spawn);
            for (int i = 0; i < points.arraySize; i++)
            {
                var target = (Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;
                data.steps[i].position = target.position = Approach(target.position);
            }
        }

        static void SetDialogueFonts(LevelOneView view)
        {
            const string fonts = "Assets/_Game/fonts/";
            Link(view, "speakerFont", AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fonts + "Super Bouncer SDF.asset"));
            Link(view, "dialogueFont", AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fonts + "Cleanow SDF.asset"));
        }

        static Dictionary<string, WordCardData> BuildWords(PuzzleSource[] puzzles)
        {
            var result = new Dictionary<string, WordCardData>();
            foreach (var puzzle in puzzles)
            {
                if (puzzle.id.StartsWith("L01")) continue;
                var asset = Asset<WordCardData>(puzzle.id);
                Edit(asset, data =>
                {
                    data.FindProperty("puzzleId").stringValue = puzzle.id;
                    data.FindProperty("title").stringValue = puzzle.title;
                    data.FindProperty("context").stringValue = puzzle.title;
                    data.FindProperty("feedback").stringValue = puzzle.feedback;
                    data.FindProperty("hint").stringValue = puzzle.hint;
                    data.FindProperty("response").stringValue = puzzle.response;
                    var list = data.FindProperty("phrases"); list.arraySize = puzzle.phrases.Length;
                    for (int i = 0; i < puzzle.phrases.Length; i++)
                    {
                        list.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue = puzzle.phrases[i].id;
                        list.GetArrayElementAtIndex(i).FindPropertyRelative("text").stringValue = puzzle.phrases[i].text;
                    }
                    var answer = data.FindProperty("answer"); answer.arraySize = 3;
                    for (int i = 0; i < 3; i++) answer.GetArrayElementAtIndex(i).stringValue = puzzle.answer[i];
                });
                result.Add(puzzle.id, asset);
            }
            return result;
        }
        static void ConfigurePropsAndStations(int chapter, CampaignChapterDirector owner, CampaignWorldState world,
            Dictionary<string, GameObject> actors, GameObject player, Transform parent, CampaignChapterData data)
        {
            var props = new List<(string id, GameObject obj, string reveal, string carry, string returned, Transform carryGrip, Transform returnGrip)>();
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Data/LevelOnePRD/Marker.mat");
            GameObject Prop(string id, Vector3 position, Vector3 scale, PrimitiveType shape)
            {
                var obj = GameObject.CreatePrimitive(shape); obj.name = id; obj.transform.SetParent(parent, false);
                obj.transform.position = position; obj.transform.localScale = scale;
                UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
                if (material != null) obj.GetComponent<Renderer>().sharedMaterial = material;
                return obj;
            }
            Transform Grip(GameObject actor, HumanBodyBones bone, string name)
            {
                var animator = actor.GetComponentsInChildren<Animator>().FirstOrDefault(a => a.isHuman);
                var hand = animator != null ? animator.GetBoneTransform(bone) : null;
                return Child(hand != null ? hand : actor.transform, name, hand != null ? hand.position : actor.transform.position + Vector3.up).transform;
            }
            var carryGrip = Grip(player, HumanBodyBones.RightHand, "Campaign item grip");
            if (chapter == 2)
            {
                var eye = Grip(actors["Radit"], HumanBodyBones.Head, "Glasses");
                eye.localPosition = new Vector3(0, .04f, .13f);
                var glasses = Child(parent, "Kacamata Radit", new Vector3(46, .9f, -28));
                foreach (float x in new[] { -.07f, .07f })
                {
                    var lens = Prop("Bingkai kacamata", glasses.transform.position + new Vector3(x, 0, 0), new Vector3(.11f, .07f, .025f), PrimitiveType.Cube);
                    lens.transform.SetParent(glasses.transform, true);
                }
                props.Add(("glasses", glasses, "L02-Q03", "L02-Q03-glasses", "L02-Q04", carryGrip, eye));
                foreach (var pair in new[] { ("Sains", Color.blue), ("Atlas", Color.yellow), ("Komik", Color.green) }.Select((value, index) => (value, index)))
                {
                    var book = Prop(pair.value.Item1, new Vector3(42 + pair.index * .3f, .8f, -25), new Vector3(.2f, .3f, .07f), PrimitiveType.Cube);
                    props.Add((book.name, book, "", "", "L02-Q03", null,
                        Child(parent, "Rak " + book.name, new Vector3(43 + pair.index * .4f, .9f, -25)).transform));
                }
            }
            if (chapter == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    var prop = Prop(new[] { "Pola kain", "Pita", "Aksesori" }[i], new Vector3(37 + i * .35f, .8f, -25),
                        new Vector3(.25f, .08f, .22f), PrimitiveType.Cube);
                    props.Add((prop.name, prop, "", "", "L03-Q04", null,
                        Child(parent, "Studio " + i, new Vector3(37, .85f + i * .1f, -24)).transform));
                }
            }
            if (chapter == 5)
            {
                var poster = Prop("Poster kelas aman", new Vector3(28, 1, 25), new Vector3(.6f, .45f, .03f), PrimitiveType.Cube);
                props.Add(("poster", poster, "", "L05-Q02-materials", "L05-Q02-station-a", carryGrip,
                    Child(parent, "Papan karya aman", new Vector3(45, 1.6f, 5)).transform));
                var art = Prop("Karya Sikka", new Vector3(37, 1.1f, -24), new Vector3(.5f, .7f, .05f), PrimitiveType.Cube);
                props.Add(("sikka-art", art, "L05-Q05", "", "", null, null));
            }
            Edit(world, serialized =>
            {
                var list = serialized.FindProperty("props"); list.arraySize = props.Count;
                for (int i = 0; i < props.Count; i++)
                {
                    var p = props[i]; var item = list.GetArrayElementAtIndex(i);
                    item.FindPropertyRelative("id").stringValue = p.id;
                    item.FindPropertyRelative("visual").objectReferenceValue = p.obj;
                    item.FindPropertyRelative("revealAfter").stringValue = p.reveal;
                    item.FindPropertyRelative("carryAfter").stringValue = p.carry;
                    item.FindPropertyRelative("returnAfter").stringValue = p.returned;
                    item.FindPropertyRelative("carryGrip").objectReferenceValue = p.carryGrip;
                    item.FindPropertyRelative("returnGrip").objectReferenceValue = p.returnGrip;
                }
            });
        }
        static void BuildCinematic(CampaignStep step, GameObject environment, Dictionary<string, GameObject> actors, GameObject player)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(step.scenePath));
            AssetDatabase.Refresh();
            var original = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                Clone(environment, scene);
                var root = new GameObject("CutsceneRoot");
                SceneManager.MoveGameObjectToScene(root, scene);
                var cast = new Dictionary<string, GameObject>();
                string[] roles = step.id == "L04-Q07" ? new[] { "Memet", "Pak Damar" } :
                    step.id == "L04-Q06" ? new[] { "Pak Damar", "Bu Ratna", "Raka" } :
                    step.id.Contains("L02") ? new[] { "Memet", "Raka" } :
                    actors.Keys.ToArray();
                int index = 0;
                Vector3 center = step.position;
                foreach (var name in roles.Where(actors.ContainsKey))
                {
                    var actor = Clone(actors[name], scene);
                    actor.SetActive(true);
                    actor.transform.SetParent(root.transform, true);
                    actor.transform.position = center + new Vector3((index++ % 4 - 1.5f) * 1.1f, 0, index / 4 * 1.4f);
                    actor.transform.rotation = Quaternion.Euler(0, 180, 0);
                    cast.Add(name, actor);
                }
                if (step.id != "L04-Q07")
                {
                    var aji = Clone(player, scene); Strip(aji); aji.name = "Aji cinematic"; aji.transform.SetParent(root.transform, true);
                    aji.transform.position = center + Vector3.back * 1.4f;
                }
                var cameraRoot = Child(root.transform, "Render Camera", center + new Vector3(2, 2.2f, -5));
                var camera = cameraRoot.AddComponent<UnityEngine.Camera>();
                camera.tag = "MainCamera";
                cameraRoot.AddComponent<AudioListener>();
                var brain = cameraRoot.AddComponent<CinemachineBrain>();
                var virtualCamera = Child(root.transform, "Cinemachine Shot", cameraRoot.transform.position).AddComponent<CinemachineCamera>();
                virtualCamera.transform.LookAt(center + Vector3.up * 1.2f);
                virtualCamera.Lens.FieldOfView = 48;
                cameraRoot.transform.rotation = virtualCamera.transform.rotation;
                var timeline = Asset<TimelineAsset>("CS_" + step.id);
                foreach (var track in timeline.GetRootTracks().ToArray()) timeline.DeleteTrack(track);
                var trackCamera = timeline.CreateTrack<CinemachineTrack>(null, "Cinemachine");
                var shot = trackCamera.CreateClip<CinemachineShot>();
                shot.duration = 6;
                var asset = (CinemachineShot)shot.asset;
                asset.VirtualCamera.exposedName = Guid.NewGuid().ToString();
                var director = root.AddComponent<PlayableDirector>();
                director.playableAsset = timeline;
                director.playOnAwake = false;
                director.SetGenericBinding(trackCamera, brain);
                director.SetReferenceValue(asset.VirtualCamera.exposedName, virtualCamera);
                foreach (var actor in cast.Values)
                {
                    var animator = actor.GetComponentsInChildren<Animator>().FirstOrDefault(a => a.runtimeAnimatorController != null);
                    var motion = animator?.runtimeAnimatorController.animationClips.FirstOrDefault(c => !c.name.StartsWith("__"));
                    if (motion == null) continue;
                    var track = timeline.CreateTrack<AnimationTrack>(null, actor.name);
                    var clip = track.CreateClip(motion); clip.duration = 6;
                    director.SetGenericBinding(track, animator);
                }
                EditorUtility.SetDirty(timeline);
                if (!EditorSceneManager.SaveScene(scene, step.scenePath)) throw new InvalidOperationException("Cutscene save failed.");
            }
            finally
            {
                SceneManager.SetActiveScene(original);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
