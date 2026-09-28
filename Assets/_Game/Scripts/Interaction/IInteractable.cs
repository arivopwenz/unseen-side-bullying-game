using UnityEngine;

namespace BullyingGame.Interaction
{
    public interface IInteractable
    {
        string GetPromptText();
        bool CanInteract();
        void Interact(GameObject interactor);
    }
}
