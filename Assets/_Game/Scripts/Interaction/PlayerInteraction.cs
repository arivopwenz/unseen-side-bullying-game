using BullyingGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BullyingGame.Interaction
{
    [RequireComponent(typeof(InteractionDetector))]
    public class PlayerInteraction : MonoBehaviour
    {
        private InteractionDetector detector;
        private GameInputActions inputActions;
        private ActiveInputDeviceTracker promptDevice;
        private TextMeshPro doorPrompt;
        private UnityEngine.Camera promptCamera;

        private void Awake()
        {
            detector = GetComponent<InteractionDetector>();
            EnsureInputActions();
        }

        private void OnEnable()
        {
            EnsureInputActions();
            promptDevice = new ActiveInputDeviceTracker();
            inputActions.Movement.Interact.performed += OnInteractPressed;
            inputActions.Movement.Interact.Enable();
        }

        private void OnDisable()
        {
            promptDevice?.Dispose();
            if (doorPrompt != null) doorPrompt.gameObject.SetActive(false);
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

        public static bool CanUseInteraction()
        {
            if (BullyingGame.Core.GameStateManager.Instance != null &&
                BullyingGame.Core.GameStateManager.Instance.CurrentState != BullyingGame.Core.GameState.Playing)
                return false;
            return BullyingGame.Dialogue.DialogueManager.Instance == null ||
                !BullyingGame.Dialogue.DialogueManager.Instance.IsDialogueActive;
        }

        private void OnInteractPressed(InputAction.CallbackContext context)
        {
            if (!CanUseInteraction()) return;
            var target = GetInteractionTarget();
            if (target != null && target.CanInteract()) target.Interact(gameObject);
        }

        private void LateUpdate()
        {
            var door = CanUseInteraction() ? GetInteractionTarget() as SwingDoor : null;
            string binding = door == null ? null : InputPromptResolver.Label(inputActions.Movement.Interact, promptDevice.Device);
            ShowDoorPrompt(door, binding);
        }

        public SwingDoor FindNearbyDoor() => detector.FindNearbyDoor();

        private IInteractable GetInteractionTarget()
        {
            var target = detector.CurrentInteractable;
            return target == null || target is SwingDoor ? FindNearbyDoor() : target;
        }

        public void ShowDoorPrompt(SwingDoor door, string binding)
        {
            if (door == null || !door.CanInteract())
            {
                if (doorPrompt != null) doorPrompt.gameObject.SetActive(false);
                return;
            }

            if (promptCamera == null || !promptCamera.isActiveAndEnabled) promptCamera = UnityEngine.Camera.main;
            if (promptCamera == null)
            {
                if (doorPrompt != null) doorPrompt.gameObject.SetActive(false);
                return;
            }

            if (doorPrompt == null)
            {
                var label = new GameObject("Door interaction prompt");
                label.transform.SetParent(transform, false);
                doorPrompt = label.AddComponent<TextMeshPro>();
                doorPrompt.fontSize = 2.8f;
                doorPrompt.fontStyle = FontStyles.Bold;
                doorPrompt.alignment = TextAlignmentOptions.Center;
                doorPrompt.outlineColor = new Color32(0, 0, 0, 255);
                doorPrompt.outlineWidth = .25f;
            }

            string action = door.GetPromptText();
            doorPrompt.text = string.IsNullOrEmpty(binding) ? action : $"<color=#FFDC82>[{binding}]</color>  {action}";
            doorPrompt.transform.SetPositionAndRotation(
                door.GetPromptPosition(promptCamera.transform.position), promptCamera.transform.rotation);
            doorPrompt.gameObject.SetActive(true);
        }
    }
}
