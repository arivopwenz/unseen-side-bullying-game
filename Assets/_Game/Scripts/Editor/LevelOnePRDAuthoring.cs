using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BullyingGame.LevelOne;
using BullyingGame.Quest;
using BullyingGame.Core;
using Unity.Cinemachine;

namespace BullyingGame.EditorTools
{
    public static class LevelOnePRDAuthoring
    {
        const string Folder = "Assets/_Game/Data/LevelOnePRD/";
        public const string RootName = "[Level01 PRD - Langkah Pertama]";
        static readonly Dictionary<string, LevelOnePoint> points = new Dictionary<string, LevelOnePoint>();
        static void Edit(UnityEngine.Object target, Action<SerializedObject> edit)
        {
            var s = new SerializedObject(target); edit(s); s.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(target);
        }
        static void Ref(UnityEngine.Object o, string name, UnityEngine.Object value) => Edit(o, s => s.FindProperty(name).objectReferenceValue = value);
        static T Asset<T>(string name) where T : ScriptableObject
        {
            string path = Folder + name + ".asset"; var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); } return a;
        }
        static GameObject Child(Transform p, string name, Vector3 position)
        {
            var g = new GameObject(name); g.transform.SetParent(p, false); g.transform.position = position; return g;
        }
        static Vector3 Ground(float x, float z)
        {
            Vector3 v = new Vector3(x, .15f, z);
            return NavMesh.SamplePosition(v, out var hit, 4, NavMesh.AllAreas) ? hit.position : v;
        }
        static Material material;
        static LevelOnePoint Point(Transform parent, LevelOneDirector director, string id, string label, float x, float z)
        {
            var g = Child(parent, id + " - " + label, Ground(x, z));
            var c = g.AddComponent<SphereCollider>(); c.isTrigger = true; c.radius = 3;
            var point = g.AddComponent<LevelOnePoint>();
            var v = GameObject.CreatePrimitive(PrimitiveType.Cylinder); v.name = "Penanda interaksi (visual)"; v.transform.SetParent(g.transform, false);
            v.transform.localPosition = new Vector3(0, .08f, 0); v.transform.localScale = new Vector3(.8f, .03f, .8f);
            UnityEngine.Object.DestroyImmediate(v.GetComponent<Collider>()); v.GetComponent<Renderer>().sharedMaterial = material;
            var labelObject = Child(g.transform, "Label penanda", g.transform.position + Vector3.up * 2.5f);
            var text = labelObject.AddComponent<TextMeshPro>(); text.font = TMP_Settings.defaultFontAsset; text.text = id + " • " + label;
            text.fontSize = 2; text.alignment = TextAlignmentOptions.Center; text.rectTransform.sizeDelta = new Vector2(5, 1.2f);
            // Labels are editor-only: runtime HUD supplies readable, camera-facing prompts.
            labelObject.tag = "EditorOnly";
            Edit(point, s => { s.FindProperty("pointId").stringValue = id; s.FindProperty("label").stringValue = label; s.FindProperty("director").objectReferenceValue = director; s.FindProperty("markerVisual").objectReferenceValue = v; });
            points.Add(id, point); return point;
        }
        static Transform Anchor(Transform parent, string name, float x, float z) => Child(parent, name, Ground(x, z)).transform;
        static GameObject Actor(Transform parent, string name, GameObject source, Transform point)
        {
            var actor = UnityEngine.Object.Instantiate(source, parent); actor.name = name; actor.SetActive(false);
            // Reuse the user's rig/model; none of the old quest, input or cinematic behaviours run on this copy.
            foreach (var b in actor.GetComponentsInChildren<MonoBehaviour>(true).Reverse()) if (b != null) { b.enabled = false; UnityEngine.Object.DestroyImmediate(b); }
            foreach (var c in actor.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
            foreach (var body in actor.GetComponentsInChildren<Rigidbody>(true)) UnityEngine.Object.DestroyImmediate(body);
            foreach (var nav in actor.GetComponentsInChildren<NavMeshAgent>(true)) UnityEngine.Object.DestroyImmediate(nav);
            actor.transform.SetPositionAndRotation(point.position, point.rotation);
            var col = actor.AddComponent<CapsuleCollider>(); col.center = Vector3.up; col.height = 2; col.radius = .3f;
            actor.SetActive(true); return actor;
        }
        static WordCardData Puzzle(string id, string title, string context, string[] words, string[] wrong, string feedback, string hint, string response)
        {
            var p = Asset<WordCardData>(id);
            // Lexically interleaved IDs prevent the list order revealing the correct sentence.
            string[] ids = { "phrase-e", "phrase-a", "phrase-c", "phrase-b", "phrase-d" };
            Edit(p, s =>
            {
                s.FindProperty("puzzleId").stringValue = id; s.FindProperty("title").stringValue = title; s.FindProperty("context").stringValue = context;
                s.FindProperty("feedback").stringValue = feedback; s.FindProperty("hint").stringValue = hint; s.FindProperty("response").stringValue = response;
                var phrases = s.FindProperty("phrases"); phrases.arraySize = 5;
                for (int i = 0; i < 5; i++) { var e = phrases.GetArrayElementAtIndex(i); e.FindPropertyRelative("id").stringValue = ids[i]; e.FindPropertyRelative("text").stringValue = i < 3 ? words[i] : wrong[i - 3]; }
                var answer = s.FindProperty("answer"); answer.arraySize = 3; for (int i = 0; i < 3; i++) answer.GetArrayElementAtIndex(i).stringValue = ids[i];
            }); return p;
        }
        static LevelOneContent Content()
        {
            var k1 = Puzzle("K-01", "Tanya kondisi", "Memet dan Raka sudah meninggalkan koridor. Bima tampak takut.", new[] { "Bim,", "kamu", "nggak apa-apa?" }, new[] { "Ah, lebay!", "Biarin saja." }, "Jangan menyepelekan ekspresi teman yang terlihat takut.", "Tanyakan keadaan teman tanpa menuduh atau mengecilkan perasaannya.", "Bima: Iya... udah biasa kok.\nAji belum memahami riwayat Bima. Mereka melanjutkan ke kelas.");
            var k2 = Puzzle("K-02", "Tawarkan bantuan", "Bima sudah berada di sisi kantin yang aman. Barangnya baru saja diambil.", new[] { "Aku bisa bantu", "cari barangmu,", "kalau kamu mau." }, new[] { "Kamu salah sendiri.", "Harus lawan, dong!" }, "Bantuan sebaiknya ditawarkan, bukan dipaksakan atau menyalahkan korban.", "Tawarkan bantuan konkret dan minta persetujuan Bima.", "Bima: Boleh, Ji. Makasih. Aku tunggu di tempat ramai.\nAji tidak mengejar Memet ke tempat sepi.");
            var k3 = Puzzle("K-03", "Dukung Bima", "Bekal dan botol sudah kembali. Bima masih takut barangnya diambil lagi.", new[] { "Kalau kamu mau,", "aku bisa temani", "cerita ke guru." }, new[] { "Lupakan saja semuanya.", "Harus sekarang!" }, "Biarkan korban memilih kapan bercerita, sambil tetap mengutamakan keselamatan.", "Dengarkan Bima, beri pilihan, dan tawarkan orang dewasa yang dapat dipercaya.", "Bima: Aku pikir dulu, ya.\nAji: Iya. Aku ada kalau kamu perlu teman. Kalau ada bahaya, kita cari bantuan guru.");
            var steps = new[]
            {
                Step("meet", "MeetBima", 0, LevelOneAction.Ask, "Temui Bima di dekat gerbang.", "Aji: Hai, aku Aji. Kamu tahu kelas VIII A di mana?", "Bima: Kebetulan aku kelas VIII A. Yuk, aku anter!", "KARTU AKSI: dekati penanda; tombol 1–3 memilih aksi, E menjalankan. Ikuti Bima ke depan kelas. TAB membuka jurnal."),
                Step("class_front", "ClassFront", 0, LevelOneAction.Observe, "Ikuti Bima dan temukan papan kelas VIII A.", "Aji membaca papan VIII A. Bima sampai bersama Aji.", "Meminta petunjuk adalah bagian dari beradaptasi di tempat baru."),
                Step("corridor", "Corridor", 1, LevelOneAction.Observe, "Amati keadaan Bima di koridor.", "Memet menghadang dan menyenggol bahu Bima.", "Memet: Wih, Bima! Sekarang punya temen baru, nih!", "Raka: Tumben ada yang mau jalan bareng!", "Bima menunduk. Memet dan Raka pergi; Aji mendekat tanpa bertengkar."),
                Step("check_bima", "BimaCorridor", 1, LevelOneAction.Ask, "Tanyakan keadaan Bima dengan Kartu Kata K-01.", "Aji: Eh, Bim...", "KARTU KATA: susun tiga frasa menjadi kalimat yang mendukung. Klik frasa berurutan lalu Ucapkan."),
                Step("introduction", "Teacher", 2, LevelOneAction.Ask, "Temui Bu Ratna untuk perkenalan kelas.", "Bu Ratna: Teman-teman, ini Aji. Tolong bantu dia beradaptasi.", "Aji: Halo, semuanya. Senang kenal kalian."),
                Step("sikka", "Sikka", 2, LevelOneAction.Observe, "Amati kursi kosong dan tanyakan kepada Bima.", "Aji: Bim, bangku itu biasanya ditempati siapa?", "Bima: Sikka. Belakangan dia sering nggak masuk.", "Aji belum tahu alasannya. Jangan menyebarkan dugaan tentang teman yang tidak masuk."),
                Step("canteen", "Canteen", 3, LevelOneAction.Observe, "Saat istirahat, temui Bima di kantin.", "Memet mengambil bekal dan botol Bima.", "Memet: Kalau nggak ada uang, bekalmu aja aku ambil!", "Bima: Jangan, itu bekal dari rumah.", "Fatimah: Aku lihat dia ke arah rak dekat kantin. Aku bantu cari, ya.", "Memet pergi. Aji tetap di area ramai dan tidak mengejarnya."),
                Step("offer", "BimaCanteen", 3, LevelOneAction.Ask, "Tenangkan Bima dan tawarkan bantuan (K-02).", "Bima tampak sedih. Aji menawarkan bantuan tanpa menyalahkannya."),
                Step("witness", "Fatimah", 3, LevelOneAction.Ask, "Tanya Fatimah tentang arah barang Bima.", "Fatimah: Memet melewati sisi meja kantin. Botol biru tampak di rak troli; bekalnya dekat bangku pojok.", "Fatimah: Aku bantu cari, ya. Kita tetap di tempat terbuka. Kalau perlu masuk area kerja, minta izin petugas dulu."),
                Step("items", "P1", 3, LevelOneAction.Inspect, "Periksa lima titik terbuka; temukan dan ambil botol serta bekal Bima.", ""),
                Step("return", "Bench", 4, LevelOneAction.Return, "Kembalikan kedua barang kepada Bima di bangku taman.", "Aji: Ini barangmu, Bim. Sudah lengkap?", "Bima: Iya, botol sama bekalnya sudah ada. Makasih... tapi aku takut besok diambil lagi."),
                Step("support", "Bench", 4, LevelOneAction.Ask, "Dengarkan Bima dan susun dukungan (K-03).", "Bima masih takut. Tawarkan dukungan tanpa memaksanya segera melapor."),
                Step("record", "Bench", 4, LevelOneAction.Record, "Catat kronologi dan pelajaran di jurnal.", "Pengamatan Aji: Memet mengejek dan menyenggol Bima di koridor; di kantin, bekal serta botol diambil tanpa izin.", "Keterangan Bima: ia takut barangnya diambil lagi. Fatimah membantu menunjukkan arah; kedua barang sudah kembali.", "Tindak lanjut: dengarkan pilihan Bima, tetap di tempat ramai, dan tawarkan pendampingan ke Bu Ratna. Jika ada bahaya, segera minta bantuan orang dewasa.")
            };
            steps[3].puzzle = k1; steps[7].puzzle = k2; steps[11].puzzle = k3; steps[9].search = true;
            steps[0].journal = "Gerbang: Aji bertemu Bima dan meminta petunjuk kelas VIII A.";
            steps[3].journal = "Koridor: Bima tampak takut setelah dihadang. 'Cuma bercanda' belum tentu aman jika teman merasa takut.";
            steps[5].journal = "Informasi Sikka: kursinya kosong; menurut Bima ia sering tidak masuk. Penyebab belum diketahui, jangan sebarkan dugaan.";
            steps[8].journal = "Petunjuk Fatimah: periksa meja, bangku dekat taman, rak troli, pot besar, dan bangku pojok. Jangan masuk area kerja tanpa izin.";
            steps[9].journal = "BOTOL-BIMA dan BEKAL-BIMA ditemukan secara aman, dua ID barang berbeda. Tidak mengambil tas milik orang lain.";
            steps[10].journal = "Barang pribadi Bima sudah dikembalikan 2/2. Mengambil barang tanpa izin dan membuat pemilik takut bukan keusilan biasa.";
            steps[11].journal = "Dukungan korban: Bima didengar, ditawari teman ke guru, dan boleh memilih kapan bercerita.";
            steps[12].journal = "Kronologi insiden tersimpan: pengamatan Aji, keterangan Bima/Fatimah, barang kembali, pilihan pendampingan Bu Ratna.";
            var titles = new[] { "L1-Q01 • Teman pertama", "L1-Q02 • Keadaan Bima", "L1-Q03 • Bangku kosong", "L1-Q04 • Bekal dan botol", "L1-Q05 • Dengarkan Bima" };
            var quests = new QuestData[5];
            for (int i = 0; i < 5; i++)
            {
                var q = Asset<QuestData>("L1-Q0" + (i + 1)); q.questId = "L1-Q0" + (i + 1); q.questTitle = titles[i]; q.questDescription = titles[i];
                q.objectives = steps.Where(s => s.questIndex == i).Select(s => new QuestObjective { objectiveId = s.id, description = s.objective, requiredAmount = 1 }).ToArray();
                q.prerequisites = i == 0 ? new QuestData[0] : new[] { quests[i - 1] }; quests[i] = q; EditorUtility.SetDirty(q);
            }
            var content = Asset<LevelOneContent>("Level01_LangkahPertama");
            Edit(content, s =>
            {
                var qs = s.FindProperty("quests"); qs.arraySize = quests.Length; for (int i = 0; i < quests.Length; i++) qs.GetArrayElementAtIndex(i).objectReferenceValue = quests[i];
                var ss = s.FindProperty("steps"); ss.arraySize = steps.Length;
                for (int i = 0; i < steps.Length; i++)
                {
                    var p = ss.GetArrayElementAtIndex(i); var st = steps[i];
                    p.FindPropertyRelative("id").stringValue = st.id; p.FindPropertyRelative("nodeId").stringValue = st.nodeId; p.FindPropertyRelative("objective").stringValue = st.objective;
                    p.FindPropertyRelative("questIndex").intValue = st.questIndex; p.FindPropertyRelative("action").enumValueIndex = (int)st.action; p.FindPropertyRelative("puzzle").objectReferenceValue = st.puzzle;
                    p.FindPropertyRelative("journal").stringValue = st.journal ?? ""; p.FindPropertyRelative("search").boolValue = st.search;
                    var ls = p.FindPropertyRelative("lines"); ls.arraySize = st.lines.Length; for (int j = 0; j < st.lines.Length; j++) ls.GetArrayElementAtIndex(j).stringValue = st.lines[j];
                }
                s.FindProperty("reflection").stringValue = "Candaan yang membuat orang takut perlu dihentikan. Mengambil barang pribadi teman tanpa izin bukan tindakan yang benar. Dukungan korban berarti mendengar, membantu secara aman, dan menawarkan orang dewasa yang dapat dipercaya.";
            }); return content;
        }
        static LevelOneStep Step(string id, string node, int quest, LevelOneAction action, string objective, params string[] lines) => new LevelOneStep { id = id, nodeId = node, questIndex = quest, action = action, objective = objective, lines = lines };

        [MenuItem("Tools/The Unseen Side/Level 1 PRD/Apply to current Level01")]
        public static void Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.name != "Level01") throw new Exception("Open existing Level01 in Edit Mode first.");
            if (scene.GetRootGameObjects().Any(g => g.name == RootName)) throw new Exception("Already applied. Move existing markers; do not rebuild over authored positions.");
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            var content = Content();
            material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "Marker.mat");
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.color = new Color(.1f, .85f, .65f); AssetDatabase.CreateAsset(material, Folder + "Marker.mat"); }
            var player = GameObject.Find("Player"); var bimaSource = GameObject.Find("Bima_Guide"); var bullySource = GameObject.Find("Nakal"); var teacherSource = GameObject.Find("IbuGuru_Kelas");
            if (player == null || bimaSource == null || bullySource == null || teacherSource == null) throw new Exception("Expected scene actors missing.");
            var root = new GameObject(RootName); Undo.RegisterCreatedObjectUndo(root, "Implement Level 1 PRD");
            var director = root.AddComponent<LevelOneDirector>(); var view = root.AddComponent<LevelOneView>(); var stage = root.AddComponent<LevelOneStaging>();
            var locations = Child(root.transform, "01 - Titik Interaksi (pindahkan dengan Move Tool)", Vector3.zero).transform;
            points.Clear();
            Point(locations, director, "MeetBima", "Bima - Gerbang", -7, -40);
            Point(locations, director, "ClassFront", "Papan VIII A", 32.8f, 21);
            Point(locations, director, "Corridor", "Kejadian koridor", 29, 20);
            Point(locations, director, "BimaCorridor", "Bima - Setelah dihadang", 30, 20);
            Point(locations, director, "Teacher", "Bu Ratna - Perkenalan", 32.8f, 25.3f);
            Point(locations, director, "Sikka", "Kursi kosong Sikka", 26.2f, 26.8f);
            Point(locations, director, "Canteen", "Insiden kantin", -57, 8);
            Point(locations, director, "BimaCanteen", "Bima - Tawarkan bantuan", -56, 10);
            Point(locations, director, "Fatimah", "Fatimah - Saksi", -56, 12);
            Point(locations, director, "Staff", "Petugas Kantin - Minta izin", -63, .5f);
            Point(locations, director, "P1", "Meja kantin - Petunjuk", -59, 6);
            Point(locations, director, "P2", "Bangku taman - Tas orang lain", -53, 14);
            Point(locations, director, "P3", "Rak troli - Botol Bima", -63, 2);
            Point(locations, director, "P4", "Pot besar - Jejak langkah", -54, 1);
            Point(locations, director, "P5", "Bangku pojok - Bekal Bima", -54, 11);
            Point(locations, director, "Bench", "Bangku taman - Kembalikan dan dukung Bima", -10, 16);
            var anchors = Child(root.transform, "02 - Posisi Aktor dan Rute (bebas dipindahkan)", Vector3.zero).transform;
            var actors = Child(root.transform, "03 - Pemeran PRD", Vector3.zero).transform;
            var bima = Actor(actors, "Bima", bimaSource, points["MeetBima"].transform);
            var memet = Actor(actors, "Memet", bullySource, points["Corridor"].transform);
            var raka = Actor(actors, "Raka", bullySource, points["Corridor"].transform);
            var teacher = Actor(actors, "Bu Ratna", teacherSource, points["Teacher"].transform);
            var fatimah = Actor(actors, "Fatimah (model sementara)", bimaSource, points["Fatimah"].transform);
            var staff = Actor(actors, "Petugas Kantin (model sementara)", teacherSource, points["Staff"].transform);
            foreach (var pair in new[] { (bima, "MeetBima"), (teacher, "Teacher"), (fatimah, "Fatimah"), (staff, "Staff") })
            {
                var anchor = pair.Item1.AddComponent<LevelOneActorAnchor>(); Ref(anchor, "point", points[pair.Item2].transform);
                Edit(anchor, s => s.FindProperty("onlyAtStart").boolValue = pair.Item1 == bima);
            }
            var agent = bima.AddComponent<NavMeshAgent>(); agent.speed = 2.3f; agent.angularSpeed = 180; agent.radius = .3f; agent.height = 2; agent.stoppingDistance = 1.4f;
            var companion = bima.AddComponent<LevelOneCompanion>(); Ref(companion, "director", director);
            Ref(companion, "home", points["MeetBima"].transform);
            var route = new[] { Anchor(anchors, "Route_01_Gerbang", -13.95f, -35), Anchor(anchors, "Route_02_Lapangan", -16.8f, -5.1f), Anchor(anchors, "Route_03_Koridor", 29, 3), points["ClassFront"].transform };
            Edit(companion, s => { var p = s.FindProperty("route"); p.arraySize = route.Length; for (int i = 0; i < route.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = route[i]; });
            var witnessAgent = fatimah.AddComponent<NavMeshAgent>(); witnessAgent.speed = 2.4f; witnessAgent.radius = .3f; witnessAgent.stoppingDistance = 2;
            var witness = fatimah.AddComponent<LevelOneCompanion>(); Ref(witness, "director", director); Edit(witness, s => s.FindProperty("witness").boolValue = true);
            Ref(witness, "home", points["Fatimah"].transform);
            // Fatimah's marker stays fixed while the actor accompanies the search.
            UnityEngine.Object.DestroyImmediate(fatimah.GetComponent<LevelOneActorAnchor>());
            Ref(stage, "bima", bima.transform); Ref(stage, "memet", memet.transform); Ref(stage, "raka", raka.transform); Ref(stage, "teacher", teacher.transform);
            Ref(stage, "corridorBima", points["BimaCorridor"].transform); Ref(stage, "corridorMemet", Anchor(anchors, "Memet_Koridor", 28, 21));
            Ref(stage, "corridorRaka", Anchor(anchors, "Raka_Koridor", 27, 20)); Ref(stage, "exit", Anchor(anchors, "Pelaku_Meninggalkan_Area", 22, 18));
            Ref(stage, "canteenBima", points["BimaCanteen"].transform); Ref(stage, "canteenMemet", Anchor(anchors, "Memet_Kantin", -58, 10)); Ref(stage, "benchBima", points["Bench"].transform);
            Ref(stage, "classroomBima", Anchor(anchors, "Bima_DalamKelas", 30.5f, 25.5f));
            var storyCamera = Child(root.transform, "04 - Kamera Adegan PRD", Vector3.zero).AddComponent<CinemachineCamera>();
            storyCamera.Priority = 0; storyCamera.Lens.FieldOfView = 48; Ref(stage, "storyCamera", storyCamera);
            var all = scene.GetRootGameObjects().Where(g => g != root).SelectMany(g => g.GetComponentsInChildren<MonoBehaviour>(true)).Where(b => b != null).ToArray();
            var disabled = new List<string>();
            var keep = new HashSet<string> { "GameStateManager", "SceneLoader", "LevelGameplayEntry", "QuestManager", "SaveManager", "AudioManager", "DialogueManager", "POVManager", "ChapterManager" };
            foreach (var b in all)
            {
                bool oldGameplay = b.GetType().Namespace != null && b.GetType().Namespace.StartsWith("BullyingGame") && !keep.Contains(b.GetType().Name);
                bool playerControl = b.gameObject == player && (b is BullyingGame.Player.PlayerMovement || b is BullyingGame.Player.PlayerInputHandler);
                if (oldGameplay && !playerControl && b.enabled) { b.enabled = false; disabled.Add(b.name + ":" + b.GetType().Name); EditorUtility.SetDirty(b); }
            }
            foreach (string name in new[] { "Bima_Guide", "Nakal", "Nakal1", "IbuGuru_Kelas", "korban1@Sad Idle", "PRDCast", "PRDParents", "WalkTalkCanvas", "Loading 2", "Loading Bully 2", "Loading Bully 3" })
            { var g = scene.GetRootGameObjects().FirstOrDefault(o => o.name == name); if (g != null) g.SetActive(false); }
            foreach (var d in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>(true))) { d.playOnAwake = false; }
            var gameCamera = UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include).First(c => c.name == "CinemachineCamera");
            foreach (var cam in UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include)) cam.Priority = cam == gameCamera ? 20 : 0;
            gameCamera.Follow = player.transform;
            ConfigureGameplayCamera();
            var controls = player.GetComponents<MonoBehaviour>().Where(b => b is BullyingGame.Player.PlayerMovement || b is BullyingGame.Player.PlayerInputHandler).Cast<Behaviour>().Concat(gameCamera.GetComponents<CinemachineInputAxisController>().Cast<Behaviour>()).ToArray();
            Edit(director, s =>
            {
                s.FindProperty("content").objectReferenceValue = content; s.FindProperty("player").objectReferenceValue = player; s.FindProperty("view").objectReferenceValue = view; s.FindProperty("staging").objectReferenceValue = stage;
                var ps = s.FindProperty("points"); ps.arraySize = points.Count; int i = 0; foreach (var p in points.Values) ps.GetArrayElementAtIndex(i++).objectReferenceValue = p;
                var cs = s.FindProperty("modalControls"); cs.arraySize = controls.Length; for (i = 0; i < controls.Length; i++) cs.GetArrayElementAtIndex(i).objectReferenceValue = controls[i];
            });
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null) new GameObject("Level01 EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)).transform.SetParent(root.transform);
            // Props are parented to their functional markers, so moving P3/P5 moves the items too.
            Prop(points["P3"].transform, "BOTOL-BIMA", PrimitiveType.Cylinder, new Vector3(.19f, .3f, .19f), new Vector3(0, .7f, 0), new Color(.1f, .35f, .9f));
            Prop(points["P5"].transform, "BEKAL-BIMA", PrimitiveType.Cube, new Vector3(.55f, .2f, .35f), new Vector3(0, .25f, 0), new Color(.9f, .65f, .12f));
            Prop(points["P2"].transform, "Tas bukan milik Bima (tidak bisa diambil)", PrimitiveType.Cube, new Vector3(.4f, .5f, .2f), new Vector3(0, .3f, 0), new Color(.35f, .2f, .45f));
            var inventory = root.AddComponent<LevelOneItemVisuals>(); Ref(inventory, "director", director); Ref(inventory, "bottle", points["P3"].transform.Find("BOTOL-BIMA").gameObject); Ref(inventory, "lunch", points["P5"].transform.Find("BEKAL-BIMA").gameObject);
            AddSearchProps();
            EnsureClassroomLink();
            foreach (var oldCanvas in scene.GetRootGameObjects().Where(g => g != root).SelectMany(g => g.GetComponentsInChildren<Canvas>(true)))
            {
                oldCanvas.enabled = false;
                var raycaster = oldCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>(); if (raycaster != null) raycaster.enabled = false;
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Directory.CreateDirectory("Logs/LevelOnePRD"); File.WriteAllLines("Logs/LevelOnePRD/DisabledLegacyBehaviours.txt", disabled);
            Selection.activeGameObject = root; Debug.Log("[Level01 PRD] Applied: 5 quests, 13 steps, 16 movable interaction points, 3 word puzzles.");
        }
        public static void ConfigureGameplayCamera()
        {
            var player = GameObject.Find("Player"); player.GetComponent<CharacterController>().stepOffset = .35f;
            var camera = UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include).First(c => c.name == "CinemachineCamera");
            Edit(camera.GetComponent<CinemachineOrbitalFollow>(), s =>
            {
                s.FindProperty("OrbitStyle").enumValueIndex = 0; s.FindProperty("Radius").floatValue = 4.2f;
                s.FindProperty("TargetOffset").vector3Value = new Vector3(.35f, 1.35f, 0);
                s.FindProperty("VerticalAxis").FindPropertyRelative("Value").floatValue = 15;
                s.FindProperty("VerticalAxis").FindPropertyRelative("Center").floatValue = 15;
                s.FindProperty("VerticalAxis").FindPropertyRelative("Range").vector2Value = new Vector2(-10, 60);
            });
            Edit(camera.GetComponent<CinemachineRotationComposer>(), s =>
            {
                s.FindProperty("TargetOffset").vector3Value = new Vector3(0, 1.25f, 0);
                s.FindProperty("Composition").FindPropertyRelative("ScreenPosition").vector2Value = Vector2.zero;
            });
            camera.Lens.FieldOfView = 55;
            var oldMarker = GameObject.Find("GroundMarker"); if (oldMarker != null) oldMarker.SetActive(false);
        }
        public static void EnsureClassroomLink()
        {
            var root = GameObject.Find(RootName);
            if (root.transform.Find("05 - Penghubung NavMesh Pintu VIII A") != null) return;
            var go = Child(root.transform, "05 - Penghubung NavMesh Pintu VIII A", new Vector3(32.8f, 0, 23.3f));
            var start = Child(go.transform, "Sisi Koridor", new Vector3(32.8f, .05f, 22));
            var end = Child(go.transform, "Sisi Kelas", new Vector3(32.8f, .26f, 24.6f));
            var link = go.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            link.startTransform = start.transform; link.endTransform = end.transform; link.width = 1.5f; link.bidirectional = true; link.autoUpdate = true; link.UpdateLink();
        }
        public static void AddSearchProps()
        {
            var found = UnityEngine.Object.FindObjectsByType<LevelOnePoint>().ToDictionary(p => p.Id);
            if (material == null) material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "Marker.mat");
            if (found["P3"].transform.Find("RakTroli_Visual") != null) return;
            Color wood = new Color(.42f, .26f, .12f), steel = new Color(.4f, .5f, .55f);
            Prop(found["P3"].transform, "RakTroli_Visual", PrimitiveType.Cube, new Vector3(1.2f, .1f, .65f), new Vector3(0, .36f, .6f), steel);
            Prop(found["P3"].transform, "RakTroli_RakAtas", PrimitiveType.Cube, new Vector3(1.2f, .1f, .65f), new Vector3(0, 1.1f, .6f), steel);
            foreach (int sign in new[] { -1, 1 }) Prop(found["P3"].transform, "RakTroli_Tiang" + sign, PrimitiveType.Cube, new Vector3(.08f, 1.1f, .65f), new Vector3(sign * .55f, .6f, .6f), steel);
            found["P3"].transform.Find("BOTOL-BIMA").localPosition = new Vector3(0, .7f, .6f);
            foreach (string id in new[] { "P2", "P5" })
            {
                Prop(found[id].transform, "BangkuPencarian_Alas", PrimitiveType.Cube, new Vector3(2, .12f, .65f), new Vector3(0, .62f, .6f), wood);
                Prop(found[id].transform, "BangkuPencarian_Sandaran", PrimitiveType.Cube, new Vector3(2, .6f, .1f), new Vector3(0, .95f, .9f), wood);
                foreach (int sign in new[] { -1, 1 }) Prop(found[id].transform, "BangkuPencarian_Kaki" + sign, PrimitiveType.Cube, new Vector3(.12f, .6f, .6f), new Vector3(sign * .8f, .3f, .6f), steel);
            }
            found["P5"].transform.Find("BEKAL-BIMA").localPosition = new Vector3(0, .2f, .6f);
            Prop(found["P4"].transform, "PotBesar_Visual", PrimitiveType.Cylinder, new Vector3(.8f, .4f, .8f), new Vector3(0, .4f, .7f), wood);
            Prop(found["P4"].transform, "PotBesar_Tanaman", PrimitiveType.Sphere, new Vector3(1.1f, .9f, 1.1f), new Vector3(0, 1.05f, .7f), new Color(.15f, .4f, .2f));
            AssetDatabase.SaveAssets();
        }
        static void Prop(Transform parent, string name, PrimitiveType type, Vector3 scale, Vector3 pos, Color color)
        {
            var g = GameObject.CreatePrimitive(type); g.name = name; g.transform.SetParent(parent, false); g.transform.localPosition = pos; g.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
            var mat = new Material(material); mat.color = color; string path = Folder + name.Split(' ')[0] + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path); if (existing == null) AssetDatabase.CreateAsset(mat, path); else { UnityEngine.Object.DestroyImmediate(mat); mat = existing; }
            g.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
