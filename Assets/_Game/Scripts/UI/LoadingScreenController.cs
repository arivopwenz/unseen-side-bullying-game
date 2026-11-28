using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using BullyingGame.Core;

namespace BullyingGame.UI
{
    /// <summary>
    /// Global persistent animated loading screen controller for UNSEEN SIDE.
    /// Uses DontDestroyOnLoad on its root GameObject and keeps LoadingCanvas hidden when idle.
    /// Handles asynchronous scene loading, unscaled-time fade in/out, real AsyncOperation progress,
    /// anti-bullying tips cycling, input blocking, and GameStateManager integration.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        public static LoadingScreenController Instance { get; private set; }
        public bool IsLoading => _isLoading;

        [Header("References")]
        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform logoRectTransform;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TMP_Text percentageText;
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private TMP_Text tipsText;
        [SerializeField] private CanvasGroup tipsCanvasGroup;
        [SerializeField] private LoadingScreenAnimator screenAnimator;

        [Header("Transition Settings")]
        [SerializeField, Range(0.05f, 1.5f)] private float fadeInDuration = 0.25f;
        [SerializeField, Range(0.05f, 1.5f)] private float fadeOutDuration = 0.35f;
        [SerializeField, Range(0f, 3f)] private float minimumDisplayDuration = 0.75f;
        [SerializeField, Range(1f, 20f)] private float progressSmoothingSpeed = 6f;

        [Header("Content Settings")]
        [SerializeField] private string loadingMessage = "Memuat cerita...";
        [SerializeField] private string preparingMessage = "Menyiapkan permainan...";
        [SerializeField, Min(1f)] private float tipsInterval = 5f;
        [SerializeField] private List<string> tipsList = new List<string>
        {
            "Tindakan kecil bisa membawa perubahan besar.",
            "Berani peduli adalah langkah pertama.",
            "Setiap teman berhak merasa aman di sekolah.",
            "Jangan takut meminta bantuan kepada guru.",
            "Kamu tidak harus menghadapi perundungan sendirian.",
            "Kata-kata yang baik dapat menguatkan seseorang.",
            "Jangan membalas perundungan dengan kekerasan.",
            "Dengarkan temanmu tanpa menghakimi.",
            "Bercanda seharusnya tidak menyakiti orang lain.",
            "Menjadi teman yang baik dimulai dari kepedulian."
        };

        private bool _isLoading;
        private int _lastTipIndex = -1;
        private float _displayedProgress;
        private Coroutine _loadCoroutine;
        private Coroutine _tipsCoroutine;

        /// <summary>
        /// Ensures a single global LoadingScreenController instance exists by loading
        /// the canonical LoadingScreen.prefab referenced by Resources/LoadingScreenConfig.
        /// </summary>
        public static LoadingScreenController EnsureInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }

            LoadingScreenController existing = FindFirstObjectByType<LoadingScreenController>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }

            LoadingScreenConfig config = Resources.Load<LoadingScreenConfig>("LoadingScreenConfig");
            if (config != null && config.loadingScreenPrefab != null)
            {
                GameObject spawned = Instantiate(config.loadingScreenPrefab);
                spawned.name = "LoadingScreen";
                Instance = spawned.GetComponent<LoadingScreenController>();
                return Instance;
            }

            return null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);

            if (screenAnimator == null)
            {
                screenAnimator = GetComponent<LoadingScreenAnimator>();
            }

            SetHiddenImmediate();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Loads target scene asynchronously with full animated loading screen,
        /// fade in/out, real AsyncOperation progress, and GameStateManager integration.
        /// </summary>
        public bool LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[LoadingScreen] Permintaan load '{sceneName}' diabaikan karena proses loading masih berlangsung.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[LoadingScreen] Scene '{sceneName}' tidak ditemukan atau belum terdaftar di Build Settings.");
                return false;
            }

            _loadCoroutine = StartCoroutine(LoadSceneRoutine(sceneName));
            return true;
        }

        /// <summary>
        /// Allows showing the loading screen directly for testing or external transitions.
        /// </summary>
        public void ShowImmediateForPreview(float progress = 0.5f)
        {
            if (loadingCanvas != null) loadingCanvas.gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            ApplyProgressVisual(progress);
            SetStatusMessage(loadingMessage);
            ShowNextTipImmediate();
            screenAnimator?.StartAnimations();
        }

        public void HideImmediate()
        {
            if (_tipsCoroutine != null)
            {
                StopCoroutine(_tipsCoroutine);
                _tipsCoroutine = null;
            }
            screenAnimator?.StopAnimations();
            SetHiddenImmediate();
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isLoading = true;
            GameState previousState = GameStateManager.Instance != null ? GameStateManager.Instance.CurrentState : GameState.MainMenu;

            Debug.Log("[LoadingScreen] Show");
            PrepareForShow();

            // Block gameplay/UI interaction and update GameState
            GameStateManager.Instance?.SetState(GameState.Loading);
            EventSystem.current?.SetSelectedGameObject(null);

            // 1. Fade In (0 -> 1) using unscaled time
            yield return FadeCanvasGroup(0f, 1f, fadeInDuration);

            // Ensure UI has rendered at full opacity before starting heavy async load
            yield return null;

            float showStartRealtime = Time.realtimeSinceStartup;
            if (tipsList != null && tipsList.Count > 1)
            {
                _tipsCoroutine = StartCoroutine(CycleTipsRoutine());
            }

            // 2. Start AsyncOperation
            Debug.Log($"[LoadingScreen] Loading target scene: {sceneName}");
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                Debug.LogError($"[LoadingScreen] SceneManager.LoadSceneAsync('{sceneName}') mengembalikan null.");
                StopTipsCycle();
                yield return FadeCanvasGroup(1f, 0f, fadeOutDuration);
                SetHiddenImmediate();
                GameStateManager.Instance?.SetState(previousState);
                _isLoading = false;
                _loadCoroutine = null;
                yield break;
            }

            operation.allowSceneActivation = false;
            bool loggedSlowWarning = false;

            // 3. Track actual loading progress (0.0 -> 0.9 mapped smoothly to 0% -> 90%)
            while (operation.progress < 0.9f)
            {
                float normalizedProgress = Mathf.Clamp01(operation.progress / 0.9f);
                float targetDisplay = normalizedProgress * 0.9f;
                _displayedProgress = Mathf.MoveTowards(_displayedProgress, targetDisplay, Time.unscaledDeltaTime * progressSmoothingSpeed);
                ApplyProgressVisual(_displayedProgress);

                if (!loggedSlowWarning && (Time.realtimeSinceStartup - showStartRealtime) > 10f)
                {
                    loggedSlowWarning = true;
                    Debug.LogWarning($"[LoadingScreen] Loading scene '{sceneName}' berlangsung lebih dari 10 detik (progress={operation.progress:F2}).");
                }

                yield return null;
            }

            Debug.Log("[LoadingScreen] Scene ready for activation");
            SetStatusMessage(preparingMessage);

            // Smoothly reach 90% and satisfy minimum display duration before activation
            while (_displayedProgress < 0.9f || (Time.realtimeSinceStartup - showStartRealtime) < minimumDisplayDuration)
            {
                _displayedProgress = Mathf.MoveTowards(_displayedProgress, 0.92f, Time.unscaledDeltaTime * progressSmoothingSpeed);
                ApplyProgressVisual(Mathf.Min(0.92f, _displayedProgress));
                yield return null;
            }

            // 4. Activate scene (always set allowSceneActivation = true BEFORE waiting for isDone!)
            Time.timeScale = 1f;
            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                _displayedProgress = Mathf.MoveTowards(_displayedProgress, 0.98f, Time.unscaledDeltaTime * progressSmoothingSpeed);
                ApplyProgressVisual(_displayedProgress);
                yield return null;
            }

            Debug.Log("[LoadingScreen] Scene activated");

            // Allow one frame for new scene Awake/Start initialization
            yield return null;
            Debug.Log("[LoadingScreen] Scene initialization completed");

            // 5. Complete progress bar cleanly to 100%
            _displayedProgress = 1f;
            ApplyProgressVisual(1f);
            yield return new WaitForSecondsRealtime(0.08f);

            // 6. Fade Out (1 -> 0) and hide visual canvas (root remains active!)
            StopTipsCycle();
            yield return FadeCanvasGroup(1f, 0f, fadeOutDuration);

            screenAnimator?.StopAnimations();
            SetHiddenImmediate();
            Debug.Log("[LoadingScreen] Hide");

            _isLoading = false;
            _loadCoroutine = null;
        }

        private void PrepareForShow()
        {
            _displayedProgress = 0f;

            if (loadingCanvas != null)
            {
                loadingCanvas.sortingOrder = 9999;
                loadingCanvas.gameObject.SetActive(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(true);
            }

            ApplyProgressVisual(0f);
            SetStatusMessage(loadingMessage);
            ShowNextTipImmediate();
            screenAnimator?.StartAnimations();
        }

        private void SetHiddenImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(false);
            }
        }

        private void ApplyProgressVisual(float progress01)
        {
            float clamped = Mathf.Clamp01(progress01);
            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = clamped;
            }

            if (percentageText != null)
            {
                int pct = Mathf.RoundToInt(clamped * 100f);
                percentageText.text = pct + "%";
            }
        }

        private void SetStatusMessage(string msg)
        {
            if (loadingText != null)
            {
                loadingText.text = msg;
            }
        }

        private void ShowNextTipImmediate()
        {
            if (tipsText == null || tipsList == null || tipsList.Count == 0)
            {
                return;
            }

            int nextIdx = PickRandomTipIndex();
            tipsText.text = tipsList[nextIdx];
            if (tipsCanvasGroup != null)
            {
                tipsCanvasGroup.alpha = 1f;
            }
        }

        private int PickRandomTipIndex()
        {
            if (tipsList.Count <= 1)
            {
                _lastTipIndex = 0;
                return 0;
            }

            int idx = Random.Range(0, tipsList.Count);
            if (idx == _lastTipIndex)
            {
                idx = (idx + 1) % tipsList.Count;
            }
            _lastTipIndex = idx;
            return idx;
        }

        private IEnumerator CycleTipsRoutine()
        {
            float waitTime = Mathf.Max(1f, tipsInterval);
            while (_isLoading)
            {
                yield return new WaitForSecondsRealtime(waitTime);
                if (!_isLoading || tipsText == null || tipsList == null || tipsList.Count <= 1)
                {
                    yield break;
                }

                if (tipsCanvasGroup != null)
                {
                    yield return FadeGroup(tipsCanvasGroup, 1f, 0f, 0.2f);
                }

                tipsText.text = tipsList[PickRandomTipIndex()];

                if (tipsCanvasGroup != null)
                {
                    yield return FadeGroup(tipsCanvasGroup, 0f, 1f, 0.25f);
                }
            }
        }

        private void StopTipsCycle()
        {
            if (_tipsCoroutine != null)
            {
                StopCoroutine(_tipsCoroutine);
                _tipsCoroutine = null;
            }
        }

        private IEnumerator FadeCanvasGroup(float from, float to, float duration)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            yield return FadeGroup(canvasGroup, from, to, duration);
        }

        private static IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
        {
            float dur = Mathf.Max(0.01f, duration);
            float elapsed = 0f;
            group.alpha = from;

            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            group.alpha = to;
        }
    }
}
