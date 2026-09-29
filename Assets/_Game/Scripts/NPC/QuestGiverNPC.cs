using UnityEngine;
using BullyingGame.Dialogue;
using BullyingGame.Quest;

namespace BullyingGame.NPC
{
    public class QuestGiverNPC : BaseNPC
    {
        [SerializeField] private QuestData questToGive;
        [SerializeField] private DialogueData introDialogue;
        [SerializeField] private DialogueData questActiveDialogue;
        [SerializeField] private DialogueData questCompletedDialogue;

        protected override void OnInteract(GameObject interactor)
        {
            var questState = QuestManager.Instance.GetQuestState(questToGive);

            switch (questState)
            {
                case QuestState.Locked:
                case QuestState.Available:
                    QuestManager.Instance.MakeQuestAvailable(questToGive);
                    QuestManager.Instance.StartQuest(questToGive);
                    PlayDialogue(introDialogue);
                    break;

                case QuestState.Active:
                    PlayDialogue(questActiveDialogue);
                    break;

                case QuestState.Completed:
                    PlayDialogue(questCompletedDialogue);
                    break;
            }
        }

        private void PlayDialogue(DialogueData dialogue)
        {
            if (dialogue == null) return;
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}
