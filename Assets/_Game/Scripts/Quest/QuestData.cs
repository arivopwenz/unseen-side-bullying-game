using UnityEngine;

namespace BullyingGame.Quest
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Game/Quest Data")]
    public class QuestData : ScriptableObject
    {
        public string questId;
        public string questTitle;
        public string questDescription;
        public QuestObjective[] objectives;
        public QuestData[] prerequisites;
    }
}
