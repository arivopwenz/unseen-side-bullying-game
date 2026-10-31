using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using BullyingGame.Core;

namespace BullyingGame.NPC
{
    /// <summary>
    /// Ambush confrontation subtitles (Tahap 3). Shows the drafted confrontation
    /// lines on a dedicated subtitle text (a second child of the WalkTalk panel,
    /// so the bar stays visually identical) while the guide is Paused for the
    /// ambush. WalkTalkDriver clears its OWN text during Paused, so a separate
    /// text object is required here. NEVER locks movement, NEVER touches
    /// GameState or DialogueManager. Pure overlay; disable this object to
    /// remove the confrontation lines (AmbushTrigger then falls back to its
    /// timed beat). This GameObject must stay ACTIVE: lines run as a coroutine.
    /// </summary>
    public class AmbushDialogueDriver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text subtitleText;

        [Header("Advance (mini-cutscene: dialog maju dengan tombol, bukan timer)")]
        [SerializeField] private bool waitForKeyAdvance = true;
        [SerializeField] private KeyCode advanceKey = KeyCode.E;

        [Header("Confrontation lines (draft Tahap 3; bully labels Nakal/Nakal1 = scene names, pending Dion/Tio/Fajar mapping)")]
        [SerializeField] private string[] lines = new string[]
        {
            "Nakal: Eh, lihat siapa ini. Si gendut bawa anak baru, sok jadi pemandu.",
            "Nakal1: Jalan aja ngos-ngosan, masih sempat sok akrab.",
            "Bima: Maaf... kami cuma mau lewat ke kelas.",
            "Nakal: Kelas? Tempat kamu itu di kantin, bukan di kelas. Minggir sana!",
            "Nakal1: Anak baru, hati-hati ya. Ketularan leletnya nanti."
        };

        [Header("Timing")]
        [SerializeField, Min(1f)] private float lineDisplayTime = 4f;

        private Coroutine lineRoutine;
        private int shownCount;
        private string lastLine = "";

        public bool IsPlaying => lineRoutine != null;
        public bool Finished { get; private set; }
        public int ShownCount => shownCount;
        public string LastLine => lastLine;
        public float TotalDuration => (lines != null ? lines.Length : 0) * lineDisplayTime;
        public bool WaitForKeyAdvance => waitForKeyAdvance;
        public float LineDisplayTime => lineDisplayTime;
        private bool advanceRequested;
        public void RequestAdvance() => advanceRequested = true;

        private bool IsAdvanceKeyPressed()
        {
            // Proyek ini memakai Input System package saja: UnityEngine.Input
            // melempar InvalidOperationException di sini, jadi tombol dibaca
            // lewat Keyboard.current (saat ini hanya E yang dipetakan).
            var kb = Keyboard.current;
            if (kb == null) return false;
            if (advanceKey == KeyCode.E) return kb.eKey.wasPressedThisFrame;
            return false;
        }
        public void ShowCue(string cue) { if (subtitleText != null) subtitleText.text = cue; }

        public void PlayLines()
        {
            if (subtitleText == null || lines == null || lines.Length == 0) { Finished = true; return; }
            if (lineRoutine != null) StopCoroutine(lineRoutine);
            Finished = false;
            shownCount = 0;
            lastLine = "";
            lineRoutine = StartCoroutine(PlaySet(0, lines.Length));
        }

        public void PlayLinesRange(int start, int count)
        {
            if (subtitleText == null || lines == null || lines.Length == 0) { Finished = true; return; }
            if (lineRoutine != null) StopCoroutine(lineRoutine);
            Finished = false;
            lineRoutine = StartCoroutine(PlaySet(start, count));
        }

        public void StopLines()
        {
            if (lineRoutine != null) { StopCoroutine(lineRoutine); lineRoutine = null; }
            if (subtitleText != null) subtitleText.text = "";
        }

        private IEnumerator PlaySet(int from, int count)
        {
            int end = Mathf.Min(lines.Length, from + count);
            for (int i = from; i < end; i++)
            {
                string line = lines[i];
                while (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState != GameState.Playing)
                    yield return null;
                if (subtitleText != null)
                    subtitleText.text = waitForKeyAdvance ? line + System.Environment.NewLine + "Tekan [E] untuk lanjut" : line;
                shownCount++;
                lastLine = line;
                float voiceLen = VoiceLinePlayer.PlayLine(line);
                if (waitForKeyAdvance)
                {
                    advanceRequested = false;
                    while (!IsAdvanceKeyPressed() && !advanceRequested)
                        yield return null;
                    advanceRequested = false;
                }
                else
                {
                    yield return new WaitForSeconds(voiceLen > 0f ? voiceLen + 0.25f : lineDisplayTime);
                }
            }
            if (subtitleText != null) subtitleText.text = "";
            lineRoutine = null;
            Finished = true;
        }
    }
}
