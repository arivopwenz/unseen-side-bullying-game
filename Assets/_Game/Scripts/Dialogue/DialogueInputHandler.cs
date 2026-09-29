using UnityEngine;
using BullyingGame.Player;

namespace BullyingGame.Dialogue
{
    public class DialogueInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;

        private void OnEnable()
        {
            DialogueManager.Instance.OnDialogueStarted += DisablePlayerInput;
            DialogueManager.Instance.OnDialogueEnded += EnablePlayerInput;
        }

        private void OnDisable()
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
