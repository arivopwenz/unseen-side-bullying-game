using UnityEngine;
using UnityEngine.InputSystem;
using BullyingGame.Interaction;

namespace BullyingGame.Events
{
    // Zona dukungan Misi 3: pemain mendekat -> prompt, tekan Interact -> dialog
    // dukungan + objective M03. Sekali pakai. Dua jalur Interact (agar tidak
    // bergantung pada satu sistem saja): (1) IInteractable untuk
    // PlayerInteraction/InteractionDetector tim, (2) tombol Interact sendiri
    // saat pemain di dalam zona. Flag done menjaga keduanya agar tidak
    // mengeksekusi dua kali.
    public class SupportZone : MonoBehaviour, IInteractable
    {
        [SerializeField] private Mission2Driver driver;
        [SerializeField] private int supportIndex;
        [SerializeField] private string prompt = "Sapa";
        [SerializeField] private string[] lines;
        [SerializeField] private float radius = 2.5f;

        private bool done;
        private bool playerInside;
        private GameInputActions inputActions;

        public bool Done => done;

        public string GetPromptText()
        {
            return prompt;
        }

        public bool CanInteract()
        {
            return !done && driver != null && driver.Mission3Active;
        }

        private void Awake()
        {
            if (GetComponent<SphereCollider>() == null)
            {
                SphereCollider col = gameObject.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = radius;
            }
        }

        private void OnEnable()
        {
            if (inputActions == null) inputActions = new GameInputActions();
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

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Dispose();
                inputActions = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInside = true;
            if (CanInteract() && driver != null) driver.ShowPrompt("Tekan [E] — " + prompt);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInside = false;
            if (driver != null) driver.ClearPrompt();
        }

        private void OnInteractPressed(InputAction.CallbackContext context)
        {
            if (!playerInside || !CanInteract()) return;
            if (BullyingGame.Core.GameStateManager.Instance != null &&
                BullyingGame.Core.GameStateManager.Instance.CurrentState != BullyingGame.Core.GameState.Playing)
            {
                return;
            }
            TrySupport();
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract()) return;
            TrySupport();
        }

        private void TrySupport()
        {
            if (done) return;
            done = true;
            if (driver != null) driver.ClearPrompt();
            if (driver != null) driver.MarkSupported(supportIndex, lines);
        }
    }
}
