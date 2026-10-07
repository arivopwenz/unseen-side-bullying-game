using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BullyingGame.Core;
using BullyingGame.Cinematics;
using BullyingGame.Save;

namespace BullyingGame.UI
{
    public class ChapterPresentationUI : MonoBehaviour
    {
        [Header("Story")]
        [SerializeField] private StoryChapterSequence sequence;
        [SerializeField] private StoryOpeningDirector opening;
        [SerializeField] private VideoCutscenePlayer video;
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI chapterText;
        [SerializeField] private TextMeshProUGUI perspectiveText;
        [SerializeField] private Button skipButton;
        [Header("Chapter End")]
        [SerializeField] private GameObject endPanel;
        [SerializeField] private TextMeshProUGUI endTitle;
        [SerializeField] private TextMeshProUGUI endMessage;
        [SerializeField] private TextMeshProUGUI nextLabel;
        [SerializeField] private Button nextButton;
        private void Start()
        {
            if (endPanel != null) endPanel.SetActive(false);
            if (sequence != null) { sequence.OnChapterStarted += Started; sequence.OnChapterCompleted += Completed; }
            if (skipButton != null) skipButton.onClick.AddListener(Skip);
            if (nextButton != null) nextButton.onClick.AddListener(Next);
        }
        private void Update()
        {
            if (skipButton != null) skipButton.gameObject.SetActive(
                GameStateManager.Instance != null && GameStateManager.Instance.CurrentState != GameState.Paused &&
                ((opening != null && opening.IsRunning) || (video != null && video.IsRunning)));
        }
        private void Started(NarrativeChapterData chapter, int number)
        {
            if (endPanel != null) endPanel.SetActive(false);
            if (chapterText != null) chapterText.text = "CHAPTER " + number + "  /  " + chapter.ChapterTitle;
            string role = chapter.Perspective == POVType.Protagonist ? "PENGANTAR CERITA" : chapter.Perspective == POVType.Victim ? "KORBAN" : chapter.Perspective == POVType.Witness ? "SAKSI" : "PEMBULLY";
            if (perspectiveText != null) perspectiveText.text = chapter.PlayerName + "  •  POV " + role;
        }
        private void Completed(NarrativeChapterData chapter, bool last)
        {
            if (last && SaveManager.Instance != null) SaveManager.Instance.CurrentData.campaignCompleted = true;
            if (endPanel != null) endPanel.SetActive(true);
            if (endTitle != null) endTitle.text = last ? "Lima bab, tiga perspektif" : "Chapter selesai";
            if (endMessage != null) endMessage.text = chapter.ClosingMessage;
            if (nextLabel != null) nextLabel.text = last ? "Menu utama" : "Lanjut ke chapter berikutnya";
        }
        private void Skip()
        {
            if (video != null && video.IsRunning) video.SkipVideo();
            else opening?.Skip();
        }
        private void Next() => sequence?.AdvanceChapter();
        private void OnDestroy()
        {
            if (sequence != null) { sequence.OnChapterStarted -= Started; sequence.OnChapterCompleted -= Completed; }
        }
    }
}
