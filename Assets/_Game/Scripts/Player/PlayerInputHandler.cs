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
            if (BullyingGame.Core.GameStateManager.Instance == null ||
                BullyingGame.Core.GameStateManager.Instance.CurrentState == BullyingGame.Core.GameState.Playing)
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

            if (BullyingGame.Core.GameStateManager.Instance != null &&
                BullyingGame.Core.GameStateManager.Instance.CurrentState != BullyingGame.Core.GameState.Playing)
            {
                playerMovement.SetMoveInput(Vector2.zero);
                playerMovement.SetSprintInput(false);
                return;
            }

            Vector2 moveInput = inputActions.Movement.Move.ReadValue<Vector2>();
            bool sprintInput = inputActions.Movement.Sprint.IsPressed();

            playerMovement.SetMoveInput(moveInput);
            playerMovement.SetSprintInput(sprintInput);

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
