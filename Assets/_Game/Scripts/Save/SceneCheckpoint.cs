using UnityEngine;
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
            data.quests = quests.CaptureProgress();
            data.handledEvents = events != null ? events.CaptureHandledEvents() : new string[0];
            var items = UnityEngine.Object.FindObjectsByType<QuestItem>();
            data.items = new QuestItemCheckpoint[items.Length];
            for (int i = 0; i < items.Length; i++) data.items[i] = items[i].CaptureCheckpoint();
            if (player != null) { data.playerPosition = player.position; data.hasPlayerPosition = true; }
            if (!SaveManager.Instance.TrySaveGame()) { saveAt = Time.unscaledTime + 5f; return false; }
            dirty = false;
            return true;
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
