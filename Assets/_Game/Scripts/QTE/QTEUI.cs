using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BullyingGame.QTE
{
    public class QTEUI : MonoBehaviour
    {
        [SerializeField] private GameObject qtePanel;
        [SerializeField] private Image timerBar;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI resultText;

        private void Start()
        {
            qtePanel.SetActive(false);
        }

        private void OnEnable()
        {
            QTEManager.Instance.OnQTEStarted += ShowQTE;
            QTEManager.Instance.OnQTEEnded += ShowResult;
            QTEManager.Instance.OnTimerUpdated += UpdateTimer;
        }

        private void OnDisable()
        {
            if (QTEManager.Instance == null) return;
            QTEManager.Instance.OnQTEStarted -= ShowQTE;
            QTEManager.Instance.OnQTEEnded -= ShowResult;
            QTEManager.Instance.OnTimerUpdated -= UpdateTimer;
        }

        private void ShowQTE(QTEData data)
        {
            qtePanel.SetActive(true);
            resultText.text = "";
            timerBar.fillAmount = 1f;

            switch (data.qteType)
            {
                case QTEType.SinglePress:
                    instructionText.text = "Tekan [E] sekarang!";
                    break;
                case QTEType.RapidPress:
                    instructionText.text = $"Tekan [E] sebanyak {data.requiredPressCount} kali!";
                    break;
                case QTEType.HoldButton:
                    instructionText.text = "Tahan [E]!";
                    break;
            }
        }

        private void UpdateTimer(float ratio)
        {
            timerBar.fillAmount = Mathf.Clamp01(ratio);
        }

        private void ShowResult(QTEResult result)
        {
            switch (result)
            {
                case QTEResult.Success:
                    resultText.text = "BERHASIL!";
                    break;
                case QTEResult.Failed:
                    resultText.text = "GAGAL!";
                    break;
            }

            Invoke(nameof(HidePanel), 2f);
        }

        private void HidePanel()
        {
            qtePanel.SetActive(false);
        }
    }
}
