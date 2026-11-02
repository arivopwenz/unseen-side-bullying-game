// DeveloperMissionSelect — HANYA UNTUK DEVELOPING, BUKAN UNTUK PEMAIN.
// Cara pakai: di Inspector GO "DeveloperMissionSelect" (scene Level01), buka list
// "Selected Mission", pilih misinya, lalu tekan Play. Game akan mulai di misi itu.
//   None     = nonaktif, main normal tanpa override (mulai dari awal seperti biasa).
//   Mission1 = mulai di Misi 1 (gerbang selatan, ikuti Bima ke kelas).
//   Mission2 = mulai di Misi 2 (koridor timur, Bima jalan ke kantin).
// Untuk final build, NONAKTIFKAN GO ini (sama seperti DebugTeleport) supaya tidak
// ada kode developer yang jalan di game rilis.
// CARA MENAMBAH MISI BARU (mis. Misi 3):
//   1) Tambah nilai di enum Mission (mis. Mission3).
//   2) Tambah case di ApplyAfterDelay() -> SetupMission3().
//   3) Tulis method SetupMission3() mengikuti pola SetupMission2 (set flag di Awake
//      + posisi + panggil driver misi tersebut).
//   4) Pilih Mission3 di Inspector, lalu Play.
// Catatan: selector ini hanya mengatur START misi (flag, posisi, driver). Ia TIDAK
// memperbaiki NavMesh. Kalau path Bima ke kantin masih Partial (gap NavMesh),
// Bima akan berhenti di ujung NavMesh — itu perlu perbaikan NavMesh terpisah,
// bukan salah selector ini.

using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using BullyingGame.Events;
using BullyingGame.NPC;

namespace BullyingGame.DebugTools
{
    public class DeveloperMissionSelect : MonoBehaviour
    {
        public enum Mission
        {
            None,
            Mission1,
            Mission2
        }

        [Header("Developer Mission Select — HANYA UNTUK DEVELOPING")]
        [Tooltip("Pilih misi untuk Play berikutnya. None = main normal tanpa override. Mission1 = mulai di Misi 1 (gerbang selatan). Mission2 = mulai di Misi 2 (Bima jalan ke kantin, dari koridor timur).")]
        public Mission selectedMission = Mission.Mission2;

        [Tooltip("Jeda sebelum setup misi diterapkan (detik, waktu nyata, tidak terpengaruh pause/slow-mo). Memberi waktu semua Start() selesai (termasuk teleport spawn) sebelum selector menimpa posisi/flag. Jangan 0.")]
        public float applyDelay = 0.3f;

        // Sekali per Play session saja. Tanpa ini, selector akan jalan lagi setiap
        // Level01 di-load ulang (mis. balik dari cutscene MasukKelas/Canteen) dan
        // menimpa posisi balik (ReturnPosition) yang benar. Reset di bawah jalan
        // tiap Play dimulai (RuntimeInitialize, bukan cuma domain reload), jadi
        // selector tetap jalan di Play berikutnya walau Reload Domain nonaktif.
        private static bool sessionStarted = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetForNewPlaySession()
        {
            sessionStarted = false;
        }

        private static readonly Vector3 SpawnPos = new Vector3(-7.10f, 0.05f, -50.09f);
        private static readonly Vector3 GuideStartPos = new Vector3(-6.10f, 0.05f, -48.67f);
        private static readonly Vector3 CorridorStartPos = new Vector3(13.63f, 0.35f, 17.49f);

        private void Awake()
        {
            // Load ulang scene (balik dari cutscene) tidak boleh mengulang setup.
            if (sessionStarted) return;

            if (selectedMission == Mission.None)
            {
                UnityEngine.Debug.Log("[DeveloperMissionSelect] Nonaktif (None) — main normal tanpa override.");
                return;
            }

            // Set flag SEBELUM Start() script lain jalan, supaya YellowCircleTrigger
            // dan BimaConversationDriver membaca flag yang benar sejak awal.
            if (selectedMission == Mission.Mission1)
            {
                YellowCircleTrigger.Mission1Complete = false;
                BimaConversationDriver.ConversationDone = false;
                BimaConversationDriver.CanteenWalkDone = false;
            }
            else if (selectedMission == Mission.Mission2)
            {
                YellowCircleTrigger.Mission1Complete = true;
                BimaConversationDriver.ConversationDone = true;
                BimaConversationDriver.CanteenWalkDone = false;
            }
        }

        private void Start()
        {
            if (sessionStarted) return;
            sessionStarted = true;
            if (selectedMission == Mission.None) return;
            StartCoroutine(ApplyAfterDelay());
        }

        private IEnumerator ApplyAfterDelay()
        {
            yield return new WaitForSecondsRealtime(applyDelay);
            switch (selectedMission)
            {
                case Mission.Mission1:
                    SetupMission1();
                    break;
                case Mission.Mission2:
                    SetupMission2();
                    break;
                default:
                    UnityEngine.Debug.LogWarning("[DeveloperMissionSelect] Misi " + selectedMission + " belum diimplementasikan setup-nya.");
                    break;
            }
        }

        private void SetupMission1()
        {
            UnityEngine.Debug.Log("[DeveloperMissionSelect] Memulai di Misi 1: Ikuti Bima ke kelas (gerbang selatan).");
            var playerGO = FindPlayer();
            if (playerGO != null)
            {
                TeleportPlayer(playerGO, SpawnPos);
                SetPlayerMovementEnabled(playerGO, true);
            }
            var bimaGO = FindBima();
            if (bimaGO != null)
            {
                var guide = bimaGO.GetComponent<GuideController>();
                if (guide != null) guide.enabled = true;
                WarpBima(bimaGO, GuideStartPos);
            }
            // Objective, minimap, dan marker Misi 1 sudah diatur YellowCircleTrigger
            // (Start normal, karena Mission1Complete = false). Selector tidak
            // menimpanya supaya tidak bentrok.
        }

        private void SetupMission2()
        {
            UnityEngine.Debug.Log("[DeveloperMissionSelect] Memulai di Misi 2: Ikuti Bima ke kantin (koridor timur).");
            // Tegaskan lagi flagnya (defensif, kalau sempat berubah di awal).
            YellowCircleTrigger.Mission1Complete = true;
            BimaConversationDriver.ConversationDone = true;
            BimaConversationDriver.CanteenWalkDone = false;

            var playerGO = FindPlayer();
            if (playerGO != null)
            {
                TeleportPlayer(playerGO, CorridorStartPos);
                SetPlayerMovementEnabled(playerGO, true);
            }
            var bimaGO = FindBima();
            if (bimaGO != null)
            {
                var guide = bimaGO.GetComponent<GuideController>();
                if (guide != null) guide.enabled = false;
                WarpBima(bimaGO, new Vector3(CorridorStartPos.x, CorridorStartPos.y, CorridorStartPos.z + 1.8f));
                if (playerGO != null)
                {
                    Vector3 pp = playerGO.transform.position;
                    bimaGO.transform.LookAt(new Vector3(pp.x, bimaGO.transform.position.y, pp.z));
                }
            }
            var drvGO = FindDriver();
            if (drvGO != null)
            {
                var drv = drvGO.GetComponent<BimaConversationDriver>();
                if (drv != null)
                {
                    PrimeDriver(drv, playerGO, bimaGO);
                    drv.StartCanteenWalk();
                }
                else UnityEngine.Debug.LogWarning("[DeveloperMissionSelect] Component BimaConversationDriver tidak ditemukan, Misi 2 tidak dimulai.");
            }
            else UnityEngine.Debug.LogWarning("[DeveloperMissionSelect] GO BimaConversationDriver tidak ditemukan, Misi 2 tidak dimulai.");
        }

        // Prime cache privat Driver (biasanya diisi SetupConversation, yang dilewati
        // saat loncat langsung ke Misi 2). Tanpa ini StartMission2 diam-diam tidak
        // jalan (bimaAgent null -> blok agent di-skip, Bima diam).
        private void PrimeDriver(BimaConversationDriver drv, GameObject playerGO, GameObject bimaGO)
        {
            var drvType = drv.GetType();
            System.Reflection.BindingFlags priv = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            if (bimaGO != null)
            {
                var fAgent = drvType.GetField("bimaAgent", priv);
                if (fAgent != null) fAgent.SetValue(drv, bimaGO.GetComponent<NavMeshAgent>());
                var fBimaTf = drvType.GetField("bimaTf", priv);
                if (fBimaTf != null) fBimaTf.SetValue(drv, bimaGO.transform);
                var fAnim = drvType.GetField("bimaAnimator", priv);
                if (fAnim != null) fAnim.SetValue(drv, bimaGO.GetComponent<Animator>());
                var fGuide = drvType.GetField("guideController", priv);
                if (fGuide != null) fGuide.SetValue(drv, bimaGO.GetComponent<GuideController>());
            }
            if (playerGO != null)
            {
                var fPlayerTf = drvType.GetField("playerTf", priv);
                if (fPlayerTf != null) fPlayerTf.SetValue(drv, playerGO.transform);
                foreach (var comp in playerGO.GetComponents<Component>())
                {
                    if (comp != null && comp.GetType().Name == "PlayerMovement")
                    {
                        var fPM = drvType.GetField("playerMovement", priv);
                        if (fPM != null) fPM.SetValue(drv, comp);
                        break;
                    }
                }
            }
        }

        // Pindah Player yang pakai CharacterController + Rigidbody: set transform saja
        // akan dikembalikan physics ke posisi Rigidbody lama (snap back ke spawn).
        // Pola benar (seperti PRDSmoke): matikan CharacterController, set posisi,
        // samakan Rigidbody, nyalakan lagi, lalu SyncTransforms.
        private void TeleportPlayer(GameObject playerGO, Vector3 dest)
        {
            if (playerGO == null) return;
            var cc = playerGO.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerGO.transform.position = dest;
            var rb = playerGO.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = dest;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            if (cc != null) cc.enabled = true;
            Physics.SyncTransforms();
        }

        private GameObject FindPlayer()
        {
            return GameObject.FindGameObjectWithTag("Player");
        }

        private GameObject FindBima()
        {
            return GameObject.Find("Bima_Guide");
        }

        private GameObject FindDriver()
        {
            return GameObject.Find("BimaConversationDriver");
        }

        private void SetPlayerMovementEnabled(GameObject playerGO, bool enabled)
        {
            if (playerGO == null) return;
            foreach (var comp in playerGO.GetComponents<Component>())
            {
                if (comp != null && comp.GetType().Name == "PlayerMovement" && comp is Behaviour)
                {
                    ((Behaviour)comp).enabled = enabled;
                    break;
                }
            }
        }

        private void WarpBima(GameObject bimaGO, Vector3 dest)
        {
            if (bimaGO == null) return;
            var agent = bimaGO.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(dest, out hit, 5.0f, NavMesh.AllAreas)) agent.Warp(hit.position);
                else agent.Warp(dest);
            }
            else bimaGO.transform.position = dest;
        }
    }
}
