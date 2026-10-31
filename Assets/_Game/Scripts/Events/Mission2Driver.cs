using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BullyingGame.Core;
using BullyingGame.Quest;
using TMPro;

namespace BullyingGame.Events
{
    // Misi 2 (Observasi Bystander) + Misi 3 (Uluran Tangan Ringan Pertama).
    // Sequence gameplay mandiri yang aktif setelah cutscene kelas selesai.
    // Progres dicerminkan ke QuestManager secara oportunistik (M02/M03):
    // bila quest belum bisa Active (prereq M01 belum Completed), sequence
    // tetap jalan dan pencerminan dicatat gagal lewat flag QuestMirrored.
    // Subtitle memakai MissionSubtitle sendiri; tidak pernah mengunci
    // movement, tidak mengubah GameState, tidak menyentuh DialogueManager.
    public class Mission2Driver : MonoBehaviour
    {
        [Header("Referensi")]
        [SerializeField] private ClassroomCutsceneDriver classroomCutscene;
        [SerializeField] private GameObject bima;
        [SerializeField] private GameObject bully1;
        [SerializeField] private GameObject bully2;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private QuestData questM02;
        [SerializeField] private QuestData questM03;

        [Header("Titik staging (y = level NavMesh setempat)")]
        [SerializeField] private Vector3 fieldBimaMark = new Vector3(-16.4f, 0.14f, -5.1f);
        [SerializeField] private Vector3 fieldGang1Mark = new Vector3(-14.5f, 0.13f, -6.5f);
        [SerializeField] private Vector3 fieldGang2Mark = new Vector3(-18.5f, 0.14f, -6.5f);
        [SerializeField] private Vector3 cornerGang1Mark = new Vector3(-3.5f, 0f, 6.5f);
        [SerializeField] private Vector3 cornerGang2Mark = new Vector3(-0.5f, 0f, 6.5f);
        [SerializeField] private Vector3 cornerWaitMark = new Vector3(-2f, 0f, 3f);

        [Header("Dialog")]
        [SerializeField] private float lineDisplayTime = 4f;
        [SerializeField] private float triggerDistance = 6f;
        [SerializeField] private string[] introLines = new string[]
        {
            "Sari: (narasi) Bel istirahat berbunyi. Sekarang giliranmu mengamati, Ji — tiga kejadian, tanpa ikut menertawakan.",
            "Sari: (narasi) Mulai dari lapangan. Bima ada di sana."
        };
        [SerializeField] private string[] incident1Lines = new string[]
        {
            "Nakal: (menunjuk) Eh, itu si gendut lagi. Lapangan bukan tempat buat yang jalannya lelet.",
            "Nakal1: Iya, istirahat aja ngos-ngosan. Pulang sana, jangan bikin malu.",
            "Bima: (menunduk) Aku cuma mau duduk di sini sebentar...",
            "Sari: (narasi) Lihat itu, Ji. Kamu tidak ikut menertawakan — itu sudah benar."
        };
        [SerializeField] private string[] incident2Lines = new string[]
        {
            "Nakal: Eh, kelompok kami sudah penuh. Cari kelompok lain aja ya.",
            "Nerd: Oh... oke. Tidak apa-apa, aku belajar sendiri saja.",
            "Sari: (narasi) Penolakan seperti itu sakitnya tidak kelihatan, Ji."
        };
        [SerializeField] private string[] incident3Lines = new string[]
        {
            "Nakal: Buku tugasmu sini. Aku yang kumpulin, atas nama aku.",
            "Nerd: (tegang) Tapi... itu tugasku. Aku yang kerjakan semalam.",
            "Nakal: Makanya jangan banyak protes. Besok bawa lagi yang baru.",
            "Sari: (narasi) Itu bukan pinjam, Ji. Itu pemerasan."
        };
        [SerializeField] private string[] closingLines = new string[]
        {
            "Sari: (narasi) Tiga kejadian sudah kamu amati. Polanya jelas sekarang: Bima dan dia selalu jadi sasaran.",
            "Sari: (narasi) Sekarang, uluran tangan ringan. Dekati mereka — sapa, tawarkan bantuan."
        };
        [SerializeField] private string[] supportClosingLines = new string[]
        {
            "Sari: (narasi) Mereka mulai percaya padamu, Ji. Itu langkah pertama — mereka siap cerita di lain waktu."
        };

        private static readonly string[] observeIds = new string[] { "observe_bima", "observe_laras", "observe_raka" };
        private static readonly string[] supportIds = new string[] { "support_bima", "support_laras", "support_raka" };

        public bool Mission2Armed { get; private set; }
        public bool Mission2Complete { get; private set; }
        public bool Mission3Active { get; private set; }
        public bool Mission3Complete { get; private set; }
        public int ObservedCount { get; private set; }
        public int SupportedCount { get; private set; }
        public bool[] Observed = new bool[3];
        public bool[] Supported = new bool[3];
        public bool QuestM02Mirrored { get; private set; }
        public bool QuestM03Mirrored { get; private set; }
        public string LastLine { get; private set; } = "";
        public int ShownCount { get; private set; }
        public string LastFail { get; private set; } = "";
        public string CurrentStage { get; private set; } = "MenungguCutsceneKelas";

        private GameObject player;
        private bool incident3Staged;
        private readonly Queue<string[]> lineQueue = new Queue<string[]>();
        private bool runnerRunning;
        private string pendingPrompt = "";
        private string shownPrompt = "";
        private bool promptWanted;

        private void Start()
        {
            player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
        }

        private void Update()
        {
            if (subtitleText != null && GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState != GameState.Playing)
            {
                subtitleText.text = "";
            }
            if (!Mission2Armed)
            {
                if (classroomCutscene != null && classroomCutscene.Finished) ArmMission2();
                return;
            }
            if (Mission3Complete) return;
            if (player == null) return;
            if (!Mission2Complete)
            {
                if (!Observed[0] && Near(fieldBimaMark)) TriggerIncident(0);
                else if (Observed[0] && !Observed[1] && Near(cornerMark)) TriggerIncident(1);
                else if (Observed[1] && !Observed[2] && incident3Staged && Near(cornerMark)) TriggerIncident(2);
                if (Observed[1] && !Observed[2] && !incident3Staged && !IsPlayingLines()) StageIncident3();
            }
            if (subtitleText != null && !IsPlayingLines() && promptWanted && pendingPrompt != "")
            {
                subtitleText.text = pendingPrompt;
                shownPrompt = pendingPrompt;
            }
        }

        private Vector3 cornerMark
        {
            get
            {
                // Titik nerd di koridor (sama dengan mark koridor tur).
                return new Vector3(-2f, -0.01f, 7f);
            }
        }

        private bool Near(Vector3 mark)
        {
            Vector3 d = player.transform.position - mark;
            d.y = 0;
            return d.magnitude <= triggerDistance;
        }

        private void ArmMission2()
        {
            Mission2Armed = true;
            CurrentStage = "Misi2";
            QuestM02Mirrored = TryStartQuest(questM02);
            SendToMark(bima, fieldBimaMark);
            SendToMark(bully1, fieldGang1Mark);
            SendToMark(bully2, fieldGang2Mark);
            EnqueueLines(introLines);
        }

        private void TriggerIncident(int index)
        {
            Observed[index] = true;
            ObservedCount++;
            MirrorObjective(questM02, observeIds[index]);
            if (index == 0)
            {
                SendToMark(bully1, cornerGang1Mark);
                SendToMark(bully2, cornerGang2Mark);
                EnqueueLines(incident1Lines);
            }
            else if (index == 1)
            {
                EnqueueLines(incident2Lines);
            }
            else
            {
                EnqueueLines(incident3Lines);
            }
            if (ObservedCount >= 3 && !Mission2Complete)
            {
                Mission2Complete = true;
                Mission3Active = true;
                CurrentStage = "Misi3";
                QuestM03Mirrored = TryStartQuest(questM03);
                EnqueueLines(closingLines);
            }
        }

        private void StageIncident3()
        {
            incident3Staged = true;
            SendToMark(bully2, cornerWaitMark);
        }

        public void MarkSupported(int index, string[] lines)
        {
            if (index < 0 || index >= Supported.Length || Supported[index]) return;
            Supported[index] = true;
            SupportedCount++;
            MirrorObjective(questM03, supportIds[index]);
            EnqueueLines(lines);
            if (SupportedCount >= 3 && !Mission3Complete)
            {
                Mission3Complete = true;
                CurrentStage = "Selesai";
                EnqueueLines(supportClosingLines);
            }
        }

        public void ShowPrompt(string text)
        {
            pendingPrompt = text;
            promptWanted = true;
        }

        public void ClearPrompt()
        {
            promptWanted = false;
            pendingPrompt = "";
            if (subtitleText != null && !IsPlayingLines() && shownPrompt != "" &&
                subtitleText.text == shownPrompt)
            {
                subtitleText.text = "";
            }
            shownPrompt = "";
        }

        public bool IsPlayingLines()
        {
            return runnerRunning || lineQueue.Count > 0;
        }

        private void EnqueueLines(string[] lines)
        {
            if (lines == null || lines.Length == 0) return;
            lineQueue.Enqueue(lines);
            if (!runnerRunning) StartCoroutine(RunQueue());
        }

        private IEnumerator RunQueue()
        {
            runnerRunning = true;
            while (lineQueue.Count > 0)
            {
                string[] set = lineQueue.Dequeue();
                foreach (string line in set)
                {
                    while (GameStateManager.Instance != null &&
                           GameStateManager.Instance.CurrentState != GameState.Playing)
                    {
                        yield return null;
                    }
                    if (subtitleText != null) subtitleText.text = line;
                    LastLine = line;
                    ShownCount++;
                    float voiceLen = VoiceLinePlayer.PlayLine(line);
                    yield return new WaitForSeconds(voiceLen > 0f ? voiceLen + 0.25f : lineDisplayTime);
                    if (subtitleText != null) subtitleText.text = "";
                }
            }
            runnerRunning = false;
        }

        private bool TryStartQuest(QuestData quest)
        {
            if (QuestManager.Instance == null || quest == null) return false;
            if (QuestManager.Instance.GetQuestState(quest) == QuestState.Locked)
            {
                QuestManager.Instance.MakeQuestAvailable(quest);
            }
            if (QuestManager.Instance.GetQuestState(quest) == QuestState.Available)
            {
                QuestManager.Instance.TryStartQuest(quest);
            }
            return QuestManager.Instance.GetQuestState(quest) == QuestState.Active;
        }

        private void MirrorObjective(QuestData quest, string objectiveId)
        {
            if (QuestManager.Instance == null || quest == null) return;
            if (QuestManager.Instance.GetQuestState(quest) == QuestState.Active)
            {
                QuestManager.Instance.UpdateObjective(quest, objectiveId);
            }
        }

        private void SendToMark(GameObject actor, Vector3 mark)
        {
            if (actor == null) return;
            NavMeshAgent agent = actor.GetComponent<NavMeshAgent>();
            if (agent == null) return;
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(mark, out hit, 2f, NavMesh.AllAreas))
            {
                LastFail = actor.name + " sample gagal " + mark.ToString("F2");
                return;
            }
            if (!agent.isOnNavMesh)
            {
                agent.Warp(hit.position);
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }
}
