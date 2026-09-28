using UnityEngine;
using UnityEngine.InputSystem;

namespace BullyingGame.Player
{
    /// <summary>
    /// Reads input from GameInputActions and feeds it to PlayerMovement.
    /// Bridges the Input System with the movement system, and manages gameplay cursor state.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private GameInputActions inputActions;
        private PlayerMovement playerMovement;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            inputActions = new GameInputActions();
        }

        private void Start()
        {
            SetCursorLock(true);
        }

        private void OnEnable()
        {
            inputActions.Movement.Enable();
        }

        private void OnDisable()
        {
            inputActions.Movement.Disable();
        }

        private void Update()
        {
            Vector2 moveInput = inputActions.Movement.Move.ReadValue<Vector2>();
            bool sprintInput = inputActions.Movement.Sprint.IsPressed();

            playerMovement.SetMoveInput(moveInput);
            playerMovement.SetSprintInput(sprintInput);

            HandleCursorToggle();
        }

        private void HandleCursorToggle()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorLock(false);
            }

            if (Cursor.lockState != CursorLockMode.Locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorLock(true);
            }
        }

        public void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }
    }
}
