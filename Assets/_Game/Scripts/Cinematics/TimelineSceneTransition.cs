using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using BullyingGame.Core;

namespace BullyingGame.Cinematics
{
    /// <summary>
    /// Automatically transitions to the target scene via ChapterSceneFlow (using the Global Loading Screen)
    /// once the assigned PlayableDirector finishes playing its Timeline.
    /// </summary>
    public class TimelineSceneTransition : MonoBehaviour
    {
        [Header("Timeline & Destination")]
        [SerializeField] private PlayableDirector director;
        [SerializeField] private string nextSceneName = "Prologue Keluar Mobil";
        [SerializeField, Min(0f)] private float delayAfterEnd = 0f;

        private bool _hasTriggered;

        private void Awake()
        {
            if (director == null)
            {
                director = GetComponent<PlayableDirector>();
            }
        }

        private void OnEnable()
        {
            if (director != null)
            {
                director.stopped += OnDirectorStopped;
            }
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.stopped -= OnDirectorStopped;
            }
        }

        private void Start()
        {
            GameStateManager.Instance?.SetState(GameState.Cinematic);
        }

        private void OnDirectorStopped(PlayableDirector stoppedDirector)
        {
            if (_hasTriggered || string.IsNullOrWhiteSpace(nextSceneName))
            {
                return;
            }

            // Ignore premature stops when exiting play mode or disabling the object
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                return;
            }

            _hasTriggered = true;
            if (delayAfterEnd > 0f)
            {
                StartCoroutine(LoadAfterDelay());
            }
            else
            {
                ChapterSceneFlow.Load(nextSceneName);
            }
        }

        private IEnumerator LoadAfterDelay()
        {
            yield return new WaitForSecondsRealtime(delayAfterEnd);
            ChapterSceneFlow.Load(nextSceneName);
        }
    }
}
