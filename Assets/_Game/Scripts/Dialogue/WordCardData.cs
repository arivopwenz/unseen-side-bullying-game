using System;
using UnityEngine;

namespace BullyingGame.LevelOne
{
    [Serializable]
    public class WordCardPhrase
    {
        public string id;
        public string text;
    }
    [CreateAssetMenu(menuName = "Game/Level One/Word Card")]
    public class WordCardData : ScriptableObject
    {
        [Header("Intent and ordered phrase IDs")]
        [SerializeField] private string puzzleId, title;
        [SerializeField, TextArea] private string context, hint, feedback, response;
        [SerializeField] private WordCardPhrase[] phrases;
        [SerializeField] private string[] answer;
        public string Id => puzzleId;
        public string Title => title;
        public string Context => context;
        public string Hint => hint;
        public string Feedback => feedback;
        public string Response => response;
        public WordCardPhrase[] Phrases => phrases;
        public string[] Answer => answer;
        public bool IsCorrect(System.Collections.Generic.IList<string> ids)
        {
            if (ids == null || answer == null || ids.Count != answer.Length) return false;
            for (int i = 0; i < answer.Length; i++) if (ids[i] != answer[i]) return false;
            return true;
        }
        public string TextFor(string id) => Array.Find(phrases, p => p.id == id)?.text ?? "";
    }
}
