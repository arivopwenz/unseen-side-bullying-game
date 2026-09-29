using UnityEngine;
using TMPro;

namespace BullyingGame.Interaction
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private InteractionDetector detector;

        private void Start()
        {
            promptPanel.SetActive(false);
        }

        private void OnEnable()
        {
            detector.OnInteractableChanged += UpdatePrompt;
        }

        private void OnDisable()
        {
            detector.OnInteractableChanged -= UpdatePrompt;
        }

        private void UpdatePrompt(IInteractable interactable)
        {
            if (interactable != null)
            {
                promptPanel.SetActive(true);
                promptText.text = interactable.GetPromptText();
            }
            else
            {
                promptPanel.SetActive(false);
            }
        }
    }
}
