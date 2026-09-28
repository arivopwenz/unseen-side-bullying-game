using System;
using UnityEngine;

namespace BullyingGame.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public event Action<DialogueLine> OnLineDisplayed;
        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;

        public bool IsDialogueActive { get; private set; }

        private DialogueData currentDialogue;
        private int currentLineIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartDialogue(DialogueData dialogue)
        {
            if (IsDialogueActive || dialogue == null || dialogue.lines.Length == 0) return;

            currentDialogue = dialogue;
            currentLineIndex = 0;
            IsDialogueActive = true;

            OnDialogueStarted?.Invoke();
            DisplayCurrentLine();
        }

        public void NextLine()
        {
            if (!IsDialogueActive) return;

            currentLineIndex++;

            if (currentLineIndex >= currentDialogue.lines.Length)
            {
                EndDialogue();
                return;
            }

            DisplayCurrentLine();
        }

        public void EndDialogue()
        {
            IsDialogueActive = false;
            currentDialogue = null;
            currentLineIndex = 0;

            OnDialogueEnded?.Invoke();
        }

        private void DisplayCurrentLine()
        {
            OnLineDisplayed?.Invoke(currentDialogue.lines[currentLineIndex]);
        }
    }
}
