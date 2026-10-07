using UnityEngine;

namespace BullyingGame.Core
{
    // Supports direct level play in the Editor and reuses the Bootstrap manager in builds.
    [DefaultExecutionOrder(-100)]
    public class LevelGameplayEntry : MonoBehaviour
    {
        private void Awake()
        {
            if (GameStateManager.Instance == null)
                new GameObject("GameStateManager").AddComponent<GameStateManager>();
        }

        private void Start()
        {
            var states = GameStateManager.Instance;
            if (states != null && (states.CurrentState == GameState.Boot ||
                states.CurrentState == GameState.Loading || states.CurrentState == GameState.MainMenu))
                states.SetState(GameState.Playing);
        }
    }
}
