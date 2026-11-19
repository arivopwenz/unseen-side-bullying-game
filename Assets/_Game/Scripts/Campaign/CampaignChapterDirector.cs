using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using BullyingGame.Core;
using BullyingGame.LevelOne;
using BullyingGame.Quest;
using BullyingGame.UI;

namespace BullyingGame.Campaign
{
    [DefaultExecutionOrder(300)]
    [RequireComponent(typeof(LevelOneView), typeof(CampaignChallenges), typeof(CampaignCinematic))]
    public sealed class CampaignChapterDirector : MonoBehaviour
    {
        [SerializeField] private CampaignChapterData content;
        [SerializeField] private GameObject player;
        [SerializeField] private Transform[] points;
        [SerializeField] private Behaviour[] modalControls;
        [SerializeField] private CampaignEvade evade;
        [SerializeField] private CampaignWorldState world;
        CampaignProgress data;
        CampaignChapterProgress progress;
        LevelOneView view;
        CampaignChallenges challenges;
        GameInputActions input;
        ActiveInputDeviceTracker device;
        readonly InputAction journal = new InputAction("Journal", InputActionType.Button);
        readonly InputAction back = new InputAction("Back", InputActionType.Button);
        readonly InputAction scroll = new InputAction("Scroll Journal", InputActionType.Value, "<Gamepad>/rightStick/y");
        bool[] controlStates;
        bool modal, loading, unreadable, prepared, freeEpilogue;
        bool saveFailed;
        Transform markedPoint;
        string chapterHeading;
        int dismissedFrame;
        string interactLabel, pauseLabel, journalLabel, dodgeLabel;

        public CampaignChapterData Content => content;
        public CampaignProgress Data => data;
        public CampaignChapterProgress Progress => progress;
        public CampaignStep Step => Ready && progress.step < content.steps.Length ? content.steps[progress.step] : null;
        public Transform Target => Step != null ? points[progress.step] : null;
        public bool Ready { get; private set; }
        public bool IsModal => modal;
        public bool Complete => Ready && Step == null;
        public GameObject Player => player;

        void Awake()
        {
            view = GetComponent<LevelOneView>();
            challenges = GetComponent<CampaignChallenges>();
            BootstrapLauncher.EnsurePersistentSystems();
            if (QuestManager.Instance == null) new GameObject("QuestManager").AddComponent<QuestManager>();
            journal.AddBinding("<Keyboard>/tab");
            journal.AddBinding("<Gamepad>/select");
            back.AddBinding("<Keyboard>/escape");
            back.AddBinding("<Gamepad>/buttonEast");
        }
        void OnEnable()
        {
            input = new GameInputActions();
            input.Movement.Interact.Enable();
            input.Movement.Pause.Enable();
            journal.Enable(); back.Enable(); scroll.Enable();
            device = new ActiveInputDeviceTracker();
            device.Changed += RefreshPrompts;
        }
        void Start()
        {
            data = CampaignCheckpoint.Load();
            unreadable = CampaignCheckpoint.LastError != null;
            progress = CampaignCheckpoint.Chapter(data, content.chapter);
            if (!string.IsNullOrEmpty(progress.stepId))
            {
                int index = Array.FindIndex(content.steps, step => step.id == progress.stepId);
                if (index >= 0) progress.step = index;
            }
            progress.step = Mathf.Clamp(progress.step, 0, content.steps.Length);
            var quests = content.steps.Select(step => step.quest).ToArray();
            QuestManager.Instance.RestoreProgress(null, quests);
            for (int i = 0; i < progress.step; i++)
            {
                QuestManager.Instance.MakeQuestAvailable(quests[i]);
                QuestManager.Instance.TryStartQuest(quests[i]);
                QuestManager.Instance.UpdateObjective(quests[i], content.steps[i].id);
            }
            world?.Initialize();
            data.activeChapter = content.chapter;
            data.unlockedChapter = Mathf.Max(data.unlockedChapter, content.chapter);
            var spawn = content.spawn;
            if (progress.hasPosition && NavMesh.SamplePosition(progress.position, out var savedPoint, 2f, NavMesh.AllAreas))
            {
                var path = new NavMeshPath();
                if (NavMesh.CalculatePath(spawn, savedPoint.position, NavMesh.AllAreas, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                    spawn = savedPoint.position;
            }
            Teleport(spawn);
            Ready = true;
            ActivateStep();
            RefreshPrompts();
            GameStateManager.Instance.SetState(GameState.Playing);
            Save();
        }
        void Update()
        {
            if (!Ready || loading) return;
            if (modal)
            {
                if (back.WasPressedThisFrame() || input.Movement.Pause.WasPressedThisFrame()) view.CancelModal();
                view.ScrollJournal(scroll.ReadValue<float>() * Time.unscaledDeltaTime);
                return;
            }
            if (dismissedFrame == Time.frameCount) return;
            if (input.Movement.Pause.WasPressedThisFrame()) { Pause(); return; }
            if (journal.WasPressedThisFrame()) { Journal(); return; }
            if (Complete)
            {
                if (!freeEpilogue) ChapterResult();
                else
                {
                    var cast = transform.Find("Characters");
                    var friend = cast == null ? null : cast.Cast<Transform>()
                        .Where(actor => actor.gameObject.activeInHierarchy &&
                            Vector3.Distance(actor.position, player.transform.position) < 3f)
                        .OrderBy(actor => Vector3.Distance(actor.position, player.transform.position)).FirstOrDefault();
                    view.RefreshCampaign(content.title, "Epilog • Jelajahi sekolah. Menu jeda untuk kembali ke menu utama.",
                        friend != null ? "[" + interactLabel + "] Sapa " + friend.name : "",
                        "[" + pauseLabel + "] Jeda", false);
                    if (friend != null && input.Movement.Interact.WasPressedThisFrame())
                    {
                        SetModal(GameState.Dialogue);
                        view.ShowLines(new[] { EpilogueLine(friend.name) }, CloseModal);
                    }
                }
                return;
            }
            string objective = evade != null && evade.Running ? evade.Instruction(dodgeLabel) : Step.objective;
            bool near = CanUse();
            view.RefreshCampaign(chapterHeading, objective,
                near && !(evade != null && evade.Running) ? "[" + interactLabel + "] " + Step.title : "",
                saveFailed ? "Progres belum tersimpan. Buka menu jeda untuk mencoba lagi." :
                "[" + journalLabel + "] Jurnal     [" + pauseLabel + "] Jeda", false, player.transform, Target);
            if (Target != null && Target != markedPoint)
            {
                markedPoint = Target;
                GroundMarker.SetTarget(Target.position);
                Minimap.SetMissionTarget(Target.position);
            }
            if (near && input.Movement.Interact.WasPressedThisFrame() && !(evade != null && evade.Running)) Use();
        }
        static string EpilogueLine(string actor)
        {
            switch (actor)
            {
                case "Bima": return "Bima: Aku lebih tenang sekarang. Kalau ada masalah, aku tahu siapa yang bisa dimintai bantuan.";
                case "Radit": return "Radit: Aku mau baca di perpustakaan. Sekarang aku nggak khawatir kacamataku diambil.";
                case "Sikka": return "Sikka: Aku mau lanjut desain dengan caraku sendiri. Makasih sudah menghargai pilihanku.";
                case "Memet": return "Memet: Aku masih menjalani pendampingan. Aku harus membuktikan perubahan lewat tindakan.";
                case "Raka": return "Raka: Aku berhenti ikut mengejek. Kalau ada yang kesulitan, aku akan cari bantuan.";
                default: return actor + ": Kita jaga suasana kelas yang nyaman bersama-sama, ya.";
            }
        }

        public bool CanUse()
        {
            if (Step == null || modal || loading || GameStateManager.Instance.CurrentState != GameState.Playing) return false;
            if (Vector3.Distance(player.transform.position, Target.position) > 2.8f) return false;
            Vector3 from = player.transform.position + Vector3.up;
            Vector3 to = Target.position + Vector3.up;
            return !Physics.RaycastAll(from, (to - from).normalized, Vector3.Distance(from, to), ~0,
                    QueryTriggerInteraction.Ignore)
                .Any(hit => !hit.transform.IsChildOf(player.transform) && hit.transform != player.transform &&
                    !(hit.collider is CapsuleCollider) && !(hit.collider is CharacterController));
        }

        public void Use()
        {
            if (!CanUse()) return;
            int index = progress.step;
            SetModal(GameState.Dialogue);
            Action run = () =>
            {
                if (!modal || progress.step != index) return;
                view.ShowLines(Step.lines, () => RunTask(index));
            };
            if (Step.actionCard && !progress.actionPresented)
            {
                view.ShowActionCard(Step.objective, () =>
                {
                    progress.actionPresented = true;
                    Save();
                    run();
                });
            }
            else run();
        }
        void RunTask(int index)
        {
            if (Step == null || progress.step != index || !modal) return;
            if (Step.words != null)
            {
                view.ShowDayTwoPuzzle(Step.words, progress.wordProgress, () => After(index), CloseModal, () => Save());
                return;
            }
            if (!string.IsNullOrEmpty(Step.scenePath))
            {
                GameStateManager.Instance.SetState(GameState.Cinematic);
                StartCoroutine(GetComponent<CampaignCinematic>().Play(Step.scenePath, Step.afterLines, view, success =>
                {
                    if (progress.step != index) return;
                    if (success) Commit(index);
                    else view.ShowOptions("Adegan gagal dimuat", "Progres berada sebelum adegan. Coba lagi.",
                        new[] { "Coba lagi", "Kembali" }, option => { if (option == 0) RunTask(index); else CloseModal(); }, CloseModal);
                }));
                return;
            }
            if (Step.kind == CampaignTaskKind.Evade)
            {
                view.ShowOptions("Menuju tempat aman", "Jaga jarak dari Memet. Tiga ancaman ditandai sebelum terjadi. " +
                    "Dua peringatan mengulang latihan. [" + dodgeLabel + "] Menghindar.",
                    new[] { "Mulai", "Mode mudah", "Kembali" }, option =>
                    {
                        if (option == 2) { CloseModal(); return; }
                        CloseModal();
                        evade.Begin(option == 1, success =>
                        {
                            SetModal(GameState.Dialogue);
                            if (success) After(index);
                            else view.ShowLines(new[] { "Kembali ke awal jalur. Perhatikan tanda ancaman dan cari bantuan orang dewasa." },
                                () => RunTask(index));
                        });
                    }, CloseModal);
                return;
            }
            if (Step.kind == CampaignTaskKind.Order || Step.questions?.Length > 0)
            {
                challenges.Open(Step, progress, () => After(index), () => Save(), CloseModal);
                return;
            }
            After(index);
        }
        void After(int index)
        {
            if (progress.step != index || !modal) return;
            if (!string.IsNullOrEmpty(Step.choiceKey))
            {
                if (Step.choiceKey == "exhibit") data.exhibitChoice = new[] { "PRIVATE", "LIMITED", "SHOW" }[progress.question];
                if (Step.choiceKey == "report") data.reportChoice = new[] { "DIRECT", "MESSAGE", "ACCOMPANY" }[progress.question];
            }
            string[] lines = Step.afterLines ?? Array.Empty<string>();
            if (Step.choiceKey == "exhibit")
            {
                if (data.exhibitChoice == "PRIVATE")
                    lines = new[] { "Sikka: Hari ini mau kusimpan dulu, Ji.", "Aji: Oke. Aku bantu bereskan mejanya, ya." };
                else if (data.exhibitChoice == "LIMITED")
                    lines = new[] { "Sikka: Aku mau tunjukkan ke Laura dulu. Aku sendiri yang ajak dia lihat.",
                        "Aji: Oke, Sik. Papannya kita pakai buat yang lain." };
                else if (lines.Length == 0)
                    lines = new[] { "Sikka: Yang ini boleh dipajang. Aku mau ikut pilih tempatnya." };
            }
            if (data.reportChoice != "DIRECT" && data.reportChoice != "UNSET" && content.chapter == 3 &&
                Step.id.EndsWith("Q06"))
                lines = data.reportChoice == "MESSAGE"
                    ? new[] { "Aji: Bu, Sikka menitipkan pesan pendek ini. Dia ingin Ibu membacanya sendiri.",
                        "Bu Ratna: Ibu baca secara pribadi. Terima kasih sudah menyampaikannya." }
                    : new[] { "Aji: Bu, Sikka ingin ditemani dulu. Dia belum siap bertemu hari ini.",
                        "Bu Ratna: Baik. Bilang ke Sikka, dia boleh memilih waktu kalau sudah siap." };
            view.ShowLines(lines, () => Commit(index));
        }
        void Commit(int index)
        {
            if (progress.step != index || !modal) return;
            string id = Step.id;
            if (!data.completedStepIds.Contains(id)) data.completedStepIds = data.completedStepIds.Concat(new[] { id }).ToArray();
            CampaignCheckpoint.AddJournal(data, Step.insight);
            QuestManager.Instance.UpdateObjective(Step.quest, id);
            progress.step++;
            progress.stepId = progress.step < content.steps.Length ? content.steps[progress.step].id : "";
            progress.question = 0;
            progress.failures = 0;
            progress.selectedOrder = Array.Empty<string>();
            progress.orderBank = Array.Empty<string>();
            progress.actionPresented = false;
            progress.wordProgress = new LevelOneSaveData();
            prepared = false;
            Save();
            ActivateStep();
            CloseModal();
        }
        void ActivateStep()
        {
            world?.Apply(data);
            if (Step == null) return;
            chapterHeading = content.title + "   ·   " + (progress.step + 1) + "/" + content.steps.Length;
            var cast = transform.Find("Characters");
            var actor = cast != null ? cast.Find(Step.actor ?? "") : null;
            if (actor != null && Step.kind != CampaignTaskKind.Evade)
            {
                Vector3 stand = Target.position;
                for (int i = 0; i < 8; i++)
                {
                    var offset = Quaternion.Euler(0, i * 45, 0) * Vector3.right * 1.3f;
                    if (!NavMesh.SamplePosition(Target.position + offset, out var hit, .8f, NavMesh.AllAreas) ||
                        NavMesh.Raycast(Target.position, hit.position, out _, NavMesh.AllAreas)) continue;
                    stand = hit.position;
                    break;
                }
                actor.position = stand;
                Vector3 facing = Vector3.ProjectOnPlane(Target.position - stand, Vector3.up);
                if (facing.sqrMagnitude > .01f) actor.rotation = Quaternion.LookRotation(facing);
            }
            QuestManager.Instance.MakeQuestAvailable(Step.quest);
            QuestManager.Instance.TryStartQuest(Step.quest);
        }
        bool Save()
        {
            if (data == null || unreadable) return false;
            progress.position = player.transform.position;
            progress.hasPosition = true;
            progress.stepId = Step?.id ?? "";
            saveFailed = !CampaignCheckpoint.Write(data);
            return !saveFailed;
        }
        void SetModal(GameState state)
        {
            if (!modal)
            {
                controlStates = modalControls.Select(control => control != null && control.enabled).ToArray();
                for (int i = 0; i < modalControls.Length; i++)
                    if (modalControls[i] != null) modalControls[i].enabled = false;
            }
            modal = true;
            view.SetGameplayVisible(false);
            markedPoint = null;
            GroundMarker.ClearTarget(); Minimap.ClearMissionTarget();
            GameStateManager.Instance.SetState(state);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }
        void CloseModal()
        {
            view.HideModal();
            modal = false;
            dismissedFrame = Time.frameCount;
            GameStateManager.Instance.SetState(GameState.Playing);
            if (controlStates != null)
                for (int i = 0; i < modalControls.Length; i++)
                    if (modalControls[i] != null) modalControls[i].enabled = controlStates[i];
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
        void Pause()
        {
            SetModal(GameState.Paused);
            view.ShowOptions("Jeda", content.title, new[] { "Lanjut", "Simpan", "Jurnal", "Menu utama" }, option =>
            {
                if (option == 0) CloseModal();
                if (option == 1) { if (Save()) CloseModal(); else SaveError(Pause); }
                if (option == 2) Journal();
                if (option == 3) { if (Save()) ChapterSceneFlow.Load("01_MainMenu"); else SaveError(Pause); }
            }, CloseModal);
        }
        void Journal()
        {
            SetModal(GameState.Paused);
            string current = Step == null ? content.reflection : "Tujuan sekarang\n" + Step.objective;
            view.ShowJournal(current + "\n\n" + string.Join("\n\n", data.journal.Reverse()), CloseModal);
        }
        void SaveError(Action retry) => view.ShowOptions("Progres belum tersimpan", CampaignCheckpoint.LastError ??
            "Coba simpan kembali.", new[] { "Coba lagi" }, _ => retry());
        void ChapterResult()
        {
            if (prepared) return;
            prepared = true;
            SetModal(GameState.Result);
            CampaignCheckpoint.AddJournal(data, content.reflection);
            bool end = content.chapter == 5;
            view.ShowOptions(end ? "UNSEEN SIDE — TAMAT" : content.title + " selesai", content.reflection,
                end ? new[] { "Credits", "Jelajahi epilog", "Menu utama" } : new[] { "Lanjut chapter berikutnya", "Jurnal" },
                option =>
                {
                    if (loading) return;
                    if (!end && option == 1) { prepared = false; Journal(); return; }
                    if (end && option == 0)
                    {
                        view.ShowLines(new[] { "UNSEEN SIDE", "Dibuat oleh tim pengembang UNSEEN SIDE.",
                            "Model, animasi, dan aset mengikuti kredit lisensi proyek.",
                            "Terima kasih sudah belajar mendengar, membantu, dan mencari perlindungan." }, () => { prepared = false; CloseModal(); });
                        return;
                    }
                    if (end && option == 1)
                    {
                        data.complete = true;
                        if (!Save()) { prepared = false; SaveError(ChapterResult); return; }
                        freeEpilogue = true;
                        CloseModal();
                        return;
                    }
                    int next = Mathf.Min(5, content.chapter + 1);
                    data.activeChapter = next;
                    data.unlockedChapter = Mathf.Max(data.unlockedChapter, next);
                    data.complete = end;
                    if (!Save()) { prepared = false; SaveError(ChapterResult); return; }
                    loading = ChapterSceneFlow.Load(end ? "01_MainMenu" : content.nextScene);
                    if (loading) view.HideModal();
                    else
                    {
                        prepared = false;
                        view.ShowOptions("Perpindahan tertunda", "Chapter berikutnya belum bisa dibuka.",
                            new[] { "Coba lagi" }, _ => ChapterResult());
                    }
                });
        }
        void RefreshPrompts()
        {
            if (device == null) return;
            string Label(InputAction action) => InputPromptResolver.Label(action, device.Device);
            interactLabel = Label(input.Movement.Interact);
            pauseLabel = Label(input.Movement.Pause);
            journalLabel = Label(journal);
            dodgeLabel = evade != null ? Label(evade.Dodge) : "";
            var ui = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            view.SetControlPrompts("[" + Label(input.Movement.Move) + "] Gerak",
                "[" + Label(input.Movement.Look) + "] Kamera", "[" + Label(input.Movement.Sprint) + "] Lari",
                "[" + Label(input.Movement.Jump) + "] Lompat", "[" + interactLabel + "] Interaksi",
                "[" + journalLabel + "] Jurnal", "[" + pauseLabel + "] Jeda", Label(ui?.submit?.action));
        }
        void Teleport(Vector3 destination)
        {
            var cc = player.GetComponent<CharacterController>();
            cc.enabled = false; player.transform.position = destination; cc.enabled = true;
        }
        void OnApplicationQuit() { if (Ready) Save(); }
        void OnApplicationPause(bool paused) { if (paused && Ready) Save(); }
        void OnDisable()
        {
            device?.Dispose(); device = null;
            input?.Movement.Disable(); input?.Dispose(); input = null;
            journal.Disable(); back.Disable(); scroll.Disable();
        }
        void OnDestroy() { journal.Dispose(); back.Dispose(); scroll.Dispose(); }
    }
}
