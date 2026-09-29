using UnityEngine;
using BullyingGame.Dialogue;
using BullyingGame.QTE;

namespace BullyingGame.Events
{
    public class TimelineSignalHandler : MonoBehaviour
    {
        [SerializeField] private DialogueData dialogueToPlay;
        [SerializeField] private QTEData qteToTrigger;
        [SerializeField] private BullyingEventData eventToTrigger;

        public void OnStartDialogue()
        {
            if (dialogueToPlay == null) return;
            DialogueManager.Instance.StartDialogue(dialogueToPlay);
        }

        public void OnStartQTE()
        {
            if (qteToTrigger == null) return;
            QTEManager.Instance.StartQTE(qteToTrigger);
        }

        public void OnTriggerBullyingEvent()
        {
            if (eventToTrigger == null) return;
            BullyingEventManager.Instance.TriggerEvent(eventToTrigger);
        }

        public void OnResolveEvent()
        {
            BullyingEventManager.Instance.ResolveEvent();
        }

        public void OnFailEvent()
        {
            BullyingEventManager.Instance.FailEvent();
        }
    }
}
