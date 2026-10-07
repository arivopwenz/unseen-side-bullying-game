using UnityEngine;
using BullyingGame.Core;

namespace BullyingGame.UI
{
    public class MobileGameplayUI : MonoBehaviour
    {
        [Header("Touch Controls")]
        [SerializeField] private GameObject movementGroup;
        [SerializeField] private GameObject qteButton;
        [SerializeField] private GameObject pauseButton;
        [SerializeField] private bool previewInEditor;
        private void Update()
        {
            bool mobile = Application.isMobilePlatform || previewInEditor;
            var state = GameStateManager.Instance != null ? GameStateManager.Instance.CurrentState : GameState.Boot;
            if (movementGroup != null) movementGroup.SetActive(mobile && state == GameState.Playing);
            if (qteButton != null) qteButton.SetActive(mobile && state == GameState.QTE);
            if (pauseButton != null) pauseButton.SetActive(mobile && state != GameState.Result && state != GameState.Paused);
        }
    }
}
