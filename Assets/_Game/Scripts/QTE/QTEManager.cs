using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BullyingGame.QTE
{
    public class QTEManager : MonoBehaviour
    {
        public static QTEManager Instance { get; private set; }

        public event Action<QTEData> OnQTEStarted;
        public event Action<QTEResult> OnQTEEnded;
        public event Action<float> OnTimerUpdated;
        public event Action<int> OnProgressUpdated;

        public bool IsQTEActive { get; private set; }
        public bool LastQTECancelled { get; private set; }
        public QTEData CurrentQTE => currentQTE;
        public float RemainingTime => Mathf.Max(0f, remainingTime);
        public int CurrentProgress => currentProgress;

        private QTEData currentQTE;
        private float remainingTime;
        private int currentProgress;
        [Header("Input System")]
        [SerializeField] private InputActionReference tapAction;
        private InputAction activeAction;
        private bool actionWasEnabled, readyForPress;
        private float heldTime;
        private int startedFrame;
        public string InputDisplayName => tapAction != null && tapAction.action != null
            ? tapAction.action.GetBindingDisplayString() : "input";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartQTE(QTEData qteData)
        {
            if (!TryStartQTE(qteData)) Debug.LogWarning("QTE not ready: check Tap action and data.", this);
        }

        public bool TryStartQTE(QTEData qteData)
        {
            if (!isActiveAndEnabled || IsQTEActive || qteData == null || qteData.timeLimit <= 0 ||
                qteData.requiredPressCount <= 0 || qteData.qteType == QTEType.Sequence ||
                (qteData.qteType == QTEType.HoldButton &&
                 (qteData.HoldDuration <= 0 || qteData.HoldDuration > qteData.timeLimit)) ||
                tapAction == null || tapAction.action == null || tapAction.action.type != InputActionType.Button)
                return false;

            currentQTE = qteData;
            LastQTECancelled = false;
            remainingTime = qteData.timeLimit;
            currentProgress = 0;
            IsQTEActive = true;
            heldTime = 0;
            startedFrame = Time.frameCount;
            activeAction = tapAction.action;
            actionWasEnabled = activeAction.enabled;
            readyForPress = !activeAction.IsPressed();
            activeAction.performed += OnTap;
            activeAction.Enable();
            OnQTEStarted?.Invoke(qteData);
            OnTimerUpdated?.Invoke(1);
            OnProgressUpdated?.Invoke(0);
            return true;
        }

        private void Update()
        {
            if (!IsQTEActive || IsPaused || Time.frameCount <= startedFrame) return;
            if (!readyForPress && !activeAction.IsPressed()) readyForPress = true;

            remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
            OnTimerUpdated?.Invoke(remainingTime / currentQTE.timeLimit);

            if (remainingTime <= 0f)
            {
                EndQTE(QTEResult.Failed);
                return;
            }

            if (currentQTE.qteType != QTEType.HoldButton || !readyForPress) return;
            heldTime = activeAction.IsPressed() ? heldTime + Time.deltaTime : 0;
            int progress = Mathf.Clamp(Mathf.RoundToInt(100 * heldTime / currentQTE.HoldDuration), 0, 100);
            if (progress != currentProgress)
            {
                currentProgress = progress;
                OnProgressUpdated?.Invoke(progress);
            }
            if (heldTime >= currentQTE.HoldDuration) EndQTE(QTEResult.Success);
        }

        private bool IsPaused => BullyingGame.Core.GameStateManager.Instance != null &&
            BullyingGame.Core.GameStateManager.Instance.CurrentState == BullyingGame.Core.GameState.Paused;

        private void OnTap(InputAction.CallbackContext context)
        {
            if (!IsQTEActive || !readyForPress || IsPaused || Time.frameCount <= startedFrame) return;
            if (currentQTE.qteType == QTEType.SinglePress) EndQTE(QTEResult.Success);
            else if (currentQTE.qteType == QTEType.RapidPress)
            {
                currentProgress++;
                OnProgressUpdated?.Invoke(currentProgress);
                if (currentProgress >= currentQTE.requiredPressCount) EndQTE(QTEResult.Success);
            }
        }

        private void EndQTE(QTEResult result, bool cancelled = false)
        {
            if (!IsQTEActive) return;
            IsQTEActive = false;
            LastQTECancelled = cancelled;
            if (result == QTEResult.Success)
            {
                currentProgress = currentQTE.qteType == QTEType.HoldButton ? 100 :
                    currentQTE.qteType == QTEType.RapidPress ? currentQTE.requiredPressCount : 1;
                OnProgressUpdated?.Invoke(currentProgress);
            }
            if (activeAction != null)
            {
                activeAction.performed -= OnTap;
                if (!actionWasEnabled) activeAction.Disable();
            }
            activeAction = null;
            currentQTE = null;
            OnQTEEnded?.Invoke(result);
        }

        public void CancelQTE() => EndQTE(QTEResult.Failed, true);
        private void OnDisable() => CancelQTE();
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
