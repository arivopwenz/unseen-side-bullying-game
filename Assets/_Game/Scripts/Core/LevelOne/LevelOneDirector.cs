using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using BullyingGame.Core;
using BullyingGame.Interaction;
using BullyingGame.Quest;
using BullyingGame.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

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
        [SerializeField] private LevelOneDayTwoFlow dayTwo;
        [SerializeField] private LevelOneBottleGrab bottleGrab;
        public bool IsDayTwo => dayTwo != null;
        public bool UsesExistingFlow => useExistingFlow;
        public event Action ProgressChanged;
        [Header("Rebindable UI actions")]
        [SerializeField] private InputAction journalAction = new InputAction("Journal", InputActionType.Button, "<Keyboard>/tab");
        readonly InputAction gamepadJournal = new InputAction("Gamepad Journal", InputActionType.Button, "<Gamepad>/select");
        readonly InputAction gamepadCancel = new InputAction("Gamepad Cancel", InputActionType.Button, "<Gamepad>/buttonEast");
        readonly InputAction gamepadScroll = new InputAction("Gamepad Journal Scroll", InputActionType.Value, "<Gamepad>/rightStick/y");
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
        public LevelOnePoint Target => Complete ? null : Searching ? points.FirstOrDefault(p => p != null && p.Id == (!Progress.bottleTaken ? "P3" : "P5")) : points.FirstOrDefault(p => p != null && p.Id == Step.nodeId);
        public int HeldCount => Progress == null || Progress.returned ? 0 : (Progress.bottleTaken ? 1 : 0) + (Progress.lunchTaken ? 1 : 0);
        public bool SaveFailed { get; private set; }
        public bool Ready { get; private set; }
        GameInputActions input;
        ActiveInputDeviceTracker promptDevice;
        PlayerInteraction playerInteraction;
        public string InteractPrompt { get; private set; }
        public string PausePrompt { get; private set; }
        public string JournalPrompt { get; private set; }

        void RefreshPrompts()
        {
            if (input == null || promptDevice == null) return;
            var device = promptDevice.Device;
            InteractPrompt = InputPromptResolver.Label(input.Movement.Interact, device);
            PausePrompt = InputPromptResolver.Label(input.Movement.Pause, device);
            JournalPrompt = InputPromptResolver.Label(device is Gamepad ? gamepadJournal : journalAction, device);
            var ui = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            var submit = InputPromptResolver.Label(ui?.submit?.action, device);
            view.SetControlPrompts(
                FormatPrompt(input.Movement.Move, "Gerak"),
                FormatPrompt(input.Movement.Look, "Kamera"),
                FormatPrompt(input.Movement.Sprint, "Lari"),
                FormatPrompt(input.Movement.Jump, "Lompat"),
                FormatPrompt(input.Movement.Interact, "Interaksi"),
                string.IsNullOrEmpty(JournalPrompt) ? "" : $"[{JournalPrompt}] Jurnal",
                string.IsNullOrEmpty(PausePrompt) ? "" : $"[{PausePrompt}] Jeda",
                submit);
            string FormatPrompt(InputAction action, string caption)
            {
                string label = InputPromptResolver.Label(action, device);
                return string.IsNullOrEmpty(label) ? "" : $"[{label}] {caption}";
            }
        }
        bool[] controlStates;
        float lastProgress;
        int dismissedFrame;
        readonly List<LevelOneAction> cards = new List<LevelOneAction>();
        int selectedCard;
        void OnEnable()
        {
            input = new GameInputActions();
            input.Movement.Interact.Enable();
            input.Movement.Pause.Enable();
            journalAction.Enable(); cardOne.Enable(); cardTwo.Enable(); cardThree.Enable();
            gamepadJournal.Enable(); gamepadCancel.Enable(); gamepadScroll.Enable();
            promptDevice = new ActiveInputDeviceTracker();
            promptDevice.Changed += RefreshPrompts;
        }
        void Start()
        {
            RefreshPrompts();
            LevelOneCheckpoint.UseDayTwo = IsDayTwo;
            Progress = LevelOneCheckpoint.Load(content.Steps.Length);
            if (Progress.externalCompleted == null) Progress.externalCompleted = new string[0];
            QuestManager.Instance.RestoreProgress(Progress.quests, content.Quests);
            EnsureQuest();
            bool classroomOpening = IsDayTwo && Step?.id == "greet";
            if (!classroomOpening && Progress.hasPosition && UnityEngine.AI.NavMesh.SamplePosition(Progress.position, out var hit, 3, UnityEngine.AI.NavMesh.AllAreas))
            {
                var controller = player.GetComponent<CharacterController>(); controller.enabled = false;
                player.transform.position = hit.position; controller.enabled = true;
            }
            if (!classroomOpening) dayTwo?.RestoreBima(Progress);
            lastProgress = Time.unscaledTime; Ready = true;
            staging?.Apply(this);
            if (useExistingFlow) ConsumeExternalSteps();
            Save();
        }
        void Update()
        {
            if (playerInteraction == null && player != null)
                playerInteraction = player.GetComponent<PlayerInteraction>();
            var door = Ready && !IsModal && dismissedFrame != Time.frameCount &&
                PlayerInteraction.CanUseInteraction() && playerInteraction != null && !playerInteraction.enabled
                ? playerInteraction.FindNearbyDoor() : null;
            if (playerInteraction != null && !playerInteraction.enabled)
                playerInteraction.ShowDoorPrompt(door, InteractPrompt);
            if (!Ready || GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Cinematic) return;
            if (IsModal)
            {
                if (gamepadCancel.WasPressedThisFrame() || input.Movement.Pause.WasPressedThisFrame())
                    view.CancelModal();
                if (IsModal) view.ScrollJournal(gamepadScroll.ReadValue<float>() * Time.unscaledDeltaTime);
                return;
            }
            if (journalAction.WasPressedThisFrame() || gamepadJournal.WasPressedThisFrame()) ShowJournal();
            else if (input.Movement.Pause.WasPressedThisFrame()) ShowPause();
            if (IsModal || dismissedFrame == Time.frameCount) return;
            Nearest = points.Where(p => p != null && p.CanInteract() && IsReachable(p)).OrderBy(p => (p.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
            var available = points.Where(p => p != null && IsRelevant(p) && IsReachable(p)).Select(ActionFor).Distinct().Take(3).ToArray();
            if (!cards.SequenceEqual(available)) { cards.Clear(); cards.AddRange(available); selectedCard = 0; }
            if (cardOne.WasPressedThisFrame()) selectedCard = 0;
            if (cardTwo.WasPressedThisFrame() && cards.Count > 1) selectedCard = 1;
            if (cardThree.WasPressedThisFrame() && cards.Count > 2) selectedCard = 2;
            if (input.Movement.Interact.WasPressedThisFrame())
            {
                if (door != null) door.Interact(player);
                else if (Nearest != null)
                {
                    var selected = cards.Count > 0 ? cards[selectedCard] : ActionFor(Nearest);
                    var target = points.Where(p => p != null && p.CanInteract() && IsReachable(p) && ActionFor(p) == selected).OrderBy(p => (p.transform.position-player.transform.position).sqrMagnitude).FirstOrDefault();
                    if (target != null) target.Interact(player);
                }
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
                if (IsDayTwo) return Progress.bottleFound ? LevelOneAction.Take : LevelOneAction.Inspect;
                if (point.Id == "Staff") return LevelOneAction.Ask;
                if (point.Id == "P3" && Progress.bottleFound && !Progress.bottleTaken || point.Id == "P5" && Progress.lunchFound && !Progress.lunchTaken) return LevelOneAction.Take;
                return LevelOneAction.Inspect;
            }
            return Step != null ? Step.action : LevelOneAction.Observe;
        }
        public bool IsRelevant(LevelOnePoint point)
        {
            if (!Ready || Complete) return false;
            if (Searching && IsDayTwo) return point.Id == "P3" && !Progress.bottleTaken;
            if (Searching) return point.Id == "Staff" && !Progress.permission || point.Id.StartsWith("P") && point.Id.Length == 2 &&
                !(point.Id == "P3" && Progress.bottleTaken) && !(point.Id == "P5" && Progress.lunchTaken);
            return point.Id == Step.nodeId;
        }
        public bool CanUse(LevelOnePoint point) => Ready && !IsModal && IsRelevant(point) &&
            (!IsDayTwo || dayTwo.CanInteract(point)) &&
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
            if (IsDayTwo && Step.id == "incident") { dayTwo.StartIncident(); return; }
            if (Searching) { Search(point); return; }
            if (Step.action == LevelOneAction.Return && (!Progress.bottleTaken || !IsDayTwo && !Progress.lunchTaken))
            {
                if (IsDayTwo) Present(new[] { "Cari botol Bima terlebih dahulu." });
                return;
            }
            if (IsDayTwo && Step.puzzle != null)
            {
                OpenDayTwoPuzzle();
                return;
            }
            var current = Step;
            SetModal(GameState.Dialogue);
            Action show = () => view.ShowLines(current.lines, () =>
            {
                if (current.puzzle != null) view.ShowPuzzle(current.puzzle, Progress.puzzleFailures, () => Resolve(current), Save);
                else if ((StepIndex == 2 || StepIndex == 6) && staging != null) staging.Depart(() => Resolve(current));
                else Resolve(current);
            });
            if (IsDayTwo && current.action == LevelOneAction.Return && bottleGrab != null)
            {
                view.SetGameplayVisible(false);
                bottleGrab.HandOver(show);
            }
            else if ((StepIndex == 2 || StepIndex == 6) && staging != null) staging.Approach(show);
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
                if (IsDayTwo) { CloseModal(); return; }
                view.ShowLines(new[] { "LEVEL 1 SELESAI — LANGKAH PERTAMA", content.Reflection, "Pelajaran tersimpan di jurnal. TAB untuk membacanya lagi. Level 2 belum diubah oleh implementasi ini." }, CloseModal);
            }
            else CloseModal();
        }
        void OpenDayTwoPuzzle()
        {
            var current = Step;
            SetModal(GameState.Dialogue);
            void Open() => view.ShowDayTwoPuzzle(current.puzzle, Progress, () => Resolve(current), CloseModal, Save);
            if (Progress.wordCardShown) Open();
            else view.ShowCard("Kartu Kata", current.objective, () =>
            {
                Progress.wordCardShown = true;
                Save();
                Open();
            });
        }
        public void BeginIncident()
        {
            SetModal(GameState.Cinematic);
            view.SetGameplayVisible(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public void FinishIncident(bool completed)
        {
            if (!IsDayTwo || Step?.id != "incident") return;
            if (!completed)
            {
                GameStateManager.Instance.SetState(GameState.Dialogue);
                view.ShowLines(new[] { "Adegan kantin gagal dimuat. Dekati penanda dan gunakan tombol interaksi untuk mencoba lagi." }, CloseModal);
                return;
            }
            Resolve(Step);
            PresentActionCard();
        }
        public void PresentActionCard()
        {
            if (IsModal || Progress.actionCardShown || Step?.id != "clue") return;
            SetModal(GameState.Dialogue);
            view.ShowActionCard("Cari dan kembalikan botol Bima", () =>
            {
                Progress.actionCardShown = true;
                Save();
                CloseModal();
            });
        }
        void Present(string[] lines)
        {
            SetModal(GameState.Dialogue);
            view.ShowLines(lines, CloseModal);
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
            if (IsDayTwo)
            {
                SetModal(GameState.Dialogue);
                if (Progress.bottleFound)
                {
                    var current = Step;
                    void FinishPickup()
                    {
                        Progress.bottleTaken = true;
                        Resolve(current);
                    }
                    if (bottleGrab == null) FinishPickup();
                    else
                    {
                        view.SetGameplayVisible(false);
                        bottleGrab.PickUp(FinishPickup);
                    }
                }
                else
                {
                    Progress.bottleFound = true;
                    Save();
                    view.ShowLines(new[] { "Ini botol Bima. Gunakan tombol interaksi lagi untuk mengambilnya." }, CloseModal);
                }
                return;
            }
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
        public string Objective => Complete ? "Pelajaran tersimpan. TAB membuka jurnal." :
            IsDayTwo && Step.id == "incident" && Progress.bimaPackedLunch ? "Ikuti Bima menuju kantin." :
            Step.objective + (Searching && !IsDayTwo ? " (" + HeldCount + "/2)" : "");
        public bool HintActive => Searching && Time.unscaledTime - lastProgress > 90;
        public string JournalText => IsDayTwo
            ? string.Join("\n\n", Progress.journal) + "\n\nTujuan saat ini: " + Objective
            : string.Join("\n\n", Progress.journal) + "\n\nBarang: " + (Progress.returned ? "2/2 sudah dikembalikan kepada Bima." : HeldCount + "/2 — " + (Progress.bottleTaken ? "BOTOL-BIMA " : "") + (Progress.lunchTaken ? "BEKAL-BIMA" : "")) + "\n\nTujuan saat ini: " + Objective;
        public void ShowJournal()
        {
            SetModal(GameState.Paused); view.ShowJournal(JournalText, CloseModal);
        }
        void ShowPause()
        {
            SetModal(GameState.Paused);
            view.ShowPause(CloseModal, () => { Save(); CloseModal(); }, () => view.ConfirmRestart(() =>
            {
                SaveFailed = !LevelOneCheckpoint.Write(LevelOneCheckpoint.NewProgress());
                if (SaveFailed) { ShowPause(); return; }
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
            dayTwo?.CaptureBima(Progress);
            Progress.quests = QuestManager.Instance.CaptureProgress(); Progress.position = player.transform.position; Progress.hasPosition = true;
            SaveFailed = !LevelOneCheckpoint.Write(Progress);
        }
        void OnApplicationQuit() { if (Ready) Save(); }
        void OnApplicationPause(bool pause) { if (pause && Ready) Save(); }
        void OnDestroy()
        {
            gamepadJournal.Dispose();
            gamepadCancel.Dispose();
            gamepadScroll.Dispose();
        }
        void OnDisable()
        {
            if (promptDevice != null)
            {
                promptDevice.Changed -= RefreshPrompts;
                promptDevice.Dispose();
                promptDevice = null;
            }
            if (input != null)
            {
                input.Movement.Disable();
                input.Dispose();
                input = null;
            }
            journalAction.Disable(); cardOne.Disable(); cardTwo.Disable(); cardThree.Disable();
            gamepadJournal.Disable(); gamepadCancel.Disable(); gamepadScroll.Disable();
        }
    }
}
