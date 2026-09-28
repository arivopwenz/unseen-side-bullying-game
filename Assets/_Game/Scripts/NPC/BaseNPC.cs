using UnityEngine;
using BullyingGame.Interaction;

namespace BullyingGame.NPC
{
    public abstract class BaseNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] protected NPCData npcData;

        private bool interactable = true;

        public string GetPromptText()
        {
            if (npcData == null) return "...";
            return $"[E] {npcData.interactionPrompt} - {npcData.npcName}";
        }

        public bool CanInteract()
        {
            return interactable && npcData != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract()) return;
            OnInteract(interactor);
        }

        protected abstract void OnInteract(GameObject interactor);

        public void SetInteractable(bool value)
        {
            interactable = value;
        }
    }
}
