# Step 16 — Notebook → Bullying Event → Timeline → Gameplay

Panduan manual berdasarkan inspeksi project dan Unity Editor pada 6 Oktober 2026.
**Arsip rancangan awal. Coding kini dikerjakan oleh Codex di script project; contoh kode di bawah sudah tidak menjadi versi terkini. Jangan menyalin ulang contoh kode ini. Panduan konfigurasi Unity terbaru diberikan langsung di chat sesuai pembagian kerja yang diminta pengguna.**

## 1. Kondisi yang benar-benar ditemukan

- Editor `6000.6.3f1`, scene aktif `Assets/_Game/Scenes/Levels/Level_01/Level01.unity`, bukan path Level_01.unity dalam handoff lama.
- Console saat inspeksi: 0 error, 0 warning; Play Mode berhenti; scene tidak dirty.
- Notebook: `/_Level/NotebookItem`, component `QuestItem`, quest `Quest_Level01_LostNotebook`, objective `find_notebook`, Destroy On Collect aktif, Visual Root dan Event To Trigger kosong.
- Scene mempunyai DialogueManager, QuestManager, BullyingEventManager. Belum ada CutsceneManager, PlayableDirector, TimelineSignalHandler, SignalReceiver, atau cinematic camera.
- Belum ditemukan file Timeline `.playable`, signal `.signal`, atau asset BullyingEventData di Assets.
- Quest hanya mempunyai satu objective, `find_notebook`, jumlah 1. `UpdateObjective()` otomatis menyelesaikan quest ketika semua objective selesai. Tidak ada objective mengembalikan buku; interaksi Guru BK setelah Completed hanya memainkan dialog apresiasi.
- CutsceneManager sekarang menggunakan GameManager, sedangkan Bootstrap mempunyai GameStateManager. Jangan menambahkan GameManager kedua untuk menutupi perbedaan ini: gunakan GameStateManager untuk flow Step 16.
- Bootstrap berada pada Build Index 0, tetapi Level01 belum terdaftar pada daftar scene build yang diperiksa. Object Bootstrap/SceneLoader ada dalam YAML; pencarian source tidak menemukan class Bootstrap/SceneLoader. Karena itu, jangan mengasumsikan loading dari Bootstrap sudah berfungsi sebelum dicoba.
- Player bergerak dengan CharacterController. Rigidbody yang ditemukan bersifat kinematic; jangan mengubahnya menjadi dynamic untuk cinematic.

Dokumen `BullyingEducationGame_Development_Handoff_v1.0.md` masih menandai Step 7. Skill `.agents/skills/project-context/SKILL.md` dan `step-implementation/SKILL.md` menandai Step 16. Script dan scene menunjukkan fondasi Step 13–15 sudah ada, tetapi koneksi Step 16 belum terpasang. Ini bukan bukti seluruh Step 1–15 sudah lulus uji terbaru.

## 2. Target dan alasan desain

```text
Quest Active → ambil notebook → sembunyikan visual, tahan progress objective
→ BullyingEventManager.Triggered → BullyingEventDirector
→ CutsceneManager → Cinematic + kunci kontrol + kamera Cinemachine
→ Timeline Signal → Resolved / Failed → Stop Timeline → pulihkan gameplay

Resolved: progress find_notebook +1 → Quest Completed → dialog apresiasi Guru BK
Failed: progress tetap 0/1 → notebook terlihat lagi → ambil lagi → progress +1
        encounter tidak dimainkan lagi
```

Pada Step 16, kegagalan mengembalikan buku ke lokasi asal. Pencurian, item-state lengkap, dan random rehide adalah Step 20–21. Kita tidak menyatakan fitur itu selesai. QTE belum dipasang: source QTE lama memakai `Input.GetKey*` dan KeyCode, bertentangan dengan aturan Input System proyek. Gunakan signal hasil sebagai pengujian fondasi director dahulu.

Data event tetap ScriptableObject. Reference scene PlayableDirector disimpan di director scene, **bukan di BullyingEventData**, karena reusable asset tidak semestinya menyimpan object scene.

## 3. Persiapan manual

1. Keluar Play Mode, simpan scene, dan simpan backup/commit pekerjaanmu.
2. Edit script di folder aslinya agar GUID `.meta` tetap sama. Jangan membuat class bernama sama di file lain.
3. Kerjakan penggantian script di bagian 4 sebelum memasang component baru. Tunggu Unity selesai compile.
4. Jangan edit YAML `.unity`, `.prefab`, atau `.asset` secara manual. Semua wiring di bagian 5 melalui Inspector/Timeline.

## 4. Script yang kamu kerjakan

### A. Ganti isi `Assets/_Game/Scripts/Events/BullyingEventManager.cs`

Manager menjaga satu event aktif dan mencatat event yang sudah ditangani selama sesi scene. Wrapper `TriggerEvent` dipertahankan agar pemanggil lama tetap compile. Result ganda diabaikan; event identity tetap tersedia selama notifikasi hasil.

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BullyingGame.Events
{
    public class BullyingEventManager : MonoBehaviour
    {
        public static BullyingEventManager Instance { get; private set; }
        public event Action<BullyingEventData> OnEventTriggered;
        public event Action<BullyingEventData, BullyingEventState> OnEventStateChanged;
        public BullyingEventData CurrentEvent { get; private set; }
        public BullyingEventState CurrentState { get; private set; }
        private readonly HashSet<string> handledEvents = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool HasHandled(BullyingEventData data)
        {
            return data != null && handledEvents.Contains(data.eventId);
        }

        public bool TryTriggerEvent(BullyingEventData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.eventId) ||
                CurrentState != BullyingEventState.Inactive || HasHandled(data) ||
                OnEventTriggered == null) return false;
            CurrentEvent = data;
            SetState(BullyingEventState.Triggered);
            OnEventTriggered.Invoke(data);
            return true;
        }

        public void TriggerEvent(BullyingEventData data) => TryTriggerEvent(data);

        public void StartEvent()
        {
            if (CurrentState == BullyingEventState.Triggered)
                SetState(BullyingEventState.InProgress);
        }

        public void WaitForResponse()
        {
            if (CurrentState == BullyingEventState.InProgress)
                SetState(BullyingEventState.WaitingResponse);
        }

        public void ResolveEvent() => Finish(BullyingEventState.Resolved);
        public void FailEvent() => Finish(BullyingEventState.Failed);

        private void Finish(BullyingEventState result)
        {
            if (CurrentEvent == null ||
                (CurrentState != BullyingEventState.Triggered &&
                 CurrentState != BullyingEventState.InProgress &&
                 CurrentState != BullyingEventState.WaitingResponse)) return;
            handledEvents.Add(CurrentEvent.eventId);
            SetState(result);
            CurrentEvent = null;
            SetState(BullyingEventState.Inactive);
        }

        private void SetState(BullyingEventState state)
        {
            CurrentState = state;
            OnEventStateChanged?.Invoke(CurrentEvent, state);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
```

Catatan belajar: ini aturan sekali per event ID selama lifetime manager Level01. Save/load lintas sesi belum ada di Step 16. Pastikan ID unik. Failed juga dihitung sudah ditangani, sehingga pengambilan kedua tidak mengulang bullying.

### B. Ganti isi `Assets/_Game/Scripts/Core/CutsceneManager.cs`

State dan callback dipasang sebelum Play. Skip menghentikan playback tanpa melompat ke akhir atau Evaluate, agar signal sukses di akhir tidak terpicu oleh skip. Hasil skip untuk encounter yang belum punya result adalah Failed.

```csharp
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace BullyingGame.Core
{
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }
        public event Action OnCutsceneStarted;
        public event Action OnCutsceneEnded;
        public PlayableDirector CurrentDirector { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool TryPlayCutscene(PlayableDirector director)
        {
            if (!isActiveAndEnabled || director == null ||
                !director.isActiveAndEnabled || director.playableAsset == null ||
                CurrentDirector != null || GameStateManager.Instance == null)
                return false;
            CurrentDirector = director;
            director.stopped += OnDirectorStopped;
            director.time = 0;
            GameStateManager.Instance.SetState(GameState.Cinematic);
            OnCutsceneStarted?.Invoke();
            director.Play();
            return true;
        }

        public void PlayCutscene(PlayableDirector director) => TryPlayCutscene(director);
        public void SkipCutscene() => StopCutscene(CurrentDirector);

        public void StopCutscene(PlayableDirector director)
        {
            if (director == null || director != CurrentDirector) return;
            director.Stop();
            // Fallback jika playback belum sempat memasuki Playing.
            if (CurrentDirector == director) OnDirectorStopped(director);
        }

        private void OnDirectorStopped(PlayableDirector director)
        {
            if (director != CurrentDirector) return;
            director.stopped -= OnDirectorStopped;
            CurrentDirector = null;
            OnCutsceneEnded?.Invoke();
            // Jangan menimpa loading/paused/dialogue yang mungkin dimulai listener.
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Cinematic)
                GameStateManager.Instance.SetState(GameState.Playing);
        }

        private void OnDisable() => StopCutscene(CurrentDirector);

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
```

Scope ini hanya cutscene real-time Level01. Script lain yang menggunakan GameManager (quiz/video/pause) belum dimigrasi. Jangan aktifkan sistem pause lama pada uji ini; penyatuan state seluruh game merupakan pekerjaan terpisah. State Playing dipulihkan untuk cinematic gameplay; jangan memakai manager ini untuk intro MainMenu sebelum kebijakan return state dirancang.

### C. Buat `Assets/_Game/Scripts/Events/BullyingEventDirector.cs`

Director ini penghubung data-event ke director Timeline yang ada di scene. Ia menyimpan kondisi kontrol sebelum cinematic, lalu mengembalikannya. Hasil signal menandai permintaan selesai; Stop dijalankan di LateUpdate agar graph tidak dihancurkan di tengah dispatch signal Timeline.

```csharp
using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;
using BullyingGame.Core;
using BullyingGame.Dialogue;
using BullyingGame.Player;

namespace BullyingGame.Events
{
    public class BullyingEventDirector : MonoBehaviour
    {
        [Header("Event dan Timeline")]
        [SerializeField] private BullyingEventData eventData;
        [SerializeField] private PlayableDirector timelineDirector;
        [SerializeField] private BullyingEventManager events;
        [SerializeField] private CutsceneManager cutscenes;
        [Header("Gameplay Lock")]
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private Behaviour[] gameplayControls = new Behaviour[0];
        [SerializeField] private GameObject promptPanel;
        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera cinematicCamera;
        [SerializeField] private int cinematicPriority = 30;

        private bool running, finishRequested;
        private bool[] controlStates;
        private PrioritySettings previousPriority;
        private CursorLockMode previousCursor;
        private bool previousCursorVisible;

        private void OnEnable()
        {
            if (events != null)
            {
                events.OnEventTriggered += Begin;
                events.OnEventStateChanged += OnResult;
            }
            if (cutscenes != null) cutscenes.OnCutsceneEnded += OnCutsceneEnded;
        }

        public bool CanStart(BullyingEventData data)
        {
            return isActiveAndEnabled && !running && data != null && data == eventData &&
                events != null && events.isActiveAndEnabled &&
                gameplayControls != null && gameplayControls.Length >= 4 &&
                cutscenes != null && cutscenes.isActiveAndEnabled &&
                cutscenes.CurrentDirector == null && movement != null &&
                cinematicCamera != null && cinematicCamera.isActiveAndEnabled &&
                timelineDirector != null && timelineDirector.isActiveAndEnabled &&
                timelineDirector.playableAsset != null &&
                timelineDirector.extrapolationMode == DirectorWrapMode.None &&
                GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Playing &&
                (DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive);
        }

        private void Begin(BullyingEventData data)
        {
            if (data != eventData) return;
            if (!CanStart(data))
            {
                Debug.LogError("Notebook cinematic belum siap; periksa wiring.", this);
                events.FailEvent();
                return;
            }
            running = true;
            finishRequested = false;
            previousCursor = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            previousPriority = cinematicCamera.Priority;
            cinematicCamera.Priority = cinematicPriority;
            movement.SetMoveInput(Vector2.zero);
            movement.SetSprintInput(false);
            controlStates = new bool[gameplayControls.Length];
            for (int i = 0; i < gameplayControls.Length; i++)
            {
                if (gameplayControls[i] == null) continue;
                controlStates[i] = gameplayControls[i].enabled;
                gameplayControls[i].enabled = false;
            }
            if (promptPanel != null) promptPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            events.StartEvent();
            if (!cutscenes.TryPlayCutscene(timelineDirector))
            {
                events.FailEvent();
                RestoreGameplay();
            }
        }

        private void OnResult(BullyingEventData data, BullyingEventState state)
        {
            if (!running || data != eventData) return;
            if (state == BullyingEventState.Resolved || state == BullyingEventState.Failed)
                finishRequested = true;
        }

        private void LateUpdate()
        {
            if (running && finishRequested && cutscenes != null &&
                cutscenes.CurrentDirector == timelineDirector)
                cutscenes.StopCutscene(timelineDirector);
        }

        private void OnCutsceneEnded()
        {
            if (!running) return;
            // Timeline habis, di-skip, atau dihentikan tanpa result: jangan hadiahkan sukses.
            if (events != null && events.CurrentEvent == eventData) events.FailEvent();
            RestoreGameplay();
        }

        private void RestoreGameplay()
        {
            if (!running) return;
            running = false;
            finishRequested = false;
            if (cinematicCamera != null) cinematicCamera.Priority = previousPriority;
            for (int i = 0; i < gameplayControls.Length; i++)
                if (gameplayControls[i] != null) gameplayControls[i].enabled = controlStates[i];
            // Target lama mungkin sudah hilang; biarkan detector memilih target baru.
            if (promptPanel != null) promptPanel.SetActive(false);
            Cursor.lockState = previousCursor;
            Cursor.visible = previousCursorVisible;
        }

        private void OnDisable()
        {
            if (running)
            {
                if (events != null && events.CurrentEvent == eventData) events.FailEvent();
                if (cutscenes != null) cutscenes.StopCutscene(timelineDirector);
                RestoreGameplay();
            }
            if (events != null)
            {
                events.OnEventTriggered -= Begin;
                events.OnEventStateChanged -= OnResult;
            }
            if (cutscenes != null) cutscenes.OnCutsceneEnded -= OnCutsceneEnded;
        }
    }
}
```

Array Gameplay Controls wajib diisi tepat sesuai bagian 5. Jangan masukkan GameObject/director ini atau CutsceneManager ke array, karena mereka harus tetap berjalan untuk cleanup. PrioritySettings disimpan sebagai struct agar nilai priority dan flag Enabled sama-sama pulih. Prompt dihitung ulang berdasarkan target setelah cinematic.

### D. Ganti isi `Assets/_Game/Scripts/Quest/QuestItem.cs`

Pickup menahan objective sampai hasil sukses, menyimpan subscription pada component yang tetap hidup, dan tidak menghancurkan buku sebelum hasil diketahui. VisualRoot boleh kosong: untuk scene saat ini renderer root otomatis disembunyikan. Collider tetap ada, tetapi CanInteract mencegah pengambilan selama pending.

```csharp
using UnityEngine;
using BullyingGame.Core;
using BullyingGame.Dialogue;
using BullyingGame.Interaction;
using BullyingGame.Events;

namespace BullyingGame.Quest
{
    public class QuestItem : MonoBehaviour, IInteractable
    {
        [Header("Quest")]
        [SerializeField] private QuestData quest;
        [SerializeField] private string objectiveId = "find_notebook";
        [SerializeField] private string promptText = "[E] Ambil Buku Catatan";
        [SerializeField] private bool destroyOnCollect = true;
        [SerializeField] private GameObject visualRoot;
        [Header("Bullying Event")]
        [SerializeField] private BullyingEventData eventToTrigger;
        [SerializeField] private BullyingEventDirector eventDirector;

        private bool pending, collected;
        private BullyingEventManager subscribedManager;
        private Renderer[] renderers;
        private bool[] rendererStates;

        private void Awake()
        {
            // Jangan mematikan root; script ini perlu tetap menerima hasil event.
            var root = visualRoot != null ? visualRoot : gameObject;
            renderers = root.GetComponentsInChildren<Renderer>(true);
            rendererStates = new bool[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                rendererStates[i] = renderers[i].enabled;
        }

        public string GetPromptText() => promptText;

        public bool CanInteract()
        {
            if (pending || collected) return false;
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
                return false;
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState != GameState.Playing) return false;
            return quest == null || (QuestManager.Instance != null &&
                QuestManager.Instance.GetQuestState(quest) == QuestState.Active);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract()) return;
            if (eventToTrigger == null)
            {
                SecureItem();
                return;
            }
            var manager = BullyingEventManager.Instance;
            if (manager == null) return;
            if (manager.HasHandled(eventToTrigger))
            {
                SecureItem();
                return;
            }
            if (eventDirector == null || !eventDirector.CanStart(eventToTrigger))
            {
                Debug.LogError("Isi Event Director dan wiring cinematic notebook.", this);
                return;
            }
            pending = true;
            SetVisible(false);
            subscribedManager = manager;
            manager.OnEventStateChanged += OnEventResult;
            if (!manager.TryTriggerEvent(eventToTrigger))
            {
                Unsubscribe();
                pending = false;
                SetVisible(true);
            }
        }

        private void OnEventResult(BullyingEventData data, BullyingEventState state)
        {
            if (!pending || data != eventToTrigger) return;
            if (state != BullyingEventState.Resolved && state != BullyingEventState.Failed)
                return;
            Unsubscribe();
            pending = false;
            if (state == BullyingEventState.Resolved) SecureItem();
            else SetVisible(true);
        }

        private void SecureItem()
        {
            if (collected) return;
            collected = true;
            if (quest != null && QuestManager.Instance != null)
                QuestManager.Instance.UpdateObjective(quest, objectiveId, 1);
            SetVisible(false);
            if (destroyOnCollect) Destroy(gameObject);
        }

        private void SetVisible(bool visible)
        {
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].enabled = visible && rendererStates[i];
        }

        private void Unsubscribe()
        {
            if (subscribedManager != null)
                subscribedManager.OnEventStateChanged -= OnEventResult;
            subscribedManager = null;
        }

        private void OnDisable()
        {
            Unsubscribe();
            if (pending)
            {
                pending = false;
                SetVisible(true);
            }
        }
    }
}
```

### E. Ubah dua method hasil di `TimelineSignalHandler.cs`

Pertahankan field dan method lain. Untuk signal hasil Level01, Event To Trigger berfungsi sebagai filter identity. Signal dari cinematic lain tidak boleh menyelesaikan event aktif yang berbeda.

```csharp
public void OnResolveEvent()
{
    var manager = BullyingEventManager.Instance;
    if (eventToTrigger != null && manager != null && manager.CurrentEvent == eventToTrigger)
        manager.ResolveEvent();
}

public void OnFailEvent()
{
    var manager = BullyingEventManager.Instance;
    if (eventToTrigger != null && manager != null && manager.CurrentEvent == eventToTrigger)
        manager.FailEvent();
}
```

Jangan menghubungkan `OnTriggerBullyingEvent` ke Timeline encounter ini: notebook sudah memicu event. Memicu event dari Timeline yang dimulai event itu sendiri menciptakan alur melingkar.

### F. Batasi interaksi dan prompt saat cinematic

Di `PlayerInteraction.cs`, tambahkan di awal `OnInteractPressed()`:

```csharp
if (BullyingGame.Core.GameStateManager.Instance != null &&
    BullyingGame.Core.GameStateManager.Instance.CurrentState != BullyingGame.Core.GameState.Playing)
    return;
if (BullyingGame.Dialogue.DialogueManager.Instance != null &&
    BullyingGame.Dialogue.DialogueManager.Instance.IsDialogueActive) return;
```

Ini juga mencegah tombol lanjut dialog menginteraksi NPC/buku lagi.

Di `InteractionPromptUI.cs`, tambahkan di awal `LateUpdate()`:

```csharp
if (BullyingGame.Core.GameStateManager.Instance != null &&
    BullyingGame.Core.GameStateManager.Instance.CurrentState != BullyingGame.Core.GameState.Playing)
{
    if (promptPanel != null) promptPanel.SetActive(false);
    return;
}
if (detector != null)
{
    var target = detector.CurrentInteractable;
    UpdatePrompt(target != null && target.CanInteract() ? target : null);
}
```

Memilih ulang prompt setiap frame juga memperbaiki kasus target yang sama: setelah cinematic detector mungkin tidak mengirim OnInteractableChanged lagi.

## 5. Pasang di Unity secara manual

### A. Pastikan state manager dan baseline quest

1. Cek Bootstrap tidak memiliki Missing Script. Jika ada, perbaiki fondasi startup terlebih dahulu; nama object saja tidak membuktikan component terpasang.
2. Untuk uji lokal langsung pada Level01, buat `[GameSystem]/GameStateManager` dengan component GameStateManager **hanya jika tidak menjalankan Bootstrap**. Untuk build normal gunakan satu manager persistent Bootstrap dan hapus object uji ini sebelum menjalankan alur scene penuh.
3. Untuk uji langsung, buat script kecil sementara `Assets/_Game/Scripts/Events/Level01PlaytestState.cs`:

```csharp
using UnityEngine;
using BullyingGame.Core;
namespace BullyingGame.Events
{
    public class Level01PlaytestState : MonoBehaviour
    {
        private void Start()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetState(GameState.Playing);
        }
    }
}
```

Pasang pada manager uji lokal. Ini hanya harness uji, hapus component/script setelah startup normal terbukti memasuki Playing. Jangan mengubah default GameStateManager dari Boot menjadi Playing secara global.

4. Play: bicara dengan Guru BK, quest muncul 0/1, gerakan dan dialog selesai dengan normal. Stop sebelum wiring berikutnya. Kalau baseline ini gagal, tangani dulu.

### B. Buat asset event

1. Project → `Assets/_Game/Data/Events` → Create → Game → Bullying Event.
2. Nama `Event_Level01_Notebook`.
3. Event Id `level01_notebook_encounter`; Event Name `Pertemuan Setelah Mengambil Buku`; Bullying Type `Verbal`; Related Quest `Quest_Level01_LostNotebook`.
4. Field witness/confront/ignore dialogue kosong untuk pengujian Step 16. Data itu belum otomatis dimainkan oleh director.

### C. Buat object sistem dan kamera

Tambahkan hierarchy berikut (nama object lama tetap):

```text
[GameSystem]
  CutsceneManager                 + CutsceneManager
  NotebookEventDirector           + BullyingEventDirector
  NotebookCinematic               + PlayableDirector
                                  + SignalReceiver
                                  + TimelineSignalHandler
Cameras
  NotebookCinematicCamera         + CinemachineCamera
```

1. Kamera baru aktif, Priority 0; atur posisi/rotasi Scene View sehingga notebook/player terlihat jelas. Gameplay camera aktif dengan priority lebih tinggi dari 0, contoh 10. DialogueCamera existing memakai 20 saat dialog; cinematic director menggunakan 30 saat encounter.
2. Gunakan CinemachineCamera; jangan menonaktifkan atau mengubah transform Main Camera yang mempunyai CinemachineBrain.
3. Pada NotebookEventDirector drag event asset, PlayableDirector NotebookCinematic, BullyingEventManager existing, CutsceneManager baru, PlayerMovement `/Player`, CinematicCamera baru.
4. Gameplay Controls ukuran 4, drag component: PlayerInputHandler `/Player`; PlayerMovement `/Player`; PlayerInteraction `/Player`; CinemachineInputAxisController `/Cameras/CinemachineCamera`.
5. Prompt Panel drag `[UI]/InteractionCanvas/PromptPanel`.
6. Jangan masukkan CharacterController atau Rigidbody ke array. CharacterController tetap dipakai untuk movement setelah gameplay dipulihkan.
7. Pada NotebookItem, isi Event To Trigger = asset event, Event Director = NotebookEventDirector. Quest dan objective tetap yang existing; Destroy On Collect boleh aktif karena destruction kini hanya saat secure. Visual Root kosong tetap bekerja melalui renderer root.
8. Pada TimelineSignalHandler di NotebookCinematic, isi Event To Trigger = asset yang sama. Field Dialogue To Play dan QTE To Trigger kosong.

Pastikan references diisi saat Edit Mode, lalu simpan. Subscription director terjadi OnEnable: kalau kamu mengisi references saat component sudah aktif di Play Mode, stop/play kembali supaya subscription dibuat.

### D. Buat Timeline sukses

1. Pilih NotebookCinematic → Window → Sequencing → Timeline → Create.
2. Simpan `Assets/_Game/Art/Cutscenes/TL_Level01_Notebook_Success.playable`.
3. PlayableDirector: Play On Awake **off**, Wrap Mode **None**, Initial Time 0, Update Method Game Time.
4. Timeline Inspector: Fixed Length sekitar 4 detik. Bila menggunakan duration berdasarkan clips, pastikan clip non-looping membuat duration minimal 4 detik. Jangan membuat Timeline berdurasi 0.
5. Untuk Step 16 cukup shot kamera statis dan signal. Gerakan bully/animasi akan dikerjakan pada step berikutnya.
6. Add Signal Track. Drag NotebookCinematic yang memiliki SignalReceiver ke binding track tersebut.
7. Tambah Signal Emitter pada detik **3.0**, sebelum akhir Timeline.
8. Create Signal, simpan `Assets/_Game/Data/Events/SIG_Notebook_Success.signal`.
9. Emit Once **on**, Retroactive **off**.
10. Di SignalReceiver, buat reaction untuk signal itu; tombol `+`, drag NotebookCinematic, pilih `TimelineSignalHandler → OnResolveEvent()`; gunakan Runtime Only.
11. Pastikan binding receiver benar dan reaction hanya satu. Simpan scene dan asset.

Emit Once membatasi pengiriman saat loop; aturan sekali per notebook tetap ditangani manager, bukan checkbox tersebut. Retroactive mengatur pengiriman ketika playback mulai melewati waktu marker. Lihat [SignalEmitter Unity](https://docs.unity.cn/Packages/com.unity.timeline%401.0/api/UnityEngine.Timeline.SignalEmitter.html). Semantik juga tersedia di source package lokal `Library/PackageCache/com.unity.timeline@c56d56b0070b/Runtime/Events/Signals/SignalEmitter.cs`.

### E. Buat Timeline gagal untuk pengujian terpisah

1. Duplicate Timeline sukses menjadi `TL_Level01_Notebook_Failure.playable` melalui Project window.
2. Ganti satu emitter hasil menjadi `SIG_Notebook_Failure.signal`; jangan menyimpan signal sukses dan gagal di Timeline yang sama.
3. Tambahkan reaction Failure pada SignalReceiver → `OnFailEvent()`.
4. Untuk menjalankan uji gagal, stop Play Mode, ganti Playable Asset pada PlayableDirector ke Timeline Failure; uji dalam sesi Play yang baru.
5. Setelah dua cabang lulus, pilih kembali Timeline Success sebagai konfigurasi demo/default. Timeline ini adalah simulasi hasil deterministik, bukan QTE.

CutsceneManager memakai callback `PlayableDirector.stopped` untuk cleanup ketika playback dihentikan. Lihat [dokumentasi Unity](https://docs.unity.com/en-us/engine/6000.7/script-reference/unityengine/playables/playabledirector/stopped).

## 6. Compile dan uji, urutannya

CLI Editor ini menyediakan `console`, bukan `get_console_logs` yang tertulis dalam skill lama. Jalankan dari root project:

```powershell
unity status
unity cmd recompile
unity cmd recompile_status
unity cmd console --level error --tail 30
unity cmd console_status
```

Ulangi recompile_status hanya sampai selesai/up_to_date; jangan memasuki Play ketika sedang compile. Console error harus 0. Untuk verifikasi manual kamu juga boleh memakai Console window dan tombol Play Unity. Codex belum mengompilasi candidate code dalam panduan ini karena script runtime belum diaplikasikan.

| Uji | Cara | Hasil wajib |
|---|---|---|
| Sebelum quest | Dekati/tekan Interact pada buku | Tidak bisa collect; event tidak berjalan |
| Baseline dialog | Bicara Guru BK, lanjutkan sampai selesai | Quest Active 0/1; tidak membuka ulang dialog karena tombol continue |
| Sukses | Ambil buku dengan Timeline Success | Buku tersembunyi, Cinematic, movement/sprint/look/interact terkunci; signal sukses membuat objective tepat +1; buku hilang; camera dan kontrol pulih |
| Quest sesudah sukses | Bicara Guru BK lagi | Dialog apresiasi; tidak ada objective baru yang tidak diminta |
| Gagal | Sesi baru, gunakan Timeline Failure | Quest tetap Active 0/1; buku terlihat di tempat semula; gameplay pulih |
| Ambil ulang | Setelah gagal, ambil buku | Langsung secure +1 dan Completed; cinematic tidak diulang |
| Signal hilang | Sesi baru, hapus/mute marker hasil | Saat duration selesai: fallback Failed, buku kembali, input/kamera pulih |
| Signal duplikat | Dua marker success berurutan | Progress tidak dua kali; completion tidak dua kali |
| Skip/stop | Hentikan PlayableDirector saat cinematic melalui Inspector debug/command | Tanpa result sebelumnya: Failed; tidak mengeksekusi signal akhir; input/kamera pulih |
| Setup rusak | Kosongkan Playable Asset sebelum Play | Pickup menolak, objective tidak bertambah, buku tetap terlihat, log jelas |
| Director disabled | Nonaktifkan component BullyingEventDirector saat cinematic | Result Failed jika belum ada result, Timeline stop, kontrol dipulihkan |
| Repeat Play | Stop lalu Play lagi | Tidak ada callback dari sesi lama; encounter kembali bisa diuji pada sesi baru |

Untuk skip manual lewat CLI dalam Play Mode (bukan tombol keyboard hardcoded):

```powershell
unity cmd eval --code 'BullyingGame.Core.CutsceneManager.Instance.SkipCutscene(); return "skip requested";'
```

Jangan memakai Stop Play Mode sebagai bukti gameplay cleanup: exiting Play mengembalikan scene, bukan membuktikan kode mengembalikan input dalam sesi yang sama.

## 7. Jika ada masalah

| Gejala | Periksa |
|---|---|
| Klik buku tidak memulai event | Quest Active; State Playing; asset event dan director sama; semua CanStart references; Wrap None |
| State tetap Boot saat langsung Play Level01 | Harness uji lokal bagian 5A atau startup Bootstrap belum bekerja |
| Cinematic bermain otomatis | Play On Awake masih aktif |
| Kamera tidak pindah | Cinematic camera aktif; priority 30; gameplay 10; CinemachineBrain tetap aktif; framing shot benar |
| Timeline selesai tetapi buku kembali | Signal result tidak terkirim; binding track, SignalAsset, receiver reaction, Runtime Only, waktu marker sebelum akhir |
| Player bergerak saat cutscene | Array lock belum berisi PlayerMovement; reset Move/Sprint harus sebelum disabling |
| Camera orbit masih bergerak | CinemachineInputAxisController belum masuk array lock |
| Buku tidak muncul setelah gagal | Root buku dimatikan/dihancurkan oleh script/track lain; pickup harus tetap aktif sampai result |
| Prompt tidak kembali | Tambahan LateUpdate belum dipasang; detector harus tetap enabled |
| QTE error Input.GetKey | QTE lama tidak termasuk fondasi Step 16; jangan bind OnStartQTE pada uji ini |

## 8. Catatan progres setelah kamu menguji

Isi berdasarkan hasil nyata, jangan langsung menandai semuanya selesai:

```text
Step 16 integrasi notebook dan Timeline:
- Script diterapkan: [ ]
- Compile 0 error / warning terkait perubahan: [ ]
- Baseline quest/dialogue: [ ]
- Success cinematic: [ ]
- Failure dan recollect tanpa replay: [ ]
- Missing signal / skip / cleanup: [ ]
- Kamera, input, cursor, prompt pulih: [ ]
- Guru BK completion dialogue: [ ]

Belum termasuk: bully group navigation, confrontation dialogue di Timeline,
QTE berbasis Input System, item-state penuh, random rehide, save/load.
Step 17 baru dimulai setelah fondasi Step 16 di atas terverifikasi.
```

Untuk pengembangan selanjutnya, integrasi dialogue di Timeline harus pause/resume Timeline dan memperbaiki DialogueInputHandler agar akhir dialog tidak membuka movement saat cinematic masih aktif. QTE harus mempunyai action map sendiri dan hasilnya dikirim ke ResolveEvent/FailEvent. Jangan menghubungkan kedua sistem itu langsung tanpa ownership input dan lifecycle pause/resume.
