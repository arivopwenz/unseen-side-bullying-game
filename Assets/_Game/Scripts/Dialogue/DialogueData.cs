using UnityEngine;

namespace BullyingGame.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Game/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public DialogueLine[] lines;
    }
}
