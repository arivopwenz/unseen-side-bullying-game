using UnityEngine;

namespace BullyingGame.NPC
{
    public class TalkableNPC : BaseNPC
    {
        [SerializeField] private string[] dialogueLines;
        private int currentLineIndex;

        protected override void OnInteract(GameObject interactor)
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
            {
                Debug.Log($"{npcData.npcName}: ...");
                return;
            }

            Debug.Log($"{npcData.npcName}: {dialogueLines[currentLineIndex]}");
            currentLineIndex = (currentLineIndex + 1) % dialogueLines.Length;
        }
    }
}
