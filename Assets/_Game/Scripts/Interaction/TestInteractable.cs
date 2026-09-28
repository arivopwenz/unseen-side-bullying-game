using UnityEngine;

namespace BullyingGame.Interaction
{
    public class TestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string promptText = "Interaksi";
        private bool isActive = true;

        public string GetPromptText()
        {
            return promptText;
        }

        public bool CanInteract()
        {
            return isActive;
        }

        public void Interact(GameObject interactor)
        {
            Debug.Log($"[Interaksi] {interactor.name} berinteraksi dengan {gameObject.name}!");
            
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);
            }
        }
    }
}
