using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;
using BullyingGame.Core;
using BullyingGame.Dialogue;
using BullyingGame.Player;
using BullyingGame.Interaction;
using BullyingGame.QTE;
using BullyingGame.NPC;

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
        [Header("Bully Group")]
        [SerializeField] private BullyGroupController bullyGroup;

        private bool running, finishRequested;
        private bool[] controlStates;
        private PrioritySettings previousPriority;
        private CursorLockMode previousCursor;
        private bool previousCursorVisible;
        private bool registered;
        private DialogueManager activeDialogue;
        private QTEManager activeQTE;
        private BullyGroupController activeGroup;
        private bool waitingForGroup;

        public bool IsRunning => running;

        private void OnEnable()
        {
            if (events != null && events.RegisterDirector(eventData, this))
            {
                registered = true;
                events.OnEventTriggered += Begin;
                events.OnEventStateChanged += OnResult;
            }
            if (cutscenes != null) cutscenes.OnCutsceneEnded += OnCutsceneEnded;
        }

        public bool CanStart(BullyingEventData data)
        {
            return registered && isActiveAndEnabled && !running && data != null && data == eventData &&
                events != null && events.isActiveAndEnabled &&
                HasRequiredControls() &&
                cutscenes != null && cutscenes.isActiveAndEnabled &&
                cutscenes.CurrentDirector == null && movement != null &&
                cinematicCamera != null && cinematicCamera.isActiveAndEnabled &&
                timelineDirector != null && timelineDirector.isActiveAndEnabled &&
                timelineDirector.playableAsset != null &&
                timelineDirector.duration > 0 && !double.IsInfinity(timelineDirector.duration) &&
                timelineDirector.extrapolationMode == DirectorWrapMode.None &&
                GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Playing &&
                (DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive);
        }

        private bool HasRequiredControls()
        {
            if (gameplayControls == null) return false;
            bool hasMovement = false, hasInput = false, hasInteraction = false, hasCameraInput = false;
            foreach (var control in gameplayControls)
            {
                if (control == null || control == this || control == cutscenes) return false;
                hasMovement |= control == movement;
                hasInput |= control is PlayerInputHandler;
                hasInteraction |= control is PlayerInteraction;
                hasCameraInput |= control is CinemachineInputAxisController;
            }
            return hasMovement && hasInput && hasInteraction && hasCameraInput;
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

        public bool PlayDialogue(DialogueData data)
        {
            var dialogue = DialogueManager.Instance;
            if (!running || finishRequested || waitingForGroup || activeDialogue != null || activeQTE != null ||
                dialogue == null || dialogue.IsDialogueActive || data == null ||
                data.lines == null || data.lines.Length == 0) return false;
            activeDialogue = dialogue;
            dialogue.OnDialogueEnded += OnDialogueEnded;
            cutscenes.PauseCutscene(timelineDirector);
            dialogue.StartDialogue(data);
            return true;
        }

        private void OnDialogueEnded()
        {
            if (activeDialogue == null) return;
            activeDialogue.OnDialogueEnded -= OnDialogueEnded;
            activeDialogue = null;
            if (running && !finishRequested) cutscenes.ResumeCutscene(timelineDirector);
        }

        public bool PlayQTE(QTEData data)
        {
            var qte = QTEManager.Instance;
            if (!running || finishRequested || waitingForGroup || activeDialogue != null || activeQTE != null ||
                qte == null || qte.IsQTEActive) return false;
            activeQTE = qte;
            qte.OnQTEEnded += OnQTEEnded;
            cutscenes.PauseCutscene(timelineDirector);
            events.WaitForResponse();
            GameStateManager.Instance.SetState(GameState.QTE);
            if (qte.TryStartQTE(data)) return true;
            OnQTEEnded(QTEResult.Failed);
            Debug.LogWarning("QTE could not start; event failed safely. Check input and data.", this);
            return false;
        }

        public bool PlayBullyApproach()
        {
            if (!running || finishRequested || activeDialogue != null || activeQTE != null || activeGroup != null)
                return false;
            if (bullyGroup == null)
            {
                Debug.LogWarning("Assign Bully Group pada Notebook Event Director sebelum memakai signal approach.", this);
                events.FailEvent();
                return false;
            }
            activeGroup = bullyGroup;
            waitingForGroup = true;
            activeGroup.OnApproachCompleted += OnBullyApproachCompleted;
            cutscenes.PauseCutscene(timelineDirector);
            if (activeGroup.TryStartApproach()) return true;
            if (waitingForGroup) OnBullyApproachCompleted(false);
            return false;
        }

        private void OnBullyApproachCompleted(bool success)
        {
            if (!waitingForGroup) return;
            waitingForGroup = false;
            if (activeGroup != null) activeGroup.OnApproachCompleted -= OnBullyApproachCompleted;
            if (!running || finishRequested) return;
            if (success) cutscenes.ResumeCutscene(timelineDirector);
            else
            {
                Debug.LogWarning($"Bully approach gagal: {activeGroup?.LastFailure}. Gameplay akan dipulihkan.", this);
                if (events != null && events.CurrentEvent == eventData) events.FailEvent();
            }
        }

        private void OnQTEEnded(QTEResult result)
        {
            if (activeQTE != null) activeQTE.OnQTEEnded -= OnQTEEnded;
            activeQTE = null;
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.QTE)
                GameStateManager.Instance.SetState(GameState.Cinematic);
            if (events == null || events.CurrentEvent != eventData) return;
            if (result == QTEResult.Success) events.ResolveEvent();
            else events.FailEvent();
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
            waitingForGroup = false;
            if (activeGroup != null)
            {
                activeGroup.OnApproachCompleted -= OnBullyApproachCompleted;
                activeGroup.EndEncounter();
                activeGroup = null;
            }
            if (activeDialogue != null)
            {
                activeDialogue.OnDialogueEnded -= OnDialogueEnded;
                activeDialogue.EndDialogue();
                activeDialogue = null;
            }
            if (activeQTE != null)
            {
                activeQTE.OnQTEEnded -= OnQTEEnded;
                activeQTE.CancelQTE();
                activeQTE = null;
            }
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.QTE)
                GameStateManager.Instance.SetState(GameState.Cinematic);
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
                events.UnregisterDirector(eventData, this);
            }
            registered = false;
            if (cutscenes != null) cutscenes.OnCutsceneEnded -= OnCutsceneEnded;
        }
    }
}
