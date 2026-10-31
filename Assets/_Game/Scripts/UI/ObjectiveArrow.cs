using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.UI
{
    /// <summary>Panah petunjuk arah di tengah atas layar. Menunjuk ke target
    /// objective aktif (relatif terhadap arah kamera) + jaraknya.</summary>
    public class ObjectiveArrow : MonoBehaviour
    {
        public static ObjectiveArrow Instance { get; private set; }

        /// <summary>UI A Way Out DINONAKTIFKAN — diganti HUD Bully (2026-10-26).
        /// Set false untuk mengembalikan (dan kembalikan pemanggilan driver).</summary>
        public static bool Suppress = true;

        private static Vector3? target;
        private static string label = "";
        private static bool visible = true;

        private Image arrowImage;
        private TMP_Text distanceText;
        private TMP_Text labelText;
        private GameObject root;
        private Transform player;
        private UnityEngine.Camera mainCam;

        public static void SetTarget(Vector3 worldPos, string labelText)
        {
            if (Suppress) return;
            target = worldPos;
            label = labelText;
        }

        public static void ClearTarget() { if (Suppress) return; target = null; }

        public static void SetVisible(bool v) { if (Suppress) return; visible = v; }

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
            if (canvas == null) { Debug.LogWarning("[ObjectiveArrow] Canvas tidak ditemukan."); return; }

            root = new GameObject("ObjectiveArrow", typeof(RectTransform));
            var rt = root.GetComponent<RectTransform>();
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -64f);
            rt.sizeDelta = new Vector2(160f, 100f);

            var arrowGO = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
            arrowGO.transform.SetParent(root.transform, false);
            var art = arrowGO.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0.5f, 1f);
            art.anchorMax = new Vector2(0.5f, 1f);
            art.pivot = new Vector2(0.5f, 1f);
            art.anchoredPosition = Vector2.zero;
            art.sizeDelta = new Vector2(44f, 44f);
            arrowImage = arrowGO.GetComponent<Image>();
            arrowImage.sprite = MakeTriangleSprite();
            arrowImage.color = Color.white;
            arrowImage.raycastTarget = false;

            var font = FindSubtitleFont();
            distanceText = MakeText(root.transform, "Distance", new Vector2(0f, -48f), 26, font);
            labelText = MakeText(root.transform, "Label", new Vector2(0f, -74f), 20, font);

            var po = GameObject.FindGameObjectWithTag("Player");
            if (po != null) player = po.transform;
            mainCam = UnityEngine.Camera.main;
        }

        private void Update()
        {
            bool show = visible && target.HasValue && player != null && arrowImage != null;
            if (root != null) root.SetActive(show);
            if (!show) return;
            if (mainCam == null) mainCam = UnityEngine.Camera.main;

            Vector3 to = target.Value - player.position;
            to.y = 0f;
            float dist = to.magnitude;
            Vector3 camFwd = mainCam != null ? mainCam.transform.forward : Vector3.forward;
            camFwd.y = 0f;
            if (camFwd.sqrMagnitude < 0.001f) camFwd = Vector3.forward;
            camFwd.Normalize();
            float angle = 0f;
            if (dist > 0.01f) angle = Vector3.SignedAngle(camFwd, to.normalized, Vector3.up);
            arrowImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            distanceText.text = Mathf.RoundToInt(dist) + " m";
            labelText.text = label;
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

        private TMP_Text MakeText(Transform parent, string name, Vector2 anchoredPos, int size, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(200f, 30f);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.font = font;
            txt.fontSize = size;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            txt.raycastTarget = false;
            return txt;
        }

        private Sprite MakeTriangleSprite()
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var clear = new Color(0f, 0f, 0f, 0f);
            var px = new Color[s * s];
            for (int i = 0; i < px.Length; i++) px[i] = clear;
            for (int y = 0; y < s; y++)
            {
                float t = (float)y / (s - 1);
                float halfW = (1f - t) * (s / 2f);
                int cx = s / 2;
                int x0 = Mathf.RoundToInt(cx - halfW);
                int x1 = Mathf.RoundToInt(cx + halfW);
                for (int x = x0; x <= x1; x++)
                {
                    if (x >= 0 && x < s) px[(s - 1 - y) * s + x] = Color.white;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f));
        }
    }
}
