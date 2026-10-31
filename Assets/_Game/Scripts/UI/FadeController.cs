using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BullyingGame.UI
{
    /// <summary>Fade layar penuh (hitam) untuk transisi cutscene. Dipakai handoff
    /// kelas (fade out -&gt; Timeline -&gt; fade in) dan reset ketahuan Guru BK.</summary>
    public class FadeController : MonoBehaviour
    {
        public static FadeController Instance { get; private set; }

        private Image fadeImage;
        private Coroutine running;

        public static void FadeOut(float dur) { if (Instance != null) Instance.StartFade(0f, 1f, dur, null); }
        public static void FadeIn(float dur) { if (Instance != null) Instance.StartFade(1f, 0f, dur, null); }

        public static void FadeOutIn(float outDur, float hold, float inDur, System.Action onBlack = null)
        {
            if (Instance != null) Instance.StartCoroutine(Instance.OutIn(outDur, hold, inDur, onBlack));
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            var canvasGO = new GameObject("FadeCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            DontDestroyOnLoad(canvasGO);

            var imgGO = new GameObject("FadeImage", typeof(RectTransform), typeof(Image));
            imgGO.transform.SetParent(canvasGO.transform, false);
            var rt = imgGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            fadeImage = imgGO.GetComponent<Image>();
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
        }

        private void StartFade(float from, float to, float dur, System.Action done)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Anim(from, to, dur, done));
        }

        private IEnumerator OutIn(float outDur, float hold, float inDur, System.Action onBlack)
        {
            yield return Anim(0f, 1f, outDur, null);
            if (onBlack != null) onBlack();
            if (hold > 0f) yield return new WaitForSeconds(hold);
            yield return Anim(1f, 0f, inDur, null);
        }

        private IEnumerator Anim(float from, float to, float dur, System.Action done)
        {
            if (dur <= 0f)
            {
                fadeImage.color = new Color(0f, 0f, 0f, to);
                if (done != null) done();
                yield break;
            }
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                fadeImage.color = new Color(0f, 0f, 0f, a);
                yield return null;
            }
            fadeImage.color = new Color(0f, 0f, 0f, to);
            if (done != null) done();
        }
    }
}
