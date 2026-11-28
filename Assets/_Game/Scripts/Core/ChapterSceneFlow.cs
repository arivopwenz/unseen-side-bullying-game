using UnityEngine;
using UnityEngine.SceneManagement;
using BullyingGame.UI;

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
            if (!CanLoad(sceneName))
            {
                Debug.LogError("Chapter scene is missing from Build Settings: " + sceneName);
                return false;
            }

            var loadingScreen = LoadingScreenController.EnsureInstance();
            if (loadingScreen != null)
            {
                if (loadingScreen.IsLoading) return destination == sceneName;
                destination = sceneName;
                return loadingScreen.LoadScene(sceneName);
            }

            if (pending != null && !pending.isDone) return destination == sceneName;
            Time.timeScale = 1f;
            GameStateManager.Instance?.SetState(GameState.Loading);
            destination = sceneName;
            pending = SceneManager.LoadSceneAsync(sceneName);
            return pending != null;
        }
    }
}
