using System.Collections;
using UnityEngine;
using TMPro;
using BullyingGame.Core;

namespace BullyingGame.NPC
{
    /// <summary>
    /// Walk-and-talk subtitles for the Chapter 1 opening tour (Tahap 2).
    /// Shows the drafted lines when the guide approaches each waypoint, as a
    /// bottom-screen subtitle overlay. NEVER locks movement, NEVER touches
    /// GameState or DialogueManager. Pure overlay; disable this object to
    /// remove the subtitles.
    /// </summary>
    public class WalkTalkDriver : MonoBehaviour
    {
        [System.Serializable]
        public class WaypointLines
        {
            public string[] lines = new string[0];
        }

        [Header("References")]
        [SerializeField] private GuideController guide;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private Transform[] waypoints = new Transform[0];

        [Header("Lines per waypoint (WP_01..WP_05, draft Tahap 2 apa adanya)")]
        [SerializeField] private WaypointLines[] waypointLines = new WaypointLines[]
        {
            new WaypointLines { lines = new string[]
            {
                "Bima: Eh, kamu anak baru ya? Aku Bima. Sini, aku anterin ke kelas.",
                "Aji: Iya, hari pertama. Makasih ya, Bima."
            } },
            new WaypointLines { lines = new string[]
            {
                "Bima: Itu Gedung Piala \u2014 piala-piala sekolah dipajang di sana.",
                "Bima: Nanti kalau gedungnya sudah jadi, kita lewat pintu tengahnya. Sekarang muter dulu, ya."
            } },
            new WaypointLines { lines = new string[]
            {
                "Bima: Ini lapangan. Istirahat nanti biasanya rame di sini."
            } },
            new WaypointLines { lines = new string[]
            {
                "Bima: Lewat koridor ini, kelas kita sudah dekat."
            } },
            new WaypointLines { lines = new string[]
            {
                "Bima: Nah, ini kelas kita. Masuk yuk \u2014 nanti aku kenalin ke teman-teman."
            } },
        };

        [Header("Timing")]
        [SerializeField, Min(1f)] private float lineDisplayTime = 4f;
        [SerializeField, Min(0.5f)] private float triggerDistance = 3f;

        private bool[] shown;
        private Coroutine lineRoutine;

        private void Awake()
        {
            shown = new bool[waypointLines.Length];
        }

        private void Update()
        {
            if (guide == null || subtitleText == null || waypoints == null) return;
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState != GameState.Playing)
            {
                ClearSubtitle();
                return;
            }
            var state = guide.CurrentState;
            if (state == GuideController.GuideState.Idle)
            {
                ResetShown();
                ClearSubtitle();
                return;
            }
            if (state == GuideController.GuideState.Paused)
            {
                ClearSubtitle();
                return;
            }
            if (state != GuideController.GuideState.Leading && state != GuideController.GuideState.Waiting
                && state != GuideController.GuideState.Arrived)
            {
                return;
            }
            int count = Mathf.Min(waypoints.Length, waypointLines.Length);
            for (int i = 0; i < count; i++)
            {
                if (shown[i] || waypoints[i] == null) continue;
                Vector3 delta = guide.transform.position - waypoints[i].position;
                delta.y = 0f;
                if (delta.sqrMagnitude <= triggerDistance * triggerDistance)
                {
                    shown[i] = true;
                    ShowSet(i);
                    break;
                }
            }
        }

        private void ResetShown()
        {
            if (shown == null) shown = new bool[waypointLines.Length];
            for (int i = 0; i < shown.Length; i++) shown[i] = false;
        }

        private void ShowSet(int index)
        {
            if (lineRoutine != null) StopCoroutine(lineRoutine);
            if (waypointLines == null || index < 0 || index >= waypointLines.Length)
            {
                ClearSubtitle();
                return;
            }
            lineRoutine = StartCoroutine(PlaySet(waypointLines[index].lines));
        }

        private IEnumerator PlaySet(string[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                subtitleText.text = "";
                yield break;
            }
            foreach (string line in lines)
            {
                subtitleText.text = line;
                float voiceLen = VoiceLinePlayer.PlayLine(line);
                yield return new WaitForSeconds(voiceLen > 0f ? voiceLen + 0.25f : lineDisplayTime);
            }
            subtitleText.text = "";
        }

        private void ClearSubtitle()
        {
            if (lineRoutine != null)
            {
                StopCoroutine(lineRoutine);
                lineRoutine = null;
            }
            if (subtitleText != null) subtitleText.text = "";
        }
    }
}
