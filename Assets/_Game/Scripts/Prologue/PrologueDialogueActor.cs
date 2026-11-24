using System;
using BullyingGame.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BullyingGame.Prologue
{
    [DisallowMultipleComponent]
    public sealed class PrologueDialogueActor : MonoBehaviour, IInteractable
    {
        [Serializable]
        public class Line
        {
            public string speaker;
            [TextArea(2, 4)] public string text;
            public Sprite portrait;
        }

        [SerializeField] private PrologueFlow flow;
        [SerializeField] private PrologueDialogueCanvas dialogueCanvas;
        [SerializeField] private PrologueDialogueShot dialogueShot;
        [SerializeField] private Animator dialogueAnimator;
        [SerializeField] private string displayName;
        [SerializeField] private GameObject canvasUtama;
        [SerializeField] private GameObject canvasKarakter;
        [SerializeField] private Text teksDialogue;
        [SerializeField] private Text namaKarakter;
        [SerializeField] private Image portrait;
        [SerializeField] private Line[] lines;
        [SerializeField] private UnityEvent onDialogueFinished;

        private int lineIndex;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
        public string GetPromptText() => "Bicara dengan " + DisplayName;
        public bool CanInteract() => isActiveAndEnabled && flow != null && flow.CanTalk(this);
        public void Interact(GameObject interactor)
        {
            if (CanInteract()) flow.TryTalk(this, interactor);
        }

        internal bool Begin()
        {
            bool hasCanvas = dialogueCanvas != null ? dialogueCanvas.IsConfigured : teksDialogue != null;
            if (!hasCanvas || lines == null || lines.Length == 0)
            {
                Debug.LogWarning("Isi Teks Dialogue dan Lines pada " + DisplayName, this);
                return false;
            }
            lineIndex = 0;
            if (dialogueShot != null) dialogueShot.Enter();
            SetVisible(true);
            ShowLine();
            return true;
        }

        internal bool Advance()
        {
            if (++lineIndex < lines.Length)
            {
                ShowLine();
                return true;
            }
            SetVisible(false);
            onDialogueFinished?.Invoke();
            return false;
        }

        private void OnDisable()
        {
            if (dialogueAnimator != null && dialogueAnimator.isActiveAndEnabled)
                dialogueAnimator.Play("Base Layer.Idle", 0, 0f);
            if (dialogueShot != null) dialogueShot.Exit();
        }

        internal void SetVisible(bool visible)
        {
            if (!visible && dialogueAnimator != null && dialogueAnimator.isActiveAndEnabled)
                dialogueAnimator.CrossFadeInFixedTime("Base Layer.Idle", .15f);
            if (!visible && dialogueShot != null) dialogueShot.Exit();
            if (dialogueCanvas != null)
            {
                dialogueCanvas.SetVisible(visible);
                return;
            }
            if (canvasUtama != null) canvasUtama.SetActive(visible);
            if (canvasKarakter != null) canvasKarakter.SetActive(visible);
            if (teksDialogue != null) teksDialogue.gameObject.SetActive(visible);
        }

        private void ShowLine()
        {
            if (dialogueAnimator != null && lineIndex <= 1)
                dialogueAnimator.CrossFadeInFixedTime(lineIndex == 0 ? "Base Layer.Standing Greeting" : "Base Layer.Talking", .15f, 0, 0f);
            var line = lines[lineIndex];
            if (dialogueCanvas != null)
            {
                dialogueCanvas.ShowLine(this, line);
                return;
            }
            teksDialogue.text = line?.text ?? "";
            if (namaKarakter != null)
                namaKarakter.text = string.IsNullOrWhiteSpace(line?.speaker) ? DisplayName : line.speaker;
            if (portrait != null)
            {
                portrait.sprite = line?.portrait;
                portrait.enabled = portrait.sprite != null;
            }
        }
    }
}
