using System;
using UnityEngine;
using UnityEngine.Playables;

namespace BullyingGame.Core
{
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }

        public event Action OnCutsceneStarted;
        public event Action OnCutsceneEnded;

        private PlayableDirector currentDirector;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void PlayCutscene(PlayableDirector director)
        {
            if (director == null || currentDirector != null) return;

            currentDirector = director;
            currentDirector.stopped += OnDirectorStopped;
            currentDirector.Play();

            GameManager.Instance?.SetState(GameState.Cinematic);
            OnCutsceneStarted?.Invoke();
        }

        public void SkipCutscene()
        {
            if (currentDirector == null) return;
            currentDirector.time = currentDirector.duration;
            currentDirector.Evaluate();
            currentDirector.Stop();
        }

        private void OnDirectorStopped(PlayableDirector director)
        {
            director.stopped -= OnDirectorStopped;
            currentDirector = null;

            GameManager.Instance?.SetState(GameState.Playing);
            OnCutsceneEnded?.Invoke();
        }
    }
}
