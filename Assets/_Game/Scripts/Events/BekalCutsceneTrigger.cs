using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.AI;

namespace BullyingGame.Events
{
    /// <summary>
    /// Trigger cutscene Timeline saja (konten Bekal dkk. dianimasikan manual oleh Ari di Timeline).
    /// Dipasang di GO ClassroomCutscene (memakai BoxCollider IsTrigger 6x3x3 yang sudah ada di (-31,15)).
    /// Ketika Aji SAJA masuk zona (tag Player; Bima tidak memicu karena tag-nya bukan Player),
    /// mainkan Timeline dari Director yang di-assign di Inspector, lalu mulai Misi Objective 2.
    /// CARA PAKAI: buat Timeline + PlayableDirector manual (saat animasi Bekal jadi), lalu drag Director
    /// tersebut ke field 'Director' komponen ini (Inspector, GO ClassroomCutscene).
    /// Jika Director belum di-assign (Timeline belum jadi), trigger TIDAK mengunci game: ia langsung
    /// lanjut ke Misi 2 (jadi trigger bisa dites sebelum Timeline jadi).
    /// Trigger sekali per Play (triggerOnce); TIDAK digate ke Misi 1/percakapan (sengaja simpel,
    /// biar bisa dites cepat via teleport; kalau mau digate ke setelah percakapan, tambah syarat
    /// ConversationDone di OnTriggerEnter).
    /// </summary>
    public class BekalCutsceneTrigger : MonoBehaviour
    {
        public PlayableDirector director;
        public bool triggerOnce = true;
        private bool triggered = false;
        private Behaviour playerMovement;

        private void OnTriggerEnter(Collider other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;
            if (triggerOnce) triggered = true;
            // Kunci Player selama cutscene (cari PlayerMovement by Name, tanpa dependensi namespace)
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) {
                foreach (var comp in playerGO.GetComponents<Component>()) {
                    if (comp != null && comp.GetType().Name == "PlayerMovement" && comp is Behaviour) {
                        playerMovement = (Behaviour)comp;
                        playerMovement.enabled = false;
                        break;
                    }
                }
            }
            // Hentikan Bima selama cutscene (biar tidak jalan saat Timeline main)
            var bimaGO = GameObject.Find("Bima_Guide");
            if (bimaGO != null) {
                var agent = bimaGO.GetComponent<NavMeshAgent>();
                if (agent != null) agent.isStopped = true;
            }
            if (director != null && director.playableAsset != null) {
                director.stopped += OnTimelineFinished;
                director.Play();
            } else {
                // Timeline belum di-assign: langsung lanjut ke Misi 2 (tidak soft-lock)
                FinishAndStartMission2();
            }
        }

        private void OnTimelineFinished(PlayableDirector d)
        {
            d.stopped -= OnTimelineFinished;
            FinishAndStartMission2();
        }

        private void FinishAndStartMission2()
        {
            if (playerMovement != null) playerMovement.enabled = true;
            var drvGO = GameObject.Find("BimaConversationDriver");
            if (drvGO != null) {
                var drv = drvGO.GetComponent<BimaConversationDriver>();
                if (drv != null) {
                    BimaConversationDriver.ConversationDone = true;
                    drv.StartCanteenWalk();
                }
            }
        }
    }
}
