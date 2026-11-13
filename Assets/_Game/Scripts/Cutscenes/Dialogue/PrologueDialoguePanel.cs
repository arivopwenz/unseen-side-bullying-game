using UnityEngine;
using TMPro;
namespace UnseenSide.Cutscenes
{
    public sealed class PrologueDialoguePanel : MonoBehaviour
    {
        private UnityEngine.Playables.PlayableDirector boundDirector;
        public void Bind(UnityEngine.Playables.PlayableDirector director)
        {
            if (boundDirector) boundDirector.stopped -= OnDirectorStopped;
            boundDirector = director;
            if (boundDirector) boundDirector.stopped += OnDirectorStopped;
        }
        private void OnDirectorStopped(UnityEngine.Playables.PlayableDirector director) { Hide(); }
        public CanvasGroup visibility;
        public TMP_Text speakerLabel;
        public TMP_Text dialogueLabel;
        public TMP_Text sequenceLabel;
        public UnityEngine.UI.Image speakerAccent;
        public void Show(string speaker, string line, string sequence, float alpha)
        {
            if (!visibility) return;
            visibility.alpha = alpha;
            visibility.interactable = false;
            visibility.blocksRaycasts = false;
            if (speakerLabel) speakerLabel.text = speaker;
            if (dialogueLabel) dialogueLabel.text = line;
            if (sequenceLabel) sequenceLabel.text = sequence;
            var color = speaker == "AJI" ? new Color32(137,213,232,255) : new Color32(255,204,112,255);
            if (speakerAccent) speakerAccent.color = color;
            if (speakerLabel) speakerLabel.color = color;
        }
        public void Hide() { if (visibility) visibility.alpha = 0; }
        private void OnDisable() { Hide(); if (boundDirector) boundDirector.stopped -= OnDirectorStopped; }
    }
}