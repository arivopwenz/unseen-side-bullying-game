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
        [SerializeField] private BullyingEventDirector eventDirector;

        public void OnStartBullyApproach()
        {
            if (eventDirector != null && eventDirector.IsRunning)
                eventDirector.PlayBullyApproach();
            else Debug.LogWarning("Signal approach membutuhkan Event Director yang sedang menjalankan event.", this);
        }

        public void OnStartDialogue()
        {
            if (dialogueToPlay == null || DialogueManager.Instance == null) return;
            if (eventDirector != null && eventDirector.IsRunning)
            {
                eventDirector.PlayDialogue(dialogueToPlay);
                return;
            }
            if (BullyingGame.Core.CutsceneManager.Instance != null &&
                BullyingGame.Core.CutsceneManager.Instance.CurrentDirector != null)
            {
                Debug.LogWarning("Assign Event Director before starting cinematic dialogue.", this);
                return;
            }
            DialogueManager.Instance.StartDialogue(dialogueToPlay);
        }

        public void OnStartQTE()
        {
            if (qteToTrigger == null || QTEManager.Instance == null) return;
            if (eventDirector != null && eventDirector.IsRunning)
            {
                eventDirector.PlayQTE(qteToTrigger);
                return;
            }
            if (BullyingGame.Core.CutsceneManager.Instance != null &&
                BullyingGame.Core.CutsceneManager.Instance.CurrentDirector != null)
            {
                Debug.LogWarning("Assign Event Director before starting cinematic QTE.", this);
                return;
            }
            QTEManager.Instance.StartQTE(qteToTrigger);
        }

        public void OnTriggerBullyingEvent()
        {
            if (eventToTrigger == null || BullyingEventManager.Instance == null) return;
            BullyingEventManager.Instance.TriggerEvent(eventToTrigger);
        }

        public void OnResolveEvent()
        {
            var manager = BullyingEventManager.Instance;
            if (eventToTrigger != null && manager != null && manager.CurrentEvent == eventToTrigger)
                manager.ResolveEvent();
        }

        public void OnFailEvent()
        {
            var manager = BullyingEventManager.Instance;
            if (eventToTrigger != null && manager != null && manager.CurrentEvent == eventToTrigger)
                manager.FailEvent();
        }
    }
}
