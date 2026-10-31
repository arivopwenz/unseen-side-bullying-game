using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.UI
{
    /// <summary>Banner objective kuning ala Bully (tengah atas, persisten).
    /// Menampilkan objective AKTIF saat ini (satu baris). Menggantikan panel misi A Way Out.</summary>
    public class MissionBanner : MonoBehaviour
    {
        private static MissionBanner instance;
        private GameObject root;
        private TextMeshProUGUI text;

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
                if (c.name == "BullyBanner") { rootObj = c.gameObject; break; }
            if (rootObj == null) return false;
            var txt = Minimap.FindDeep(rootObj.transform, "Text");
            if (txt == null) return false;
            var tmp = txt.GetComponent<TextMeshProUGUI>();
            if (tmp == null) return false;
            root = rootObj;
            text = tmp;
            return true;
        }

        private void BuildProcedural()
        {
            var canvas = GameObject.Find("WalkTalkCanvas");
            if (canvas == null) return;
            var canvasT = canvas.GetComponent<RectTransform>();
            if (canvasT == null) return;

            root = new GameObject("BullyBanner", typeof(RectTransform));
            var rootT = root.GetComponent<RectTransform>();
            rootT.SetParent(canvasT, false);
            rootT.anchorMin = new Vector2(0.5f, 1f);
            rootT.anchorMax = new Vector2(0.5f, 1f);
            rootT.pivot = new Vector2(0.5f, 1f);
            rootT.anchoredPosition = new Vector2(0f, -36f);
            rootT.sizeDelta = new Vector2(760f, 44f);

            var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
            var stripT = strip.GetComponent<RectTransform>();
            stripT.SetParent(rootT, false);
            stripT.anchorMin = new Vector2(0.5f, 0.5f);
            stripT.anchorMax = new Vector2(0.5f, 0.5f);
            stripT.sizeDelta = new Vector2(760f, 44f);
            var stripI = strip.GetComponent<Image>();
            stripI.sprite = Sprite.Create(Minimap.MakeWhiteTexture(), new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            stripI.color = new Color(0f, 0f, 0f, 0.35f);
            stripI.raycastTarget = false;

            var txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var txtT = txt.GetComponent<RectTransform>();
            txtT.SetParent(rootT, false);
            txtT.anchorMin = new Vector2(0.5f, 0.5f);
            txtT.anchorMax = new Vector2(0.5f, 0.5f);
            txtT.sizeDelta = new Vector2(740f, 40f);
            text = txt.GetComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.9f, 0f, 1f);
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            var sub = GameObject.Find("WalkTalkSubtitle");
            if (sub != null)
            {
                var st = sub.GetComponent<TMP_Text>();
                if (st != null && st.font != null) text.font = st.font;
            }
            root.SetActive(false);
        }

        private static void Ensure()
        {
            if (instance != null) return;
            var arr = FindObjectsByType<MissionBanner>(FindObjectsInactive.Include);
            if (arr.Length > 0) instance = arr[0];
        }

        public static void SetObjective(string objective)
        {
            Ensure();
            if (instance == null || instance.root == null || instance.text == null) return;
            instance.text.text = objective;
            instance.root.SetActive(true);
        }

        public static void ClearObjective()
        {
            Ensure();
            if (instance == null || instance.root == null) return;
            instance.root.SetActive(false);
        }
    }
}
