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
        [SerializeField] private string requiredObjectiveId;
        [SerializeField] private string itemId = "";
        [Header("Bullying Event")]
        [SerializeField] private BullyingEventData eventToTrigger;
        [SerializeField] private BullyingEventDirector eventDirector;

        private bool pending, collected, awaitingReveal;
        private QuestItemState stateBeforeEncounter;
        public QuestItemState State { get; private set; } = QuestItemState.Available;
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
            if (pending || collected || awaitingReveal) return false;
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
                return false;
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState != GameState.Playing) return false;
            return quest == null || (QuestManager.Instance != null &&
                QuestManager.Instance.GetQuestState(quest) == QuestState.Active &&
                (string.IsNullOrWhiteSpace(requiredObjectiveId) ||
                 QuestManager.Instance.IsObjectiveCompleted(quest, requiredObjectiveId)));
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
            stateBeforeEncounter = State;
            State = QuestItemState.Collected;
            SetVisible(false);
            subscribedManager = manager;
            manager.OnEventStateChanged += OnEventResult;
            if (!manager.TryTriggerEvent(eventToTrigger))
            {
                Unsubscribe();
                pending = false;
                State = stateBeforeEncounter;
                SetVisible(true);
            }
        }

        private void OnEventResult(BullyingEventData data, BullyingEventState state)
        {
            if (!pending || data != eventToTrigger) return;
            if (state != BullyingEventState.Resolved && state != BullyingEventState.Failed)
                return;
            bool rehide = state == BullyingEventState.Failed && subscribedManager != null &&
                subscribedManager.LastFailureReason == BullyingEventFailureReason.QTEFailed;
            Unsubscribe();
            pending = false;
            if (state == BullyingEventState.Resolved) SecureItem();
            else
            {
                State = stateBeforeEncounter;
                if (rehide)
                {
                    State = QuestItemState.Stolen;
                    var relocator = GetComponent<QuestItemRehide>();
                    if (relocator != null && relocator.isActiveAndEnabled && relocator.TryRehide())
                        State = QuestItemState.Rehidden;
                    else
                    {
                        State = stateBeforeEncounter;
                        if (relocator == null || !relocator.isActiveAndEnabled)
                            Debug.LogWarning("Tambahkan dan aktifkan Quest Item Rehide pada item, lalu isi Hide Points untuk spawn acak saat QTE gagal.", this);
                    }
                }
                awaitingReveal = true;
            }
        }

        private void Update()
        {
            if (awaitingReveal && (GameStateManager.Instance == null ||
                GameStateManager.Instance.CurrentState == GameState.Playing))
            {
                awaitingReveal = false;
                SetVisible(true);
            }
        }

        private void SecureItem()
        {
            if (collected) return;
            collected = true;
            State = QuestItemState.Secured;
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

        public QuestItemCheckpoint CaptureCheckpoint() => new QuestItemCheckpoint
        {
            itemId = itemId, state = State, position = transform.position
        };

        public void RestoreCheckpoint(QuestItemCheckpoint checkpoint)
        {
            if (pending || string.IsNullOrWhiteSpace(itemId)) return;
            bool secured = QuestManager.Instance != null && QuestManager.Instance.IsObjectiveCompleted(quest, objectiveId);
            if (secured || (checkpoint != null && checkpoint.itemId == itemId && checkpoint.state == QuestItemState.Secured))
            {
                State = QuestItemState.Secured;
                collected = true;
                SetVisible(false);
            }
            else if (checkpoint != null && checkpoint.itemId == itemId && checkpoint.state == QuestItemState.Rehidden)
            {
                var rehide = GetComponent<QuestItemRehide>();
                if (rehide != null && rehide.IsAuthoredLocation(checkpoint.position))
                {
                    transform.position = checkpoint.position;
                    State = QuestItemState.Rehidden;
                    Physics.SyncTransforms();
                }
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
            if (pending || awaitingReveal)
            {
                if (pending) State = stateBeforeEncounter;
                pending = false;
                awaitingReveal = false;
                SetVisible(true);
            }
        }
    }
}
