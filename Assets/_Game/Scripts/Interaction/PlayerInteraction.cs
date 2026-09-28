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
            inputActions = new GameInputActions();
        }

        private void OnEnable()
        {
            inputActions.Movement.Interact.performed += OnInteractPressed;
            inputActions.Movement.Interact.Enable();
        }

        private void OnDisable()
        {
            inputActions.Movement.Interact.performed -= OnInteractPressed;
            inputActions.Movement.Interact.Disable();
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }

        private void OnInteractPressed(InputAction.CallbackContext context)
        {
            if (detector.CurrentInteractable != null && detector.CurrentInteractable.CanInteract())
            {
                detector.CurrentInteractable.Interact(gameObject);
            }
        }
    }
}