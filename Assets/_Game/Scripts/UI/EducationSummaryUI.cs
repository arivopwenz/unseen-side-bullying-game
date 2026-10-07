using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BullyingGame.Core;
using BullyingGame.Save;

namespace BullyingGame.UI
{
    public class EducationSummaryUI : MonoBehaviour
    {
        [SerializeField] private GameObject summaryPanel;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button continueButton;

        private void Awake()
        {
            if (summaryPanel != null) summaryPanel.SetActive(false);

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        public void ShowSummary(string moralMessage)
        {
            int empathyScore = 0;
            if (SaveManager.Instance != null)
            {
                empathyScore = SaveManager.Instance.CurrentData.empathyScore;
            }

            if (scoreText != null)
            {
                scoreText.text = $"Poin Refleksi: {empathyScore}";
            }

            if (messageText != null)
            {
                messageText.text = moralMessage;
            }

            if (summaryPanel != null)
            {
                summaryPanel.SetActive(true);
            }

            GameStateManager.Instance?.SetState(GameState.Result);
        }

        private void OnContinueClicked()
        {
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
            }

            GameStateManager.Instance?.SetState(GameState.Playing);
        }
    }
}
