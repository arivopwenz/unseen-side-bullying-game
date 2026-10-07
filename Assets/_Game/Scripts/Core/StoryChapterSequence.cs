using System;
using System.Collections.Generic;
using UnityEngine;
using BullyingGame.Cinematics;
using BullyingGame.Quest;
using BullyingGame.Quiz;
using BullyingGame.Dialogue;
using BullyingGame.Events;
using BullyingGame.Save;

namespace BullyingGame.Core
{
    [DefaultExecutionOrder(200)]
    public class StoryChapterSequence : MonoBehaviour
    {
        [Header("Campaign")]
        [SerializeField] private NarrativeChapterData[] chapters;
        [SerializeField] private StoryOpeningDirector opening;
        [SerializeField] private PlayablePOVController playablePOV;
        [SerializeField] private VideoCutscenePlayer video;
        [SerializeField] private OutdoorChapterPresentation outdoor;
        public event Action<NarrativeChapterData, int> OnChapterStarted;
        public event Action<NarrativeChapterData, bool> OnChapterCompleted;
        public NarrativeChapterData CurrentChapter => chapters != null && chapterIndex < chapters.Length ? chapters[chapterIndex] : null;
        public QuestData ActiveQuest { get; private set; }
        public int ChapterNumber => chapterIndex + 1;
        public bool IsChapterComplete { get; private set; }
        private int chapterIndex, reflectionIndex;
        private bool advancePending, reflectionPending, waitingForVideo;
        private QuestManager quests;
        private QuizManager quiz;

        private void Start()
        {
            quests = QuestManager.Instance;
            quiz = QuizManager.Instance;
            if (quests == null || quiz == null || chapters == null || chapters.Length == 0) return;
            quests.OnQuestStateChanged += QuestChanged;
            quiz.OnQuizClosed += ReflectionClosed;
            if (opening != null) opening.OnFinished += OpeningFinished;
            if (video != null) video.OnVideoFinished += VideoFinished;
            var known = new List<QuestData>();
            foreach (var chapter in chapters)
                if (chapter != null && chapter.Missions != null) known.AddRange(chapter.Missions);
            var data = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : new GameSaveData();
            quests.RestoreProgress(data.quests, known.ToArray());
            BullyingEventManager.Instance?.RestoreHandledEvents(data.handledEvents);
            chapterIndex = Mathf.Clamp(data.currentChapter - 1, 0, chapters.Length - 1);
            // Older saves may store a chapter number without its prerequisite mission records.
            for (int i=0; i<chapterIndex; i++)
            {
                bool complete=true;
                foreach(var mission in chapters[i].Missions) if(mission.IsApplicable) complete &= quests.GetQuestState(mission)==QuestState.Completed;
                if(!complete) { chapterIndex=i; break; }
            }
            bool resuming = false;
            foreach (var mission in CurrentChapter.Missions)
                resuming |= quests.GetQuestState(mission) == QuestState.Active || quests.GetQuestState(mission) == QuestState.Completed;
            playablePOV?.Apply(CurrentChapter.Perspective);
            playablePOV?.SetDisplayName(CurrentChapter.PlayerName);
            if (resuming && data.hasPlayerPosition && playablePOV != null &&
                UnityEngine.AI.NavMesh.SamplePosition(data.playerPosition, out var hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
                playablePOV.Teleport(hit.position, Quaternion.identity);
            foreach (var item in UnityEngine.Object.FindObjectsByType<QuestItem>())
            {
                QuestItemCheckpoint record = null;
                var identity = item.CaptureCheckpoint().itemId;
                if (data.items != null) foreach (var saved in data.items)
                    if (saved != null && saved.itemId == identity) { record = saved; break; }
                item.RestoreCheckpoint(record);
            }
            EnterChapter(!resuming);
        }
        private void EnterChapter(bool playOpening)
        {
            IsChapterComplete = false;
            ActiveQuest = null;
            reflectionIndex = 0;
            outdoor?.Apply(chapterIndex);
            GetComponent<ChapterActorLayout>()?.Apply(ChapterNumber);
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CurrentData.currentChapter = ChapterNumber;
                SaveManager.Instance.CurrentData.currentPOV = CurrentChapter.Perspective;
            }
            OnChapterStarted?.Invoke(CurrentChapter, ChapterNumber);
            ChapterManager.Instance?.SyncChapter(ChapterNumber);
            if (playOpening && video != null && CurrentChapter.IntroVideo != null)
            {
                waitingForVideo = true;
                if (video.TryPlayVideo(CurrentChapter.IntroVideo)) return;
                waitingForVideo = false;
            }
            if (playOpening && opening != null && opening.TryPlay(CurrentChapter.OpeningDialogue)) return;
            advancePending = true;
        }
        private void VideoFinished()
        {
            if (!waitingForVideo) return;
            waitingForVideo = false;
            if (opening != null && opening.TryPlay(CurrentChapter.OpeningDialogue)) return;
            advancePending = true;
        }
        private void OpeningFinished() => advancePending = true;
        private void QuestChanged(QuestData quest, QuestState state)
        {
            if (quest == ActiveQuest && state == QuestState.Completed) advancePending = true;
        }
        private void LateUpdate()
        {
            if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentState != GameState.Playing ||
                (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) ||
                (CutsceneManager.Instance != null && CutsceneManager.Instance.CurrentDirector != null)) return;
            if (advancePending)
            {
                advancePending = false;
                NextMission();
            }
            if (reflectionPending && !quiz.IsQuizActive)
            {
                reflectionPending = false;
                StartReflection();
            }
        }
        private void NextMission()
        {
            var missions = CurrentChapter.Missions;
            for (int i = 0; i < missions.Length; i++)
            {
                if (!missions[i].IsApplicable) continue;
                if (quests.GetQuestState(missions[i]) == QuestState.Completed) continue;
                ActiveQuest = missions[i];
                if (quests.GetQuestState(ActiveQuest) != QuestState.Active)
                {
                    quests.MakeQuestAvailable(ActiveQuest);
                    if (!quests.TryStartQuest(ActiveQuest))
                    {
                        Debug.LogError("Mission prerequisite/data invalid: " + ActiveQuest.name, this);
                        return;
                    }
                }
                else
                {
                    // A restored active quest needs a HUD refresh without restarting its progress.
                    var hud = UnityEngine.Object.FindAnyObjectByType<BullyingGame.UI.QuestHUDUI>();
                    if (hud != null) hud.RefreshQuest(ActiveQuest);
                }
                reflectionPending = i == missions.Length - 1 && CurrentChapter.Reflections != null && CurrentChapter.Reflections.Length > 0;
                return;
            }
            ActiveQuest = null;
            IsChapterComplete = true;
            GameStateManager.Instance.SetState(GameState.Result);
            OnChapterCompleted?.Invoke(CurrentChapter, chapterIndex == chapters.Length - 1);
        }
        private void StartReflection()
        {
            while(reflectionIndex < CurrentChapter.Reflections.Length && SaveManager.Instance != null &&
                SaveManager.Instance.IsReflectionClosed(ReflectionId())) reflectionIndex++;
            if (reflectionIndex >= CurrentChapter.Reflections.Length)
            {
                quests.UpdateObjective(ActiveQuest, "reflect");
                return;
            }
            if (!quiz.TryStartQuiz(CurrentChapter.Reflections[reflectionIndex], ReflectionId())) reflectionPending = true;
        }
        private string ReflectionId() => ActiveQuest.questId + "/reflection/" + reflectionIndex;
        private void ReflectionClosed()
        {
            if (ActiveQuest == null || IsChapterComplete) return;
            reflectionIndex++;
            reflectionPending = true;
        }
        public void AdvanceChapter()
        {
            if (!IsChapterComplete) return;
            if (chapterIndex == chapters.Length - 1)
            {
                if (SaveManager.Instance != null) { SaveManager.Instance.CurrentData.campaignCompleted = true; SaveManager.Instance.SaveGame(); }
                GameStateManager.Instance.SetState(GameState.Loading);
                UnityEngine.SceneManagement.SceneManager.LoadScene("01_MainMenu");
                return;
            }
            chapterIndex++;
            GameStateManager.Instance.SetState(GameState.Playing);
            playablePOV?.Apply(CurrentChapter.Perspective);
            playablePOV?.SetDisplayName(CurrentChapter.PlayerName);
            EnterChapter(true);
        }
        private void OnDestroy()
        {
            if (quests != null) quests.OnQuestStateChanged -= QuestChanged;
            if (quiz != null) quiz.OnQuizClosed -= ReflectionClosed;
            if (opening != null) opening.OnFinished -= OpeningFinished;
            if (video != null) video.OnVideoFinished -= VideoFinished;
        }
    }
}
