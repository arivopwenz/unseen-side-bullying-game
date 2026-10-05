using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.Quiz
{
    public class QuizUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TextMeshProUGUI[] optionTexts;

        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackResultText;
        [SerializeField] private TextMeshProUGUI feedbackExplanationText;
        [SerializeField] private Button continueButton;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (feedbackPanel != null) feedbackPanel.SetActive(false);

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void Start()
        {
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.OnQuizStarted += DisplayQuestion;
                QuizManager.Instance.OnAnswerSubmitted += ShowFeedback;
            }
        }

        private void OnDestroy()
        {
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.OnQuizStarted -= DisplayQuestion;
                QuizManager.Instance.OnAnswerSubmitted -= ShowFeedback;
            }
        }

        private void DisplayQuestion(QuizQuestion question)
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            if (feedbackPanel != null) feedbackPanel.SetActive(false);

            if (questionText != null)
            {
                questionText.text = question.questionText;
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < question.options.Length)
                {
                    optionButtons[i].gameObject.SetActive(true);
                    if (optionTexts != null && i < optionTexts.Length)
                    {
                        optionTexts[i].text = question.options[i];
                    }
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnOptionClicked(int index)
        {
            QuizManager.Instance?.SubmitAnswer(index);
        }

        private void ShowFeedback(bool isCorrect, string explanation)
        {
            if (feedbackPanel != null) feedbackPanel.SetActive(true);

            if (feedbackResultText != null)
            {
                feedbackResultText.text = isCorrect ? "Jawaban Tepat!" : "Kurang Tepat";
            }

            if (feedbackExplanationText != null)
            {
                feedbackExplanationText.text = explanation;
            }
        }

        private void OnContinueClicked()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (feedbackPanel != null) feedbackPanel.SetActive(false);
            QuizManager.Instance?.CloseQuiz();
        }
    }
}
