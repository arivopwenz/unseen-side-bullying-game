using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using BullyingGame.Core;
using BullyingGame.Dialogue;

namespace BullyingGame.Cinematics
{
    [Serializable]
    public struct OpeningSpeakerFocus
    {
        [SerializeField] private string speaker;
        [SerializeField] private Transform actor;
        [SerializeField] private Unity.Cinemachine.CinemachineCamera shot;
        [SerializeField] private float timelineTime;
        public string Speaker => speaker;
        public Transform Actor => actor;
        public Unity.Cinemachine.CinemachineCamera Shot => shot;
        public float TimelineTime => timelineTime;
    }
    public class StoryOpeningDirector : MonoBehaviour
    {
        [Header("Timeline")]
        [SerializeField] private PlayableDirector timeline;
        [SerializeField] private CutsceneManager cutscenes;
        [Header("Skip Summary")]
        [SerializeField] private DialogueData skipSummary;
        [Header("Gameplay Controls")]
        [SerializeField] private Behaviour[] controls;
        [Header("Opening Blocking")]
        [SerializeField] private Transform[] actors;
        [SerializeField] private Transform[] stagePoints;
        [SerializeField] private Transform player;
        [Header("Dialogue Framing")]
        [SerializeField] private Unity.Cinemachine.CinemachineCamera dialogueShot;
        [SerializeField] private OpeningSpeakerFocus[] speakers;
        public event Action OnFinished;
        public bool IsRunning { get; private set; }
        public bool IsShowingSkipSummary { get; private set; }
        private DialogueData dialogue;
        private DialogueManager dialogueManager;
        private bool[] enabledStates;
        private Vector3[] positions;
        private Quaternion[] rotations;
        private Transform[] activeActors;
        private CursorLockMode cursor;
        private bool visible;
        private double dialogueTime;
        private DialogueData activeSummary;
        private DialogueManager summaryManager;
        private bool summaryPending;

        public bool TryPlay(DialogueData openingDialogue)
        {
            if (IsRunning || timeline == null || timeline.playableAsset == null || cutscenes == null ||
                cutscenes.CurrentDirector != null || controls == null || actors == null || stagePoints == null ||
                actors.Length != stagePoints.Length) return false;
            dialogue = openingDialogue;
            cursor = Cursor.lockState; visible = Cursor.visible;
            enabledStates = new bool[controls.Length];
            for (int i = 0; i < controls.Length; i++)
                if (controls[i] != null)
                {
                    enabledStates[i] = controls[i].enabled;
                    if (controls[i] is BullyingGame.Player.PlayerMovement movement) { movement.SetMoveInput(Vector2.zero); movement.SetSprintInput(false); }
                    controls[i].enabled = false;
                }
            activeActors = (Transform[])actors.Clone();
            var sequence=UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
            if(player != null && sequence != null && speakers != null)
                foreach(var speaker in speakers) if(speaker.Speaker == sequence.CurrentChapter.PlayerName)
                    for(int i=0;i<activeActors.Length;i++) if(activeActors[i] == speaker.Actor) activeActors[i]=player;
            positions = new Vector3[actors.Length]; rotations = new Quaternion[actors.Length];
            for (int i = 0; i < actors.Length; i++)
            {
                if (activeActors[i] == null) continue;
                positions[i] = activeActors[i].position; rotations[i] = activeActors[i].rotation;
                if (stagePoints[i] != null) Place(activeActors[i], stagePoints[i].position, stagePoints[i].rotation);
            }
            IsRunning = true;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            cutscenes.OnCutsceneEnded += Finished;
            if (cutscenes.TryPlayCutscene(timeline)) return true;
            Restore(); return false;
        }
        private static void Place(Transform actor, Vector3 position, Quaternion rotation)
        {
            var agent = actor.GetComponent<NavMeshAgent>();
            var controller = actor.GetComponent<CharacterController>();
            bool controllerEnabled = controller != null && controller.enabled;
            if (controllerEnabled) controller.enabled = false;
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) agent.Warp(position);
            else actor.position = position;
            actor.rotation = rotation;
            if (controllerEnabled) controller.enabled = true;
        }
        public void StartOpeningDialogue()
        {
            if (!IsRunning || dialogueManager != null || dialogue == null || DialogueManager.Instance == null) return;
            dialogueManager = DialogueManager.Instance;
            dialogueTime=timeline.time;
            cutscenes.PauseCutscene(timeline);
            dialogueManager.OnDialogueEnded += DialogueFinished;
            dialogueManager.OnLineDisplayed += FrameLine;
            if (!dialogueManager.TryStartDialogue(dialogue)) DialogueFinished();
        }
        private void DialogueFinished()
        {
            if (dialogueManager != null) { dialogueManager.OnDialogueEnded -= DialogueFinished; dialogueManager.OnLineDisplayed -= FrameLine; }
            dialogueManager = null;
            if (IsRunning) { timeline.time=Math.Max(dialogueTime, timeline.duration-1.4); timeline.Evaluate(); cutscenes.ResumeCutscene(timeline); }
        }
        private void FrameLine(DialogueLine line)
        {
            if(!IsRunning || timeline==null || line==null) return;
            Transform focus=null;
            Unity.Cinemachine.CinemachineCamera authoredShot = null;
            float shotTime = 1.2f;
            if(speakers!=null) foreach(var speaker in speakers) if(speaker.Speaker==line.speakerName)
                { focus=speaker.Actor; authoredShot=speaker.Shot; shotTime=speaker.TimelineTime; break; }
            var sequence=UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
            if(player!=null && sequence!=null && line.speakerName==sequence.CurrentChapter.PlayerName) focus=player;
            if(authoredShot != null) timeline.time = shotTime;
            else if(focus!=null && dialogueShot!=null)
            {
                var position=focus.position+focus.forward*2.8f+focus.right*.7f+Vector3.up*1.65f;
                dialogueShot.transform.SetPositionAndRotation(position,Quaternion.LookRotation(focus.position+Vector3.up*1.3f-position));
                timeline.time=4.5;
            }
            else timeline.time=1.2;
            timeline.Evaluate();
        }
        public void Skip()
        {
            if (!IsRunning || IsShowingSkipSummary || cutscenes == null ||
                GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Paused) return;
            cutscenes.PauseCutscene(timeline);
            // Stop the original conversation without completing either the opening or a mission.
            if (dialogueManager != null)
            {
                var manager = dialogueManager;
                manager.OnDialogueEnded -= DialogueFinished;
                manager.OnLineDisplayed -= FrameLine;
                dialogueManager = null;
                if (manager.CurrentDialogue == dialogue) manager.EndDialogue();
            }
            activeSummary = skipSummary != null ? skipSummary : dialogue;
            if (activeSummary == null || activeSummary.lines == null || activeSummary.lines.Length == 0)
            {
                // An invalid authoring reference must not leave controls permanently locked.
                cutscenes.StopCutscene(timeline);
                return;
            }
            IsShowingSkipSummary = true;
            summaryPending = true;
            TryShowSummary();
        }
        private void Update()
        {
            if (summaryPending && IsRunning && GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState != GameState.Paused) TryShowSummary();
        }
        private void TryShowSummary()
        {
            var manager = DialogueManager.Instance;
            if (!summaryPending || manager == null || manager.IsDialogueActive) return;
            summaryManager = manager;
            manager.OnDialogueEnded += SummaryFinished;
            if (manager.TryStartDialogue(activeSummary)) summaryPending = false;
            else
            {
                manager.OnDialogueEnded -= SummaryFinished;
                summaryManager = null;
            }
        }
        private void SummaryFinished()
        {
            bool completed = summaryManager != null && summaryManager.LastDialogueCompleted;
            if (summaryManager != null) summaryManager.OnDialogueEnded -= SummaryFinished;
            summaryManager = null;
            if (!IsRunning) return;
            if (!completed)
            {
                // Cancellation does not acknowledge the learning message. It can be read again.
                summaryPending = true;
                return;
            }
            summaryPending = false;
            cutscenes.StopCutscene(timeline);
        }
        private void Finished()
        {
            if (!IsRunning) return;
            Restore();
            OnFinished?.Invoke();
        }
        private void Restore()
        {
            if (!IsRunning) return;
            IsRunning = false;
            IsShowingSkipSummary = false;
            summaryPending = false;
            if (summaryManager != null)
            {
                var manager = summaryManager;
                manager.OnDialogueEnded -= SummaryFinished;
                summaryManager = null;
                if (manager.CurrentDialogue == activeSummary) manager.EndDialogue();
            }
            activeSummary = null;
            if (cutscenes != null) cutscenes.OnCutsceneEnded -= Finished;
            if (dialogueManager != null)
            {
                var manager = dialogueManager;
                manager.OnDialogueEnded -= DialogueFinished;
                manager.OnLineDisplayed -= FrameLine;
                dialogueManager = null;
                if (manager.CurrentDialogue == dialogue) manager.EndDialogue();
            }
            for (int i = 0; i < activeActors.Length; i++) if (activeActors[i] != null) Place(activeActors[i], positions[i], rotations[i]);
            for (int i = 0; i < controls.Length; i++) if (controls[i] != null) controls[i].enabled = enabledStates[i];
            Cursor.lockState = cursor; Cursor.visible = visible;
        }
        private void OnDisable()
        {
            if (IsRunning && cutscenes != null) cutscenes.StopCutscene(timeline);
            Restore();
        }
    }
}
