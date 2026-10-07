using UnityEngine;
using TMPro;
using BullyingGame.Quest;
using BullyingGame.Core;

namespace BullyingGame.UI
{
    public class QuestHUDUI : MonoBehaviour
    {
        [SerializeField] private GameObject hudContainer;
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField] private TextMeshProUGUI questObjectiveText;
        [SerializeField] private CanvasGroup canvasGroup;

        private QuestData activeQuest;

        private void LateUpdate()
        {
            if (hudContainer == null) return;
            bool visible = activeQuest != null && QuestManager.Instance != null &&
                QuestManager.Instance.GetQuestState(activeQuest) == QuestState.Active &&
                GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Playing;
            hudContainer.SetActive(visible);
        }

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
                if (obj.IsCompleted) continue;
                if (result.Length > 0) result += "\n";
                result += obj.description;
                if (obj.requiredAmount > 1) result += $" ({obj.currentAmount}/{obj.requiredAmount})";
            }

            questObjectiveText.text = result;
            if (hudContainer != null && questTitleText != null)
            {
                float titleHeight = questTitleText.GetPreferredValues(questTitleText.rectTransform.rect.width, 1000).y;
                float objectiveHeight = questObjectiveText.GetPreferredValues(questObjectiveText.rectTransform.rect.width, 1000).y;
                questTitleText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, titleHeight);
                questObjectiveText.rectTransform.anchoredPosition = new Vector2(24, -50 - titleHeight - 12);
                questObjectiveText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, objectiveHeight);
                hudContainer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    Mathf.Clamp(50 + titleHeight + 12 + objectiveHeight + 55, 150, 340));
            }
        }
    }
}
