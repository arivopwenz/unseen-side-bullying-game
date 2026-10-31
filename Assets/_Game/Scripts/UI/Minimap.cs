using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.UI
{
    /// <summary>Minimap bulat ala Bully (kanan atas, north-up).
    /// Menampilkan: panah pemain (putih), Bima (hijau), Guru BK saat patroli (merah),
    /// marker misi (kuning "!"), dan skema gedung sekolah. Menggantikan panah tengah-atas A Way Out.</summary>
    public class Minimap : MonoBehaviour
    {
        private static Minimap instance;

        // Batas dunia yang dipetakan (gerbang selatan s.d. ruang BK).
        private const float minX = -52f, maxX = 28f, minZ = -58f, maxZ = 28f, R = 88f;

        private RectTransform content;
        private RectTransform playerArrow;
        private RectTransform bimaDot;
        private RectTransform bkDot;
        private RectTransform missionDot;
        private Transform playerT;
        private Transform bimaT;
        private Transform bkT;
        private Vector3 missionTarget;
        private bool hasMissionTarget;
        private float dotTimer;

        public static Texture2D MakeCircleTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float c = size / 2f;
            float r = size / 2f - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c, dy = y - c;
                    tex.SetPixel(x, y, (dx * dx + dy * dy <= r * r) ? Color.white : Color.clear);
                }
            tex.Apply();
            return tex;
        }

        public static Texture2D MakeWhiteTexture()
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return tex;
        }

        public static Texture2D MakeTriangleTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 a = new Vector2(0.5f, 0.95f), b = new Vector2(0.12f, 0.08f), c = new Vector2(0.88f, 0.08f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2((x + 0.5f) / size, (y + 0.5f) / size);
                    float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
                    float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
                    float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
                    bool inside = (d1 >= 0f && d2 >= 0f && d3 >= 0f) || (d1 <= 0f && d2 <= 0f && d3 <= 0f);
                    tex.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            tex.Apply();
            return tex;
        }

        private static Sprite ToSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>Cari descendant UI berdasarkan nama (termasuk yang nonaktif).
        /// Dipakai semua HUD agar bisa memakai objek scene buatan manual (bukan hasil kode).</summary>
        public static RectTransform FindDeep(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform c in parent)
            {
                if (c.name == name) return c.GetComponent<RectTransform>();
                var r = FindDeep(c, name);
                if (r != null) return r;
            }
            return null;
        }

        /// <summary>Pakai objek scene "BullyMinimap" bila sudah ada (dibuat manual di Editor).
        /// Bila tidak ada, kembalikan false agar Awake membangunnya via kode (fallback lama).</summary>
        private bool TryBindScene()
        {
            var canvas = GameObject.Find("WalkTalkCanvas");
            if (canvas == null) return false;
            RectTransform root = null;
            foreach (Transform c in canvas.transform)
                if (c.name == "BullyMinimap") { root = c.GetComponent<RectTransform>(); break; }
            if (root == null) return false;
            var ct = FindDeep(root, "Content");
            var pa = FindDeep(root, "PlayerArrow");
            var bd = FindDeep(root, "BimaDot");
            var kd = FindDeep(root, "BKDot");
            var md = FindDeep(root, "MissionDot");
            if (ct == null || pa == null || bd == null || kd == null || md == null) return false;
            content = ct;
            playerArrow = pa;
            bimaDot = bd;
            bkDot = kd;
            missionDot = md;
            return true;
        }

        private static TMP_FontAsset GetFont()
        {
            var sub = GameObject.Find("WalkTalkSubtitle");
            if (sub != null)
            {
                var t = sub.GetComponent<TMP_Text>();
                if (t != null && t.font != null) return t.font;
            }
            return null;
        }

        private void Awake()
        {
            instance = this;
            if (TryBindScene()) return;
            BuildProcedural();
        }

        private void BuildProcedural()
        {
            var canvas = GameObject.Find("WalkTalkCanvas");
            if (canvas == null) return;
            var canvasT = canvas.GetComponent<RectTransform>();
            if (canvasT == null) return;

            var circle = ToSprite(MakeCircleTexture(256));
            var white = ToSprite(MakeWhiteTexture());
            var tri = ToSprite(MakeTriangleTexture(64));

            var root = new GameObject("BullyMinimap", typeof(RectTransform));
            var rootT = root.GetComponent<RectTransform>();
            rootT.SetParent(canvasT, false);
            rootT.anchorMin = new Vector2(1f, 1f);
            rootT.anchorMax = new Vector2(1f, 1f);
            rootT.pivot = new Vector2(1f, 1f);
            rootT.anchoredPosition = new Vector2(-24f, -24f);
            rootT.sizeDelta = new Vector2(204f, 204f);

            var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var bgT = bg.GetComponent<RectTransform>();
            bgT.SetParent(rootT, false);
            bgT.anchorMin = new Vector2(0.5f, 0.5f);
            bgT.anchorMax = new Vector2(0.5f, 0.5f);
            bgT.sizeDelta = new Vector2(204f, 204f);
            var bgI = bg.GetComponent<Image>();
            bgI.sprite = circle;
            bgI.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
            bgI.raycastTarget = false;

            var maskObj = new GameObject("Mask", typeof(RectTransform), typeof(Image), typeof(Mask));
            var maskT = maskObj.GetComponent<RectTransform>();
            maskT.SetParent(rootT, false);
            maskT.anchorMin = new Vector2(0.5f, 0.5f);
            maskT.anchorMax = new Vector2(0.5f, 0.5f);
            maskT.sizeDelta = new Vector2(200f, 200f);
            var maskI = maskObj.GetComponent<Image>();
            maskI.sprite = circle;
            maskI.color = Color.white;
            maskI.raycastTarget = false;
            maskObj.GetComponent<Mask>().showMaskGraphic = false;

            var contentObj = new GameObject("Content", typeof(RectTransform));
            content = contentObj.GetComponent<RectTransform>();
            content.SetParent(maskT, false);
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(200f, 200f);
            content.anchoredPosition = Vector2.zero;

            var inner = new GameObject("InnerBg", typeof(RectTransform), typeof(Image));
            var innerT = inner.GetComponent<RectTransform>();
            innerT.SetParent(content, false);
            innerT.anchorMin = new Vector2(0.5f, 0.5f);
            innerT.anchorMax = new Vector2(0.5f, 0.5f);
            innerT.sizeDelta = new Vector2(200f, 200f);
            var innerI = inner.GetComponent<Image>();
            innerI.sprite = circle;
            innerI.color = new Color(0.08f, 0.1f, 0.08f, 0.95f);
            innerI.raycastTarget = false;

            // Skema gedung (perkiraan dari layout dunia).
            AddRect(-30f, 5f, 5f, 20f, new Color(0.22f, 0.22f, 0.22f, 1f), white);   // gedung kelas
            AddRect(-32f, 12f, -20f, 5f, new Color(0.16f, 0.28f, 0.16f, 1f), white); // lapangan
            AddRect(-46f, -33f, -5f, 3f, new Color(0.28f, 0.22f, 0.16f, 1f), white); // kantin
            AddRect(-10f, -4f, -52f, -47f, new Color(0.22f, 0.22f, 0.22f, 1f), white); // gerbang
            AddRect(18f, 24f, 14f, 20f, new Color(0.22f, 0.22f, 0.22f, 1f), white);   // ruang BK

            bimaDot = MakeDot("BimaDot", new Color(0.25f, 0.75f, 0.25f, 1f), 14f, circle);
            bkDot = MakeDot("BKDot", new Color(0.9f, 0.25f, 0.25f, 1f), 14f, circle);
            missionDot = MakeDot("MissionDot", new Color(1f, 0.85f, 0f, 1f), 22f, circle);
            // Tanda "!" di atas dot misi.
            var mark = new GameObject("Mark", typeof(RectTransform), typeof(TextMeshProUGUI));
            var markT = mark.GetComponent<RectTransform>();
            markT.SetParent(missionDot, false);
            markT.anchorMin = new Vector2(0.5f, 0.5f);
            markT.anchorMax = new Vector2(0.5f, 0.5f);
            markT.sizeDelta = new Vector2(22f, 22f);
            markT.anchoredPosition = Vector2.zero;
            var markTMP = mark.GetComponent<TextMeshProUGUI>();
            markTMP.text = "!";
            markTMP.fontSize = 18;
            markTMP.alignment = TextAlignmentOptions.Center;
            markTMP.color = Color.black;
            markTMP.fontStyle = FontStyles.Bold;
            markTMP.raycastTarget = false;
            var f = GetFont();
            if (f != null) markTMP.font = f;

            playerArrow = new GameObject("PlayerArrow", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            playerArrow.SetParent(content, false);
            playerArrow.anchorMin = new Vector2(0.5f, 0.5f);
            playerArrow.anchorMax = new Vector2(0.5f, 0.5f);
            playerArrow.sizeDelta = new Vector2(18f, 18f);
            var paI = playerArrow.GetComponent<Image>();
            paI.sprite = tri;
            paI.color = Color.white;
            paI.raycastTarget = false;

            // Huruf N (north-up, statis di atas minimap).
            var nObj = new GameObject("North", typeof(RectTransform), typeof(TextMeshProUGUI));
            var nT = nObj.GetComponent<RectTransform>();
            nT.SetParent(rootT, false);
            nT.anchorMin = new Vector2(0.5f, 1f);
            nT.anchorMax = new Vector2(0.5f, 1f);
            nT.sizeDelta = new Vector2(24f, 24f);
            nT.anchoredPosition = new Vector2(0f, 8f);
            var nTMP = nObj.GetComponent<TextMeshProUGUI>();
            nTMP.text = "N";
            nTMP.fontSize = 18;
            nTMP.alignment = TextAlignmentOptions.Center;
            nTMP.color = Color.white;
            nTMP.fontStyle = FontStyles.Bold;
            nTMP.raycastTarget = false;
            if (f != null) nTMP.font = f;
        }

        private void AddRect(float x0, float x1, float z0, float z1, Color color, Sprite white)
        {
            var o = new GameObject("Schematic", typeof(RectTransform), typeof(Image));
            var t = o.GetComponent<RectTransform>();
            t.SetParent(content, false);
            t.anchorMin = new Vector2(0.5f, 0.5f);
            t.anchorMax = new Vector2(0.5f, 0.5f);
            Vector2 c0 = WorldToMap(new Vector3(x0, 0f, z0));
            Vector2 c1 = WorldToMap(new Vector3(x1, 0f, z1));
            t.anchoredPosition = new Vector2((c0.x + c1.x) / 2f, (c0.y + c1.y) / 2f);
            t.sizeDelta = new Vector2(Mathf.Abs(c1.x - c0.x), Mathf.Abs(c1.y - c0.y));
            var img = o.GetComponent<Image>();
            img.sprite = white;
            img.color = color;
            img.raycastTarget = false;
        }

        private RectTransform MakeDot(string name, Color color, float size, Sprite circle)
        {
            var o = new GameObject(name, typeof(RectTransform), typeof(Image));
            var t = o.GetComponent<RectTransform>();
            t.SetParent(content, false);
            t.anchorMin = new Vector2(0.5f, 0.5f);
            t.anchorMax = new Vector2(0.5f, 0.5f);
            t.sizeDelta = new Vector2(size, size);
            var img = o.GetComponent<Image>();
            img.sprite = circle;
            img.color = color;
            img.raycastTarget = false;
            return t;
        }

        private Vector2 WorldToMap(Vector3 world)
        {
            float u = Mathf.Clamp01((world.x - minX) / (maxX - minX));
            float v = Mathf.Clamp01((world.z - minZ) / (maxZ - minZ));
            Vector2 p = new Vector2((u - 0.5f) * 2f * R, (v - 0.5f) * 2f * R);
            if (p.magnitude > R) p = p.normalized * R;
            return p;
        }

        private static void Ensure()
        {
            if (instance != null) return;
            var arr = FindObjectsByType<Minimap>(FindObjectsInactive.Include);
            if (arr.Length > 0) instance = arr[0];
        }

        public static void SetMissionTarget(Vector3 worldPos)
        {
            Ensure();
            if (instance == null) return;
            instance.missionTarget = worldPos;
            instance.hasMissionTarget = true;
        }

        public static void ClearMissionTarget()
        {
            Ensure();
            if (instance == null) return;
            instance.hasMissionTarget = false;
        }

        private void Update()
        {
            if (content == null) return;
            if (playerT == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerT = p.transform;
            }
            if (bimaT == null)
            {
                var b = GameObject.Find("Bima_Guide");
                if (b != null) bimaT = b.transform;
            }
            if (bkT == null)
            {
                var k = GameObject.Find("GuruBK_Placeholder");
                if (k != null) bkT = k.transform;
            }
            dotTimer += Time.deltaTime;
            if (dotTimer < 0.1f) return;
            dotTimer = 0f;

            if (playerT != null && playerArrow != null)
            {
                playerArrow.anchoredPosition = WorldToMap(playerT.position);
                playerArrow.localEulerAngles = new Vector3(0f, 0f, -playerT.eulerAngles.y);
                playerArrow.gameObject.SetActive(true);
            }
            if (bimaT != null && bimaDot != null)
            {
                bimaDot.anchoredPosition = WorldToMap(bimaT.position);
                bimaDot.gameObject.SetActive(bimaT.gameObject.activeSelf);
            }
            if (bkT != null && bkDot != null)
            {
                bkDot.anchoredPosition = WorldToMap(bkT.position);
                bkDot.gameObject.SetActive(bkT.gameObject.activeSelf);
            }
            if (missionDot != null)
            {
                missionDot.gameObject.SetActive(hasMissionTarget);
                if (hasMissionTarget)
                {
                    missionDot.anchoredPosition = WorldToMap(missionTarget);
                    float s = 1f + 0.2f * Mathf.Sin(Time.time * 5f);
                    missionDot.localScale = new Vector3(s, s, 1f);
                }
            }
        }
    }
}
