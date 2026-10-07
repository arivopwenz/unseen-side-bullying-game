using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.Quiz
{
    public class QuizUI : MonoBehaviour
    {
        [Header("Question")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TextMeshProUGUI[] optionTexts;
        [Header("Feedback")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackResultText;
        [SerializeField] private TextMeshProUGUI feedbackExplanationText;
        [SerializeField] private Button continueButton;
        private QuizManager manager;
        private void Awake()
        {
            if(panelRoot!=null) panelRoot.SetActive(false);
            if(feedbackPanel!=null) feedbackPanel.SetActive(false);
            if(optionButtons!=null) for(int i=0;i<optionButtons.Length;i++)
            {
                int index=i;
                if(optionButtons[i]!=null) optionButtons[i].onClick.AddListener(()=>manager?.SubmitAnswer(index));
            }
            if(continueButton!=null) continueButton.onClick.AddListener(Continue);
        }
        private void Start()
        {
            manager=QuizManager.Instance;
            if(manager==null) return;
            manager.OnQuizStarted+=DisplayQuestion;manager.OnAnswerSubmitted+=ShowFeedback;manager.OnQuizClosed+=Hide;
            if(manager.IsQuizActive) DisplayQuestion(manager.CurrentQuestion);
        }
        private void DisplayQuestion(QuizQuestion question)
        {
            if(panelRoot!=null) panelRoot.SetActive(true);
            if(feedbackPanel!=null) feedbackPanel.SetActive(false);
            if(questionText!=null) questionText.text=question.questionText;
            if(continueButton!=null) continueButton.gameObject.SetActive(false);
            if(optionButtons==null) return;
            for(int i=0;i<optionButtons.Length;i++)
            {
                if(optionButtons[i]==null) continue;
                bool visible=question.options!=null && i<question.options.Length;
                optionButtons[i].gameObject.SetActive(visible);optionButtons[i].interactable=true;
                if(visible && optionTexts!=null && i<optionTexts.Length && optionTexts[i]!=null)
                    optionTexts[i].text=question.options[i];
            }
        }
        private void ShowFeedback(bool correct,string explanation)
        {
            if(feedbackPanel!=null) feedbackPanel.SetActive(true);
            if(feedbackResultText!=null) feedbackResultText.text=correct ? "Pilihan yang membantu" : "Mari pertimbangkan kembali";
            if(feedbackExplanationText!=null) feedbackExplanationText.text=explanation;
            if(optionButtons!=null) foreach(var button in optionButtons) if(button!=null) button.interactable=false;
            if(continueButton!=null) continueButton.gameObject.SetActive(true);
        }
        private void Continue()
        {
            if(manager==null || !manager.IsAnswered) return;
            Hide();
            manager.CloseQuiz();
        }
        private void Hide()
        {
            if(panelRoot!=null) panelRoot.SetActive(false);
            if(feedbackPanel!=null) feedbackPanel.SetActive(false);
        }
        private void OnDestroy()
        {
            if(manager==null) return;
            manager.OnQuizStarted-=DisplayQuestion;manager.OnAnswerSubmitted-=ShowFeedback;manager.OnQuizClosed-=Hide;
        }
    }
}
