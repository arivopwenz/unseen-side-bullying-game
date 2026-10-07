using System;
using UnityEngine;
using BullyingGame.Dialogue;
using BullyingGame.Quest;

namespace BullyingGame.NPC
{
    [Serializable]
    public struct MissionConversation
    {
        [SerializeField] private QuestData quest;
        [SerializeField] private string objectiveId;
        [SerializeField] private string requiredObjectiveId;
        [SerializeField] private DialogueData dialogue;
        [SerializeField] private BullyingGame.Events.NarrativeActivityData activity;
        public QuestData Quest => quest;
        public string ObjectiveId => objectiveId;
        public string RequiredObjectiveId => requiredObjectiveId;
        public DialogueData Dialogue => dialogue;
        public BullyingGame.Events.NarrativeActivityData Activity => activity;
    }

    public class MissionDialogueNPC : BaseNPC
    {
        [Header("Mission Conversations")]
        [SerializeField] private MissionConversation[] conversations;
        [SerializeField] private DialogueData fallbackDialogue;
        private DialogueManager subscribed;
        private DialogueData pendingDialogue;
        private MissionConversation pendingConversation;
        public bool OffersObjective(QuestData quest)
        {
            var manager=QuestManager.Instance;
            if(!isActiveAndEnabled || manager==null || conversations==null) return false;
            foreach(var conversation in conversations)
                if(conversation.Quest==quest && !manager.IsObjectiveCompleted(quest,conversation.ObjectiveId) &&
                    (string.IsNullOrWhiteSpace(conversation.RequiredObjectiveId) || manager.IsObjectiveCompleted(quest,conversation.RequiredObjectiveId))) return true;
            return false;
        }
        protected override void OnInteract(GameObject interactor)
        {
            var manager = DialogueManager.Instance;
            var quests = QuestManager.Instance;
            if (manager == null || quests == null) return;
            if (conversations != null) foreach (var conversation in conversations)
            {
                if (quests.GetQuestState(conversation.Quest) != QuestState.Active ||
                    quests.IsObjectiveCompleted(conversation.Quest, conversation.ObjectiveId) ||
                    (!string.IsNullOrWhiteSpace(conversation.RequiredObjectiveId) &&
                     !quests.IsObjectiveCompleted(conversation.Quest, conversation.RequiredObjectiveId))) continue;
                if (!manager.TryStartDialogue(conversation.Dialogue)) return;
                pendingConversation = conversation;
                pendingDialogue = conversation.Dialogue;
                subscribed = manager;
                subscribed.OnDialogueEnded += Finished;
                return;
            }
            manager.TryStartDialogue(fallbackDialogue);
        }
        private void Finished()
        {
            bool complete = subscribed != null && subscribed.LastDialogueCompleted;
            Unsubscribe();
            if (!complete || QuestManager.Instance == null) return;
            var conversation=pendingConversation;
            if(conversation.Activity == null) { QuestManager.Instance.UpdateObjective(conversation.Quest, conversation.ObjectiveId);return; }
            var director=UnityEngine.Object.FindAnyObjectByType<BullyingGame.Events.NarrativeActivityDirector>();
            if(director != null) director.TryStart(conversation.Activity, completed =>
            { if(completed && QuestManager.Instance != null) QuestManager.Instance.UpdateObjective(conversation.Quest, conversation.ObjectiveId); });
        }
        private void Unsubscribe()
        {
            if (subscribed != null) subscribed.OnDialogueEnded -= Finished;
            subscribed = null;
            pendingDialogue = null;
        }
        protected override void OnDisable()
        {
            var manager = subscribed;
            bool owns = manager != null && manager.CurrentDialogue == pendingDialogue;
            Unsubscribe();
            if (owns) manager.EndDialogue();
            base.OnDisable();
        }
    }
}
