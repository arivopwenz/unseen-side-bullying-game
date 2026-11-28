using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BullyingGame.Save;
using BullyingGame.Audio;

namespace BullyingGame.Core
{
    [DefaultExecutionOrder(-150)]
    public class BootstrapLauncher : MonoBehaviour
    {
        public static void EnsurePersistentSystems()
        {
            if (GameStateManager.Instance == null) new GameObject("GameStateManager").AddComponent<GameStateManager>();
            if (SaveManager.Instance == null) new GameObject("SaveManager").AddComponent<SaveManager>();
            if (AudioManager.Instance == null) new GameObject("AudioManager").AddComponent<AudioManager>();
            if (ChapterManager.Instance == null) new GameObject("ChapterManager").AddComponent<ChapterManager>();
            BullyingGame.UI.LoadingScreenController.EnsureInstance();
        }
        private void Awake() => EnsurePersistentSystems();
        private IEnumerator Start()
        {
            var loadingScreen = BullyingGame.UI.LoadingScreenController.EnsureInstance();
            if (loadingScreen != null)
            {
                loadingScreen.LoadScene("01_MainMenu");
                yield break;
            }
            GameStateManager.Instance.SetState(GameState.Loading);
            yield return SceneManager.LoadSceneAsync("01_MainMenu");
        }
    }
}
