using UnityEngine;

namespace BullyingGame.Events
{
    /// <summary>Zona sembunyi di kantin. Memberi tahu CanteenSequenceDirector
    /// saat Player masuk/keluar, agar status bersembunyi bisa dihitung.</summary>
    public class HideZone : MonoBehaviour
    {
        public CanteenSequenceDirector director;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && director != null) director.SetHideZoneState(this, true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && director != null) director.SetHideZoneState(this, false);
        }
    }
}
