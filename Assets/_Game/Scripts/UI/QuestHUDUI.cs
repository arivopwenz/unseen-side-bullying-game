using UnityEngine;
using TMPro;
using BullyingGame.Quest;

namespace BullyingGame.UI
{
    public class QuestHUDUI : MonoBehaviour
    {
        [SerializeField] private GameObject hudContainer;
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField] private TextMeshProUGUI questObjectiveText;
        [SerializeField] private CanvasGroup canvasGroup;

        private QuestData activeQuest;

        private void Awake()
        {
            if (hudContainer == null)
            {
                var panel = transform.Find("QuestPanel");
                if (panel != null)
                {
                    hudContainer = panel.gameObject;
                }
            }

            if (questTitleText == null && hudContainer != null)
            {
                var titleObj = hudContainer.transform.Find("QuestTitle");
                if (titleObj != null)
                {
                    questTitleText = titleObj.GetComponent<TextMeshProUGUI>();
                }
            }

            if (questObjectiveText == null && hudContainer != null)
            {
                var objObj = hudContainer.transform.Find("QuestObjective");
                if (objObj != null)
                {
                    questObjectiveText = objObj.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        private void Start()
        {
            if (hudContainer != null)
            {
                hudContainer.SetActive(false);
            }

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStateChanged += HandleQuestStateChanged;
                QuestManager.Instance.OnObjectiveProgress += HandleObjectiveProgress;
            }
        }

        private void OnDestroy()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
                QuestManager.Instance.OnObjectiveProgress -= HandleObjectiveProgress;
            }
        }

        private void HandleQuestStateChanged(QuestData quest, QuestState newState)
        {
            if (newState == QuestState.Active)
            {
                activeQuest = quest;
                DisplayQuest(quest);
            }
            else if (newState == QuestState.Completed && activeQuest == quest)
            {
                if (questTitleText != null)
                {
                    questTitleText.text = quest.questTitle + " (Selesai)";
                }
                if (questObjectiveText != null)
                {
                    questObjectiveText.text = quest.CompletionMessage;
                }
            }
        }

        private void HandleObjectiveProgress(QuestData quest, QuestObjective objective)
        {
            if (activeQuest == quest)
            {
                UpdateObjectiveDisplay(quest);
            }
        }

        private void DisplayQuest(QuestData quest)
        {
            if (hudContainer != null)
            {
                hudContainer.SetActive(true);
            }

            if (questTitleText != null)
            {
                questTitleText.text = quest.questTitle;
            }

            UpdateObjectiveDisplay(quest);
        }

        public void RefreshQuest(QuestData quest)
        {
            if (quest == null) return;
            activeQuest = quest;
            DisplayQuest(quest);
        }

        private void UpdateObjectiveDisplay(QuestData quest)
        {
            if (questObjectiveText == null || QuestManager.Instance == null)
            {
                return;
            }

            var objectives = QuestManager.Instance.GetObjectives(quest);
            if (objectives == null || objectives.Length == 0)
            {
                questObjectiveText.text = quest.questDescription;
                return;
            }

            string result = "";
            for (int i = 0; i < objectives.Length; i++)
            {
                var obj = objectives[i];
                string status = obj.IsCompleted ? "[V] " : "- ";
                result += $"{status}{obj.description} ({obj.currentAmount}/{obj.requiredAmount})";
                if (i < objectives.Length - 1)
                {
                    result += "\n";
                }
            }

            questObjectiveText.text = result;
        }
    }
}
