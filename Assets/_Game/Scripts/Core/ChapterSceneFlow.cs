using UnityEngine;
using UnityEngine.SceneManagement;

namespace BullyingGame.Core
{
    public static class ChapterSceneFlow
    {
        public static bool CanLoad(string sceneName) => !string.IsNullOrWhiteSpace(sceneName) &&
            Application.CanStreamedLevelBeLoaded(sceneName);

        public static bool Load(string sceneName)
        {
            if (!CanLoad(sceneName))
            {
                Debug.LogError("Chapter scene is missing from Build Settings: " + sceneName);
                return false;
            }
            Time.timeScale = 1f;
            GameStateManager.Instance?.SetState(GameState.Loading);
            SceneManager.LoadSceneAsync(sceneName);
            return true;
        }
    }
}
