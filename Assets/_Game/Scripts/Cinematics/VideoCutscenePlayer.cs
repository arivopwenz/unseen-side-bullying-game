using System;
using UnityEngine;
using UnityEngine.Video;
using BullyingGame.Core;

namespace BullyingGame.Cinematics
{
    public class VideoCutscenePlayer : MonoBehaviour
    {
        public static VideoCutscenePlayer Instance { get; private set; }

        public event Action OnVideoStarted;
        public event Action OnVideoFinished;

        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private GameObject videoScreenUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }

            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached += OnVideoEndReached;
            }

            if (videoScreenUI != null)
            {
                videoScreenUI.SetActive(false);
            }
        }

        public void PlayVideo(VideoClip clip)
        {
            if (videoPlayer == null || clip == null) return;

            videoPlayer.clip = clip;

            if (videoScreenUI != null)
            {
                videoScreenUI.SetActive(true);
            }

            GameManager.Instance?.SetState(GameState.Cinematic);
            videoPlayer.Play();
            OnVideoStarted?.Invoke();
        }

        public void SkipVideo()
        {
            if (videoPlayer == null || !videoPlayer.isPlaying) return;

            videoPlayer.Stop();
            FinishVideoPlayback();
        }

        private void OnVideoEndReached(VideoPlayer source)
        {
            FinishVideoPlayback();
        }

        private void FinishVideoPlayback()
        {
            if (videoScreenUI != null)
            {
                videoScreenUI.SetActive(false);
            }

            GameManager.Instance?.SetState(GameState.Playing);
            OnVideoFinished?.Invoke();
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoEndReached;
            }
        }
    }
}
