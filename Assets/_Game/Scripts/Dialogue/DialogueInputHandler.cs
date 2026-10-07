using UnityEngine;
using BullyingGame.Player;
using BullyingGame.Core;

namespace BullyingGame.Dialogue
{
    public class DialogueInputHandler : MonoBehaviour
    {
        [Header("Gameplay Lock")]
        [SerializeField] private PlayerMovement playerMovement;

        private DialogueManager subscribedManager;
        private bool inputLocked, movementWasEnabled, cursorWasVisible;
        private CursorLockMode previousCursor;

        private void Start()
        {
            if (playerMovement == null) playerMovement = FindAnyObjectByType<PlayerMovement>();
            SubscribeEvents();
        }

        private void OnEnable() => SubscribeEvents();

        private void SubscribeEvents()
        {
            var manager = DialogueManager.Instance;
            if (manager == null || subscribedManager == manager) return;
            UnsubscribeEvents();
            subscribedManager = manager;
            manager.OnDialogueStarted += DisablePlayerInput;
            manager.OnDialogueEnded += RestorePlayerInput;
            if (manager.IsDialogueActive) DisablePlayerInput();
        }

        private void DisablePlayerInput()
        {
            if (inputLocked) return;
            inputLocked = true;
            previousCursor = Cursor.lockState;
            cursorWasVisible = Cursor.visible;
            if (playerMovement != null)
            {
                movementWasEnabled = playerMovement.enabled;
                playerMovement.SetMoveInput(Vector2.zero);
                playerMovement.SetSprintInput(false);
                playerMovement.enabled = false;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void RestorePlayerInput()
        {
            if (!inputLocked) return;
            inputLocked = false;
            bool cinematic = CutsceneManager.Instance != null && CutsceneManager.Instance.CurrentDirector != null;
            if (playerMovement != null)
            {
                playerMovement.SetMoveInput(Vector2.zero);
                playerMovement.SetSprintInput(false);
                playerMovement.enabled = movementWasEnabled && !cinematic;
            }
            Cursor.lockState = previousCursor;
            Cursor.visible = cursorWasVisible;
        }

        private void UnsubscribeEvents()
        {
            if (subscribedManager != null)
            {
                subscribedManager.OnDialogueStarted -= DisablePlayerInput;
                subscribedManager.OnDialogueEnded -= RestorePlayerInput;
            }
            subscribedManager = null;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            RestorePlayerInput();
        }
    }
}
