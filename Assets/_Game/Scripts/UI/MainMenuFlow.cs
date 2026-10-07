using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BullyingGame.Core;
using BullyingGame.Save;

namespace BullyingGame.UI
{
    [DefaultExecutionOrder(-120)]
    public class MainMenuFlow : MonoBehaviour
    {
        [Header("Menu")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text statusText;
        [Header("Chapter Scenes")]
        [SerializeField] private NarrativeChapterData[] chapters;
        private bool loading;
        private void Awake() => BootstrapLauncher.EnsurePersistentSystems();
        private void Start()
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameStateManager.Instance.SetState(GameState.MainMenu);
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", .8f);
            if (newGameButton != null) newGameButton.onClick.AddListener(NewGame);
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ContinueGame);
                continueButton.interactable = SaveManager.Instance.HasSave;
            }
            if (settingsButton != null) settingsButton.onClick.AddListener(ToggleSettings);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }
            if (statusText != null) statusText.text = SaveManager.Instance.CurrentData.campaignCompleted
                ? "Cerita selesai. Mulai baru untuk memainkan ketiga perspektif lagi."
                : "WASD bergerak • Mouse kamera • E bicara/ambil • Space QTE • Esc jeda";
        }
        public void NewGame()
        {
            if (loading) return;
            SaveManager.Instance.ResetProgress();
            StartCoroutine(LoadLevel());
        }
        public void ContinueGame()
        {
            if (loading || !SaveManager.Instance.HasSave) return;
            SaveManager.Instance.LoadGame();
            StartCoroutine(LoadLevel());
        }
        private IEnumerator LoadLevel()
        {
            loading = true;
            if (newGameButton != null) newGameButton.interactable = false;
            if (continueButton != null) continueButton.interactable = false;
            if (statusText != null) statusText.text = "Memuat cerita...";
            GameStateManager.Instance.SetState(GameState.Loading);
            int index = Mathf.Clamp(SaveManager.Instance.CurrentData.currentChapter - 1, 0, chapters.Length - 1);
            string scene = chapters[index].SceneName;
            if (ChapterSceneFlow.CanLoad(scene)) yield return SceneManager.LoadSceneAsync(scene);
            else
            {
                loading = false;
                GameStateManager.Instance.SetState(GameState.MainMenu);
                if (newGameButton != null) newGameButton.interactable = true;
                if (continueButton != null) continueButton.interactable = SaveManager.Instance.HasSave;
                if (statusText != null) statusText.text = "Scene chapter belum tersedia.";
            }
        }
        public void ToggleSettings() { if (settingsPanel != null) settingsPanel.SetActive(!settingsPanel.activeSelf); }
        private void SetVolume(float value) { AudioListener.volume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume); }
        private void OnDisable() => PlayerPrefs.Save();
        public void Quit() => Application.Quit();
    }
}
