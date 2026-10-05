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
            playerMovement.SetMoveInput(Vector2.zero);
            playerMovement.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void EnablePlayerInput()
        {
            playerMovement.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
