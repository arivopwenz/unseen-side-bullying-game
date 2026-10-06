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
