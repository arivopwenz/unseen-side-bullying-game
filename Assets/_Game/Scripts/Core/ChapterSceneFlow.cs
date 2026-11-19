using UnityEngine;
using UnityEngine.SceneManagement;

namespace BullyingGame.Core
{
    public static class ChapterSceneFlow
    {
        private static AsyncOperation pending;
        private static string destination;

        public static bool CanLoad(string sceneName) => !string.IsNullOrWhiteSpace(sceneName) &&
            Application.CanStreamedLevelBeLoaded(sceneName);

        public static bool Load(string sceneName)
        {
            if (pending != null && !pending.isDone) return destination == sceneName;
            if (!CanLoad(sceneName))
            {
                Debug.LogError("Chapter scene is missing from Build Settings: " + sceneName);
                return false;
            }
            Time.timeScale = 1f;
            GameStateManager.Instance?.SetState(GameState.Loading);
            destination = sceneName;
            pending = SceneManager.LoadSceneAsync(sceneName);
            return pending != null;
        }
    }
}
