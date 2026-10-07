using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using BullyingGame.Core;
using BullyingGame.Save;

namespace BullyingGame.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        [Header("Input")]
        [SerializeField] private InputActionReference pauseAction;
        private bool isPaused,actionWasEnabled,timelineWasPlaying;
        private GameState previousState;
        private PlayableDirector pausedDirector;
        private BullyingGame.Cinematics.VideoCutscenePlayer pausedVideo;
        private float previousTimeScale;
        private void Awake()
        {
            if(pausePanel!=null) pausePanel.SetActive(false);
            if(resumeButton!=null) resumeButton.onClick.AddListener(ResumeGame);
            if(restartButton!=null) restartButton.onClick.AddListener(RestartLevel);
            if(quitButton!=null) quitButton.onClick.AddListener(QuitGame);
        }
        private void OnEnable()
        {
            if(pauseAction==null || pauseAction.action==null) return;
            actionWasEnabled=pauseAction.action.enabled;
            pauseAction.action.performed+=PausePressed;pauseAction.action.Enable();
        }
        private void PausePressed(InputAction.CallbackContext context) => TogglePause();
        public void TogglePause() { if(isPaused) ResumeGame();else PauseGame(); }
        public void PauseGame()
        {
            var states=GameStateManager.Instance;
            if(isPaused || states==null || states.CurrentState==GameState.Loading || states.CurrentState==GameState.Boot ||
                states.CurrentState==GameState.MainMenu || states.CurrentState==GameState.Result) return;
            isPaused=true;previousState=states.CurrentState;previousTimeScale=Time.timeScale;
            pausedDirector=CutsceneManager.Instance!=null ? CutsceneManager.Instance.CurrentDirector : null;
            timelineWasPlaying=pausedDirector!=null && pausedDirector.state==PlayState.Playing;
            if(timelineWasPlaying) pausedDirector.Pause();
            pausedVideo=BullyingGame.Cinematics.VideoCutscenePlayer.Instance;
            if(pausedVideo!=null && pausedVideo.IsRunning) pausedVideo.PauseVideo();
            Time.timeScale=0;states.SetState(GameState.Paused);
            if(pausePanel!=null) pausePanel.SetActive(true);
        }
        public void ResumeGame()
        {
            if(!isPaused) return;
            isPaused=false;Time.timeScale=previousTimeScale;
            if(pausePanel!=null) pausePanel.SetActive(false);
            if(GameStateManager.Instance!=null && GameStateManager.Instance.CurrentState==GameState.Paused)
                GameStateManager.Instance.SetState(previousState);
            if(timelineWasPlaying && pausedDirector!=null && CutsceneManager.Instance!=null &&
                CutsceneManager.Instance.CurrentDirector==pausedDirector) pausedDirector.Resume();
            pausedDirector=null;
            if(pausedVideo!=null && pausedVideo.IsRunning) pausedVideo.ResumeVideo();
            pausedVideo=null;
        }
        public void RestartLevel()
        {
            ResumeGame();Time.timeScale=1;
            GameStateManager.Instance?.SetState(GameState.Loading);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        public void QuitGame() { ResumeGame();Application.Quit(); }
        private void OnDisable()
        {
            ResumeGame();
            if(pauseAction!=null && pauseAction.action!=null)
            {
                pauseAction.action.performed-=PausePressed;
                if(!actionWasEnabled) pauseAction.action.Disable();
            }
        }
    }
}
