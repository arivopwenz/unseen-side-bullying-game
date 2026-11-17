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
            if (teksDialogue == null || lines == null || lines.Length == 0)
            {
                Debug.LogWarning("Isi Teks Dialogue dan Lines pada " + DisplayName, this);
                return false;
            }
            lineIndex = 0;
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

        internal void SetVisible(bool visible)
        {
            if (canvasUtama != null) canvasUtama.SetActive(visible);
            if (canvasKarakter != null) canvasKarakter.SetActive(visible);
            if (teksDialogue != null) teksDialogue.gameObject.SetActive(visible);
        }

        private void ShowLine()
        {
            var line = lines[lineIndex];
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
