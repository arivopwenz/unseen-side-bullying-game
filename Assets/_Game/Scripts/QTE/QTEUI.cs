using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.QTE
{
    public class QTEUI : MonoBehaviour
    {
        [Header("Panel dan Instruksi")]
        [SerializeField] private GameObject qtePanel;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI resultText;
        [Header("Waktu")]
        [SerializeField] private Image timerBar;
        [SerializeField] private TextMeshProUGUI timerText;
        [Header("Progres")]
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [Header("Hasil")]
        [SerializeField] private Color successColor = new Color(0.43f, 0.9f, 0.72f);
        [SerializeField] private Color failureColor = new Color(1f, 0.75f, 0.4f);
        private QTEManager subscribedManager;
        private QTEData displayedQTE;

        private void Start()
        {
            if (qtePanel != null) qtePanel.SetActive(false);
            SubscribeEvents();
            if (subscribedManager != null && subscribedManager.IsQTEActive)
                ShowQTE(subscribedManager.CurrentQTE);
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (subscribedManager != null || QTEManager.Instance == null) return;
            subscribedManager = QTEManager.Instance;
            subscribedManager.OnQTEStarted += ShowQTE;
            subscribedManager.OnQTEEnded += ShowResult;
            subscribedManager.OnTimerUpdated += UpdateTimer;
            subscribedManager.OnProgressUpdated += UpdateProgress;
            if (subscribedManager.IsQTEActive) ShowQTE(subscribedManager.CurrentQTE);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(HidePanel));
            if (subscribedManager == null) return;
            subscribedManager.OnQTEStarted -= ShowQTE;
            subscribedManager.OnQTEEnded -= ShowResult;
            subscribedManager.OnTimerUpdated -= UpdateTimer;
            subscribedManager.OnProgressUpdated -= UpdateProgress;
            subscribedManager = null;
        }

        private void ShowQTE(QTEData data)
        {
            if (data == null) return;
            displayedQTE = data;
            CancelInvoke(nameof(HidePanel));
            if (qtePanel != null) qtePanel.SetActive(true);
            if (resultText != null) resultText.text = "";
            float remaining = subscribedManager != null ? subscribedManager.RemainingTime : data.timeLimit;
            UpdateTimer(remaining / data.timeLimit);
            UpdateProgress(subscribedManager != null ? subscribedManager.CurrentProgress : 0);
            if (instructionText == null) return;
            string input = subscribedManager != null ? subscribedManager.InputDisplayName : "input";
            instructionText.fontSize=data.NarrativeEffort ? 23 : 26;
            if(data.NarrativeEffort) { instructionText.text=data.NarrativeInstruction.Replace("{input}",input);return; }

            switch (data.qteType)
            {
                case QTEType.SinglePress:
                    instructionText.text = $"Tekan [{input}] sekarang!";
                    break;
                case QTEType.RapidPress:
                    instructionText.text = $"Tekan [{input}] sebanyak {data.requiredPressCount} kali!";
                    break;
                case QTEType.HoldButton:
                    instructionText.text = $"Tahan [{input}] selama {data.HoldDuration:0.#} detik!";
                    break;
            }
        }

        private void UpdateTimer(float ratio)
        {
            if (timerBar != null) timerBar.fillAmount = Mathf.Clamp01(ratio);
            if (timerText != null && subscribedManager != null)
                timerText.text = $"{subscribedManager.RemainingTime:0.0} dtk";
        }

        private void UpdateProgress(int progress)
        {
            if (displayedQTE == null) return;
            int target = displayedQTE.qteType == QTEType.HoldButton ? 100 :
                displayedQTE.qteType == QTEType.RapidPress ? displayedQTE.requiredPressCount : 1;
            float ratio = target > 0 ? Mathf.Clamp01((float)progress / target) : 0f;
            if (progressBar != null) progressBar.fillAmount = ratio;
            if (progressText != null)
                progressText.text = displayedQTE.qteType == QTEType.HoldButton
                    ? $"{Mathf.RoundToInt(ratio * 100)}%" : $"{Mathf.Min(progress, target)}/{target}";
        }

        private void ShowResult(QTEResult result)
        {
            if(displayedQTE != null && displayedQTE.NarrativeEffort) { HidePanel();return; }
            if (resultText == null) { HidePanel(); return; }
            switch (result)
            {
                case QTEResult.Success:
                    resultText.text = "BERHASIL!";
                    resultText.color = successColor;
                    break;
                case QTEResult.Failed:
                    resultText.text = "BELUM BERHASIL";
                    resultText.color = failureColor;
                    break;
            }

            Invoke(nameof(HidePanel), 2f);
        }

        private void HidePanel()
        {
            if (qtePanel != null) qtePanel.SetActive(false);
        }
    }
}
