using UnityEngine;
using TMPro;

namespace BullyingGame.Interaction
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private InteractionDetector detector;
        [SerializeField] private float verticalOffset = 0.35f;

        private Transform currentTargetTransform;
        private UnityEngine.Camera mainCamera;
        private Canvas parentCanvas;

        private void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            parentCanvas = GetComponentInParent<Canvas>();
            if (detector == null)
            {
                detector = FindAnyObjectByType<InteractionDetector>();
            }
        }

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = FindAnyObjectByType<UnityEngine.Camera>();
            }
            if (promptPanel != null)
            {
                promptPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (detector == null)
            {
                detector = FindAnyObjectByType<InteractionDetector>();
            }

            if (detector != null)
            {
                detector.OnInteractableChanged += UpdatePrompt;
            }
        }

        private void OnDisable()
        {
            if (detector != null)
            {
                detector.OnInteractableChanged -= UpdatePrompt;
            }
        }

        private void UpdatePrompt(IInteractable interactable)
        {
            if (interactable != null)
            {
                if (interactable is Component comp)
                {
                    currentTargetTransform = comp.transform;
                }
                else
                {
                    currentTargetTransform = null;
                }

                if (promptText != null)
                {
                    promptText.text = interactable.GetPromptText();
                }

                if (promptPanel != null)
                {
                    promptPanel.SetActive(true);
                    UpdatePosition();
                }
            }
            else
            {
                currentTargetTransform = null;
                if (promptPanel != null)
                {
                    promptPanel.SetActive(false);
                }
            }
        }

        private void LateUpdate()
        {
            if (Dialogue.DialogueManager.Instance != null && Dialogue.DialogueManager.Instance.IsDialogueActive)
            {
                if (promptPanel != null && promptPanel.activeSelf)
                {
                    promptPanel.SetActive(false);
                }
                return;
            }

            if (currentTargetTransform != null && promptPanel != null && promptPanel.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (promptPanel == null || currentTargetTransform == null) return;

            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindAnyObjectByType<UnityEngine.Camera>();
                    if (mainCamera == null) return;
                }
            }

            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }

            Vector3 targetTop;
            var col = currentTargetTransform.GetComponentInChildren<Collider>();
            if (col != null)
            {
                targetTop = col.bounds.center + Vector3.up * (col.bounds.extents.y + verticalOffset);
            }
            else
            {
                targetTop = currentTargetTransform.position + Vector3.up * 1.2f;
            }

            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
            {
                parentCanvas.transform.position = targetTop;
                parentCanvas.transform.rotation = mainCamera.transform.rotation;
                promptPanel.transform.localPosition = Vector3.zero;
                return;
            }

            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetTop);
            if (screenPos.z > 0)
            {
                promptPanel.transform.position = screenPos;
            }
            else
            {
                promptPanel.SetActive(false);
            }
        }
    }
}
