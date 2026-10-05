using UnityEngine;
using BullyingGame.Interaction;
using BullyingGame.Events;

namespace BullyingGame.Quest
{
    public class QuestItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private QuestData quest;
        [SerializeField] private string objectiveId = "find_notebook";
        [SerializeField] private string promptText = "[E] Ambil Buku Catatan";
        [SerializeField] private bool destroyOnCollect = true;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private BullyingEventData eventToTrigger;

        public string GetPromptText()
        {
            return promptText;
        }

        public bool CanInteract()
        {
            if (quest == null)
            {
                return true;
            }

            if (QuestManager.Instance == null)
            {
                return false;
            }

            return QuestManager.Instance.GetQuestState(quest) == QuestState.Active;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract())
            {
                return;
            }

            if (QuestManager.Instance != null && quest != null)
            {
                QuestManager.Instance.UpdateObjective(quest, objectiveId, 1);
            }

            if (eventToTrigger != null && BullyingEventManager.Instance != null)
            {
                BullyingEventManager.Instance.TriggerEvent(eventToTrigger);
            }

            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else if (visualRoot != null)
            {
                visualRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
