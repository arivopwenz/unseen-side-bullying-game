using UnityEngine;

namespace BullyingGame.Events
{
    [RequireComponent(typeof(Collider))]
    public class BullyingEventTrigger : MonoBehaviour
    {
        [SerializeField] private BullyingEventData eventData;
        [SerializeField] private bool triggerOnce = true;

        private bool hasTriggered;

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && hasTriggered) return;
            if (!other.CompareTag("Player")) return;

            if (BullyingEventManager.Instance != null &&
                BullyingEventManager.Instance.TryTriggerEvent(eventData))
                hasTriggered = true;
        }
    }
}
