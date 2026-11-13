using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using BullyingGame.Core;
using BullyingGame.Quest;

namespace BullyingGame.LevelOne
{
    [DefaultExecutionOrder(300)]
    public class LevelOneDirector : MonoBehaviour
    {
        [Header("Scene bindings — points remain freely movable")]
        [SerializeField] private LevelOneContent content;
        [SerializeField] private GameObject player;
        [SerializeField] private LevelOnePoint[] points;
        [SerializeField] private LevelOneView view;
        [SerializeField] private Behaviour[] modalControls;
        [SerializeField] private LevelOneStaging staging;
        [Header("Use the scene's original guide, trigger zones and cutscenes")]
        [SerializeField] private bool useExistingFlow;
        public bool UsesExistingFlow => useExistingFlow;
        public event Action ProgressChanged;
        [Header("Rebindable UI actions")]
        [SerializeField] private InputAction journalAction = new InputAction("Journal", InputActionType.Button, "<Keyboard>/tab");
        [SerializeField] private InputAction pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        [SerializeField] private InputAction cardOne = new InputAction("CardOne", InputActionType.Button, "<Keyboard>/1");
        [SerializeField] private InputAction cardTwo = new InputAction("CardTwo", InputActionType.Button, "<Keyboard>/2");
        [SerializeField] private InputAction cardThree = new InputAction("CardThree", InputActionType.Button, "<Keyboard>/3");
        public LevelOneSaveData Progress { get; private set; }
        public GameObject Player => player;
        public int StepIndex => Progress == null ? 0 : Progress.step;
        public bool Complete => content != null && Progress != null && StepIndex >= content.Steps.Length;
        public LevelOneStep Step => Complete || content == null ? null : content.Steps[StepIndex];
        public bool Searching => Step != null && Step.search;
        public bool IsModal { get; private set; }
        public LevelOnePoint Nearest { get; private set; }
        public LevelOnePoint Target => Complete ? null : Searching ? points.FirstOrDefault(p => p.Id == (!Progress.bottleTaken ? "P3" : "P5")) : points.FirstOrDefault(p => p.Id == Step.nodeId);
        public int HeldCount => Progress == null || Progress.returned ? 0 : (Progress.bottleTaken ? 1 : 0) + (Progress.lunchTaken ? 1 : 0);
        public bool SaveFailed { get; private set; }
        public bool Ready { get; private set; }
        GameInputActions input;
        bool[] controlStates;
        float lastProgress;
        int dismissedFrame;
        readonly List<LevelOneAction> cards = new List<LevelOneAction>();
        int selectedCard;
        void OnEnable()
        {
            input = new GameInputActions(); input.Movement.Interact.Enable();
            journalAction.Enable(); pauseAction.Enable(); cardOne.Enable(); cardTwo.Enable(); cardThree.Enable();
        }
        void Start()
        {
            Progress = LevelOneCheckpoint.Load(content.Steps.Length);
            if (Progress.externalCompleted == null) Progress.externalCompleted = new string[0];
            QuestManager.Instance.RestoreProgress(Progress.quests, content.Quests);
            EnsureQuest();
            if (Progress.hasPosition && UnityEngine.AI.NavMesh.SamplePosition(Progress.position, out var hit, 3, UnityEngine.AI.NavMesh.AllAreas))
            {
                var controller = player.GetComponent<CharacterController>(); controller.enabled = false;
                player.transform.position = hit.position; controller.enabled = true;
            }
            lastProgress = Time.unscaledTime; Ready = true;
            staging?.Apply(this);
            if (useExistingFlow) ConsumeExternalSteps();
            Save();
        }
        void Update()
        {
            if (!Ready || GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Cinematic) return;
            if (journalAction.WasPressedThisFrame() && !IsModal) ShowJournal();
            if (pauseAction.WasPressedThisFrame() && !IsModal) ShowPause();
            if (IsModal || dismissedFrame == Time.frameCount) return;
            Nearest = points.Where(p => p.CanInteract() && IsReachable(p)).OrderBy(p => (p.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
            var available = points.Where(p => IsRelevant(p) && IsReachable(p)).Select(ActionFor).Distinct().Take(3).ToArray();
            if (!cards.SequenceEqual(available)) { cards.Clear(); cards.AddRange(available); selectedCard = 0; }
            if (cardOne.WasPressedThisFrame()) selectedCard = 0;
            if (cardTwo.WasPressedThisFrame() && cards.Count > 1) selectedCard = 1;
            if (cardThree.WasPressedThisFrame() && cards.Count > 2) selectedCard = 2;
            if (Nearest != null && input.Movement.Interact.WasPressedThisFrame())
            {
                var selected = cards.Count > 0 ? cards[selectedCard] : ActionFor(Nearest);
                var target = points.Where(p => p.CanInteract() && IsReachable(p) && ActionFor(p) == selected).OrderBy(p => (p.transform.position-player.transform.position).sqrMagnitude).FirstOrDefault();
                if (target != null) target.Interact(player);
            }
            view.Refresh(this, cards.Select(ActionName).ToArray(), selectedCard);
        }
        public static string ActionName(LevelOneAction action)
        {
            switch (action) { case LevelOneAction.Ask: return "TANYA"; case LevelOneAction.Inspect: return "PERIKSA"; case LevelOneAction.Take: return "AMBIL"; case LevelOneAction.Return: return "KEMBALIKAN"; case LevelOneAction.Record: return "CATAT"; default: return "AMATI"; }
        }
        public LevelOneAction ActionFor(LevelOnePoint point)
        {
            if (Searching)
            {
                if (point.Id == "Staff") return LevelOneAction.Ask;
                if (point.Id == "P3" && Progress.bottleFound && !Progress.bottleTaken || point.Id == "P5" && Progress.lunchFound && !Progress.lunchTaken) return LevelOneAction.Take;
                return LevelOneAction.Inspect;
            }
            return Step != null ? Step.action : LevelOneAction.Observe;
        }
        public bool IsRelevant(LevelOnePoint point)
        {
            if (!Ready || Complete) return false;
            if (Searching) return point.Id == "Staff" && !Progress.permission || point.Id.StartsWith("P") && point.Id.Length == 2 &&
                !(point.Id == "P3" && Progress.bottleTaken) && !(point.Id == "P5" && Progress.lunchTaken);
            return point.Id == Step.nodeId;
        }
        public bool CanUse(LevelOnePoint point) => Ready && !IsModal && IsRelevant(point) &&
            (!useExistingFlow || StepIndex == 5 || StepIndex >= 7) &&
            (StepIndex != 1 || staging == null || staging.BimaNear(point.transform)) &&
            GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Playing;
        public bool IsReachable(LevelOnePoint point)
        {
            if (player == null || point == null || Vector3.Distance(player.transform.position, point.transform.position) > point.Radius) return false;
            Vector3 origin = player.transform.position + Vector3.up * 1.25f;
            Vector3 end = point.transform.position + Vector3.up * 1.25f;
            foreach (var hit in Physics.RaycastAll(origin, (end-origin).normalized, (end-origin).magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(player.transform) || hit.transform == player.transform || hit.transform.IsChildOf(point.transform) || hit.transform == point.transform) continue;
                // Other actors are not opaque walls; they must not block a conversation.
                if (hit.collider is CharacterController || hit.collider is CapsuleCollider) continue;
                return false;
            }
            return true;
        }
        public void Use(LevelOnePoint point)
        {
            if (!CanUse(point) || !IsReachable(point)) return;
            if (Searching) { Search(point); return; }
            if (Step.action == LevelOneAction.Return && (!Progress.bottleTaken || !Progress.lunchTaken)) return;
            var current = Step;
            SetModal(GameState.Dialogue);
            Action show = () => view.ShowLines(current.lines, () =>
            {
                if (current.puzzle != null) view.ShowPuzzle(current.puzzle, Progress.puzzleFailures, () => Resolve(current), Save);
                else if ((StepIndex == 2 || StepIndex == 6) && staging != null) staging.Depart(() => Resolve(current));
                else Resolve(current);
            });
            if ((StepIndex == 2 || StepIndex == 6) && staging != null) staging.Approach(show);
            else show();
        }
        void Resolve(LevelOneStep current)
        {
            if (current != Step) return;
            if (current.nodeId == "Sikka") Progress.sikka = true;
            if (current.action == LevelOneAction.Return) Progress.returned = true;
            if (!string.IsNullOrEmpty(current.journal)) AddJournal(current.journal);
            QuestManager.Instance.UpdateObjective(content.Quests[current.questIndex], current.id);
            Progress.step++; lastProgress = Time.unscaledTime;
            EnsureQuest(); staging?.Apply(this); Save();
            ProgressChanged?.Invoke();
            if (Complete)
            {
                AddJournal(content.Reflection); Save();
                view.ShowLines(new[] { "LEVEL 1 SELESAI — LANGKAH PERTAMA", content.Reflection, "Pelajaran tersimpan di jurnal. TAB untuk membacanya lagi. Level 2 belum diubah oleh implementasi ini." }, CloseModal);
            }
            else CloseModal();
        }
        public bool ExternalDone(string id) => Progress != null && Progress.externalCompleted.Contains(id);
        public void CompleteExternal(string id)
        {
            if (!useExistingFlow || !Ready || ExternalDone(id)) return;
            Progress.externalCompleted = Progress.externalCompleted.Concat(new[] { id }).ToArray();
            ConsumeExternalSteps(); Save(); ProgressChanged?.Invoke();
        }
        void ConsumeExternalSteps()
        {
            while (!Complete && ExternalDone(Step.id))
            {
                EnsureQuest();
                if (!string.IsNullOrEmpty(Step.journal)) AddJournal(Step.journal);
                QuestManager.Instance.UpdateObjective(content.Quests[Step.questIndex], Step.id);
                Progress.step++;
            }
            EnsureQuest();
        }
        public void PlayExternalWordCard(string stepId, Action finished)
        {
            if (!useExistingFlow || IsModal || ExternalDone(stepId)) return;
            var step = content.Steps.FirstOrDefault(s => s.id == stepId);
            if (step == null || step.puzzle == null) return;
            SetModal(GameState.Dialogue);
            view.ShowPuzzle(step.puzzle, Progress.puzzleFailures, () =>
            {
                CompleteExternal(stepId); CloseModal(); finished?.Invoke();
            }, Save);
        }
        void Search(LevelOnePoint point)
        {
            string message;
            if (point.Id == "Staff") { Progress.permission = true; message = "Aji: Bu, boleh ambil botol biru berlabel Bima di rak?\nPetugas kantin: Boleh. Saya dampingi, ya. Tunggu di sisi pengunjung; jangan masuk area kerja."; }
            else if (point.Id == "P3")
            {
                if (!Progress.bottleFound) { Progress.bottleFound = true; message = "Botol biru berlabel Bima terlihat di rak. Minta izin petugas sebelum mengambilnya. Pilih AMBIL setelah mendapat izin."; }
                else if (!Progress.permission) message = "Area kerja kantin. TANYA petugas di penanda Petugas Kantin dahulu; Aji tidak masuk tanpa izin.";
                else { Progress.bottleTaken = true; message = "BOTOL BIMA diperoleh. Petugas menyerahkan botol dari area kerja. Barang milik teman harus dikembalikan."; }
            }
            else if (point.Id == "P5")
            {
                if (!Progress.lunchFound) { Progress.lunchFound = true; message = "Kotak bekal Bima ada di bawah bangku terbuka. Aman dijangkau. Gunakan AMBIL untuk menyimpannya."; }
                else { Progress.lunchTaken = true; message = "BEKAL BIMA diperoleh. Tidak perlu mengejar Memet ke tempat sepi."; }
            }
            else if (point.Id == "P2") message = "Aji: Ini bukan barang Bima, jangan kuambil.\nTas ini tetap di tempatnya. Menghormati barang orang lain juga bagian dari membantu.";
            else if (point.Id == "P4") message = "Jejak langkah menuju bangku pojok terbuka (P5). Coba periksa bagian bawah bangku.";
            else message = "Fatimah: Memet melewati sisi meja ini. Aku lihat botol biru dekat rak troli (P3); bekalnya mungkin dekat bangku pojok (P5).";
            if (!Progress.inspected.Contains(point.Id)) { var list = Progress.inspected.ToList(); list.Add(point.Id); Progress.inspected = list.ToArray(); }
            AddJournal(message); Save(); lastProgress = Time.unscaledTime;
            SetModal(GameState.Dialogue);
            view.ShowLines(new[] { message }, () =>
            {
                if (Progress.bottleTaken && Progress.lunchTaken) Resolve(Step);
                else CloseModal();
            });
        }
        void EnsureQuest()
        {
            if (Complete) return;
            var quest = content.Quests[Step.questIndex];
            QuestManager.Instance.MakeQuestAvailable(quest); QuestManager.Instance.TryStartQuest(quest);
        }
        void AddJournal(string text)
        {
            if (Progress.journal.Contains(text)) return;
            var list = Progress.journal.ToList(); list.Add(text); Progress.journal = list.ToArray();
            Debug.Log("[Level01 PRD] " + text);
        }
        public string QuestTitle => Complete ? "Level 1 selesai" : content.Quests[Step.questIndex].questTitle;
        public string Objective => Complete ? "Pelajaran tersimpan. TAB membuka jurnal." : Step.objective + (Searching ? " (" + HeldCount + "/2)" : "");
        public bool HintActive => Searching && Time.unscaledTime - lastProgress > 90;
        public string JournalText => string.Join("\n\n", Progress.journal) + "\n\nBarang: " + (Progress.returned ? "2/2 sudah dikembalikan kepada Bima." : HeldCount + "/2 — " + (Progress.bottleTaken ? "BOTOL-BIMA " : "") + (Progress.lunchTaken ? "BEKAL-BIMA" : "")) + "\n\nTujuan saat ini: " + Objective;
        public void ShowJournal()
        {
            SetModal(GameState.Paused); view.ShowJournal(JournalText, CloseModal);
        }
        void ShowPause()
        {
            SetModal(GameState.Paused);
            view.ShowPause(CloseModal, () => { Save(); CloseModal(); }, () => view.ConfirmRestart(() =>
            {
                Progress = new LevelOneSaveData(); QuestManager.Instance.RestoreProgress(null, content.Quests); EnsureQuest(); Save();
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }, ShowPause));
        }
        void SetModal(GameState state)
        {
            if (!IsModal)
            {
                controlStates = modalControls.Select(c => c != null && c.enabled).ToArray();
                for (int i = 0; i < modalControls.Length; i++) if (modalControls[i] != null) modalControls[i].enabled = false;
            }
            IsModal = true; GameStateManager.Instance.SetState(state);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }
        void CloseModal()
        {
            view.HideModal(); IsModal = false; dismissedFrame = Time.frameCount;
            GameStateManager.Instance.SetState(GameState.Playing);
            if (controlStates != null) for (int i = 0; i < modalControls.Length; i++) if (modalControls[i] != null) modalControls[i].enabled = controlStates[i];
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
        public void Save()
        {
            if (Progress == null || player == null || QuestManager.Instance == null) return;
            Progress.quests = QuestManager.Instance.CaptureProgress(); Progress.position = player.transform.position; Progress.hasPosition = true;
            SaveFailed = !LevelOneCheckpoint.Write(Progress);
        }
        void OnApplicationQuit() { if (Ready) Save(); }
        void OnApplicationPause(bool pause) { if (pause && Ready) Save(); }
        void OnDisable()
        {
            input?.Dispose(); input = null; journalAction.Disable(); pauseAction.Disable(); cardOne.Disable(); cardTwo.Disable(); cardThree.Disable();
        }
    }
}
