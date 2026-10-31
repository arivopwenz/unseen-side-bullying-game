using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.UI
{
    /// <summary>Jam sekolah ala Bully (kiri atas): jam analog + teks waktu + label jadwal.
    /// Waktu berjalan pelan (1 menit game per 10 detik nyata); label jadwal di-set driver per fase.</summary>
    public class ClockHUD : MonoBehaviour
    {
        private static ClockHUD instance;
        private TextMeshProUGUI timeText;
        private TextMeshProUGUI scheduleText;
        private RectTransform hourHand;
        private RectTransform minHand;
        private float gameMinutes = 420f;

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
            RectTransform root = null;
            foreach (Transform c in canvas.transform)
                if (c.name == "BullyClock") { root = c.GetComponent<RectTransform>(); break; }
            if (root == null) return false;
            var tt = Minimap.FindDeep(root, "Time");
            var st = Minimap.FindDeep(root, "Schedule");
            var hh = Minimap.FindDeep(root, "HourHand");
            var mh = Minimap.FindDeep(root, "MinHand");
            if (tt == null || st == null || hh == null || mh == null) return false;
            timeText = tt.GetComponent<TextMeshProUGUI>();
            scheduleText = st.GetComponent<TextMeshProUGUI>();
            hourHand = hh;
            minHand = mh;
            return timeText != null && scheduleText != null;
        }

        private void BuildProcedural()
        {
            var canvas = GameObject.Find("WalkTalkCanvas");
            if (canvas == null) return;
            var canvasT = canvas.GetComponent<RectTransform>();
            if (canvasT == null) return;

            var root = new GameObject("BullyClock", typeof(RectTransform));
            var rootT = root.GetComponent<RectTransform>();
            rootT.SetParent(canvasT, false);
            rootT.anchorMin = new Vector2(0f, 1f);
            rootT.anchorMax = new Vector2(0f, 1f);
            rootT.pivot = new Vector2(0f, 1f);
            rootT.anchoredPosition = new Vector2(24f, -24f);
            rootT.sizeDelta = new Vector2(190f, 84f);

            var circle = Sprite.Create(Minimap.MakeCircleTexture(128), new Rect(0f, 0f, 128f, 128f), new Vector2(0.5f, 0.5f));
            var face = new GameObject("Face", typeof(RectTransform), typeof(Image));
            var faceT = face.GetComponent<RectTransform>();
            faceT.SetParent(rootT, false);
            faceT.anchorMin = new Vector2(0f, 0.5f);
            faceT.anchorMax = new Vector2(0f, 0.5f);
            faceT.pivot = new Vector2(0f, 0.5f);
            faceT.anchoredPosition = new Vector2(0f, 0f);
            faceT.sizeDelta = new Vector2(64f, 64f);
            var faceI = face.GetComponent<Image>();
            faceI.sprite = circle;
            faceI.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            faceI.raycastTarget = false;

            hourHand = MakeHand(faceT, 4f, 24f);
            minHand = MakeHand(faceT, 3f, 30f);

            var dot = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            var dotT = dot.GetComponent<RectTransform>();
            dotT.SetParent(faceT, false);
            dotT.anchorMin = new Vector2(0.5f, 0.5f);
            dotT.anchorMax = new Vector2(0.5f, 0.5f);
            dotT.sizeDelta = new Vector2(8f, 8f);
            var dotI = dot.GetComponent<Image>();
            dotI.sprite = circle;
            dotI.color = Color.white;
            dotI.raycastTarget = false;

            var timeObj = new GameObject("Time", typeof(RectTransform), typeof(TextMeshProUGUI));
            var timeT = timeObj.GetComponent<RectTransform>();
            timeT.SetParent(rootT, false);
            timeT.anchorMin = new Vector2(0f, 1f);
            timeT.anchorMax = new Vector2(0f, 1f);
            timeT.pivot = new Vector2(0f, 1f);
            timeT.anchoredPosition = new Vector2(72f, 0f);
            timeT.sizeDelta = new Vector2(110f, 28f);
            timeText = timeObj.GetComponent<TextMeshProUGUI>();
            timeText.text = "07:00";
            timeText.fontSize = 20;
            timeText.alignment = TextAlignmentOptions.Left;
            timeText.color = Color.white;
            timeText.fontStyle = FontStyles.Bold;
            timeText.raycastTarget = false;

            var schObj = new GameObject("Schedule", typeof(RectTransform), typeof(TextMeshProUGUI));
            var schT = schObj.GetComponent<RectTransform>();
            schT.SetParent(rootT, false);
            schT.anchorMin = new Vector2(0f, 1f);
            schT.anchorMax = new Vector2(0f, 1f);
            schT.pivot = new Vector2(0f, 1f);
            schT.anchoredPosition = new Vector2(72f, -30f);
            schT.sizeDelta = new Vector2(110f, 24f);
            scheduleText = schObj.GetComponent<TextMeshProUGUI>();
            scheduleText.text = "Pagi";
            scheduleText.fontSize = 16;
            scheduleText.alignment = TextAlignmentOptions.Left;
            scheduleText.color = Color.white;
            scheduleText.raycastTarget = false;

            var sub = GameObject.Find("WalkTalkSubtitle");
            if (sub != null)
            {
                var st = sub.GetComponent<TMP_Text>();
                if (st != null && st.font != null)
                {
                    timeText.font = st.font;
                    scheduleText.font = st.font;
                }
            }
        }

        private RectTransform MakeHand(RectTransform faceT, float w, float len)
        {
            var o = new GameObject("Hand", typeof(RectTransform), typeof(Image));
            var t = o.GetComponent<RectTransform>();
            t.SetParent(faceT, false);
            t.anchorMin = new Vector2(0.5f, 0.5f);
            t.anchorMax = new Vector2(0.5f, 0.5f);
            t.pivot = new Vector2(0.5f, 0f);
            t.anchoredPosition = Vector2.zero;
            t.sizeDelta = new Vector2(w, len);
            var img = o.GetComponent<Image>();
            img.sprite = Sprite.Create(Minimap.MakeWhiteTexture(), new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            img.color = Color.white;
            img.raycastTarget = false;
            return t;
        }

        private static void Ensure()
        {
            if (instance != null) return;
            var arr = FindObjectsByType<ClockHUD>(FindObjectsInactive.Include);
            if (arr.Length > 0) instance = arr[0];
        }

        public static void SetSchedule(string label)
        {
            Ensure();
            if (instance == null || instance.scheduleText == null) return;
            instance.scheduleText.text = label;
        }

        public static void SetTime(int hour, int minute)
        {
            Ensure();
            if (instance == null) return;
            instance.gameMinutes = hour * 60f + minute;
        }

        private void Update()
        {
            gameMinutes += Time.deltaTime * 0.1f;
            int total = Mathf.FloorToInt(gameMinutes);
            int h = (total / 60) % 24;
            int m = total % 60;
            if (timeText != null) timeText.text = h.ToString("00") + ":" + m.ToString("00");
            if (hourHand != null)
            {
                float ha = ((h % 12) + m / 60f) / 12f * 360f;
                hourHand.localEulerAngles = new Vector3(0f, 0f, -ha);
            }
            if (minHand != null)
            {
                float ma = m / 60f * 360f;
                minHand.localEulerAngles = new Vector3(0f, 0f, -ma);
            }
        }
    }
}
