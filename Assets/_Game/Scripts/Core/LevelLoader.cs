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
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            GameManager.Instance?.SetState(GameState.Loading);

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

            GameManager.Instance?.SetState(GameState.Playing);
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}
