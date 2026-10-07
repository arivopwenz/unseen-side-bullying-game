using System;
using UnityEngine;
using BullyingGame.Core;

namespace BullyingGame.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public event Action<DialogueLine> OnLineDisplayed;
        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;
        public event Action<DialogueResponse[]> OnChoicesRequested;

        public bool IsDialogueActive { get; private set; }
        public bool LastDialogueCompleted { get; private set; }
        public DialogueLine CurrentLine => IsDialogueActive && activeLines != null && currentLineIndex < activeLines.Length ? activeLines[currentLineIndex] : null;
        public bool IsAwaitingChoice { get; private set; }
        public DialogueData CurrentDialogue => currentDialogue;

        private DialogueData currentDialogue;
        private int currentLineIndex;
        private bool ownsDialogueState;
        private DialogueLine[] activeLines;
        private int selectedChoice = -1;
        private bool responsePlaying;

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
            TryStartDialogue(dialogue);
        }

        public bool TryStartDialogue(DialogueData dialogue)
        {
            if (!isActiveAndEnabled || IsDialogueActive || dialogue == null ||
                dialogue.lines == null || dialogue.lines.Length == 0) return false;
            foreach (var line in dialogue.lines)
                if (line == null) return false;

            currentDialogue = dialogue;
            activeLines = dialogue.lines;
            int context = BullyingGame.Save.SaveManager.Instance != null ? BullyingGame.Save.SaveManager.Instance.GetNarrativeChoice(dialogue.ContextChoiceId) : -1;
            if(context >= 0 && dialogue.ContextResponses != null && context < dialogue.ContextResponses.Length &&
                dialogue.ContextResponses[context] != null && dialogue.ContextResponses[context].Lines != null)
            {
                var prefix = dialogue.ContextResponses[context].Lines;
                activeLines = new DialogueLine[prefix.Length + dialogue.lines.Length];
                System.Array.Copy(prefix, activeLines, prefix.Length);
                System.Array.Copy(dialogue.lines, 0, activeLines, prefix.Length, dialogue.lines.Length);
            }
            selectedChoice = -1;
            responsePlaying = false;
            IsAwaitingChoice = false;
            currentLineIndex = 0;
            IsDialogueActive = true;
            LastDialogueCompleted = false;
            ownsDialogueState = GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Playing;
            if (ownsDialogueState) GameStateManager.Instance.SetState(GameState.Dialogue);

            OnDialogueStarted?.Invoke();
            if (IsDialogueActive && currentDialogue == dialogue) DisplayCurrentLine();
            return IsDialogueActive;
        }

        public void NextLine()
        {
            if (!IsDialogueActive || IsAwaitingChoice || IsPaused) return;

            currentLineIndex++;

            if (currentLineIndex >= activeLines.Length)
            {
                var sequence = UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
                if(!responsePlaying && sequence != null && sequence.CurrentChapter.AllowsDialogueChoices &&
                    POVManager.Instance != null && POVManager.Instance.CurrentPOV == POVType.Protagonist &&
                    !string.IsNullOrWhiteSpace(currentDialogue.ChoiceId) && currentDialogue.Responses != null && currentDialogue.Responses.Length == 3)
                {
                    currentLineIndex = activeLines.Length - 1;
                    IsAwaitingChoice = true;
                    OnChoicesRequested?.Invoke(currentDialogue.Responses);
                    return;
                }
                FinishDialogue(true);
                return;
            }

            DisplayCurrentLine();
        }

        public void EndDialogue()
        {
            FinishDialogue(false);
        }

        private bool IsPaused => GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Paused;
        public bool SelectChoice(int index)
        {
            if(!IsDialogueActive || !IsAwaitingChoice || IsPaused || index < 0 || index >= currentDialogue.Responses.Length) return false;
            var response = currentDialogue.Responses[index];
            if(response == null || response.Lines == null || response.Lines.Length == 0) return false;
            foreach(var line in response.Lines) if(line == null) return false;
            selectedChoice = index;
            responsePlaying = true;
            IsAwaitingChoice = false;
            activeLines = response.Lines;
            currentLineIndex = 0;
            DisplayCurrentLine();
            return true;
        }

        private void FinishDialogue(bool completed)
        {
            if (!IsDialogueActive) return;
            LastDialogueCompleted = completed;
            if(completed && selectedChoice >= 0) BullyingGame.Save.SaveManager.Instance?.RecordNarrativeChoice(currentDialogue.ChoiceId, selectedChoice);
            IsAwaitingChoice = false;
            IsDialogueActive = false;
            currentDialogue = null;
            activeLines = null;
            currentLineIndex = 0;
            if (ownsDialogueState && GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Dialogue)
                GameStateManager.Instance.SetState(GameState.Playing);
            ownsDialogueState = false;

            OnDialogueEnded?.Invoke();
        }

        private void DisplayCurrentLine()
        {
            OnLineDisplayed?.Invoke(CurrentLine);
        }

        private void OnDisable() => EndDialogue();

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
