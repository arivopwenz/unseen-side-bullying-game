using UnityEngine;
using UnityEngine.InputSystem;

namespace BullyingGame.Interaction
{
    [RequireComponent(typeof(InteractionDetector))]
    public class PlayerInteraction : MonoBehaviour
    {
        private InteractionDetector detector;
        private GameInputActions inputActions;

        private void Awake()
        {
            detector = GetComponent<InteractionDetector>();
            EnsureInputActions();
        }

        private void OnEnable()
        {
            EnsureInputActions();
            inputActions.Movement.Interact.performed += OnInteractPressed;
            inputActions.Movement.Interact.Enable();
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Movement.Interact.performed -= OnInteractPressed;
                inputActions.Movement.Interact.Disable();
            }
        }

        private void EnsureInputActions()
        {
            if (inputActions == null)
            {
                inputActions = new GameInputActions();
            }
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
            inputActions = null;
        }

        private void OnInteractPressed(InputAction.CallbackContext context)
        {
            if (detector != null && detector.CurrentInteractable != null && detector.CurrentInteractable.CanInteract())
            {
                detector.CurrentInteractable.Interact(gameObject);
            }
        }
    }
}