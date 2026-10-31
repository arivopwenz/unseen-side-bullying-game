using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using BullyingGame.Events;
using BullyingGame.NPC;
using BullyingGame.UI;

namespace BullyingGame.DebugTools
{
    /// <summary>
    /// Debug teleport untuk testing (hanya jalan di Play, tekan tombol angka).
    /// 1 = Gerbang Selatan (-7.10, -50.09), 2 = Koridor (9.74, 17.01),
    /// 3 = Kantin (-39.50, -1.00), 4 = Halaman Kelas (-20, 12.50).
    /// 0 = Langsung ke kantin + Misi 2 selesai (untuk test cutscene setelah kantin).
    /// Teleport memindahkan Player + Bima bersama (Bima 0.8m di depan Player, menghadap Player),
    /// menonaktifkan GuideController (biar Bima diam di tempat, tidak jalan tur),
    /// Bima di-Warp ke NavMesh (bukan transform.position, biar agent tetap on-NavMesh).
    /// Nonaktifkan/hapus GO DebugTeleport untuk build final (debug only).
    /// </summary>
    public class DebugTeleport : MonoBehaviour
    {
        private Vector3 gatePos = new Vector3(-7.10f, 0.05f, -50.09f);
        private Vector3 corridorPos = new Vector3(9.74f, 0f, 17.01f);
        private Vector3 canteenPos = new Vector3(-39.50f, -0.07f, -1.00f);
        private Vector3 yardPos = new Vector3(-20f, 0f, 12.50f);

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.digit1Key.wasPressedThisFrame) TeleportTo(gatePos);
            else if (kb.digit2Key.wasPressedThisFrame) TeleportTo(corridorPos);
            else if (kb.digit3Key.wasPressedThisFrame) TeleportTo(canteenPos);
            else if (kb.digit4Key.wasPressedThisFrame) TeleportTo(yardPos);
            else if (kb.digit0Key.wasPressedThisFrame) SkipToCanteenComplete();
        }

        private void TeleportTo(Vector3 dest)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) playerGO.transform.position = dest;
            var bimaGO = GameObject.Find("Bima_Guide");
            if (bimaGO != null)
            {
                var guide = bimaGO.GetComponent<GuideController>();
                if (guide != null) guide.enabled = false;
                var agent = bimaGO.GetComponent<NavMeshAgent>();
                Vector3 bimaDest = new Vector3(dest.x, dest.y, dest.z + 0.8f);
                if (agent != null)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(bimaDest, out hit, 5.0f, NavMesh.AllAreas)) agent.Warp(hit.position);
                    else agent.Warp(bimaDest);
                }
                else bimaGO.transform.position = bimaDest;
                Vector3 pp = playerGO != null ? playerGO.transform.position : dest;
                bimaGO.transform.LookAt(new Vector3(pp.x, bimaGO.transform.position.y, pp.z));
            }
        }

        private void SkipToCanteenComplete()
        {
            TeleportTo(canteenPos);
            YellowCircleTrigger.Mission1Complete = true;
            BimaConversationDriver.ConversationDone = true;
            BimaConversationDriver.CanteenWalkDone = true;
            MissionBanner.SetObjective("Sampai di kantin");
            GroundMarker.ClearTarget();
            Minimap.ClearMissionTarget();
        }
    }
}
