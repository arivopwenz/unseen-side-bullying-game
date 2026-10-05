using System;
using UnityEngine;
using BullyingGame.Core;

namespace BullyingGame.Quiz
{
    [System.Serializable]
    public struct QuizQuestion
    {
        public string questionText;
        public string[] options;
        public int correctOptionIndex;
        public string educationalExplanation;
    }

    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        public event Action<QuizQuestion> OnQuizStarted;
        public event Action<bool, string> OnAnswerSubmitted;

        public QuizQuestion CurrentQuestion { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartQuiz(QuizQuestion question)
        {
            CurrentQuestion = question;
            GameManager.Instance?.SetState(GameState.Quiz);
            OnQuizStarted?.Invoke(question);
        }

        public void SubmitAnswer(int chosenIndex)
        {
            bool isCorrect = (chosenIndex == CurrentQuestion.correctOptionIndex);

            if (isCorrect)
            {
                Save.SaveManager.Instance?.AddEmpathyScore(10);
            }

            OnAnswerSubmitted?.Invoke(isCorrect, CurrentQuestion.educationalExplanation);
        }

        public void CloseQuiz()
        {
            GameManager.Instance?.SetState(GameState.Playing);
        }
    }
}
