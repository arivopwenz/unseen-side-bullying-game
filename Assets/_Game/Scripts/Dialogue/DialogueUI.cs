using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace BullyingGame.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button continueButton;

        [Header("Camera")]
        [SerializeField] private Unity.Cinemachine.CinemachineCamera dialogueCamera;

        [Header("Voice & Audio")]
        [SerializeField] private AudioSource voiceAudioSource;

        [Header("Typewriter Settings")]
        [SerializeField] private float typingSpeed = 0.035f;

        private Coroutine typingCoroutine;
        private bool isTyping;
        private string currentFullText = "";

        private void Awake()
        {
            if (voiceAudioSource == null)
            {
                voiceAudioSource = GetComponent<AudioSource>();
            }
        }

        private void Update()
        {
            if (dialoguePanel != null && dialoguePanel.activeSelf)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.eKey.wasPressedThisFrame ||
                        Keyboard.current.enterKey.wasPressedThisFrame)
                    {
                        SkipOrNext();
                    }
                }
            }
        }

        private void Start()
        {
            if (dialogueCamera != null)
            {
                dialogueCamera.Priority = 0;
            }
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
                continueButton.gameObject.SetActive(false);
            }
            SubscribeEvents();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (DialogueManager.Instance == null) return;
            DialogueManager.Instance.OnDialogueStarted -= ShowPanel;
            DialogueManager.Instance.OnLineDisplayed -= UpdateLine;
            DialogueManager.Instance.OnDialogueEnded -= HidePanel;

            DialogueManager.Instance.OnDialogueStarted += ShowPanel;
            DialogueManager.Instance.OnLineDisplayed += UpdateLine;
            DialogueManager.Instance.OnDialogueEnded += HidePanel;
        }

        private void UnsubscribeEvents()
        {
            if (DialogueManager.Instance == null) return;
            DialogueManager.Instance.OnDialogueStarted -= ShowPanel;
            DialogueManager.Instance.OnLineDisplayed -= UpdateLine;
            DialogueManager.Instance.OnDialogueEnded -= HidePanel;
        }

        private void ShowPanel()
        {
            if (dialogueCamera != null)
            {
                dialogueCamera.Priority = 20;
            }
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
        }

        private void HidePanel()
        {
            if (dialogueCamera != null)
            {
                dialogueCamera.Priority = 0;
            }
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            isTyping = false;
            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
            }
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private void UpdateLine(DialogueLine line)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = line.speakerName;
            }

            if (voiceAudioSource != null)
            {
                voiceAudioSource.Stop();
                if (line.voiceClip != null)
                {
                    voiceAudioSource.clip = line.voiceClip;
                    voiceAudioSource.Play();
                }
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            currentFullText = line.text;
            typingCoroutine = StartCoroutine(TypewriterRoutine(line.text));
        }

        private IEnumerator TypewriterRoutine(string text)
        {
            isTyping = true;
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }

            if (dialogueText != null)
            {
                dialogueText.text = "";
                for (int i = 0; i < text.Length; i++)
                {
                    dialogueText.text += text[i];
                    yield return new WaitForSeconds(typingSpeed);
                }
            }

            isTyping = false;
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
            }
        }

        public void SkipOrNext()
        {
            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                if (dialogueText != null)
                {
                    dialogueText.text = currentFullText;
                }
                isTyping = false;
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(true);
                }
            }
            else
            {
                DialogueManager.Instance.NextLine();
            }
        }

        private void OnContinueClicked()
        {
            SkipOrNext();
        }
    }
}
