using UnityEngine;
using BullyingGame.Dialogue;


namespace BullyingGame.NPC
{
    public class TalkableNPC : BaseNPC
    {
        [SerializeField] private DialogueData dialogueData;

        protected override void OnInteract(GameObject interactor)
        {
            if (dialogueData == null) return;
            DialogueManager.Instance.StartDialogue(dialogueData);

        }
    }
}
