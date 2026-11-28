using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using BullyingGame.Audio;
using BullyingGame.Campaign;
using BullyingGame.Core;

namespace BullyingGame.UI
{
    [Serializable]
    public class ChapterEntry
    {
        public string chapterId;
        public string chapterTitle;

        [TextArea(2, 5)]
        public string chapterDescription;

        public RectTransform chapterCard;
        public CanvasGroup canvasGroup;

        [Tooltip("Optional locked badge/overlay child inside the chapter card.")]
        public GameObject lockedOverlay;

        public string sceneName;
        public bool isUnlocked = true;

        [Tooltip("Minimum CampaignCheckpoint.unlockedChapter required when progression unlock is enabled (0 = always unlocked).")]
        public int requiredUnlockedChapter = 0;

        [Tooltip("Optional custom requirement message displayed when this chapter is locked.")]
        public string unlockRequirementText = "Selesaikan bagian sebelumnya untuk membuka chapter ini.";
    }

    /// <summary>
    /// Vertical Chapter Selection Carousel for UNSEEN SIDE (01_MainMenu -> Canvas_menu_ch).
    /// Supports smooth Ease Out Cubic snap transitions, RectMask2D soft masking,
    /// continuous distance-based fade & scaling (preserving non-uniform card scales),
    /// animated title/description transitions, keyboard/mouse/gamepad input,
    /// progression unlock validation, audio feedback, and existing scene loading.
    /// </summary>
    public class ChapterCarouselController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Carousel viewport RectTransform (ch) containing RectMask2D.")]
        [SerializeField] private RectTransform carouselViewport;

        [Tooltip("Container RectTransform (ChapterContent) holding all chapter cards.")]
        [SerializeField] private RectTransform chapterContent;

        [Tooltip("Ordered list of chapter entries displayed in the vertical carousel.")]
        [SerializeField] private List<ChapterEntry> chapterEntries = new List<ChapterEntry>();

        [Tooltip("Legacy Unity UI Text for chapter title (judul-level).")]
        [SerializeField] private Text chapterTitleText;

        [Tooltip("Legacy Unity UI Text for chapter description (Deskripsi-level).")]
        [SerializeField] private Text chapterDescriptionText;

        [Tooltip("Optional TextMeshProUGUI for chapter title (used if legacy Text is not assigned).")]
        [SerializeField] private TMP_Text chapterTitleTMP;

        [Tooltip("Optional TextMeshProUGUI for chapter description (used if legacy Text is not assigned).")]
        [SerializeField] private TMP_Text chapterDescriptionTMP;

        [Tooltip("Optional CanvasGroup for title text fade transitions.")]
        [SerializeField] private CanvasGroup titleCanvasGroup;

        [Tooltip("Optional CanvasGroup for description text fade transitions.")]
        [SerializeField] private CanvasGroup descriptionCanvasGroup;

        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button confirmButton;

        [Tooltip("Optional reference to trigger_menu for returning to Main Menu on Back/Escape.")]
        [SerializeField] private trigger_menu menuTrigger;

        [Header("Optional Audio References")]
        [SerializeField] private AudioClip navigationSfx;
        [SerializeField] private AudioClip confirmSfx;
        [SerializeField] private AudioClip backSfx;
        [SerializeField] private AudioClip lockedSfx;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.45f;

        [Header("Layout")]
        [Tooltip("Center anchoredPosition inside ChapterContent for the selected chapter card.")]
        [SerializeField] private Vector2 centerPosition = Vector2.zero;

        [Tooltip("Vertical spacing in UI units between adjacent chapter cards.")]
        [SerializeField, Range(100f, 900f)] private float cardSpacing = 430f;

        [Tooltip("Initial chapter index selected when the carousel opens.")]
        [SerializeField] private int startingIndex = 0;

        [Header("Transition")]
        [Tooltip("Duration in seconds for vertical slide snap transition.")]
        [SerializeField, Range(0.05f, 1.5f)] private float transitionDuration = 0.4f;

        [Tooltip("Optional custom easing curve. When useCustomEasingCurve is false, native Ease Out Cubic is used.")]
        [SerializeField] private bool useCustomEasingCurve = false;

        [SerializeField] private AnimationCurve easingCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 3f),
            new Keyframe(1f, 1f, 0f, 0f)
        );

        [Tooltip("Scale multiplier applied to the selected card (multiplied with each card's original localScale).")]
        [SerializeField, Range(0.5f, 1.5f)] private float selectedScale = 1.0f;

        [Tooltip("Scale multiplier applied to adjacent unselected cards.")]
        [SerializeField, Range(0.5f, 1.2f)] private float unselectedScale = 0.88f;

        [Tooltip("Scale multiplier applied to distant cards (2+ positions away).")]
        [SerializeField, Range(0.5f, 1.0f)] private float distantScale = 0.80f;

        [Tooltip("Enable continuous dynamic scaling based on distance from center.")]
        [SerializeField] private bool enableDynamicScaling = true;

        [Header("Fade")]
        [SerializeField, Range(0f, 1f)] private float selectedAlpha = 1.0f;
        [SerializeField, Range(0f, 1f)] private float adjacentAlpha = 0.35f;
        [SerializeField, Range(0f, 1f)] private float distantAlpha = 0.0f;

        [Tooltip("Optional curve shaping alpha falloff from normalized distance 0 (center) to 2 (distant).")]
        [SerializeField] private AnimationCurve fadeCurve = new AnimationCurve(
            new Keyframe(0f, 1.0f),
            new Keyframe(1f, 0.35f),
            new Keyframe(2f, 0.0f)
        );

        [SerializeField, Range(0.02f, 0.5f)] private float titleFadeOutDuration = 0.12f;
        [SerializeField, Range(0.02f, 0.6f)] private float titleFadeInDuration = 0.22f;
        [SerializeField, Range(0f, 50f)] private float textSlideOffset = 15f;

        [Header("Input")]
        [SerializeField, Range(0.05f, 0.8f)] private float inputCooldown = 0.2f;
        [SerializeField, Range(0.01f, 1f)] private float scrollSensitivity = 0.1f;
        [SerializeField, Range(0.1f, 0.9f)] private float gamepadDeadzone = 0.5f;
        [SerializeField] private bool loopNavigation = false;
        [SerializeField] private bool rememberLastSelection = true;

        [Header("Progression")]
        [Tooltip("When enabled, reads CampaignCheckpoint.Load().unlockedChapter without modifying save data.")]
        [SerializeField] private bool useProgressionUnlock = true;

        [Header("Events")]
        public UnityEvent<int> onChapterSelected;
        public UnityEvent<ChapterEntry> onChapterConfirmed;
        public UnityEvent onBackRequested;

        public int SelectedIndex => _selectedIndex;
        public int ChapterCount => chapterEntries != null ? chapterEntries.Count : 0;
        public ChapterEntry CurrentChapter => IsValidIndex(_selectedIndex) ? chapterEntries[_selectedIndex] : null;

        private int _selectedIndex;
        private bool _hasInitialized;
        private bool _isTransitioning;
        private bool _isLoadingScene;
        private float _lastInputTime = -10f;
        private bool _stickNeutral = true;

        private Vector3[] _originalCardScales;
        private Vector2[] _transitionStartPositions;
        private Vector2[] _transitionTargetPositions;

        private Vector2 _titleBaseAnchoredPos;
        private Vector2 _descBaseAnchoredPos;
        private bool _hasCachedTextPositions;

        private Coroutine _slideCoroutine;
        private Coroutine _textFadeCoroutine;

        private void Awake()
        {
            CacheCardData();
            CacheTextPositions();
            WireButtons();
        }

        private void OnEnable()
        {
            _isLoadingScene = false;
            _isTransitioning = false;
            _lastInputTime = Time.unscaledTime - inputCooldown;
            _stickNeutral = true;

            CacheCardData();
            CacheTextPositions();
            RefreshUnlockStates();

            if (!_hasInitialized || !rememberLastSelection)
            {
                _selectedIndex = ClampIndex(startingIndex);
                _hasInitialized = true;
            }
            else
            {
                _selectedIndex = ClampIndex(_selectedIndex);
            }

            StopActiveCoroutines();
            SnapAllCardsImmediate(_selectedIndex);
            ApplyChapterInfoImmediate(_selectedIndex);
            UpdateNavigationButtons();
        }

        private void OnDisable()
        {
            StopActiveCoroutines();
            _isTransitioning = false;
        }

        private void Update()
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy || _isLoadingScene || ChapterCount == 0)
            {
                return;
            }

            HandleKeyboardInput();
            HandleGamepadInput();
            HandleMouseScrollInput();
        }

        #region Public API

        public void NextChapter()
        {
            if (_isLoadingScene || ChapterCount <= 1)
            {
                return;
            }

            int nextIndex = _selectedIndex + 1;
            if (nextIndex >= ChapterCount)
            {
                if (!loopNavigation)
                {
                    return;
                }
                nextIndex = 0;
            }

            SelectChapter(nextIndex);
        }

        public void PreviousChapter()
        {
            if (_isLoadingScene || ChapterCount <= 1)
            {
                return;
            }

            int prevIndex = _selectedIndex - 1;
            if (prevIndex < 0)
            {
                if (!loopNavigation)
                {
                    return;
                }
                prevIndex = ChapterCount - 1;
            }

            SelectChapter(prevIndex);
        }

        public void SelectChapter(int index)
        {
            if (_isLoadingScene || ChapterCount == 0)
            {
                return;
            }

            int clamped = ClampIndex(index);
            if (clamped == _selectedIndex && !_isTransitioning)
            {
                return;
            }

            _selectedIndex = clamped;
            _lastInputTime = Time.unscaledTime;

            PlaySfx(navigationSfx);
            StartSlideTransition(_selectedIndex);
            StartTextTransition(_selectedIndex);
            UpdateNavigationButtons();
            onChapterSelected?.Invoke(_selectedIndex);
        }

        public void ConfirmChapter()
        {
            if (_isLoadingScene || !IsValidIndex(_selectedIndex))
            {
                return;
            }

            ChapterEntry entry = chapterEntries[_selectedIndex];
            if (!entry.isUnlocked)
            {
                PlaySfx(lockedSfx);
                Debug.Log($"[ChapterCarousel] Chapter '{entry.chapterTitle}' masih terkunci.");
                return;
            }

            if (string.IsNullOrWhiteSpace(entry.sceneName) || !ChapterSceneFlow.CanLoad(entry.sceneName))
            {
                PlaySfx(lockedSfx);
                Debug.LogWarning($"[ChapterCarousel] Scene '{entry.sceneName}' untuk chapter '{entry.chapterTitle}' tidak ditemukan di Build Settings.");
                return;
            }

            _isLoadingScene = true;
            PlaySfx(confirmSfx);
            onChapterConfirmed?.Invoke(entry);

            if (LevelLoader.Instance != null)
            {
                LevelLoader.Instance.LoadScene(entry.sceneName);
            }
            else if (!ChapterSceneFlow.Load(entry.sceneName))
            {
                _isLoadingScene = false;
                Debug.LogWarning($"[ChapterCarousel] Gagal memuat scene '{entry.sceneName}'. Input diaktifkan kembali.");
            }
        }

        public void RefreshChapterInfo()
        {
            RefreshUnlockStates();
            ApplyChapterInfoImmediate(_selectedIndex);
            UpdateNavigationButtons();
        }

        public void ResetSelection()
        {
            StopActiveCoroutines();
            _selectedIndex = ClampIndex(startingIndex);
            SnapAllCardsImmediate(_selectedIndex);
            ApplyChapterInfoImmediate(_selectedIndex);
            UpdateNavigationButtons();
        }

        public void RequestBack()
        {
            if (_isLoadingScene)
            {
                return;
            }

            PlaySfx(backSfx);
            onBackRequested?.Invoke();

            if (menuTrigger != null)
            {
                menuTrigger.ReturnToMainMenu();
            }
        }

        #endregion

        #region Initialization & Caching

        private void CacheCardData()
        {
            int count = ChapterCount;
            if (count == 0)
            {
                return;
            }

            if (_originalCardScales == null || _originalCardScales.Length != count)
            {
                _originalCardScales = new Vector3[count];
                _transitionStartPositions = new Vector2[count];
                _transitionTargetPositions = new Vector2[count];

                for (int i = 0; i < count; i++)
                {
                    ChapterEntry entry = chapterEntries[i];
                    if (entry != null && entry.chapterCard != null)
                    {
                        _originalCardScales[i] = entry.chapterCard.localScale;
                        if (entry.canvasGroup == null)
                        {
                            entry.canvasGroup = entry.chapterCard.GetComponent<CanvasGroup>();
                        }

                        // Ensure all chapter cards stay active for smooth continuous vertical carousel
                        if (!entry.chapterCard.gameObject.activeSelf)
                        {
                            entry.chapterCard.gameObject.SetActive(true);
                        }

                        AttachCardClickHandler(entry.chapterCard.gameObject, i);
                    }
                    else
                    {
                        _originalCardScales[i] = Vector3.one;
                    }
                }
            }
        }

        private void AttachCardClickHandler(GameObject cardGo, int cardIndex)
        {
            ChapterCardClickProxy proxy = cardGo.GetComponent<ChapterCardClickProxy>();
            if (proxy == null)
            {
                proxy = cardGo.AddComponent<ChapterCardClickProxy>();
            }
            proxy.Bind(this, cardIndex);
        }

        internal void HandleCardClicked(int cardIndex)
        {
            if (_isLoadingScene || !isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                return;
            }

            if (cardIndex == _selectedIndex)
            {
                ConfirmChapter();
            }
            else
            {
                SelectChapter(cardIndex);
            }
        }

        private void CacheTextPositions()
        {
            if (_hasCachedTextPositions)
            {
                return;
            }

            RectTransform titleRt = GetTitleRectTransform();
            RectTransform descRt = GetDescriptionRectTransform();

            if (titleRt != null)
            {
                _titleBaseAnchoredPos = titleRt.anchoredPosition;
                if (titleCanvasGroup == null)
                {
                    titleCanvasGroup = titleRt.GetComponent<CanvasGroup>();
                }
            }

            if (descRt != null)
            {
                _descBaseAnchoredPos = descRt.anchoredPosition;
                if (descriptionCanvasGroup == null)
                {
                    descriptionCanvasGroup = descRt.GetComponent<CanvasGroup>();
                }
            }

            _hasCachedTextPositions = titleRt != null || descRt != null;
        }

        private void WireButtons()
        {
            if (previousButton != null)
            {
                previousButton.onClick.RemoveListener(PreviousChapter);
                previousButton.onClick.AddListener(PreviousChapter);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(NextChapter);
                nextButton.onClick.AddListener(NextChapter);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(ConfirmChapter);
                confirmButton.onClick.AddListener(ConfirmChapter);
            }
        }

        private void RefreshUnlockStates()
        {
            if (!useProgressionUnlock || ChapterCount == 0)
            {
                UpdateLockedOverlays();
                return;
            }

            int unlockedChapter = 1;
            try
            {
                CampaignProgress progress = CampaignCheckpoint.Load();
                if (progress != null)
                {
                    unlockedChapter = Mathf.Max(1, progress.unlockedChapter);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ChapterCarousel] Gagal membaca CampaignCheckpoint: {e.Message}");
            }

            for (int i = 0; i < ChapterCount; i++)
            {
                ChapterEntry entry = chapterEntries[i];
                if (entry == null)
                {
                    continue;
                }

                entry.isUnlocked = entry.requiredUnlockedChapter <= 0 || unlockedChapter >= entry.requiredUnlockedChapter;
            }

            UpdateLockedOverlays();
        }

        private void UpdateLockedOverlays()
        {
            for (int i = 0; i < ChapterCount; i++)
            {
                ChapterEntry entry = chapterEntries[i];
                if (entry != null && entry.lockedOverlay != null)
                {
                    entry.lockedOverlay.SetActive(!entry.isUnlocked);
                }
            }
        }

        #endregion

        #region Input Handling (New Input System)

        private bool CanAcceptNavigationInput()
        {
            return (Time.unscaledTime - _lastInputTime) >= inputCooldown;
        }

        private void HandleKeyboardInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }

            if (kb.escapeKey.wasPressedThisFrame)
            {
                RequestBack();
                return;
            }

            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            {
                ConfirmChapter();
                return;
            }

            if (!CanAcceptNavigationInput())
            {
                return;
            }

            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            {
                PreviousChapter();
            }
            else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
            {
                NextChapter();
            }
        }

        private void HandleGamepadInput()
        {
            Gamepad gp = Gamepad.current;
            if (gp == null)
            {
                return;
            }

            if (gp.buttonEast.wasPressedThisFrame)
            {
                RequestBack();
                return;
            }

            if (gp.buttonSouth.wasPressedThisFrame)
            {
                ConfirmChapter();
                return;
            }

            float stickY = gp.leftStick.y.ReadValue();
            if (Mathf.Abs(stickY) < gamepadDeadzone * 0.5f)
            {
                _stickNeutral = true;
            }

            if (!CanAcceptNavigationInput())
            {
                return;
            }

            if (gp.dpad.up.wasPressedThisFrame || (_stickNeutral && stickY >= gamepadDeadzone))
            {
                _stickNeutral = false;
                PreviousChapter();
            }
            else if (gp.dpad.down.wasPressedThisFrame || (_stickNeutral && stickY <= -gamepadDeadzone))
            {
                _stickNeutral = false;
                NextChapter();
            }
        }

        private void HandleMouseScrollInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !CanAcceptNavigationInput())
            {
                return;
            }

            float scrollY = mouse.scroll.y.ReadValue();
            if (Mathf.Abs(scrollY) < scrollSensitivity)
            {
                return;
            }

            if (scrollY > 0f)
            {
                PreviousChapter();
            }
            else if (scrollY < 0f)
            {
                NextChapter();
            }
        }

        #endregion

        #region Vertical Layout, Slide Transition, Fade & Scaling

        private float GetTargetY(int selectedIdx, int cardIdx)
        {
            float offset = selectedIdx - cardIdx;
            return centerPosition.y + offset * cardSpacing;
        }

        private void SnapAllCardsImmediate(int selectedIdx)
        {
            int count = ChapterCount;
            for (int i = 0; i < count; i++)
            {
                ChapterEntry entry = chapterEntries[i];
                if (entry == null || entry.chapterCard == null)
                {
                    continue;
                }

                Vector2 targetPos = new Vector2(centerPosition.x, GetTargetY(selectedIdx, i));
                entry.chapterCard.anchoredPosition = targetPos;
                UpdateCardVisualByPosition(i, entry);
            }
        }

        private void StartSlideTransition(int targetSelectedIdx)
        {
            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
            }

            int count = ChapterCount;
            for (int i = 0; i < count; i++)
            {
                ChapterEntry entry = chapterEntries[i];
                if (entry != null && entry.chapterCard != null)
                {
                    _transitionStartPositions[i] = entry.chapterCard.anchoredPosition;
                    _transitionTargetPositions[i] = new Vector2(centerPosition.x, GetTargetY(targetSelectedIdx, i));
                }
            }

            _slideCoroutine = StartCoroutine(SlideTransitionRoutine());
        }

        private IEnumerator SlideTransitionRoutine()
        {
            _isTransitioning = true;
            float duration = Mathf.Max(0.01f, transitionDuration);
            float elapsed = 0f;
            int count = ChapterCount;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EvaluateEasing(t);

                for (int i = 0; i < count; i++)
                {
                    ChapterEntry entry = chapterEntries[i];
                    if (entry == null || entry.chapterCard == null)
                    {
                        continue;
                    }

                    entry.chapterCard.anchoredPosition = Vector2.LerpUnclamped(
                        _transitionStartPositions[i],
                        _transitionTargetPositions[i],
                        eased
                    );
                    UpdateCardVisualByPosition(i, entry);
                }

                yield return null;
            }

            // Exact snap to eliminate any floating-point drift
            for (int i = 0; i < count; i++)
            {
                ChapterEntry entry = chapterEntries[i];
                if (entry == null || entry.chapterCard == null)
                {
                    continue;
                }

                entry.chapterCard.anchoredPosition = _transitionTargetPositions[i];
                UpdateCardVisualByPosition(i, entry);
            }

            _isTransitioning = false;
            _slideCoroutine = null;
        }

        private float EvaluateEasing(float t)
        {
            if (useCustomEasingCurve && easingCurve != null && easingCurve.length >= 2)
            {
                return easingCurve.Evaluate(t);
            }

            // Ease Out Cubic: 1 - (1 - t)^3
            float inv = 1f - t;
            return 1f - (inv * inv * inv);
        }

        private void UpdateCardVisualByPosition(int cardIdx, ChapterEntry entry)
        {
            float spacing = Mathf.Max(1f, cardSpacing);
            float distance = Mathf.Abs(entry.chapterCard.anchoredPosition.y - centerPosition.y);
            float normalizedDistance = distance / spacing;

            // Continuous alpha from actual position
            float alpha;
            if (fadeCurve != null && fadeCurve.length >= 2)
            {
                alpha = Mathf.Clamp01(fadeCurve.Evaluate(normalizedDistance));
            }
            else if (normalizedDistance <= 1f)
            {
                alpha = Mathf.Lerp(selectedAlpha, adjacentAlpha, normalizedDistance);
            }
            else
            {
                alpha = Mathf.Lerp(adjacentAlpha, distantAlpha, Mathf.Clamp01(normalizedDistance - 1f));
            }

            if (entry.canvasGroup != null)
            {
                entry.canvasGroup.alpha = alpha;
                entry.canvasGroup.blocksRaycasts = alpha > 0.05f;
                entry.canvasGroup.interactable = alpha > 0.05f;
            }

            // Continuous dynamic scaling preserving each card's original non-uniform localScale
            if (enableDynamicScaling && _originalCardScales != null && cardIdx < _originalCardScales.Length)
            {
                float scaleMult;
                if (normalizedDistance <= 1f)
                {
                    scaleMult = Mathf.Lerp(selectedScale, unselectedScale, normalizedDistance);
                }
                else
                {
                    scaleMult = Mathf.Lerp(unselectedScale, distantScale, Mathf.Clamp01(normalizedDistance - 1f));
                }

                entry.chapterCard.localScale = _originalCardScales[cardIdx] * scaleMult;
            }
        }

        #endregion

        #region Dynamic Chapter Information & Text Transition

        private void ApplyChapterInfoImmediate(int index)
        {
            if (!IsValidIndex(index))
            {
                return;
            }

            SetTextStrings(chapterEntries[index]);

            if (titleCanvasGroup != null) titleCanvasGroup.alpha = 1f;
            if (descriptionCanvasGroup != null) descriptionCanvasGroup.alpha = 1f;

            RectTransform titleRt = GetTitleRectTransform();
            RectTransform descRt = GetDescriptionRectTransform();
            if (_hasCachedTextPositions)
            {
                if (titleRt != null) titleRt.anchoredPosition = _titleBaseAnchoredPos;
                if (descRt != null) descRt.anchoredPosition = _descBaseAnchoredPos;
            }
        }

        private void StartTextTransition(int targetIndex)
        {
            if (!IsValidIndex(targetIndex))
            {
                return;
            }

            if (_textFadeCoroutine != null)
            {
                StopCoroutine(_textFadeCoroutine);
            }

            _textFadeCoroutine = StartCoroutine(TextTransitionRoutine(chapterEntries[targetIndex]));
        }

        private IEnumerator TextTransitionRoutine(ChapterEntry targetEntry)
        {
            RectTransform titleRt = GetTitleRectTransform();
            RectTransform descRt = GetDescriptionRectTransform();

            // 1. Fade Out old title & description
            float outDur = Mathf.Max(0.01f, titleFadeOutDuration);
            float elapsed = 0f;
            float startTitleAlpha = titleCanvasGroup != null ? titleCanvasGroup.alpha : 1f;
            float startDescAlpha = descriptionCanvasGroup != null ? descriptionCanvasGroup.alpha : 1f;

            while (elapsed < outDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / outDur);
                if (titleCanvasGroup != null) titleCanvasGroup.alpha = Mathf.Lerp(startTitleAlpha, 0f, t);
                if (descriptionCanvasGroup != null) descriptionCanvasGroup.alpha = Mathf.Lerp(startDescAlpha, 0f, t);

                if (_hasCachedTextPositions && textSlideOffset > 0f)
                {
                    float yOff = Mathf.Lerp(0f, textSlideOffset, t);
                    if (titleRt != null) titleRt.anchoredPosition = _titleBaseAnchoredPos + new Vector2(0f, yOff);
                    if (descRt != null) descRt.anchoredPosition = _descBaseAnchoredPos + new Vector2(0f, yOff);
                }

                yield return null;
            }

            // 2. Swap text strings
            SetTextStrings(targetEntry);

            // 3. Fade In new title & description with subtle upward slide settle
            float inDur = Mathf.Max(0.01f, titleFadeInDuration);
            elapsed = 0f;

            while (elapsed < inDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / inDur);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                if (titleCanvasGroup != null) titleCanvasGroup.alpha = eased;
                if (descriptionCanvasGroup != null) descriptionCanvasGroup.alpha = eased;

                if (_hasCachedTextPositions && textSlideOffset > 0f)
                {
                    float yOff = Mathf.Lerp(-textSlideOffset, 0f, eased);
                    if (titleRt != null) titleRt.anchoredPosition = _titleBaseAnchoredPos + new Vector2(0f, yOff);
                    if (descRt != null) descRt.anchoredPosition = _descBaseAnchoredPos + new Vector2(0f, yOff);
                }

                yield return null;
            }

            if (titleCanvasGroup != null) titleCanvasGroup.alpha = 1f;
            if (descriptionCanvasGroup != null) descriptionCanvasGroup.alpha = 1f;
            if (_hasCachedTextPositions)
            {
                if (titleRt != null) titleRt.anchoredPosition = _titleBaseAnchoredPos;
                if (descRt != null) descRt.anchoredPosition = _descBaseAnchoredPos;
            }

            _textFadeCoroutine = null;
        }

        private void SetTextStrings(ChapterEntry entry)
        {
            string title = entry.chapterTitle ?? string.Empty;
            string desc = entry.chapterDescription ?? string.Empty;

            if (!entry.isUnlocked && !string.IsNullOrWhiteSpace(entry.unlockRequirementText))
            {
                desc = $"{desc}\n[Terkunci] {entry.unlockRequirementText}";
            }

            if (chapterTitleText != null) chapterTitleText.text = title;
            if (chapterDescriptionText != null) chapterDescriptionText.text = desc;
            if (chapterTitleTMP != null) chapterTitleTMP.text = title;
            if (chapterDescriptionTMP != null) chapterDescriptionTMP.text = desc;
        }

        private void UpdateNavigationButtons()
        {
            if (previousButton != null)
            {
                previousButton.interactable = loopNavigation || _selectedIndex > 0;
            }

            if (nextButton != null)
            {
                nextButton.interactable = loopNavigation || _selectedIndex < ChapterCount - 1;
            }

            if (confirmButton != null)
            {
                ChapterEntry current = CurrentChapter;
                confirmButton.interactable = current != null && current.isUnlocked;
            }
        }

        #endregion

        #region Helpers

        private RectTransform GetTitleRectTransform()
        {
            if (chapterTitleText != null) return chapterTitleText.rectTransform;
            if (chapterTitleTMP != null) return chapterTitleTMP.rectTransform;
            return null;
        }

        private RectTransform GetDescriptionRectTransform()
        {
            if (chapterDescriptionText != null) return chapterDescriptionText.rectTransform;
            if (chapterDescriptionTMP != null) return chapterDescriptionTMP.rectTransform;
            return null;
        }

        private int ClampIndex(int index)
        {
            if (ChapterCount == 0) return 0;
            return Mathf.Clamp(index, 0, ChapterCount - 1);
        }

        private bool IsValidIndex(int index)
        {
            return chapterEntries != null && index >= 0 && index < chapterEntries.Count && chapterEntries[index] != null;
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;
            AudioManager.Instance?.PlaySFX(clip, sfxVolume);
        }

        private void StopActiveCoroutines()
        {
            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
                _slideCoroutine = null;
            }

            if (_textFadeCoroutine != null)
            {
                StopCoroutine(_textFadeCoroutine);
                _textFadeCoroutine = null;
            }
        }

        #endregion
    }

    /// <summary>
    /// Lightweight click proxy attached to each chapter card so clicking the selected card
    /// confirms it and clicking an adjacent visible card selects it.
    /// </summary>
    internal class ChapterCardClickProxy : MonoBehaviour, IPointerClickHandler
    {
        private ChapterCarouselController _controller;
        private int _index;

        public void Bind(ChapterCarouselController controller, int index)
        {
            _controller = controller;
            _index = index;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _controller?.HandleCardClicked(_index);
        }
    }
}
