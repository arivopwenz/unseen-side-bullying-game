using UnityEngine;
using BullyingGame.Dialogue;
using BullyingGame.Quest;

namespace BullyingGame.Events
{
    [CreateAssetMenu(fileName = "NewBullyingEvent", menuName = "Game/Bullying Event")]
    public class BullyingEventData : ScriptableObject
    {
        public string eventId;
        public string eventName;
        public BullyingType bullyingType;
        public DialogueData witnessDialogue;
        public DialogueData confrontDialogue;
        public DialogueData ignoreDialogue;
        public QuestData relatedQuest;
    }
}
