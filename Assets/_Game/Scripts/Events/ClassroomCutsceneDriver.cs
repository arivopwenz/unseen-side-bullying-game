using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using BullyingGame.NPC;
using BullyingGame.Core;

namespace BullyingGame.Events
{
    // Cutscene kelas (Tahap 4b): dipicu saat Player masuk kelas lewat pintu.
    // Soft cutscene ala walk-talk/ambush: TIDAK mengunci movement, TIDAK mengubah
    // GameState, TIDAK me-teleport Player. Aktor (Bima + gang) berjalan ke mark
    // masing-masing lewat NavMesh (pintu via DoorLink_Kelas).
    // Dialog memakai subtitle sendiri (ClassroomSubtitle) agar tidak bentrok
    // dengan walk-talk (sudah selesai saat tur Arrived) maupun ambush.
    public class ClassroomCutsceneDriver : MonoBehaviour
    {
        [SerializeField] private GameObject bima;
        [SerializeField] private GameObject bully1;
        [SerializeField] private GameObject bully2;
        [SerializeField] private Vector3 bimaMark = new Vector3(-21.5f, 0f, 13.2f);
        [SerializeField] private Vector3 bully1Mark = new Vector3(-22f, 0f, 13f);
        [SerializeField] private Vector3 bully2Mark = new Vector3(-18f, 0f, 13f);
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private float lineDisplayTime = 4f;
        [SerializeField] private float gangWaitTimeout = 15f;
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private string[] lines = new string[]
        {
            "[Bel sekolah berbunyi]",
            "Aji: Selamat pagi, semuanya. Nama saya Aji, murid baru. Senang bertemu kalian.",
            "Ibu Guru: Selamat datang di kelas ini, Aji. Semoga cepat betah ya.",
            "Nakal: (dari pintu) Eh, anak baru sok percaya diri. Tadi aja dibuntuti si gendut.",
            "Nakal1: Hah, perkenalan macam apa itu. Duduk sana, jangan banyak gaya.",
            "Bima: (pelan) Udah, Ji... jangan diladenin."
        };

        private bool hasTriggered;
        private bool isPlaying;

        public bool HasTriggered { get { return hasTriggered; } }
        public bool IsPlaying { get { return isPlaying; } }
        public bool Finished { get; private set; }
        public int ShownCount { get; private set; }
        public string LastLine { get; private set; }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;
            if (other == null || !other.CompareTag("Player")) return;
            StartSequence();
        }

        public void StartSequence()
        {
            if (hasTriggered && triggerOnce) return;
            hasTriggered = true;
            if (isPlaying) return;
            StartCoroutine(RunSequence());
        }

        private static void SendToMark(GameObject go, Vector3 mark)
        {
            if (go == null) return;
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent == null || !agent.isOnNavMesh) return;
            agent.isStopped = false;
            agent.SetDestination(mark);
        }

        private static bool Arrived(GameObject go)
        {
            if (go == null) return true;
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent == null) return true;
            return !agent.hasPath || agent.remainingDistance < 1.5f;
        }

        private IEnumerator RunSequence()
        {
            isPlaying = true;
            Finished = false;

            if (bima != null)
            {
                var guide = bima.GetComponent<GuideController>();
                if (guide != null && guide.IsGuiding) guide.StopGuide();
            }
            SendToMark(bima, bimaMark);
            SendToMark(bully1, bully1Mark);
            SendToMark(bully2, bully2Mark);

            int gangLineIndex = 3;
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == gangLineIndex)
                {
                    float waited = 0f;
                    while (waited < gangWaitTimeout && !(Arrived(bully1) && Arrived(bully2)))
                    {
                        yield return null;
                        waited += Time.deltaTime;
                    }
                }
                while (GameStateManager.Instance != null
                    && GameStateManager.Instance.CurrentState != GameState.Playing)
                {
                    yield return null;
                }
                if (subtitleText != null) subtitleText.text = lines[i];
                LastLine = lines[i];
                ShownCount++;
                float voiceLen = VoiceLinePlayer.PlayLine(lines[i]);
                yield return new WaitForSeconds(voiceLen > 0f ? voiceLen + 0.25f : lineDisplayTime);
            }

            if (subtitleText != null) subtitleText.text = "";
            Finished = true;
            isPlaying = false;
        }
    }
}
