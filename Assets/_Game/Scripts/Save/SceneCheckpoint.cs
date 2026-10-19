using UnityEngine;
using System.Collections.Generic;
using BullyingGame.Core;
using BullyingGame.Quest;
using BullyingGame.Events;
using BullyingGame.Quiz;

namespace BullyingGame.Save
{
    [DefaultExecutionOrder(250)]
    public class SceneCheckpoint : MonoBehaviour
    {
        [Header("Checkpoint")]
        [SerializeField] private Transform player;
        private bool dirty;
        private bool failureShown;
        private float saveAt;
        private QuestManager quests;
        private BullyingEventManager events;
        private StoryChapterSequence sequence;
        private QuizManager quiz;
        private void Start()
        {
            quests = QuestManager.Instance;
            events = BullyingEventManager.Instance;
            sequence = GetComponent<StoryChapterSequence>();
            quiz = QuizManager.Instance;
            if (quests != null) { quests.OnObjectiveProgress += Progress; quests.OnQuestStateChanged += QuestChanged; }
            if (events != null) events.OnEventStateChanged += EventChanged;
            if (sequence != null) { sequence.OnChapterStarted += ChapterStarted; sequence.OnChapterCompleted += ChapterCompleted; }
            if (quiz != null) quiz.OnQuizClosed += ReflectionClosed;
            MarkDirty();
        }
        private void Progress(QuestData quest, QuestObjective objective) => MarkDirty();
        private void QuestChanged(QuestData quest, QuestState state) => MarkDirty();
        private void EventChanged(BullyingEventData data, BullyingEventState state) => MarkDirty();
        private void ChapterStarted(NarrativeChapterData data, int number) => MarkDirty();
        private void ChapterCompleted(NarrativeChapterData data, bool last) { MarkDirty(); CaptureNow(); }
        private void ReflectionClosed() { MarkDirty(); CaptureNow(); }
        private void MarkDirty() { dirty = true; saveAt = Time.unscaledTime + .25f; }
        private void Update()
        {
            if (dirty && Time.unscaledTime >= saveAt && GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Playing) CaptureNow();
        }
        public bool CaptureNow()
        {
            if (SaveManager.Instance == null || quests == null || GameStateManager.Instance == null ||
                (GameStateManager.Instance.CurrentState != GameState.Playing && GameStateManager.Instance.CurrentState != GameState.Result)) return false;
            var data = SaveManager.Instance.CurrentData;
            if (sequence != null)
            {
                SaveManager.Instance.SetChapterCheckpoint(sequence.ChapterNumber, "CH" + sequence.ChapterNumber.ToString("00") + "/stable");
                data.missionId = sequence.ActiveQuest != null ? sequence.ActiveQuest.questId : "M" + (sequence.ChapterNumber * 3).ToString("00");
                if (sequence.IsChapterComplete && System.Array.IndexOf(data.completedChapters, data.chapterId) < 0)
                {
                    var completed = new List<string>(data.completedChapters) { data.chapterId };
                    data.completedChapters = completed.ToArray();
                }
            }
            data.quests = quests.CaptureProgress();
            data.handledEvents = events != null ? events.CaptureHandledEvents() : new string[0];
            var items = UnityEngine.Object.FindObjectsByType<QuestItem>();
            var records = new Dictionary<string, QuestItemCheckpoint>();
            if (data.items != null) foreach (var item in data.items)
                if (item != null && !string.IsNullOrWhiteSpace(item.itemId)) records[item.itemId] = item;
            foreach (var item in items)
            {
                var record = item.CaptureCheckpoint();
                records[record.itemId] = record;
            }
            data.items = new List<QuestItemCheckpoint>(records.Values).ToArray();
            if (player != null) { data.playerPosition = player.position; data.hasPlayerPosition = true; }
            if (!SaveManager.Instance.TrySaveGame())
            {
                dirty = true;
                saveAt = Time.unscaledTime + 5f;
                ShowSaveFailure();
                return false;
            }
            failureShown = false;
            dirty = false;
            return true;
        }
        private void ShowSaveFailure()
        {
            if (failureShown || BullyingGame.UI.StoryOverlay.Current != null) return;
            failureShown = true;
            var state = GameStateManager.Instance.CurrentState;
            float timeScale = Time.timeScale;
            GameStateManager.Instance.SetState(GameState.Paused);
            Time.timeScale = 0;
            BullyingGame.UI.StoryOverlay.Show("Checkpoint belum tersimpan",
                "Progres sesi ini masih tersedia, tetapi belum berhasil disimpan. Coba simpan lagi sebelum keluar.\n\n" + SaveManager.Instance.LastError,
                new[] { "Coba simpan lagi", "Kembali ke permainan" }, index =>
                {
                    BullyingGame.UI.StoryOverlay.Current?.Dismiss();
                    if (index == 0) { failureShown = false; CaptureNow(); }
                }, () =>
                {
                    Time.timeScale = timeScale;
                    if (GameStateManager.Instance != null) GameStateManager.Instance.SetState(state);
                });
        }
        private void OnApplicationPause(bool paused) { if (paused) CaptureNow(); }
        private void OnApplicationQuit() => CaptureNow();
        private void OnDestroy()
        {
            if (quests != null) { quests.OnObjectiveProgress -= Progress; quests.OnQuestStateChanged -= QuestChanged; }
            if (events != null) events.OnEventStateChanged -= EventChanged;
            if (sequence != null) { sequence.OnChapterStarted -= ChapterStarted; sequence.OnChapterCompleted -= ChapterCompleted; }
            if (quiz != null) quiz.OnQuizClosed -= ReflectionClosed;
        }
    }
}
