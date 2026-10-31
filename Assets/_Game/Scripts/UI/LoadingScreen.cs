using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BullyingGame.UI
{
    /// <summary>
    /// Layar loading MAKSIMAL antar scene (DontDestroyOnLoad, singleton).
    /// Tampil: fade halus + latar gradient prosedural (atau artwork bila Background Art diisi)
    /// + vignette + judul UNSEEN SIDE (bayangan) + progress bar ASLI (dari LoadSceneAsync)
    /// + persen + spinner berputar + teks Loading... beranimasi. Min tampil 1.8 detik supaya
    /// gambar/progress sempat terlihat, bukan sekadar kedip.
    /// Pakai: LoadingScreen.LoadSceneWithLoading("NamaScene"). Bila Instance tidak ada
    /// (scene dibuka langsung tanpa lewat Level01), fallback ke LoadScene biasa.
    /// Gambar: isi Background Art di Inspector (LoadingScreen di Level01) dengan Sprite kamu.
    /// Revert: hapus GO LoadingScreen dari Level01 + kembalikan LoadScene biasa di
    /// YellowCircleTrigger & CutsceneAutoReturn.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance;
        [Header("Gambar latar (kosong = gradient prosedural)")]
        public Sprite backgroundArt;
        [Header("Durasi")]
        public float fadeDuration = 0.5f;
        public float minDisplayTime = 1.8f;
        private CanvasGroup canvasGroup;
        private Image backgroundImage;
        private Image vignetteImage;
        private Text loadingText;
        private Image progressFill;
        private Text percentText;
        private RectTransform spinnerRect;
        private bool isLoading;
        private Coroutine dotsRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            foreach (var img in GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject.name == "Background") backgroundImage = img;
                if (img.gameObject.name == "Vignette") vignetteImage = img;
                if (img.gameObject.name == "ProgressFill") progressFill = img;
            }
            foreach (var txt in GetComponentsInChildren<Text>(true))
            {
                if (txt.gameObject.name == "LoadingText") loadingText = txt;
                if (txt.gameObject.name == "PercentText") percentText = txt;
            }
            foreach (var rt in GetComponentsInChildren<RectTransform>(true))
                if (rt.gameObject.name == "Spinner") spinnerRect = rt;
            if (backgroundImage != null)
            {
                if (backgroundArt != null) { backgroundImage.sprite = backgroundArt; backgroundImage.color = Color.white; }
                else { backgroundImage.sprite = MakeGradientSprite(); backgroundImage.color = Color.white; }
            }
            if (vignetteImage != null) { vignetteImage.sprite = MakeVignetteSprite(); vignetteImage.color = Color.white; }
            if (progressFill != null) progressFill.fillAmount = 0f;
            if (percentText != null) percentText.text = "0%";
            if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; canvasGroup.interactable = false; }
        }

        private void Update()
        {
            if (isLoading && spinnerRect != null) spinnerRect.Rotate(0f, 0f, -180f * Time.unscaledDeltaTime);
        }

        private Sprite MakeGradientSprite()
        {
            int w = 2, h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color top = new Color(0.10f, 0.13f, 0.20f, 1f);
            Color bottom = new Color(0.02f, 0.02f, 0.03f, 1f);
            for (int y = 0; y < h; y++)
            {
                Color c = Color.Lerp(bottom, top, (float)y / (h - 1));
                for (int x = 0; x < w; x++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, w, h), new Vector2(0.5f, 0.5f));
        }

        private Sprite MakeVignetteSprite()
        {
            int s = 128;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float dx = (x / (float)(s - 1)) * 2f - 1f;
                    float dy = (y / (float)(s - 1)) * 2f - 1f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01((dist - 0.5f) / 0.7f) * 0.65f;
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, a));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f));
        }

        public static void LoadSceneWithLoading(string sceneName)
        {
            if (Instance != null) Instance.StartCoroutine(Instance.LoadRoutine(sceneName));
            else SceneManager.LoadScene(sceneName);
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            isLoading = true;
            if (canvasGroup != null) yield return Fade(1f);
            if (loadingText != null) dotsRoutine = StartCoroutine(AnimateDots());
            float start = Time.realtimeSinceStartup;
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null)
            {
                while (!op.isDone)
                {
                    float p = Mathf.Clamp01(op.progress / 0.9f);
                    if (progressFill != null) progressFill.fillAmount = p;
                    if (percentText != null) percentText.text = (p * 100f).ToString("0") + "%";
                    yield return null;
                }
                if (progressFill != null) progressFill.fillAmount = 1f;
                if (percentText != null) percentText.text = "100%";
            }
            else SceneManager.LoadScene(sceneName);
            float elapsed = Time.realtimeSinceStartup - start;
            if (elapsed < minDisplayTime) yield return new WaitForSecondsRealtime(minDisplayTime - elapsed);
            if (dotsRoutine != null) StopCoroutine(dotsRoutine);
            if (canvasGroup != null) yield return Fade(0f);
            isLoading = false;
        }

        private IEnumerator Fade(float target)
        {
            float from = canvasGroup.alpha;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, target, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = target;
            canvasGroup.blocksRaycasts = target > 0.5f;
            canvasGroup.interactable = false;
        }

        private IEnumerator AnimateDots()
        {
            int dots = 0;
            while (isLoading)
            {
                if (loadingText != null) loadingText.text = "Loading" + new string('.', dots);
                dots = (dots + 1) % 4;
                yield return new WaitForSecondsRealtime(0.4f);
            }
        }
    }
}
