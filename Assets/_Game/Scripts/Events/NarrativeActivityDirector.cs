using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using TMPro;
using Unity.Cinemachine;
using BullyingGame.Core;
using BullyingGame.Dialogue;
using BullyingGame.QTE;

namespace BullyingGame.Events
{
    public class NarrativeActivityDirector : MonoBehaviour
    {
        [Header("Learning UI")]
        [SerializeField] private GameObject mathPanel;
        [SerializeField] private TMP_Text instruction;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TMP_Text[] answerLabels;
        [Header("Cinematic Framing")]
        [SerializeField] private CinemachineCamera activityCamera;
        [SerializeField] private Transform player;
        [SerializeField] private Transform[] gang;
        [SerializeField] private Transform confrontationStage;
        [SerializeField] private Transform arithmeticStage;
        [SerializeField] private Transform arithmeticCameraAnchor;
        [SerializeField] private Transform[] arithmeticGangPoints;
        [SerializeField] private GameObject playerFood;
        [SerializeField] private GameObject takenFood;
        [SerializeField] private Behaviour[] controls;
        private NarrativeActivityData activity;
        private Action<bool> finished;
        private DialogueManager dialogue;
        private QTEManager qte;
        private bool[] controlStates;
        private CursorLockMode cursor;
        private bool cursorVisible;
        private PrioritySettings cameraPriority;
        private Vector3[] actorPositions;
        private Quaternion[] actorRotations;
        private bool awaitingAftermath, awaitingConfrontation, cancelling;
        public bool IsRunning => activity != null;
        public bool IsMathAwaitingAnswer => IsRunning && activity.Kind == NarrativeActivityKind.Arithmetic && mathPanel.activeSelf;
        public bool IsPressureActive => IsRunning && activity.Kind == NarrativeActivityKind.Coercion;
        private void Start()
        {
            mathPanel.SetActive(false);
            for(int i=0;i<answerButtons.Length;i++) { int index=i; answerButtons[i].onClick.AddListener(() => SubmitAnswer(index)); }
        }
        public bool TryStart(NarrativeActivityData data, Action<bool> callback)
        {
            if(IsRunning || data == null || GameStateManager.Instance == null || GameStateManager.Instance.CurrentState != GameState.Playing ||
                DialogueManager.Instance == null || DialogueManager.Instance.IsDialogueActive || QTEManager.Instance == null || QTEManager.Instance.IsQTEActive) return false;
            if(data.Kind == NarrativeActivityKind.Arithmetic && (data.Answers == null || data.Answers.Length != answerButtons.Length || data.CorrectAnswer < 0 || data.CorrectAnswer >= data.Answers.Length)) return false;
            activity=data; finished=callback; dialogue=DialogueManager.Instance; qte=QTEManager.Instance;
            cursor=Cursor.lockState; cursorVisible=Cursor.visible;
            controlStates=new bool[controls.Length];
            for(int i=0;i<controls.Length;i++) if(controls[i] != null) { controlStates[i]=controls[i].enabled; if(controls[i] is BullyingGame.Player.PlayerMovement movement) { movement.SetMoveInput(Vector2.zero);movement.SetSprintInput(false); } controls[i].enabled=false; }
            if(activityCamera != null) { cameraPriority=activityCamera.Priority; activityCamera.Priority=35; }
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            if(data.Kind == NarrativeActivityKind.Arithmetic)
            {
                if (arithmeticStage != null) TeleportPlayer(arithmeticStage);
                StageGang();
                Frame(false);
                GameStateManager.Instance.SetState(GameState.Quiz);
                instruction.text=data.Instruction;
                for(int i=0;i<answerButtons.Length;i++) { answerLabels[i].text=data.Answers[i];answerButtons[i].interactable=true; }
                mathPanel.SetActive(true);
                EventSystem.current?.SetSelectedGameObject(answerButtons[0].gameObject);
                return true;
            }
            if(confrontationStage != null)
            {
                TeleportPlayer(confrontationStage);
            }
            if(playerFood != null) playerFood.SetActive(true);
            StageGang(); Frame(true);
            if (data.ConfrontationDialogue != null)
            {
                GameStateManager.Instance.SetState(GameState.Cinematic);
                awaitingConfrontation = true;
                dialogue.OnDialogueEnded += ConfrontationFinished;
                if (dialogue.TryStartDialogue(data.ConfrontationDialogue)) return true;
                Complete(false); return false;
            }
            return StartEffort();
        }
        private void TeleportPlayer(Transform point)
        {
            var controller = player.GetComponent<CharacterController>();
            bool enabled = controller != null && controller.enabled;
            if (enabled) controller.enabled = false;
            player.SetPositionAndRotation(point.position, point.rotation);
            if (enabled) controller.enabled = true;
            Physics.SyncTransforms();
        }
        private void ConfrontationFinished()
        {
            dialogue.OnDialogueEnded -= ConfrontationFinished;
            awaitingConfrontation = false;
            if (dialogue.LastDialogueCompleted) StartEffort();
            else Complete(false);
        }
        private bool StartEffort()
        {
            GameStateManager.Instance.SetState(GameState.QTE);
            qte.OnQTEEnded += EffortEnded;
            if(BullyingGame.UI.PlayerAccessibility.AssistedQTE) { qte.OnQTEEnded-=EffortEnded;StartAftermath(true);return true; }
            if(qte.TryStartQTE(activity.EffortQTE)) return true;
            Complete(false);return false;
        }
        private void Frame(bool confrontation)
        {
            if(activityCamera == null || player == null) return;
            if (!confrontation && arithmeticCameraAnchor != null)
            {
                activityCamera.transform.SetPositionAndRotation(arithmeticCameraAnchor.position, arithmeticCameraAnchor.rotation);
                activityCamera.Lens.FieldOfView = 56;
                return;
            }
            Vector3 focus=player.position+Vector3.up*1.25f+(confrontation ? player.forward*1.2f : Vector3.forward*1.1f);
            Vector3 position=confrontation ? focus-player.forward*4.8f+player.right*6.7f+Vector3.up*1.1f : player.position+Vector3.right*3.6f+Vector3.back*1.1f+Vector3.up*2;
            focus -= Vector3.up * .55f;
            activityCamera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(focus-position));
            activityCamera.Lens.FieldOfView=confrontation ? 53 : 46;
        }
        private void StageGang()
        {
            actorPositions=new Vector3[gang.Length];actorRotations=new Quaternion[gang.Length];
            for(int i=0;i<gang.Length;i++)
            {
                if(gang[i] == null) continue;
                actorPositions[i]=gang[i].position;actorRotations[i]=gang[i].rotation;
                var position=activity.Kind == NarrativeActivityKind.Arithmetic && arithmeticGangPoints != null && i < arithmeticGangPoints.Length && arithmeticGangPoints[i] != null
                    ? arithmeticGangPoints[i].position : activity.Kind == NarrativeActivityKind.Arithmetic
                    ? player.position+player.right*(2.3f+i*.45f)-player.forward*(.4f+i*.6f)
                    : player.position+player.forward*(2.2f+i*.35f)+player.right*((i-1)*1.3f);
                var facing = player.position - position; facing.y = 0;
                Place(gang[i],position,Quaternion.LookRotation(facing.sqrMagnitude > .01f ? facing : -player.forward));
            }
        }
        private static void Place(Transform actor,Vector3 position,Quaternion rotation)
        {
            var agent=actor.GetComponent<NavMeshAgent>();
            if(agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) { agent.ResetPath();agent.Warp(position); }
            else actor.position=position;
            actor.rotation=rotation;
        }
        public void SubmitAnswer(int index)
        {
            if(!IsMathAwaitingAnswer || GameStateManager.Instance.CurrentState != GameState.Quiz || index < 0 || index >= activity.Answers.Length) return;
            mathPanel.SetActive(false);
            StartAftermath(index == activity.CorrectAnswer);
        }
        private void EffortEnded(QTEResult result)
        {
            if(qte != null) qte.OnQTEEnded -= EffortEnded;
            if(cancelling || !IsRunning) return;
            if(qte.LastQTECancelled) { Complete(false);return; }
            StartAftermath(result == QTEResult.Success);
        }
        private void StartAftermath(bool success)
        {
            if(activity.Kind == NarrativeActivityKind.Coercion)
            {
                if(playerFood != null) playerFood.SetActive(false);
                if(takenFood != null) takenFood.SetActive(true);
            }
            GameStateManager.Instance.SetState(GameState.Cinematic);
            var aftermath=activity.Aftermath(success);
            awaitingAftermath=true;
            dialogue.OnDialogueEnded+=AftermathFinished;
            if(!dialogue.TryStartDialogue(aftermath)) Complete(false);
        }
        private void AftermathFinished() { if(awaitingAftermath) Complete(dialogue.LastDialogueCompleted); }
        public void Cancel()
        {
            if(GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Paused)
                UnityEngine.Object.FindAnyObjectByType<BullyingGame.UI.PauseMenuUI>()?.ResumeGame();
            Complete(false);
        }
        private void Complete(bool completed)
        {
            if(!IsRunning) return;
            cancelling=true;
            if(qte != null) { qte.OnQTEEnded-=EffortEnded; if(qte.IsQTEActive) qte.CancelQTE(); }
            if(dialogue != null) { dialogue.OnDialogueEnded-=AftermathFinished; dialogue.OnDialogueEnded-=ConfrontationFinished; if((awaitingAftermath || awaitingConfrontation) && dialogue.IsDialogueActive) dialogue.EndDialogue(); }
            awaitingAftermath=false;
            awaitingConfrontation=false;
            mathPanel.SetActive(false);
            if(playerFood != null) playerFood.SetActive(false);
            if(takenFood != null) takenFood.SetActive(false);
            if(activityCamera != null) activityCamera.Priority=cameraPriority;
            if(actorPositions != null) for(int i=0;i<gang.Length;i++) if(gang[i] != null) Place(gang[i],actorPositions[i],actorRotations[i]);
            actorPositions=null;
            // Release modal ownership before restoring our original controls.
            if(GameStateManager.Instance != null && (GameStateManager.Instance.CurrentState == GameState.Quiz || GameStateManager.Instance.CurrentState == GameState.Cinematic || GameStateManager.Instance.CurrentState == GameState.QTE)) GameStateManager.Instance.SetState(GameState.Playing);
            for(int i=0;i<controlStates.Length;i++) if(controls[i] != null) controls[i].enabled=controlStates[i];
            Cursor.lockState=cursor;Cursor.visible=cursorVisible;
            var callback=finished; finished=null; activity=null;cancelling=false;
            callback?.Invoke(completed);
        }
        private void OnDisable() => Complete(false);
    }
}
