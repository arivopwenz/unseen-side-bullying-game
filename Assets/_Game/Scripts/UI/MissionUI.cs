using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.UI
{
    /// <summary>Panel misi kanan atas ala A Way Out: simple, rapi, jelas.
    /// Judul + daftar objective (selesai = x + coret, aktif = &gt;, berikut = -).</summary>
    public class MissionUI : MonoBehaviour
    {
        public static MissionUI Instance { get; private set; }

        /// <summary>UI A Way Out DINONAKTIFKAN — diganti HUD Bully (2026-10-26).
        /// Set false untuk mengembalikan (dan kembalikan pemanggilan driver).</summary>
        public static bool Suppress = true;

        private static string pendingTitle = "";
        private static string[] pendingItems = new string[0];
        private static int pendingCurrent = 0;
        private static bool[] pendingDone = new bool[0];
        private static bool hasPending = false;

        private GameObject root;
        private RectTransform panelRect;
        private TMP_Text titleText;
        private readonly System.Collections.Generic.List<TMP_Text> lineTexts =
            new System.Collections.Generic.List<TMP_Text>();
        private TMP_FontAsset font;

        public static void SetObjectives(string title, string[] items, int currentIndex, bool[] done)
        {
            if (Suppress) return;
            pendingTitle = title;
            pendingItems = items ?? new string[0];
            pendingCurrent = currentIndex;
            pendingDone = done ?? new bool[0];
            hasPending = true;
            if (Instance != null) Instance.Apply();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (Suppress) return;

            Canvas canvas = null;
            var wt = GameObject.Find("WalkTalkCanvas");
            if (wt != null) canvas = wt.GetComponent<Canvas>();
            if (canvas == null)
            {
                var all = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (all.Length > 0) canvas = all[0];
            }
            if (canvas == null) { Debug.LogWarning("[MissionUI] Canvas tidak ditemukan."); return; }

            font = FindSubtitleFont();

            root = new GameObject("MissionUI", typeof(RectTransform));
            var rt = root.GetComponent<RectTransform>();
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-24f, -24f);
            rt.sizeDelta = new Vector2(360f, 120f);
            panelRect = rt;

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);
            bg.raycastTarget = false;

            titleText = MakeText(root.transform, "MissionTitle", new Vector2(16f, -8f), 22, true);
            titleText.alignment = TextAlignmentOptions.Left;

            if (hasPending) Apply();
        }

        private void Apply()
        {
            if (titleText == null) return;
            titleText.text = pendingTitle;
            int n = pendingItems.Length;
            while (lineTexts.Count < n)
            {
                var t = MakeText(root.transform, "MissionLine" + lineTexts.Count,
                    new Vector2(16f, -40f - lineTexts.Count * 26f), 20, false);
                t.alignment = TextAlignmentOptions.Left;
                lineTexts.Add(t);
            }
            for (int i = 0; i < lineTexts.Count; i++)
            {
                var t = lineTexts[i];
                if (i < n)
                {
                    t.gameObject.SetActive(true);
                    bool done = i < pendingDone.Length && pendingDone[i];
                    string body = pendingItems[i];
                    if (done)
                    {
                        t.text = "x <s>" + body + "</s>";
                        t.color = new Color(0.75f, 0.75f, 0.75f, 1f);
                    }
                    else if (i == pendingCurrent)
                    {
                        t.text = "> " + body;
                        t.color = Color.white;
                    }
                    else
                    {
                        t.text = "- " + body;
                        t.color = new Color(0.75f, 0.75f, 0.75f, 1f);
                    }
                    var lrt = t.GetComponent<RectTransform>();
                    lrt.anchoredPosition = new Vector2(16f, -40f - i * 26f);
                }
                else
                {
                    t.gameObject.SetActive(false);
                }
            }
            panelRect.sizeDelta = new Vector2(360f, 48f + n * 26f);
        }

        private TMP_FontAsset FindSubtitleFont()
        {
            var sub = GameObject.Find("WalkTalkSubtitle");
            if (sub != null)
            {
                var t = sub.GetComponent<TMP_Text>();
                if (t != null && t.font != null) return t.font;
            }
            var fonts = FindObjectsByType<TMP_FontAsset>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return fonts.Length > 0 ? fonts[0] : null;
        }

        private TMP_Text MakeText(Transform parent, string name, Vector2 anchoredPos, int size, bool bold)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(330f, 30f);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Left;
            txt.raycastTarget = false;
            return txt;
        }
    }
}
