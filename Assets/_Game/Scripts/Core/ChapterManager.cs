using System;
using UnityEngine;
using BullyingGame.Save;

namespace BullyingGame.Core
{
    public class ChapterManager : MonoBehaviour
    {
        public static ChapterManager Instance { get; private set; }

        public event Action<int> OnChapterChanged;

        [SerializeField] private int totalChapters = 5;

        public int CurrentChapter { get; private set; } = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (SaveManager.Instance != null)
            {
                CurrentChapter = Mathf.Clamp(SaveManager.Instance.CurrentData.currentChapter, 1, totalChapters);
            }
        }

        public void AdvanceToNextChapter()
        {
            if (CurrentChapter < totalChapters)
            {
                CurrentChapter++;
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.CurrentData.currentChapter = CurrentChapter;
                    SaveManager.Instance.SaveGame();
                }
                OnChapterChanged?.Invoke(CurrentChapter);
            }
        }

        public void SetChapter(int chapterNumber)
        {
            CurrentChapter = Mathf.Clamp(chapterNumber, 1, totalChapters);
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CurrentData.currentChapter = CurrentChapter;
                SaveManager.Instance.SaveGame();
            }
            OnChapterChanged?.Invoke(CurrentChapter);
        }
        public void SyncChapter(int chapterNumber)
        {
            CurrentChapter = Mathf.Clamp(chapterNumber, 1, totalChapters);
            OnChapterChanged?.Invoke(CurrentChapter);
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
