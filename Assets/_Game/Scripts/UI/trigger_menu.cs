using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BullyingGame.UI
{
    /// <summary>
    /// Handles smooth transitions between the Main Menu (Canvas_Utama) and
    /// Chapter Select menu (Canvas_menu_ch), including smooth camera movement,
    /// Animator crossfade transitions, root-motion lock to prevent drift,
    /// and pressing Esc to return to the Main Menu.
    /// </summary>
    public class trigger_menu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject canvasUtama;
        [SerializeField] private GameObject canvasMenuCh;

        [Header("Camera Transition")]
        [SerializeField] private Transform targetCamera;
        [SerializeField] private Vector3 cameraWorldPosition = new Vector3(-54.72700119018555f, 0.9549999833106995f, 5.4120001792907715f);
        [SerializeField] private Quaternion cameraWorldRotation = new Quaternion(-0.02957443706691265f, -0.35656529664993286f, -0.027450673282146454f, 0.9333986639976501f);
        [SerializeField] private Vector3 cameraScale = new Vector3(1.0f, 1.0f, 1.0f);
        [SerializeField] private float cameraTransitionDuration = 0.75f;

        [Header("Character Animation")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private string selectMenuStateName = "pose-select-menu";
        [SerializeField] private string idleStateName = "iddle";
        [SerializeField] private float animationTransitionDuration = 0.35f;

        private Vector3 _initialCameraPosition;
        private Quaternion _initialCameraRotation;
        private Vector3 _initialCameraScale;
        private bool _hasCapturedInitialCamera;

        private Vector3 _initialCharacterPosition;
        private Quaternion _initialCharacterRotation;
        private bool _hasCapturedInitialCharacter;

        private bool _isInChapterMenu;
        private Coroutine _cameraTransitionCoroutine;

        private void Awake()
        {
            CaptureInitialStates();
        }

        private void Update()
        {
            if (!_isInChapterMenu)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ReturnToMainMenu();
            }
        }

        private void LateUpdate()
        {
            // Keep menu character anchored to its exact initial transform so repeated transitions never drift
            if (_hasCapturedInitialCharacter && characterAnimator != null)
            {
                characterAnimator.transform.SetPositionAndRotation(_initialCharacterPosition, _initialCharacterRotation);
            }
        }

        private void CaptureInitialStates()
        {
            if (!_hasCapturedInitialCamera && targetCamera != null)
            {
                _initialCameraPosition = targetCamera.position;
                _initialCameraRotation = targetCamera.rotation.normalized;
                _initialCameraScale = targetCamera.localScale;
                _hasCapturedInitialCamera = true;
            }

            if (!_hasCapturedInitialCharacter && characterAnimator != null)
            {
                characterAnimator.applyRootMotion = false;
                _initialCharacterPosition = characterAnimator.transform.position;
                _initialCharacterRotation = characterAnimator.transform.rotation.normalized;
                _hasCapturedInitialCharacter = true;
            }
        }

        /// <summary>
        /// Transitions from Main Menu to Chapter Select:
        /// activates Canvas_menu_ch, deactivates Canvas_Utama,
        /// smoothly moves the camera, and crossfades to pose-select-menu.
        /// </summary>
        public void TriggerMenu()
        {
            CaptureInitialStates();

            if (_isInChapterMenu)
            {
                return;
            }

            _isInChapterMenu = true;

            if (canvasMenuCh != null)
            {
                canvasMenuCh.SetActive(true);
            }

            if (canvasUtama != null)
            {
                canvasUtama.SetActive(false);
            }

            StartCameraTransition(cameraWorldPosition, cameraWorldRotation.normalized, cameraScale);
            PlayAnimatorCrossFade(selectMenuStateName);
        }

        /// <summary>
        /// Returns from Chapter Select back to Main Menu:
        /// deactivates Canvas_menu_ch, reactivates Canvas_Utama,
        /// smoothly moves the camera back, and crossfades back to idle.
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (!_isInChapterMenu)
            {
                return;
            }

            _isInChapterMenu = false;

            if (canvasMenuCh != null)
            {
                canvasMenuCh.SetActive(false);
            }

            if (canvasUtama != null)
            {
                canvasUtama.SetActive(true);
            }

            if (_hasCapturedInitialCamera)
            {
                StartCameraTransition(_initialCameraPosition, _initialCameraRotation, _initialCameraScale);
            }

            PlayAnimatorCrossFade(idleStateName);
        }

        private void StartCameraTransition(Vector3 destinationPos, Quaternion destinationRot, Vector3 destinationScale)
        {
            if (targetCamera == null)
            {
                return;
            }

            if (_cameraTransitionCoroutine != null)
            {
                StopCoroutine(_cameraTransitionCoroutine);
            }

            _cameraTransitionCoroutine = StartCoroutine(TransitionCameraRoutine(destinationPos, destinationRot.normalized, destinationScale));
        }

        private IEnumerator TransitionCameraRoutine(Vector3 destinationPos, Quaternion destinationRot, Vector3 destinationScale)
        {
            Vector3 startPos = targetCamera.position;
            Quaternion startRot = targetCamera.rotation.normalized;
            Vector3 startScale = targetCamera.localScale;

            float duration = Mathf.Max(0.01f, cameraTransitionDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);

                targetCamera.SetPositionAndRotation(
                    Vector3.Lerp(startPos, destinationPos, eased),
                    Quaternion.Slerp(startRot, destinationRot, eased)
                );
                targetCamera.localScale = Vector3.Lerp(startScale, destinationScale, eased);

                yield return null;
            }

            targetCamera.SetPositionAndRotation(destinationPos, destinationRot);
            targetCamera.localScale = destinationScale;
            _cameraTransitionCoroutine = null;
        }

        private void PlayAnimatorCrossFade(string stateName)
        {
            if (characterAnimator == null || string.IsNullOrEmpty(stateName))
            {
                return;
            }

            characterAnimator.CrossFadeInFixedTime(stateName, Mathf.Max(0f, animationTransitionDuration), 0);
        }
    }
}
