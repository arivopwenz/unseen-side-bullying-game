using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;
using BullyingGame.Core;

namespace BullyingGame.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button continueButton;
        [Header("Dialogue Input")]
        [SerializeField] private InputActionReference advanceAction;
        [Header("Camera")]
        [SerializeField] private CinemachineCamera dialogueCamera;
        [Header("Voice & Audio")]
        [SerializeField] private AudioSource voiceAudioSource;
        [Header("Typewriter Settings")]
        [SerializeField, Min(0f)] private float typingSpeed = 0.035f;

        private DialogueManager subscribedManager;
        private Coroutine typingCoroutine;
        private bool isTyping, cameraOwned, actionWasEnabled, readyForAdvance, warnedAboutInput;
        private PrioritySettings previousCameraPriority;
        private InputAction activeAction;
        private int startedFrame;
        private float effectiveTypingSpeed;

        private void Awake()
        {
            if (voiceAudioSource == null) voiceAudioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            SubscribeEvents();
            if (subscribedManager == null || !subscribedManager.IsDialogueActive) HidePanel();
        }

        private void OnEnable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
                continueButton.onClick.AddListener(OnContinueClicked);
            }
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            var manager = DialogueManager.Instance;
            if (manager == null || subscribedManager == manager) return;
            UnsubscribeEvents();
            subscribedManager = manager;
            manager.OnDialogueStarted += ShowPanel;
            manager.OnLineDisplayed += UpdateLine;
            manager.OnDialogueEnded += HidePanel;
            if (manager.IsDialogueActive)
            {
                ShowPanel();
                UpdateLine(manager.CurrentLine);
            }
        }

        private void UnsubscribeEvents()
        {
            if (subscribedManager != null)
            {
                subscribedManager.OnDialogueStarted -= ShowPanel;
                subscribedManager.OnLineDisplayed -= UpdateLine;
                subscribedManager.OnDialogueEnded -= HidePanel;
            }
            subscribedManager = null;
        }

        private void ShowPanel()
        {
            startedFrame = Time.frameCount;
            bool cinematic = CutsceneManager.Instance != null && CutsceneManager.Instance.CurrentDirector != null;
            if (!cinematic && dialogueCamera != null && !cameraOwned)
            {
                previousCameraPriority = dialogueCamera.Priority;
                cameraOwned = true;
                dialogueCamera.Priority = 20;
            }
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            StartAdvanceInput();
        }

        private void StartAdvanceInput()
        {
            if (activeAction != null) return;
            var action = advanceAction != null ? advanceAction.action : null;
            if (action == null || action.type != InputActionType.Button)
            {
                if (!warnedAboutInput)
                {
                    Debug.LogWarning("Isi Dialogue UI > Advance Action dengan GameInput / Dialogue / Advance. Tombol Continue tetap dapat digunakan.", this);
                    warnedAboutInput = true;
                }
                return;
            }
            activeAction = action;
            actionWasEnabled = action.enabled;
            readyForAdvance = !action.IsPressed();
            action.performed += OnAdvancePerformed;
            action.Enable();
        }

        private void StopAdvanceInput()
        {
            if (activeAction == null) return;
            activeAction.performed -= OnAdvancePerformed;
            if (!actionWasEnabled) activeAction.Disable();
            activeAction = null;
        }

        private void Update()
        {
            if (activeAction != null && !readyForAdvance && !activeAction.IsPressed()) readyForAdvance = true;
        }

        private void OnAdvancePerformed(InputAction.CallbackContext context)
        {
            if (readyForAdvance) SkipOrNext();
        }

        private void HidePanel()
        {
            StopAdvanceInput();
            if (cameraOwned && dialogueCamera != null) dialogueCamera.Priority = previousCameraPriority;
            cameraOwned = false;
            StopTyping();
            if (voiceAudioSource != null) voiceAudioSource.Stop();
            if (continueButton != null) continueButton.gameObject.SetActive(false);
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        private void StopTyping()
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }

        private void UpdateLine(DialogueLine line)
        {
            if (line == null) return;
            if (speakerNameText != null) speakerNameText.text = line.speakerName;
            if (voiceAudioSource != null)
            {
                voiceAudioSource.Stop();
                if (line.voiceClip != null)
                {
                    voiceAudioSource.clip = line.voiceClip;
                    voiceAudioSource.Play();
                }
            }
            StopTyping();
            typingCoroutine = StartCoroutine(TypewriterRoutine(line.text ?? ""));
        }

        private IEnumerator TypewriterRoutine(string text)
        {
            isTyping = true;
            effectiveTypingSpeed = BullyingGame.UI.PlayerAccessibility.InstantText ? 0 : typingSpeed;
            if (continueButton != null) continueButton.gameObject.SetActive(false);
            if (dialogueText != null)
            {
                dialogueText.text = text;
                dialogueText.maxVisibleCharacters = 0;
                dialogueText.ForceMeshUpdate();
                int characters = dialogueText.textInfo.characterCount;
                for (int i = 1; i <= characters; i++)
                {
                    dialogueText.maxVisibleCharacters = i;
                    float elapsed = 0;
                    while (elapsed < effectiveTypingSpeed)
                    {
                        if (!IsPaused) elapsed += Time.deltaTime;
                        yield return null;
                    }
                }
                dialogueText.maxVisibleCharacters = int.MaxValue;
            }
            isTyping = false;
            typingCoroutine = null;
            if (continueButton != null) continueButton.gameObject.SetActive(true);
        }

        private bool IsPaused => GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentState == GameState.Paused;

        public void SkipOrNext()
        {
            if (subscribedManager == null || !subscribedManager.IsDialogueActive ||
                Time.frameCount <= startedFrame || IsPaused || subscribedManager.IsAwaitingChoice) return;
            if (isTyping)
            {
                StopTyping();
                if (dialogueText != null) dialogueText.maxVisibleCharacters = int.MaxValue;
                if (continueButton != null) continueButton.gameObject.SetActive(true);
            }
            else subscribedManager.NextLine();
        }

        private void OnContinueClicked() => SkipOrNext();

        private void OnDisable()
        {
            if (subscribedManager != null && subscribedManager.IsDialogueActive) subscribedManager.EndDialogue();
            UnsubscribeEvents();
            if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueClicked);
            HidePanel();
        }
    }
}
