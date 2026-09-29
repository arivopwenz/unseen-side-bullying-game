using System;
using UnityEngine;

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

        private QTEData currentQTE;
        private float remainingTime;
        private int currentProgress;

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
            if (IsQTEActive || qteData == null) return;

            currentQTE = qteData;
            remainingTime = qteData.timeLimit;
            currentProgress = 0;
            IsQTEActive = true;

            OnQTEStarted?.Invoke(qteData);
        }

        private void Update()
        {
            if (!IsQTEActive) return;

            remainingTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(remainingTime / currentQTE.timeLimit);

            if (remainingTime <= 0f)
            {
                EndQTE(QTEResult.Failed);
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            switch (currentQTE.qteType)
            {
                case QTEType.SinglePress:
                    if (Input.GetKeyDown(KeyCode.E))
                        EndQTE(QTEResult.Success);
                    break;

                case QTEType.RapidPress:
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        currentProgress++;
                        OnProgressUpdated?.Invoke(currentProgress);
                        if (currentProgress >= currentQTE.requiredPressCount)
                            EndQTE(QTEResult.Success);
                    }
                    break;

                case QTEType.HoldButton:
                    if (Input.GetKey(KeyCode.E))
                    {
                        currentProgress++;
                        float holdRatio = (float)currentProgress / (currentQTE.timeLimit * 60f);
                        OnProgressUpdated?.Invoke(currentProgress);
                        if (holdRatio >= 0.9f)
                            EndQTE(QTEResult.Success);
                    }
                    break;
            }
        }

        private void EndQTE(QTEResult result)
        {
            IsQTEActive = false;
            currentQTE = null;
            OnQTEEnded?.Invoke(result);
        }
    }
}
