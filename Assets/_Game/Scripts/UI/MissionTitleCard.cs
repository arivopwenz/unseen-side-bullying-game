using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace BullyingGame.UI
{
    /// <summary>Kartu judul misi emas ala Bully ("The Candidate" style).
    /// Muncul di tengah layar saat misi dimulai: label kecil + judul besar emas, lalu menghilang.</summary>
    public class MissionTitleCard : MonoBehaviour
    {
        private static MissionTitleCard instance;
        private GameObject root;
        private CanvasGroup group;
        private TextMeshProUGUI labelText;
        private TextMeshProUGUI titleText;
        private Coroutine routine;

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
                if (c.name == "BullyTitleCard") { rootObj = c.gameObject; break; }
            if (rootObj == null) return false;
            var lab = Minimap.FindDeep(rootObj.transform, "Label");
            var tit = Minimap.FindDeep(rootObj.transform, "Title");
            if (lab == null || tit == null) return false;
            var labTmp = lab.GetComponent<TextMeshProUGUI>();
            var titTmp = tit.GetComponent<TextMeshProUGUI>();
            if (labTmp == null || titTmp == null) return false;
            root = rootObj;
            group = rootObj.GetComponent<CanvasGroup>();
            if (group == null) group = rootObj.AddComponent<CanvasGroup>();
            labelText = labTmp;
            titleText = titTmp;
            return true;
        }

        private void BuildProcedural()
        {
            var canvas = GameObject.Find("WalkTalkCanvas");
            if (canvas == null) return;
            var canvasT = canvas.GetComponent<RectTransform>();
            if (canvasT == null) return;

            root = new GameObject("BullyTitleCard", typeof(RectTransform), typeof(CanvasGroup));
            var rootT = root.GetComponent<RectTransform>();
            rootT.SetParent(canvasT, false);
            rootT.anchorMin = new Vector2(0.5f, 0.5f);
            rootT.anchorMax = new Vector2(0.5f, 0.5f);
            rootT.anchoredPosition = new Vector2(0f, 80f);
            rootT.sizeDelta = new Vector2(820f, 140f);
            group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;

            var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
            var stripT = strip.GetComponent<RectTransform>();
            stripT.SetParent(rootT, false);
            stripT.anchorMin = new Vector2(0.5f, 0.5f);
            stripT.anchorMax = new Vector2(0.5f, 0.5f);
            stripT.sizeDelta = new Vector2(820f, 140f);
            var stripI = strip.GetComponent<Image>();
            stripI.sprite = Sprite.Create(Minimap.MakeWhiteTexture(), new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            stripI.color = new Color(0f, 0f, 0f, 0.25f);
            stripI.raycastTarget = false;

            var lab = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var labT = lab.GetComponent<RectTransform>();
            labT.SetParent(rootT, false);
            labT.anchorMin = new Vector2(0.5f, 0.5f);
            labT.anchorMax = new Vector2(0.5f, 0.5f);
            labT.sizeDelta = new Vector2(800f, 30f);
            labT.anchoredPosition = new Vector2(0f, 42f);
            labelText = lab.GetComponent<TextMeshProUGUI>();
            labelText.text = "";
            labelText.fontSize = 20;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            labelText.fontStyle = FontStyles.Bold;
            labelText.raycastTarget = false;

            var tit = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            var titT = tit.GetComponent<RectTransform>();
            titT.SetParent(rootT, false);
            titT.anchorMin = new Vector2(0.5f, 0.5f);
            titT.anchorMax = new Vector2(0.5f, 0.5f);
            titT.sizeDelta = new Vector2(800f, 60f);
            titT.anchoredPosition = new Vector2(0f, -12f);
            titleText = tit.GetComponent<TextMeshProUGUI>();
            titleText.text = "";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.78f, 0f, 1f);
            titleText.fontStyle = FontStyles.Bold;
            titleText.raycastTarget = false;

            var sub = GameObject.Find("WalkTalkSubtitle");
            if (sub != null)
            {
                var st = sub.GetComponent<TMP_Text>();
                if (st != null && st.font != null)
                {
                    labelText.font = st.font;
                    titleText.font = st.font;
                }
            }
            root.SetActive(false);
        }

        private static void Ensure()
        {
            if (instance != null) return;
            var arr = FindObjectsByType<MissionTitleCard>(FindObjectsInactive.Include);
            if (arr.Length > 0) instance = arr[0];
        }

        public static void ShowTitle(string label, string title)
        {
            Ensure();
            if (instance == null || instance.root == null) return;
            instance.labelText.text = label;
            instance.titleText.text = title;
            if (instance.routine != null) instance.StopCoroutine(instance.routine);
            instance.routine = instance.StartCoroutine(instance.PlayCard());
        }

        private IEnumerator PlayCard()
        {
            root.SetActive(true);
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Clamp01(t / 0.4f);
                yield return null;
            }
            group.alpha = 1f;
            yield return new WaitForSeconds(2.2f);
            t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                group.alpha = 1f - Mathf.Clamp01(t / 0.5f);
                yield return null;
            }
            group.alpha = 0f;
            root.SetActive(false);
            routine = null;
        }
    }
}
