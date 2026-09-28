using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button continueButton;

        private void Start()
        {
            dialoguePanel.SetActive(false);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnEnable()
        {
            DialogueManager.Instance.OnDialogueStarted += ShowPanel;
            DialogueManager.Instance.OnLineDisplayed += UpdateLine;
            DialogueManager.Instance.OnDialogueEnded += HidePanel;
        }

        private void OnDisable()
        {
            if (DialogueManager.Instance == null) return;
            DialogueManager.Instance.OnDialogueStarted -= ShowPanel;
            DialogueManager.Instance.OnLineDisplayed -= UpdateLine;
            DialogueManager.Instance.OnDialogueEnded -= HidePanel;
        }

        private void ShowPanel()
        {
            dialoguePanel.SetActive(true);
        }

        private void HidePanel()
        {
            dialoguePanel.SetActive(false);
        }

        private void UpdateLine(DialogueLine line)
        {
            speakerNameText.text = line.speakerName;
            dialogueText.text = line.text;
        }

        private void OnContinueClicked()
        {
            DialogueManager.Instance.NextLine();
        }
    }
}
