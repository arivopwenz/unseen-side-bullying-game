using UnityEngine;
using BullyingGame.Quest;
using BullyingGame.Dialogue;
using BullyingGame.Quiz;

namespace BullyingGame.Core
{
    [CreateAssetMenu(menuName = "Game/Narrative Chapter", fileName = "Chapter")]
    public class NarrativeChapterData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string chapterTitle;
        [SerializeField] private string playerName;
        [SerializeField] private POVType perspective;
        [SerializeField] private bool allowsDialogueChoices;
        [Header("Sequence")]
        [SerializeField] private QuestData[] missions;
        [SerializeField] private DialogueData openingDialogue;
        [SerializeField] private UnityEngine.Video.VideoClip introVideo;
        [SerializeField] private QuizQuestion[] reflections;
        [SerializeField, TextArea] private string closingMessage;
        public string ChapterTitle => chapterTitle;
        public string PlayerName => playerName;
        public POVType Perspective => perspective;
        public bool AllowsDialogueChoices => allowsDialogueChoices;
        public QuestData[] Missions => missions;
        public DialogueData OpeningDialogue => openingDialogue;
        public UnityEngine.Video.VideoClip IntroVideo => introVideo;
        public QuizQuestion[] Reflections => reflections;
        public string ClosingMessage => closingMessage;
    }
}
