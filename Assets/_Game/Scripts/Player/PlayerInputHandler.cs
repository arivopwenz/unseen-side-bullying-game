using UnityEngine;
using UnityEngine.InputSystem;

namespace BullyingGame.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private GameInputActions inputActions;
        private PlayerMovement playerMovement;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            EnsureInputActions();
        }

        private void Start()
        {
            SetCursorLock(true);
        }

        private void OnEnable()
        {
            EnsureInputActions();
            inputActions.Movement.Enable();
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Movement.Disable();
            }
        }

        private void EnsureInputActions()
        {
            if (inputActions == null)
            {
                inputActions = new GameInputActions();
            }
        }

        private void Update()
        {
            if (inputActions == null || playerMovement == null) return;

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
            inputActions = null;
        }
    }
}
