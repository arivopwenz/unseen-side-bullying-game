using System.Collections;
using UnityEngine;
using BullyingGame.Interaction;
using BullyingGame.Dialogue;

namespace BullyingGame.NPC
{
    public abstract class BaseNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] protected NPCData npcData;
        [SerializeField] protected Transform dialogueSpawnPoint;
        [SerializeField] protected bool resetRotationAfterDialogue = true;

        private bool interactable = true;
        private Quaternion originalRotation;
        private Coroutine resetRotationCoroutine;

        protected virtual void Awake()
        {
            originalRotation = transform.rotation;
            if (dialogueSpawnPoint == null)
            {
                dialogueSpawnPoint = transform.Find("PlayerDialogueSpawnPoint");
                if (dialogueSpawnPoint == null)
                {
                    dialogueSpawnPoint = transform.Find("DialogueSpawnPoint");
                }
            }
        }

        protected virtual void Start()
        {
            SubscribeDialogueEvents();
        }

        protected virtual void OnEnable()
        {
            SubscribeDialogueEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeDialogueEvents();
        }

        private void SubscribeDialogueEvents()
        {
            if (DialogueManager.Instance == null) return;
            DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
            DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
        }

        private void UnsubscribeDialogueEvents()
        {
            if (DialogueManager.Instance == null) return;
            DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        }

        private void HandleDialogueEnded()
        {
            if (resetRotationAfterDialogue)
            {
                if (resetRotationCoroutine != null)
                {
                    StopCoroutine(resetRotationCoroutine);
                }
                resetRotationCoroutine = StartCoroutine(ResetRotationRoutine());
            }
        }

        private IEnumerator ResetRotationRoutine()
        {
            Quaternion startRot = transform.rotation;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.rotation = Quaternion.Slerp(startRot, originalRotation, t);
                yield return null;
            }

            transform.rotation = originalRotation;
            resetRotationCoroutine = null;
        }

        public string GetPromptText()
        {
            if (npcData == null) return "...";
            return $"[E] {npcData.interactionPrompt} - {npcData.npcName}";
        }

        public bool CanInteract()
        {
            return interactable && npcData != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract()) return;

            if (interactor != null)
            {
                if (resetRotationCoroutine != null)
                {
                    StopCoroutine(resetRotationCoroutine);
                    resetRotationCoroutine = null;
                }

                if (dialogueSpawnPoint != null)
                {
                    var controller = interactor.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        controller.enabled = false;
                    }

                    interactor.transform.position = dialogueSpawnPoint.position;

                    Vector3 toNpc = transform.position - interactor.transform.position;
                    toNpc.y = 0f;
                    if (toNpc.sqrMagnitude > 0.001f)
                    {
                        interactor.transform.rotation = Quaternion.LookRotation(toNpc);
                    }
                    else
                    {
                        interactor.transform.rotation = dialogueSpawnPoint.rotation;
                    }

                    if (controller != null)
                    {
                        controller.enabled = true;
                    }
                }
                else
                {
                    Vector3 toNpc = transform.position - interactor.transform.position;
                    toNpc.y = 0f;
                    if (toNpc.sqrMagnitude > 0.001f)
                    {
                        interactor.transform.rotation = Quaternion.LookRotation(toNpc);
                    }
                }

                Vector3 toPlayer = interactor.transform.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(toPlayer);
                }
            }

            OnInteract(interactor);
        }

        protected abstract void OnInteract(GameObject interactor);

        public void SetInteractable(bool value)
        {
            interactable = value;
        }

        private void OnDrawGizmosSelected()
        {
            if (dialogueSpawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(dialogueSpawnPoint.position, 0.4f);
                Gizmos.DrawLine(transform.position, dialogueSpawnPoint.position);
            }
        }
    }
}
