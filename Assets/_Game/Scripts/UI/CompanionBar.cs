using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.UI
{
    /// <summary>Bar companion ala Bully (kanan bawah, nama + bar kuning).
    /// Dipakai saat misi escort/lindungi (Bima). Bukan bar darah — ini bar keberanian/kepercayaan.</summary>
    public class CompanionBar : MonoBehaviour
    {
        private static CompanionBar instance;
        private GameObject root;
        private TextMeshProUGUI nameText;
        private RectTransform fillRect;
        private float fullWidth = 260f;
        private float fillHeight = 18f;

        private void Awake()
        {
            instance = this;
            if (TryBindScene()) return;
            BuildProcedural();
        }

        private bool TryBindScene()
        {
            var canvas = GameObject.Find("WalkTalkCanvas");
            if (canvas == null) return false;
            GameObject rootObj = null;
            foreach (Transform c in canvas.transform)
                if (c.name == "BullyCompanionBar") { rootObj = c.gameObject; break; }
            if (rootObj == null) return false;
            var nm = Minimap.FindDeep(rootObj.transform, "Name");
            var fl = Minimap.FindDeep(rootObj.transform, "BarFill");
            var bg = Minimap.FindDeep(rootObj.transform, "BarBg");
            if (nm == null || fl == null) return false;
            var tmp = nm.GetComponent<TextMeshProUGUI>();
            if (tmp == null) return false;
            root = rootObj;
            nameText = tmp;
            fillRect = fl;
            if (bg != null) { fullWidth = bg.sizeDelta.x; fillHeight = bg.sizeDelta.y; }
            return true;
        }

        private void BuildProcedural()
        {
            var canvas = GameObject.Find("WalkTalkCanvas");
            if (canvas == null) return;
            var canvasT = canvas.GetComponent<RectTransform>();
            if (canvasT == null) return;

            root = new GameObject("BullyCompanionBar", typeof(RectTransform));
            var rootT = root.GetComponent<RectTransform>();
            rootT.SetParent(canvasT, false);
            rootT.anchorMin = new Vector2(1f, 0f);
            rootT.anchorMax = new Vector2(1f, 0f);
            rootT.pivot = new Vector2(1f, 0f);
            rootT.anchoredPosition = new Vector2(-24f, 24f);
            rootT.sizeDelta = new Vector2(280f, 56f);

            var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            var nameT = nameObj.GetComponent<RectTransform>();
            nameT.SetParent(rootT, false);
            nameT.anchorMin = new Vector2(0f, 1f);
            nameT.anchorMax = new Vector2(0f, 1f);
            nameT.pivot = new Vector2(0f, 1f);
            nameT.anchoredPosition = new Vector2(0f, 0f);
            nameT.sizeDelta = new Vector2(280f, 26f);
            nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.text = "";
            nameText.fontSize = 18;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.color = Color.white;
            nameText.fontStyle = FontStyles.Bold;
            nameText.raycastTarget = false;

            var bg = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
            var bgT = bg.GetComponent<RectTransform>();
            bgT.SetParent(rootT, false);
            bgT.anchorMin = new Vector2(0f, 0f);
            bgT.anchorMax = new Vector2(0f, 0f);
            bgT.pivot = new Vector2(0f, 0f);
            bgT.anchoredPosition = new Vector2(0f, 0f);
            bgT.sizeDelta = new Vector2(260f, 18f);
            var bgI = bg.GetComponent<Image>();
            bgI.sprite = Sprite.Create(Minimap.MakeWhiteTexture(), new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            bgI.color = new Color(0f, 0f, 0f, 0.6f);
            bgI.raycastTarget = false;

            var fill = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
            fillRect = fill.GetComponent<RectTransform>();
            fillRect.SetParent(bgT, false);
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = new Vector2(0f, 0f);
            fillRect.sizeDelta = new Vector2(260f, 18f);
            var fillI = fill.GetComponent<Image>();
            fillI.sprite = Sprite.Create(Minimap.MakeWhiteTexture(), new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            fillI.color = new Color(1f, 0.85f, 0f, 1f);
            fillI.raycastTarget = false;

            var sub = GameObject.Find("WalkTalkSubtitle");
            if (sub != null)
            {
                var st = sub.GetComponent<TMP_Text>();
                if (st != null && st.font != null) nameText.font = st.font;
            }
            root.SetActive(false);
        }

        private static void Ensure()
        {
            if (instance != null) return;
            var arr = FindObjectsByType<CompanionBar>(FindObjectsInactive.Include);
            if (arr.Length > 0) instance = arr[0];
        }

        public static void SetCompanion(string name, float fill01, bool visible)
        {
            Ensure();
            if (instance == null || instance.root == null) return;
            instance.root.SetActive(visible);
            if (!visible) return;
            instance.nameText.text = name;
            float f = Mathf.Clamp01(fill01);
            instance.fillRect.sizeDelta = new Vector2(instance.fullWidth * f, instance.fillHeight);
            var img = instance.fillRect.GetComponent<Image>();
            if (img != null)
            {
                img.color = f < 0.35f
                    ? Color.Lerp(new Color(0.9f, 0.2f, 0.2f, 1f), new Color(1f, 0.85f, 0f, 1f), f / 0.35f)
                    : new Color(1f, 0.85f, 0f, 1f);
            }
        }
    }
}
