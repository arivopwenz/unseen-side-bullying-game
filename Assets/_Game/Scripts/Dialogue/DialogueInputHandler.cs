using UnityEngine;
using BullyingGame.Player;

namespace BullyingGame.Dialogue
{
    public class DialogueInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;

        private void Start()
        {
            if (playerMovement == null)
            {
                playerMovement = FindAnyObjectByType<PlayerMovement>();
            }
            SubscribeEvents();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (DialogueManager.Instance == null) return;
            DialogueManager.Instance.OnDialogueStarted -= DisablePlayerInput;
            DialogueManager.Instance.OnDialogueEnded -= EnablePlayerInput;

            DialogueManager.Instance.OnDialogueStarted += DisablePlayerInput;
            DialogueManager.Instance.OnDialogueEnded += EnablePlayerInput;
        }

        private void UnsubscribeEvents()
        {
            if (DialogueManager.Instance == null) return;
            DialogueManager.Instance.OnDialogueStarted -= DisablePlayerInput;
            DialogueManager.Instance.OnDialogueEnded -= EnablePlayerInput;
        }

        private void DisablePlayerInput()
        {
            if (playerMovement == null) return;
            playerMovement.SetMoveInput(Vector2.zero);
            playerMovement.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void EnablePlayerInput()
        {
            if (playerMovement != null)
            {
                bool cinematic = BullyingGame.Core.CutsceneManager.Instance != null &&
                    BullyingGame.Core.CutsceneManager.Instance.CurrentDirector != null;
                playerMovement.enabled = !cinematic;
                playerMovement.SetMoveInput(Vector2.zero);
                playerMovement.SetSprintInput(false);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
