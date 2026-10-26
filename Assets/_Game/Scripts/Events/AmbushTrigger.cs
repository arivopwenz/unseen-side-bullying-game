using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Cinemachine;
using BullyingGame.NPC;

namespace BullyingGame.Events
{
    // Ambush mini-cutscene for the Chapter 1 tour (Tahap 4c).
    // When the Player (or the guide) enters the midway zone, the tour pauses,
    // the bully group approaches its confrontation points and faces the Player,
    // then the mini-cutscene runs: player locked, dedicated Cinemachine camera,
    // dialogue advances on E, a placeholder physical beat (shove) plays,
    // then the group returns home and the tour resumes.
    [RequireComponent(typeof(Collider))]
    public class AmbushTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GuideController guide;
        [SerializeField] private BullyGroupController group;
        [Header("Beat")]
        [SerializeField, Min(0f)] private float confrontBeat = 5f;
        [SerializeField] private bool triggerOnce = true;

        [Header("Confrontation dialogue (Tahap 3; if unassigned, the timed beat is used)")]
        [SerializeField] private AmbushDialogueDriver dialogue;

        [Header("Mini-cutscene (Tahap 4c)")]
        [SerializeField] private CinemachineCamera ambushCamera;
        [SerializeField] private CinemachineCamera gameplayCamera;

        private bool hasTriggered;
        private bool beatRunning;
        private GameObject lockedPlayer;
        private List<MonoBehaviour> lockedBehaviours;
        private bool cameraActivated;

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && hasTriggered) return;
            if (guide == null || group == null) return;
            bool isPlayer = other.CompareTag("Player");
            bool isGuide = other.gameObject == guide.gameObject;
            if (!isPlayer && !isGuide) return;
            hasTriggered = true;
            guide.PauseGuide();
            if (!group.TryStartApproach())
            {
                // Approach could not start (bad wiring): don't strand the tour.
                guide.ResumeGuide();
                return;
            }
            LockPlayer();
            group.OnApproachCompleted += OnApproachCompleted;
        }

        private void OnApproachCompleted(bool success)
        {
            group.OnApproachCompleted -= OnApproachCompleted;
            if (!success || guide == null) { guide?.ResumeGuide(); UnlockPlayer(); return; }
            if (!beatRunning)
            {
                ActivateCineCamera();
                if (dialogue != null) StartCoroutine(ConfrontWithDialogue());
                else StartCoroutine(ConfrontBeat());
            }
        }

        private System.Collections.IEnumerator ConfrontWithDialogue()
        {
            beatRunning = true;
            FaceGuideToBully();
            dialogue.PlayLinesRange(0, 2);
            yield return StartCoroutine(WaitDialogue(2));
            yield return StartCoroutine(HitBeat());
            FaceGuideToBully();
            dialogue.PlayLinesRange(2, 3);
            yield return StartCoroutine(WaitDialogue(3));
            group.EndEncounter();
            yield return new WaitForSeconds(0.5f);
            guide.ResumeGuide();
            EndConfrontation();
            beatRunning = false;
        }

        private System.Collections.IEnumerator WaitDialogue(int partLines)
        {
            float elapsed = 0f;
            float timeout = dialogue.WaitForKeyAdvance ? 600f : dialogue.LineDisplayTime * partLines + 10f;
            while (!dialogue.Finished && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (!dialogue.Finished) dialogue.StopLines();
        }

        private System.Collections.IEnumerator ConfrontBeat()
        {
            beatRunning = true;
            yield return new WaitForSeconds(confrontBeat);
            group.EndEncounter();
            yield return new WaitForSeconds(0.5f);
            guide.ResumeGuide();
            EndConfrontation();
            beatRunning = false;
        }

        private void EndConfrontation()
        {
            UnlockPlayer();
            DeactivateCineCamera();
        }

        private void LockPlayer()
        {
            lockedPlayer = GameObject.FindGameObjectWithTag("Player");
            lockedBehaviours = new List<MonoBehaviour>();
            if (lockedPlayer == null) return;
            foreach (var mb in lockedPlayer.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                string tn = mb.GetType().Name;
                if (tn == "PlayerMovement" || tn == "PlayerInputHandler" || tn == "PlayerInteraction" || tn == "InteractionDetector")
                {
                    if (mb.enabled) { lockedBehaviours.Add(mb); mb.enabled = false; }
                }
            }
            var rb = lockedPlayer.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic) rb.linearVelocity = Vector3.zero;
        }

        private void UnlockPlayer()
        {
            if (lockedBehaviours != null)
            {
                foreach (var mb in lockedBehaviours)
                    if (mb != null) mb.enabled = true;
                lockedBehaviours = null;
            }
            lockedPlayer = null;
        }

        private void ActivateCineCamera()
        {
            if (ambushCamera == null || cameraActivated) return;
            cameraActivated = true;
            ambushCamera.gameObject.SetActive(true);
            if (gameplayCamera != null) gameplayCamera.gameObject.SetActive(false);
        }

        private void DeactivateCineCamera()
        {
            if (ambushCamera == null || !cameraActivated) return;
            cameraActivated = false;
            if (gameplayCamera != null) gameplayCamera.gameObject.SetActive(true);
            ambushCamera.gameObject.SetActive(false);
        }

        // Bima menghadap ke Nakal selama konfrontasi (Ari 2026-10-26: di
        // mini-cutscene Bima harus melihat pembully, bukan membelakanginya).
        // Dipanggil di awal dialog dan sesudah HitBeat (posisi berubah).
        private void FaceGuideToBully()
        {
            if (guide == null || group == null) return;
            BullyNPC nakal = null;
            for (int i = 0; i < group.MemberCount; i++)
            {
                var m = group.GetMemberNPC(i);
                if (m != null && m.name == "Nakal") { nakal = m; break; }
            }
            if (nakal == null) return;
            Vector3 dir = nakal.transform.position - guide.transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.001f) return;
            guide.transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        private System.Collections.IEnumerator HitBeat()
        {
            BullyNPC nakal = null;
            for (int i = 0; i < group.MemberCount; i++)
            {
                var m = group.GetMemberNPC(i);
                if (m != null && m.name == "Nakal") { nakal = m; break; }
            }
            if (nakal == null || guide == null || dialogue == null) yield break;
            var nakalAgent = nakal.GetComponent<NavMeshAgent>();
            var bimaAgent = guide.GetComponent<NavMeshAgent>();
            if (nakalAgent == null || bimaAgent == null) yield break;
            Vector3 bimaPos = guide.transform.position;
            Vector3 nakalPos = nakal.transform.position;
            Vector3 dir = bimaPos - nakalPos;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.001f) dir = nakal.transform.forward;
            dir.y = 0;
            dir.Normalize();
            dialogue.ShowCue("Nakal mendorong Bima!");
            bool nakalUpd = nakalAgent.updatePosition;
            bool bimaUpd = bimaAgent.updatePosition;
            nakalAgent.updatePosition = false;
            bimaAgent.updatePosition = false;
            Vector3 nakalFrom = nakal.transform.position;
            Vector3 nakalTo = bimaPos - dir * 0.75f;
            Vector3 bimaFrom = bimaPos;
            Vector3 bimaTo = bimaPos + dir * 0.5f;
            try
            {
                float t = 0f;
                while (t < 0.15f)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / 0.15f);
                    nakal.transform.position = Vector3.Lerp(nakalFrom, nakalTo, k);
                    guide.transform.position = Vector3.Lerp(bimaFrom, bimaTo, k);
                    yield return null;
                }
                yield return new WaitForSeconds(0.25f);
                Vector3 nakalBack = nakalTo - dir * 0.2f;
                t = 0f;
                while (t < 0.15f)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / 0.15f);
                    nakal.transform.position = Vector3.Lerp(nakalTo, nakalBack, k);
                    yield return null;
                }
            }
            finally
            {
                nakalAgent.updatePosition = nakalUpd;
                bimaAgent.updatePosition = bimaUpd;
                NavMeshHit hit;
                Vector3 np = nakal.transform.position;
                if (NavMesh.SamplePosition(np, out hit, 1.5f, NavMesh.AllAreas)) nakalAgent.Warp(hit.position);
                else nakalAgent.Warp(np);
                Vector3 bp = guide.transform.position;
                if (NavMesh.SamplePosition(bp, out hit, 1.5f, NavMesh.AllAreas)) bimaAgent.Warp(hit.position);
                else bimaAgent.Warp(bp);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}
