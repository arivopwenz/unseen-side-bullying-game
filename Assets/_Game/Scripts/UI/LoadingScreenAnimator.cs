using UnityEngine;
using UnityEngine.UI;

namespace BullyingGame.UI
{
    /// <summary>
    /// Handles lightweight, unscaled-time animations for the Global Loading Screen:
    /// smooth logo floating/breathing/tilt, 3 bouncing indicator dots,
    /// and optional animated highlight sweep across the progress bar.
    /// Caches original transforms so repeated shows never accumulate drift.
    /// </summary>
    public class LoadingScreenAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform logoContainer;
        [SerializeField] private RectTransform[] loadingDots;
        [SerializeField] private CanvasGroup[] loadingDotCanvasGroups;
        [SerializeField] private RectTransform progressHighlight;
        [SerializeField] private RectTransform progressBarBounds;

        [Header("Logo Animation")]
        [SerializeField] private bool enableLogoFloating = true;
        [SerializeField, Range(0f, 30f)] private float floatAmplitude = 12f;
        [SerializeField, Min(0.2f)] private float floatDuration = 2.4f;
        [SerializeField, Range(0f, 10f)] private float rotationAmplitude = 1.5f;
        [SerializeField, Range(0f, 0.1f)] private float breathingScale = 0.02f;

        [Header("Loading Indicator Dots")]
        [SerializeField, Range(0f, 30f)] private float dotBounceHeight = 10f;
        [SerializeField, Min(0.2f)] private float dotAnimationDuration = 1.2f;
        [SerializeField, Range(0f, 0.5f)] private float dotDelay = 0.15f;
        [SerializeField, Range(0f, 1f)] private float dotMinAlpha = 0.35f;
        [SerializeField, Range(0f, 1f)] private float dotMaxAlpha = 1.0f;

        [Header("Progress Bar Highlight")]
        [SerializeField] private bool enableAnimatedProgressHighlight = true;
        [SerializeField, Min(0.2f)] private float highlightSweepDuration = 1.6f;

        private bool _isPlaying;
        private float _elapsed;
        private bool _hasCachedDefaults;

        private Vector2 _logoOriginalPos;
        private Vector3 _logoOriginalScale;
        private Quaternion _logoOriginalRot;
        private Vector2[] _dotOriginalPositions;
        private Vector2 _highlightOriginalPos;

        private void Awake()
        {
            CacheDefaults();
        }

        public void CacheDefaults()
        {
            if (_hasCachedDefaults)
            {
                return;
            }

            if (logoContainer != null)
            {
                _logoOriginalPos = logoContainer.anchoredPosition;
                _logoOriginalScale = logoContainer.localScale;
                _logoOriginalRot = logoContainer.localRotation;
            }

            if (loadingDots != null && loadingDots.Length > 0)
            {
                _dotOriginalPositions = new Vector2[loadingDots.Length];
                for (int i = 0; i < loadingDots.Length; i++)
                {
                    if (loadingDots[i] != null)
                    {
                        _dotOriginalPositions[i] = loadingDots[i].anchoredPosition;
                    }
                }
            }

            if (progressHighlight != null)
            {
                _highlightOriginalPos = progressHighlight.anchoredPosition;
            }

            _hasCachedDefaults = true;
        }

        public void StartAnimations()
        {
            CacheDefaults();
            ResetVisuals();
            _elapsed = 0f;
            _isPlaying = true;
        }

        public void StopAnimations()
        {
            _isPlaying = false;
            ResetVisuals();
        }

        public void ResetVisuals()
        {
            if (!_hasCachedDefaults)
            {
                return;
            }

            if (logoContainer != null)
            {
                logoContainer.anchoredPosition = _logoOriginalPos;
                logoContainer.localScale = _logoOriginalScale;
                logoContainer.localRotation = _logoOriginalRot;
            }

            if (loadingDots != null && _dotOriginalPositions != null)
            {
                int count = Mathf.Min(loadingDots.Length, _dotOriginalPositions.Length);
                for (int i = 0; i < count; i++)
                {
                    if (loadingDots[i] != null)
                    {
                        loadingDots[i].anchoredPosition = _dotOriginalPositions[i];
                    }
                    if (loadingDotCanvasGroups != null && i < loadingDotCanvasGroups.Length && loadingDotCanvasGroups[i] != null)
                    {
                        loadingDotCanvasGroups[i].alpha = dotMaxAlpha;
                    }
                }
            }

            if (progressHighlight != null)
            {
                progressHighlight.anchoredPosition = _highlightOriginalPos;
                progressHighlight.gameObject.SetActive(enableAnimatedProgressHighlight);
            }
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            UpdateLogoAnimation();
            UpdateDotsAnimation();
            UpdateProgressHighlight();
        }

        private void UpdateLogoAnimation()
        {
            if (!enableLogoFloating || logoContainer == null)
            {
                return;
            }

            float period = Mathf.Max(0.2f, floatDuration);
            float phase = (_elapsed * Mathf.PI * 2f) / period;
            float wave = Mathf.Sin(phase);
            float rotWave = Mathf.Sin(phase * 0.5f);

            logoContainer.anchoredPosition = _logoOriginalPos + Vector2.up * (wave * floatAmplitude);
            logoContainer.localScale = _logoOriginalScale * (1f + wave * breathingScale);
            logoContainer.localRotation = _logoOriginalRot * Quaternion.Euler(0f, 0f, rotWave * rotationAmplitude);
        }

        private void UpdateDotsAnimation()
        {
            if (loadingDots == null || _dotOriginalPositions == null)
            {
                return;
            }

            float duration = Mathf.Max(0.2f, dotAnimationDuration);
            int count = Mathf.Min(loadingDots.Length, _dotOriginalPositions.Length);

            for (int i = 0; i < count; i++)
            {
                RectTransform dot = loadingDots[i];
                if (dot == null)
                {
                    continue;
                }

                float t = (_elapsed - (i * dotDelay)) / duration;
                float cycle = t - Mathf.Floor(t); // [0, 1)
                // Smooth sine bounce during first half of cycle, gentle rest on second half
                float bounce = cycle < 0.5f ? Mathf.Sin(cycle * 2f * Mathf.PI) : 0f;
                bounce = Mathf.Max(0f, bounce);

                dot.anchoredPosition = _dotOriginalPositions[i] + Vector2.up * (bounce * dotBounceHeight);

                if (loadingDotCanvasGroups != null && i < loadingDotCanvasGroups.Length && loadingDotCanvasGroups[i] != null)
                {
                    loadingDotCanvasGroups[i].alpha = Mathf.Lerp(dotMinAlpha, dotMaxAlpha, bounce);
                }
            }
        }

        private void UpdateProgressHighlight()
        {
            if (progressHighlight == null)
            {
                return;
            }

            if (!enableAnimatedProgressHighlight)
            {
                if (progressHighlight.gameObject.activeSelf)
                {
                    progressHighlight.gameObject.SetActive(false);
                }
                return;
            }

            if (!progressHighlight.gameObject.activeSelf)
            {
                progressHighlight.gameObject.SetActive(true);
            }

            float width = progressBarBounds != null ? progressBarBounds.rect.width : 720f;
            float sweepPeriod = Mathf.Max(0.2f, highlightSweepDuration);
            float u = (_elapsed % sweepPeriod) / sweepPeriod;
            float halfW = width * 0.5f + progressHighlight.rect.width * 0.5f;
            float x = Mathf.Lerp(-halfW, halfW, u);
            progressHighlight.anchoredPosition = new Vector2(x, _highlightOriginalPos.y);
        }
    }
}
