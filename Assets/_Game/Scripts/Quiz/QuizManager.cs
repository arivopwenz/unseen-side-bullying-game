using System;
using UnityEngine;
using BullyingGame.Core;

namespace BullyingGame.Quiz
{
    [Serializable]
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
        public event Action<bool,string> OnAnswerSubmitted;
        public event Action OnQuizClosed;
        public QuizQuestion CurrentQuestion { get; private set; }
        public bool IsQuizActive { get; private set; }
        public bool IsAnswered { get; private set; }
        private string reflectionId;
        private void Awake()
        {
            if(Instance!=null && Instance!=this) { Destroy(gameObject);return; } Instance=this;
        }
        public void StartQuiz(QuizQuestion question) => TryStartQuiz(question);
        public bool TryStartQuiz(QuizQuestion question, string checkpointId = null)
        {
            if(!isActiveAndEnabled || IsQuizActive || string.IsNullOrWhiteSpace(question.questionText) ||
                question.options==null || question.options.Length<2 || question.correctOptionIndex<0 ||
                question.correctOptionIndex>=question.options.Length || GameStateManager.Instance==null ||
                GameStateManager.Instance.CurrentState!=GameState.Playing) return false;
            CurrentQuestion=question;reflectionId=checkpointId;IsQuizActive=true;IsAnswered=false;
            GameStateManager.Instance.SetState(GameState.Quiz);
            OnQuizStarted?.Invoke(question);return true;
        }
        public void SubmitAnswer(int index)
        {
            if(!IsQuizActive || IsAnswered || index<0 || index>=CurrentQuestion.options.Length ||
                GameStateManager.Instance==null || GameStateManager.Instance.CurrentState!=GameState.Quiz) return;
            IsAnswered=true;
            bool correct=index==CurrentQuestion.correctOptionIndex;
            if(!string.IsNullOrWhiteSpace(reflectionId)) Save.SaveManager.Instance?.RecordReflectionAnswer(reflectionId,correct);
            else if(correct) Save.SaveManager.Instance?.AddEmpathyScore(10);
            OnAnswerSubmitted?.Invoke(correct,CurrentQuestion.educationalExplanation);
        }
        public void CloseQuiz()
        {
            if(!IsQuizActive || !IsAnswered) return;
            if(!string.IsNullOrWhiteSpace(reflectionId)) Save.SaveManager.Instance?.CloseReflection(reflectionId);
            IsQuizActive=false;
            if(GameStateManager.Instance!=null && GameStateManager.Instance.CurrentState==GameState.Quiz)
                GameStateManager.Instance.SetState(GameState.Playing);
            OnQuizClosed?.Invoke();
        }
        private void OnDisable()
        {
            if(!IsQuizActive) return;
            IsQuizActive=false;
            if(GameStateManager.Instance!=null && GameStateManager.Instance.CurrentState==GameState.Quiz)
                GameStateManager.Instance.SetState(GameState.Playing);
        }
        private void OnDestroy() { if(Instance==this) Instance=null; }
    }
}
