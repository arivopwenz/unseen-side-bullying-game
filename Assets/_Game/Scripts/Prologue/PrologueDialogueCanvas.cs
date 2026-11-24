using System;
using UnityEngine;
using UnityEngine.UI;

namespace BullyingGame.Prologue
{
    [DisallowMultipleComponent]
    public sealed class PrologueDialogueCanvas : MonoBehaviour
    {
        [Serializable]
        public sealed class CharacterPortrait
        {
            public GameObject character;
            public string speakerName;
            [Tooltip("Drag sprite karakter ke sini.")]
            public Sprite sprite;
        }

        [Header("Canvas dan teks di Hierarchy")]
        [SerializeField] private GameObject canvasUtama;
        [SerializeField] private Image imageKarakter;
        [SerializeField] private Text teksDialogue;
        [SerializeField] private Text namaKarakter;

        [Header("Aji dari Canvas yang sudah dibuat")]
        [SerializeField] private Sprite ajiSprite;

        [Header("Sprite semua karakter")]
        [SerializeField] private CharacterPortrait[] characters = Array.Empty<CharacterPortrait>();

        public bool IsConfigured => canvasUtama != null && imageKarakter != null && teksDialogue != null;

        private void Awake()
        {
            if (ajiSprite == null && imageKarakter != null) ajiSprite = imageKarakter.sprite;
        }

        internal void SetVisible(bool visible)
        {
            if (canvasUtama != null) canvasUtama.SetActive(visible);
            if (imageKarakter != null) imageKarakter.gameObject.SetActive(visible);
            if (teksDialogue != null) teksDialogue.gameObject.SetActive(visible);
            if (namaKarakter != null) namaKarakter.gameObject.SetActive(visible);
        }

        internal void ShowLine(PrologueDialogueActor actor, PrologueDialogueActor.Line line)
        {
            string speaker = string.IsNullOrWhiteSpace(line?.speaker) ? actor.DisplayName : line.speaker;
            teksDialogue.text = line?.text ?? "";
            if (namaKarakter != null) namaKarakter.text = speaker;

            Sprite sprite = line?.portrait;
            if (sprite == null) sprite = FindPortrait(actor, speaker);
            imageKarakter.sprite = sprite;
            imageKarakter.enabled = sprite != null;
        }

        internal void ShowLine(string speaker, string text)
        {
            SetVisible(true);
            teksDialogue.text = text;
            if (namaKarakter != null) namaKarakter.text = speaker;
            string name = speaker.Split('(')[0].Trim();
            imageKarakter.sprite = FindPortrait(null, name);
            imageKarakter.enabled = imageKarakter.sprite != null;
        }

        private Sprite FindPortrait(PrologueDialogueActor actor, string speaker)
        {
            if (string.Equals(speaker.Trim(), "Aji", StringComparison.OrdinalIgnoreCase)) return ajiSprite;
            foreach (var entry in characters)
            {
                if (entry == null) continue;
                bool namedSpeaker = string.Equals(entry.speakerName?.Trim(), speaker.Trim(), StringComparison.OrdinalIgnoreCase);
                bool ownSpeaker = actor != null && entry.character == actor.gameObject &&
                    string.Equals(speaker, actor.DisplayName, StringComparison.OrdinalIgnoreCase);
                if (namedSpeaker || ownSpeaker) return entry.sprite;
            }
            return null;
        }
    }
}
