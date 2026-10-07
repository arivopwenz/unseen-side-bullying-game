using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BullyingGame.Core;

namespace BullyingGame.Core
{
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.8f;
        public bool IsLoading { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName)
        {
            if(IsLoading || string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName)) return;
            IsLoading=true;
            Time.timeScale=1;
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            GameStateManager.Instance?.SetState(GameState.Loading);

            if (fadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeRoutine(1f));
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            if (fadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeRoutine(0f));
            }

            IsLoading=false;
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration && fadeCanvasGroup != null)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            if(fadeCanvasGroup != null) fadeCanvasGroup.alpha = targetAlpha;
        }
        private void OnDestroy() { if(Instance==this) Instance=null; }
    }
}
