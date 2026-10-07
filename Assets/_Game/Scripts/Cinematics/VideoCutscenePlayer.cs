using System;
using UnityEngine;
using UnityEngine.Video;
using BullyingGame.Core;

namespace BullyingGame.Cinematics
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoCutscenePlayer : MonoBehaviour
    {
        public static VideoCutscenePlayer Instance { get; private set; }
        public event Action OnVideoStarted;
        public event Action OnVideoFinished;
        [Header("Video")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private GameObject videoScreenUI;
        [SerializeField,Min(1)] private float prepareTimeout=10;
        [Header("Gameplay")]
        [SerializeField] private Behaviour[] controls;
        public bool IsRunning { get; private set; }
        private bool[] enabledStates;
        private CursorLockMode cursor;
        private bool visible;
        private float deadline;
        private bool paused;
        private void Awake()
        {
            if(Instance!=null && Instance!=this) { Destroy(gameObject);return; }
            Instance=this;
            if(videoPlayer==null) videoPlayer=GetComponent<VideoPlayer>();
            videoPlayer.playOnAwake=false;videoPlayer.isLooping=false;
            videoPlayer.loopPointReached+=Ended;videoPlayer.prepareCompleted+=Prepared;videoPlayer.errorReceived+=Error;
            if(videoScreenUI!=null) videoScreenUI.SetActive(false);
        }
        public void PlayVideo(VideoClip clip) => TryPlayVideo(clip);
        public bool TryPlayVideo(VideoClip clip)
        {
            if(!isActiveAndEnabled || IsRunning || clip==null || videoPlayer==null || GameStateManager.Instance==null ||
                (CutsceneManager.Instance!=null && CutsceneManager.Instance.CurrentDirector!=null)) return false;
            IsRunning=true;deadline=Time.unscaledTime+prepareTimeout;
            cursor=Cursor.lockState;visible=Cursor.visible;
            enabledStates=new bool[controls==null ? 0 : controls.Length];
            for(int i=0;i<enabledStates.Length;i++) if(controls[i]!=null)
            {
                enabledStates[i]=controls[i].enabled;
                if(controls[i] is BullyingGame.Player.PlayerMovement movement) { movement.SetMoveInput(Vector2.zero);movement.SetSprintInput(false); }
                controls[i].enabled=false;
            }
            videoPlayer.clip=clip;
            if(videoScreenUI!=null) videoScreenUI.SetActive(true);
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            GameStateManager.Instance.SetState(GameState.Cinematic);
            OnVideoStarted?.Invoke();videoPlayer.Prepare();return true;
        }
        private void Update()
        {
            if (IsRunning && paused) { deadline += Time.unscaledDeltaTime; return; }
            if(IsRunning && !videoPlayer.isPrepared && Time.unscaledTime>deadline)
            { Debug.LogWarning("Video intro belum siap; lanjutkan ke Timeline.",this);Finish(true); }
        }
        private void Prepared(VideoPlayer source) { if(IsRunning && !paused) source.Play(); }
        public void PauseVideo() { if(IsRunning) { paused=true; videoPlayer.Pause(); } }
        public void ResumeVideo() { if(IsRunning) { paused=false; if(videoPlayer.isPrepared) videoPlayer.Play(); } }
        private void Ended(VideoPlayer source) => Finish(true);
        private void Error(VideoPlayer source,string message) { Debug.LogWarning("Video intro: "+message,this);Finish(true); }
        public void SkipVideo() => Finish(true);
        private void Finish(bool notify)
        {
            if(!IsRunning) return;
            IsRunning=false;paused=false;videoPlayer.Stop();
            if(videoScreenUI!=null) videoScreenUI.SetActive(false);
            for(int i=0;i<enabledStates.Length;i++) if(controls[i]!=null) controls[i].enabled=enabledStates[i];
            Cursor.lockState=cursor;Cursor.visible=visible;
            if(GameStateManager.Instance!=null && GameStateManager.Instance.CurrentState==GameState.Cinematic)
                GameStateManager.Instance.SetState(GameState.Playing);
            if(notify) OnVideoFinished?.Invoke();
        }
        private void OnDisable() => Finish(false);
        private void OnDestroy()
        {
            if(videoPlayer!=null) { videoPlayer.loopPointReached-=Ended;videoPlayer.prepareCompleted-=Prepared;videoPlayer.errorReceived-=Error; }
            if(Instance==this) Instance=null;
        }
    }
}
