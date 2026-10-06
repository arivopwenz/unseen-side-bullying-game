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
        public PlayableDirector CurrentDirector { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool TryPlayCutscene(PlayableDirector director)
        {
            if (!isActiveAndEnabled || director == null ||
                !director.isActiveAndEnabled || director.playableAsset == null ||
                CurrentDirector != null || GameStateManager.Instance == null)
                return false;
            CurrentDirector = director;
            director.stopped += OnDirectorStopped;
            director.time = 0;
            GameStateManager.Instance.SetState(GameState.Cinematic);
            OnCutsceneStarted?.Invoke();
            director.Play();
            return true;
        }

        public void PlayCutscene(PlayableDirector director) => TryPlayCutscene(director);
        public void SkipCutscene() => StopCutscene(CurrentDirector);

        public void PauseCutscene(PlayableDirector director)
        {
            if (director != null && director == CurrentDirector) director.Pause();
        }

        public void ResumeCutscene(PlayableDirector director)
        {
            if (director != null && director == CurrentDirector) director.Resume();
        }

        public void StopCutscene(PlayableDirector director)
        {
            if (director == null || director != CurrentDirector) return;
            director.Stop();
            // Fallback jika playback belum sempat memasuki Playing.
            if (CurrentDirector == director) OnDirectorStopped(director);
        }

        private void OnDirectorStopped(PlayableDirector director)
        {
            if (director != CurrentDirector) return;
            director.stopped -= OnDirectorStopped;
            CurrentDirector = null;
            OnCutsceneEnded?.Invoke();
            // Jangan menimpa loading/paused/dialogue yang mungkin dimulai listener.
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Cinematic)
                GameStateManager.Instance.SetState(GameState.Playing);
        }

        private void OnDisable() => StopCutscene(CurrentDirector);

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
