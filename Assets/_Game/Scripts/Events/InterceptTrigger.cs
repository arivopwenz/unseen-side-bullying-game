using UnityEngine;

namespace BullyingGame.Events
{
    /// <summary>Zona trigger pencegatan (tanpa fade, ala ambush awal).
    /// Saat Player masuk area ini selama Objective 2 berjalan (ToCanteen/Arrived),
    /// CanteenSequenceDirector memulai konfrontasi pencegatan.</summary>
    public class InterceptTrigger : MonoBehaviour
    {
        public CanteenSequenceDirector director;

        private void Start()
        {
            if (director == null)
            {
                var arr = FindObjectsByType<CanteenSequenceDirector>(FindObjectsInactive.Include);
                if (arr.Length > 0) director = arr[0];
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && director != null)
                director.RequestIntercept();
        }
    }
}
