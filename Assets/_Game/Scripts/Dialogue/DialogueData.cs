using UnityEngine;

namespace BullyingGame.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Game/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public DialogueLine[] lines;
        [Header("Aji: Three Responses")]
        [SerializeField] private string choiceId;
        [SerializeField] private DialogueResponse[] responses;
        [Header("Response To An Earlier Choice")]
        [SerializeField] private string contextChoiceId;
        [SerializeField] private DialogueResponse[] contextResponses;
        public string ChoiceId => choiceId;
        public DialogueResponse[] Responses => responses;
        public string ContextChoiceId => contextChoiceId;
        public DialogueResponse[] ContextResponses => contextResponses;
    }

    [System.Serializable]
    public class DialogueResponse
    {
        [SerializeField, TextArea] private string label;
        [SerializeField] private DialogueLine[] lines;
        public string Label => label;
        public DialogueLine[] Lines => lines;
    }
}
