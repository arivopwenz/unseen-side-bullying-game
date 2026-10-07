using UnityEngine;
using BullyingGame.Dialogue;
using BullyingGame.QTE;

namespace BullyingGame.Events
{
    public enum NarrativeActivityKind { Arithmetic, Coercion }
    [CreateAssetMenu(menuName="Game/Narrative Activity")]
    public class NarrativeActivityData : ScriptableObject
    {
        [Header("Activity")]
        [SerializeField] private NarrativeActivityKind kind;
        [SerializeField, TextArea] private string instruction;
        [SerializeField] private string[] answers;
        [SerializeField] private int correctAnswer;
        [SerializeField] private QTEData effortQTE;
        [Header("Aftermath: Both Paths Continue")]
        [SerializeField] private DialogueData successDialogue;
        [SerializeField] private DialogueData failureDialogue;
        public NarrativeActivityKind Kind => kind;
        public string Instruction => instruction;
        public string[] Answers => answers;
        public int CorrectAnswer => correctAnswer;
        public QTEData EffortQTE => effortQTE;
        public DialogueData Aftermath(bool success) => success ? successDialogue : failureDialogue;
    }
}
