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
        [Header("Optional Narrative Route")]
        [SerializeField] private string routeChoiceId;
        [SerializeField, Range(0, 2)] private int routeChoiceIndex;
        public bool IsApplicable => string.IsNullOrWhiteSpace(routeChoiceId) ||
            (BullyingGame.Save.SaveManager.Instance != null &&
             BullyingGame.Save.SaveManager.Instance.GetNarrativeChoice(routeChoiceId) == routeChoiceIndex);
        [Header("HUD Setelah Objective Selesai")]
        [SerializeField, TextArea] private string completionMessage = "Kembali dan bicara dengan pemberi misi.";
        public string CompletionMessage => string.IsNullOrWhiteSpace(completionMessage)
            ? "Misi selesai." : completionMessage;
    }
}
