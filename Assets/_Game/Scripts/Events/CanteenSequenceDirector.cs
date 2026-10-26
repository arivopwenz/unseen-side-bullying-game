using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using TMPro;
using BullyingGame.NPC;
using BullyingGame.UI;

namespace BullyingGame.Events
{
    /// <summary>Objective 2 - Kantin (spek baru 2026-10-26, perintah Ari).
    /// Urutan: tiba di depan kelas -&gt; Cutscene 1 (Timeline Ari, slot 1) -&gt; fade
    /// -&gt; Cutscene 2 (Timeline Ari, slot 2: Bima ngobrol dgn Aji + bel istirahat)
    /// -&gt; Objective 2 dimulai: Bima + Aji jalan ke kantin SAMBIL NGOBROL -&gt;
    /// dicegat pembully (trigger zone, TANPA fade, ala ambush awal) -&gt; rebut
    /// paksa botol -&gt; kamera Cinemachine tracking Nakal+Nakal1 LARI ke kantin
    /// -&gt; Aji cari botol di meja jauh DIPANTAU Guru BK (spotting delay) -&gt; selesai.
    /// Slot Timeline kosong = dilewati (bel dibunyikan game bila slot 2 kosong).</summary>
    public class CanteenSequenceDirector : MonoBehaviour
    {
        public enum CanteenPhase { Idle, ToCanteen, Arrived, Confront, HideAndSeek, Complete }

        public CanteenPhase Phase { get; private set; } = CanteenPhase.Idle;
        public bool HasStarted { get { return Phase != CanteenPhase.Idle; } }
        public bool IsComplete { get { return Phase == CanteenPhase.Complete; } }

        [Header("Referensi")]
        public GuideController guide;
        public ClassroomCutsceneDriver classroom;
        public GameObject bima;
        public GameObject bully1;
        public GameObject bully2;
        public GameObject bottle;
        public GameObject guruBK;
        public TMP_Text subtitleText;
        public TMP_Text promptText;
        public UnityEngine.Camera cutsceneCamera;
        public Transform bellProp;
        public AudioSource bellAudio;
        public GameObject timelineSlot;
        public GameObject timelineSlot2;
        public GameObject interceptZone;
        public GameObject trackingCamera;
        public Transform trackingTarget;
        public List<Transform> bkPatrolPoints = new List<Transform>();
        public Transform bully1CP;
        public Transform bully2CP;
        public Transform bully1HideSpot;
        public Transform bully2HideSpot;
        public Transform bimaCanteenMark;
        public Transform bottleSpot;
        public Transform playerResetPoint;
        public int canteenLegStartIndex = 7;

        [Header("Tuning")]
        public float playerArriveDistance = 8f;
        public float talkDistance = 3f;
        public float bottleTakeDistance = 2f;
        public float bkDetectDistance = 4.5f;
        public float lineDisplayTime = 4f;

        private Transform player;
        private bool handoffRunning;
        private bool confrontRunning;
        private bool interceptFired;
        private Coroutine walkChatRoutine;
        private bool spotting;
        private float spottingTimer;
        private Component cineCamComp;
        private bool useCineCam;
        private bool resetting;
        private bool hidden;
        private bool testE;
        private int zonesInside;
        private bool inHideArea { get { return zonesInside > 0; } }
        private float origBase1 = -0.080f;
        private float origBase2 = -0.089f;
        private const float canteenBaseOffset = -0.035f;
        private AudioClip bellClip;
        private Coroutine bkPatrolRoutine;

        private readonly string[] confrontLines = new string[]
        {
            "Nakal: Wah, si gendut bawa botol baru ya? Sini, buat gue!",
            "Nakal1: Hahaha, botolnya lucu. Cocok buat orang gendut.",
            "Nakal: Nah, ketemu juga lo! Mau ke mana, gendut?",
            "Nakal: Ini punya gue sekarang! Gue sembunyiin, cari sendiri sana!",
            "Bima: Tolong... botolku... Aji, bantu aku...",
        };

        private readonly string[] walkChatLines = new string[]
        {
            "Bima: Aji, istirahat yuk... aku mau ke kantin.",
            "Aji: Ayo, aku temenin. Kamu mau beli apa?",
            "Bima: Aku bawa botol baru... tadi pagi dibeliin ibu.",
            "Aji: Bagus tuh. Jangan sampai hilang ya.",
            "Bima: Iya... makasih ya udah mau nemenin aku.",
        };

        private void Start()
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
            if (guide == null)
            {
                var guides = FindObjectsByType<GuideController>(FindObjectsInactive.Include);
                if (guides.Length > 0) guide = guides[0];
            }
            if (classroom == null)
            {
                var rooms = FindObjectsByType<ClassroomCutsceneDriver>(FindObjectsInactive.Include);
                if (rooms.Length > 0) classroom = rooms[0];
            }
            if (bima == null) bima = GameObject.Find("Bima_Guide");
            if (bully1 == null) bully1 = GameObject.Find("Nakal");
            if (bully2 == null) bully2 = GameObject.Find("Nakal1");
            if (guide != null) guide.onArrived.AddListener(OnGuideArrived);
            if (guruBK != null) guruBK.SetActive(false);
            if (cutsceneCamera != null) cutsceneCamera.enabled = false;
            SetPrompt("");
        }

        public void SetHideZoneState(HideZone zone, bool inside)
        {
            zonesInside += inside ? 1 : -1;
            if (zonesInside < 0) zonesInside = 0;
            if (!inHideArea && hidden) hidden = false;
        }

        private void OnGuideArrived()
        {
            if (Phase == CanteenPhase.ToCanteen) Phase = CanteenPhase.Arrived;
        }

        private void Update()
        {
            if (player == null || resetting) return;

            if (Phase == CanteenPhase.Idle)
            {
                if (!handoffRunning && classroom != null && classroom.Finished)
                {
                    handoffRunning = true;
                    StartCoroutine(StartHandoff());
                }
                return;
            }

            if (Phase == CanteenPhase.ToCanteen)
            {
                // Jalan ke kantin: chat jalan via coroutine; arrival via OnGuideArrived.
                return;
            }

            if (Phase == CanteenPhase.Arrived)
            {
                if (!confrontRunning && bima != null &&
                    Vector3.Distance(player.position, bima.transform.position) <= playerArriveDistance)
                {
                    confrontRunning = true;
                    StartCoroutine(StartConfront());
                }
                return;
            }

            if (Phase == CanteenPhase.HideAndSeek)
            {
                // Prioritas: ambil botol dulu, baru deteksi ketahuan.
                if (bottle != null && bottle.activeSelf &&
                    Vector3.Distance(player.position, bottle.transform.position) <= bottleTakeDistance)
                {
                    SetPrompt("Tekan [E] untuk mengambil botol");
                    if (PressedE())
                    {
                        CompleteSequence();
                        return;
                    }
                }
                else if (inHideArea && !hidden)
                {
                    SetPrompt("Tekan [E] untuk bersembunyi");
                    if (PressedE()) hidden = true;
                }
                else if (hidden)
                {
                    SetPrompt("Kamu bersembunyi - Tekan [E] untuk keluar");
                    if (PressedE()) hidden = false;
                }
                else
                {
                    SetPrompt("");
                }

                // Guru BK: spotting delay ala prefect Bully (0.8 detik menoleh dulu).
                if (guruBK != null && guruBK.activeSelf && !hidden)
                {
                    float dBK = Vector3.Distance(player.position, guruBK.transform.position);
                    if (dBK <= bkDetectDistance)
                    {
                        if (!spotting)
                        {
                            spotting = true;
                            spottingTimer = 0f;
                            var agBK = guruBK.GetComponent<NavMeshAgent>();
                            if (agBK != null) agBK.isStopped = true;
                            SetPrompt("Guru BK melihatmu! Cepat bersembunyi!");
                        }
                        else
                        {
                            spottingTimer += Time.deltaTime;
                            Vector3 dir = player.position - guruBK.transform.position;
                            dir.y = 0f;
                            if (dir.sqrMagnitude > 0.001f)
                            {
                                Quaternion target = Quaternion.LookRotation(dir);
                                guruBK.transform.rotation = Quaternion.Slerp(guruBK.transform.rotation, target, 10f * Time.deltaTime);
                            }
                            if (spottingTimer >= 0.8f)
                            {
                                spotting = false;
                                var agBK2 = guruBK.GetComponent<NavMeshAgent>();
                                if (agBK2 != null) agBK2.isStopped = false;
                                StartCoroutine(CaughtReset());
                            }
                        }
                    }
                    else if (spotting)
                    {
                        spotting = false;
                        var agBK = guruBK.GetComponent<NavMeshAgent>();
                        if (agBK != null) agBK.isStopped = false;
                    }
                }
                else if (spotting)
                {
                    spotting = false;
                    var agBK = guruBK.GetComponent<NavMeshAgent>();
                    if (agBK != null) agBK.isStopped = false;
                }
                return;
            }
        }

        private bool PressedE()
        {
            if (testE) { testE = false; return true; }
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.eKey.wasPressedThisFrame;
        }

        /// <summary>Hook tes: mensimulasikan tombol E (untuk harness verifikasi).</summary>
        public void TestPressE() { testE = true; }

        private IEnumerator PlayTimelineSlot(GameObject slot)
        {
            PlayableDirector pd = null;
            if (slot != null) pd = slot.GetComponent<PlayableDirector>();
            if (pd != null && pd.playableAsset != null)
            {
                pd.Play();
                float timeout = 0f;
                while (pd.state == PlayState.Playing && timeout < 60f)
                {
                    timeout += Time.deltaTime;
                    yield return null;
                }
                pd.Stop();
            }
            yield break;
        }

        private IEnumerator StartHandoff()
        {
            // Tiba di depan kelas -> Cutscene 1 (Timeline Ari, slot 1) dengan fade.
            FadeController.FadeOut(0.5f);
            yield return new WaitForSeconds(0.6f);
            yield return PlayTimelineSlot(timelineSlot);
            FadeController.FadeIn(0.5f);
            yield return new WaitForSeconds(0.6f);

            // Bullies keluar dari kelas (disembunyikan) sebelum chat Bima-Aji.
            if (bully1 != null) bully1.SetActive(false);
            if (bully2 != null) bully2.SetActive(false);

            // Fade in/out menuju Cutscene 2 (Timeline Ari, slot 2: Bima ngobrol dgn Aji + bel).
            FadeController.FadeOut(0.5f);
            yield return new WaitForSeconds(0.6f);
            yield return PlayTimelineSlot(timelineSlot2);
            FadeController.FadeIn(0.5f);
            yield return new WaitForSeconds(0.6f);

            // Kalau Cutscene 2 belum dibuat Ari, game membunyikan bel istirahat sendiri
            // (bila slot 2 sudah di-assign, bel dipercaya ada di Timeline Ari).
            PlayableDirector pd2 = timelineSlot2 != null ? timelineSlot2.GetComponent<PlayableDirector>() : null;
            if (pd2 == null || pd2.playableAsset == null) PlayBellSound();

            // Objective 2 dimulai: Bima + Aji jalan ke kantin.
            if (guide != null)
            {
                StartObjective2();
            }
            else
            {
                if (bima != null && bimaCanteenMark != null)
                {
                    var ag = bima.GetComponent<NavMeshAgent>();
                    if (ag != null && ag.isOnNavMesh) ag.Warp(bimaCanteenMark.position);
                    else bima.transform.position = bimaCanteenMark.position;
                }
                MissionTitleCard.ShowTitle("OBJECTIVE 2", "Kantin");
                MissionBanner.SetObjective("Temui Bima di kantin");
                Vector3 fbPlace = bimaCanteenMark != null ? bimaCanteenMark.position : new Vector3(-39.5f, 0f, -1f);
                GroundMarker.SetTarget(fbPlace);
                Minimap.SetMissionTarget(fbPlace);
                CompanionBar.SetCompanion("Bima", 1f, true);
                ClockHUD.SetSchedule("Istirahat");
                interceptFired = false;
                if (interceptZone != null) interceptZone.SetActive(true);
                Phase = CanteenPhase.Arrived;
            }
        }

        private void StartObjective2()
        {
            Phase = CanteenPhase.ToCanteen;
            MissionTitleCard.ShowTitle("OBJECTIVE 2", "Kantin");
            MissionBanner.SetObjective("Ikuti Bima ke kantin");
            Vector3 canteenPlace = bimaCanteenMark != null ? bimaCanteenMark.position : new Vector3(-39.5f, 0f, -1f);
            GroundMarker.SetTarget(canteenPlace);
            Minimap.SetMissionTarget(canteenPlace);
            CompanionBar.SetCompanion("Bima", 1f, true);
            ClockHUD.SetSchedule("Istirahat");
            if (bottle != null && bima != null)
            {
                bottle.SetActive(true);
                bottle.transform.SetParent(bima.transform, false);
                bottle.transform.localPosition = new Vector3(0.35f, 1.05f, 0.25f);
                bottle.transform.localRotation = Quaternion.identity;
            }
            interceptFired = false;
            if (interceptZone != null) interceptZone.SetActive(true);
            if (guide != null) guide.BeginLeg(canteenLegStartIndex);
            if (walkChatRoutine != null) StopCoroutine(walkChatRoutine);
            walkChatRoutine = StartCoroutine(WalkChat());
        }

        private IEnumerator WalkChat()
        {
            foreach (string line in walkChatLines)
            {
                if (Phase != CanteenPhase.ToCanteen && Phase != CanteenPhase.Arrived) yield break;
                SetSubtitle(line);
                yield return new WaitForSeconds(4f);
            }
            SetSubtitle("");
        }

        /// <summary>Dipanggil InterceptTrigger saat Player masuk zona pencegatan (tanpa fade).</summary>
        public void RequestIntercept()
        {
            if (Phase != CanteenPhase.ToCanteen && Phase != CanteenPhase.Arrived) return;
            if (interceptFired || confrontRunning) return;
            interceptFired = true;
            confrontRunning = true;
            if (interceptZone != null) interceptZone.SetActive(false);
            if (walkChatRoutine != null) { StopCoroutine(walkChatRoutine); walkChatRoutine = null; }
            SetSubtitle("");
            StartCoroutine(StartConfront());
        }

        private IEnumerator StartConfront()
        {
            Phase = CanteenPhase.Confront;
            MissionBanner.SetObjective("Bima dicegat pembully");
            Vector3 zonePos = interceptZone != null ? interceptZone.transform.position : new Vector3(-27f, 0f, -0.5f);
            Vector3 confrontPlace = interceptZone != null ? zonePos : (bimaCanteenMark != null ? bimaCanteenMark.position : new Vector3(-39.5f, 0f, -1f));
            GroundMarker.SetTarget(confrontPlace);
            Minimap.SetMissionTarget(confrontPlace);
            CompanionBar.SetCompanion("Bima", 0.7f, true);

            // Bullies MENCEGAT dari sisi jalan (flank zona), lalu mendekati Bima.
            Vector3 flank1 = zonePos + new Vector3(4f, 0f, -3.5f);
            Vector3 flank2 = zonePos + new Vector3(-4f, 0f, 3.5f);
            SpawnBullyAt(bully1, flank1, ref origBase1);
            SpawnBullyAt(bully2, flank2, ref origBase2);
            if (bima != null)
            {
                Vector3 bp = bima.transform.position;
                SetAgentSpeed(bully1, 4.5f);
                SetAgentSpeed(bully2, 4.5f);
                if (bully1 != null) yield return WalkAgent(bully1, bp + new Vector3(1.5f, 0f, 0.5f), 12f);
                if (bully2 != null) yield return WalkAgent(bully2, bp + new Vector3(-1.5f, 0f, -0.5f), 12f);
            }
            FaceTarget(bully1, bima);
            FaceTarget(bully2, bima);
            FaceTarget(bima, bully1);

            LockPlayer();

            // Dialog ejekan + pencegatan.
            yield return ShowLine(confrontLines[0]);
            yield return ShowLine(confrontLines[1]);
            yield return ShowLine(confrontLines[2]);

            // Hit beat ala ambush: Nakal mendekat + dorong Bima.
            yield return Shove();

            // Rebut paksa botol.
            yield return ShowLine(confrontLines[3]);
            if (bottle != null && bully1 != null)
            {
                bottle.transform.SetParent(bully1.transform, false);
                bottle.transform.localPosition = new Vector3(0.35f, 1.05f, 0.25f);
                bottle.transform.localRotation = Quaternion.identity;
            }
            yield return ShowLine(confrontLines[4]);
            SetSubtitle("");

            // Tracking run: bullies LARI ke kantin, kamera Cinemachine mengikuti.
            yield return TrackingRun();

            // Pemain bebas + objective cari botol dipantau Guru BK.
            UnlockPlayer();
            StartHideAndSeek();
        }

        private void SpawnBullyAt(GameObject bully, Vector3 pos, ref float origBase)
        {
            if (bully == null) return;
            bully.SetActive(true);
            var ag = bully.GetComponent<NavMeshAgent>();
            if (ag != null)
            {
                origBase = ag.baseOffset;
                ag.baseOffset = canteenBaseOffset;
                ag.isStopped = true;
                ag.ResetPath();
                if (ag.isOnNavMesh) ag.Warp(pos);
                else bully.transform.position = pos;
                ag.isStopped = false;
            }
            else
            {
                bully.transform.position = pos;
            }
        }

        private void SetAgentSpeed(GameObject obj, float speed)
        {
            if (obj == null) return;
            var ag = obj.GetComponent<NavMeshAgent>();
            if (ag != null) ag.speed = speed;
        }

        private void ResolveTrackingCamera()
        {
            cineCamComp = null;
            useCineCam = false;
            if (trackingCamera != null)
            {
                foreach (var comp in trackingCamera.GetComponents<Component>())
                {
                    if (comp != null && comp.GetType().FullName == "Unity.Cinemachine.CinemachineCamera")
                    {
                        cineCamComp = comp;
                        break;
                    }
                }
                if (cineCamComp != null)
                {
                    try
                    {
                        var t = cineCamComp.GetType();
                        var pri = t.GetProperty("Priority");
                        if (pri != null) pri.SetValue(cineCamComp, 20, null);
                        var look = t.GetProperty("LookAt");
                        if (look != null && trackingTarget != null) look.SetValue(cineCamComp, trackingTarget, null);
                        useCineCam = true;
                    }
                    catch { useCineCam = false; }
                }
            }
            if (useCineCam)
            {
                trackingCamera.SetActive(true);
            }
            else if (cutsceneCamera != null)
            {
                cutsceneCamera.enabled = true;
            }
        }

        private void ReleaseTrackingCamera()
        {
            if (useCineCam && trackingCamera != null) trackingCamera.SetActive(false);
            else if (cutsceneCamera != null) cutsceneCamera.enabled = false;
            useCineCam = false;
            cineCamComp = null;
        }

        private IEnumerator TrackingRun()
        {
            ResolveTrackingCamera();
            // Nakal (bawa botol) lari ke meja jauh; Nakal1 lari ke hide spot.
            SetAgentSpeed(bully1, 6f);
            SetAgentSpeed(bully2, 6f);
            if (bully1 != null && bottleSpot != null)
            {
                var ag = bully1.GetComponent<NavMeshAgent>();
                if (ag != null && ag.isOnNavMesh) { ag.isStopped = false; ag.SetDestination(bottleSpot.position); }
            }
            if (bully2 != null && bully2HideSpot != null)
            {
                var ag = bully2.GetComponent<NavMeshAgent>();
                if (ag != null && ag.isOnNavMesh) { ag.isStopped = false; ag.SetDestination(bully2HideSpot.position); }
            }
            float t = 0f;
            while (t < 20f)
            {
                t += Time.deltaTime;
                if (bully1 != null && bully2 != null && trackingTarget != null)
                {
                    Vector3 mid = (bully1.transform.position + bully2.transform.position) / 2f;
                    trackingTarget.position = mid;
                    Vector3 camPos = mid + new Vector3(6f, 4f, 7f);
                    if (useCineCam && trackingCamera != null) trackingCamera.transform.position = camPos;
                    else if (cutsceneCamera != null)
                    {
                        cutsceneCamera.transform.position = camPos;
                        cutsceneCamera.transform.LookAt(mid + Vector3.up * 1f);
                    }
                }
                bool b1done = bully1 == null || bottleSpot == null || AgentArrived(bully1, bottleSpot.position);
                bool b2done = bully2 == null || bully2HideSpot == null || AgentArrived(bully2, bully2HideSpot.position);
                if (b1done && b2done) break;
                yield return null;
            }
            // Botol diletakkan di meja jauh oleh Nakal.
            if (bottle != null && bottleSpot != null)
            {
                bottle.transform.SetParent(null, true);
                bottle.transform.position = bottleSpot.position + new Vector3(0f, 0.05f, 0f);
            }
            // Nakal lanjut ke hide spot (tetap di-tracking).
            if (bully1 != null && bully1HideSpot != null)
            {
                yield return WalkAgent(bully1, bully1HideSpot.position, 12f);
            }
            ReleaseTrackingCamera();
        }

        private bool AgentArrived(GameObject obj, Vector3 dest)
        {
            if (obj == null) return true;
            var ag = obj.GetComponent<NavMeshAgent>();
            if (ag == null || !ag.isOnNavMesh) return Vector3.Distance(obj.transform.position, dest) <= 0.6f;
            return !ag.pathPending && ag.remainingDistance <= 0.6f;
        }

        private void StartHideAndSeek()
        {
            Phase = CanteenPhase.HideAndSeek;
            MissionBanner.SetObjective("Ambil botol di meja jauh");
            Vector3 bottlePlace = bottleSpot != null ? bottleSpot.position : new Vector3(-44f, 0f, -3.5f);
            GroundMarker.SetTarget(bottlePlace);
            Minimap.SetMissionTarget(bottlePlace);
            CompanionBar.SetCompanion("Bima", 0.9f, true);
            SetSubtitle("Guru BK sedang berpatroli di kantin - jangan sampai ketahuan!");
            if (guruBK != null)
            {
                guruBK.SetActive(true);
                var ag = guruBK.GetComponent<NavMeshAgent>();
                if (ag != null && bkPatrolPoints.Count > 0)
                {
                    if (ag.isOnNavMesh) ag.Warp(bkPatrolPoints[0].position);
                    else guruBK.transform.position = bkPatrolPoints[0].position;
                }
                if (bkPatrolRoutine != null) StopCoroutine(bkPatrolRoutine);
                bkPatrolRoutine = StartCoroutine(BKPatrolLoop());
            }
        }

        private IEnumerator BKPatrolLoop()
        {
            var ag = guruBK != null ? guruBK.GetComponent<NavMeshAgent>() : null;
            if (ag == null || bkPatrolPoints.Count == 0) yield break;
            ag.speed = 2f;
            int i = 0;
            while (true)
            {
                var dest = bkPatrolPoints[i % bkPatrolPoints.Count].position;
                ag.isStopped = false;
                ag.SetDestination(dest);
                float timeout = 0f;
                while (timeout < 20f)
                {
                    timeout += Time.deltaTime;
                    if (!ag.pathPending && ag.remainingDistance <= 0.6f) break;
                    yield return null;
                }
                i++;
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator CaughtReset()
        {
            if (resetting) yield break;
            resetting = true;
            SetPrompt("");
            FadeController.FadeOut(0.3f);
            yield return new WaitForSeconds(0.4f);
            if (player != null && playerResetPoint != null)
            {
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = playerResetPoint.position;
                player.transform.rotation = Quaternion.identity;
                if (cc != null) cc.enabled = true;
            }
            if (guruBK != null && bkPatrolPoints.Count > 0)
            {
                var ag = guruBK.GetComponent<NavMeshAgent>();
                if (ag != null)
                {
                    if (ag.isOnNavMesh) ag.Warp(bkPatrolPoints[0].position);
                    ag.isStopped = false;
                }
                if (bkPatrolRoutine != null) StopCoroutine(bkPatrolRoutine);
                bkPatrolRoutine = StartCoroutine(BKPatrolLoop());
            }
            hidden = false;
            zonesInside = 0;
            FadeController.FadeIn(0.3f);
            SetPrompt("Kamu ketahuan! Bersembunyilah di balik meja atau konter!");
            yield return new WaitForSeconds(3f);
            SetPrompt("");
            resetting = false;
        }

        private void CompleteSequence()
        {
            if (bottle != null) bottle.SetActive(false);
            RestoreBullyBase(bully1, origBase1);
            RestoreBullyBase(bully2, origBase2);
            Phase = CanteenPhase.Complete;
            MissionTitleCard.ShowTitle("OBJECTIVE 2", "Selesai");
            MissionBanner.ClearObjective();
            GroundMarker.ClearTarget();
            Minimap.ClearMissionTarget();
            CompanionBar.SetCompanion("", 0f, false);
            SetPrompt("");
            SetSubtitle("");
        }

        private void SpawnBully(GameObject bully, Transform cp, ref float origBase)
        {
            if (bully == null || cp == null) return;
            bully.SetActive(true);
            var ag = bully.GetComponent<NavMeshAgent>();
            if (ag != null)
            {
                origBase = ag.baseOffset;
                ag.baseOffset = canteenBaseOffset;
                ag.isStopped = true;
                ag.ResetPath();
                if (ag.isOnNavMesh) ag.Warp(cp.position);
                else bully.transform.position = cp.position;
                ag.isStopped = false;
            }
            else
            {
                bully.transform.position = cp.position;
            }
        }

        private void RestoreBullyBase(GameObject bully, float origBase)
        {
            if (bully == null) return;
            var ag = bully.GetComponent<NavMeshAgent>();
            if (ag != null)
            {
                ag.baseOffset = origBase;
                ag.ResetPath();
                ag.isStopped = false;
            }
        }

        private void FaceTarget(GameObject obj, GameObject targetObj)
        {
            if (obj == null || targetObj == null) return;
            Vector3 d = targetObj.transform.position - obj.transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > 0.001f) obj.transform.rotation = Quaternion.LookRotation(d);
        }

        private IEnumerator ShowLine(string line)
        {
            SetSubtitle(line);
            yield return new WaitForSeconds(0.25f);
            float t = 0f;
            while (t < lineDisplayTime)
            {
                t += Time.deltaTime;
                if (PressedE()) break;
                yield return null;
            }
        }

        private IEnumerator Shove()
        {
            if (bully1 == null || bima == null) yield break;
            var ag1 = bully1.GetComponent<NavMeshAgent>();
            var agB = bima.GetComponent<NavMeshAgent>();
            Vector3 start1 = bully1.transform.position;
            Vector3 dir = bima.transform.position - start1;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) yield break;
            dir.Normalize();
            Vector3 end1 = start1 + dir * 0.5f;
            Vector3 startB = bima.transform.position;
            Vector3 endB = startB + dir * 0.4f;
            if (ag1 != null) ag1.updatePosition = false;
            if (agB != null) agB.updatePosition = false;
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.25f);
                bully1.transform.position = Vector3.Lerp(start1, end1, k);
                bima.transform.position = Vector3.Lerp(startB, endB, k);
                yield return null;
            }
            if (ag1 != null) { ag1.Warp(end1); ag1.updatePosition = true; }
            if (agB != null) { agB.Warp(endB); agB.updatePosition = true; }
        }

        private IEnumerator WalkAgent(GameObject obj, Vector3 dest, float timeout)
        {
            if (obj == null) yield break;
            var ag = obj.GetComponent<NavMeshAgent>();
            if (ag == null || !ag.isOnNavMesh)
            {
                obj.transform.position = dest;
                yield break;
            }
            ag.isStopped = false;
            ag.SetDestination(dest);
            float t = 0f;
            while (t < timeout)
            {
                t += Time.deltaTime;
                if (!ag.pathPending && ag.remainingDistance <= 0.6f) break;
                yield return null;
            }
            ag.isStopped = true;
        }

        private void LockPlayer()
        {
            if (player == null) return;
            foreach (var mb in player.GetComponentsInChildren<MonoBehaviour>())
            {
                if (mb == null) continue;
                string n = mb.GetType().Name;
                if (n == "PlayerMovement" || n == "PlayerInputHandler" || n == "PlayerInteraction" || n == "InteractionDetector")
                {
                    mb.enabled = false;
                }
            }
        }

        private void UnlockPlayer()
        {
            if (player == null) return;
            foreach (var mb in player.GetComponentsInChildren<MonoBehaviour>())
            {
                if (mb == null) continue;
                string n = mb.GetType().Name;
                if (n == "PlayerMovement" || n == "PlayerInputHandler" || n == "PlayerInteraction" || n == "InteractionDetector")
                {
                    mb.enabled = true;
                }
            }
        }

        private void SetSubtitle(string s) { if (subtitleText != null) subtitleText.text = s; }
        private void SetPrompt(string s) { if (promptText != null) promptText.text = s; }

        private void PlayBellSound()
        {
            if (bellAudio == null) return;
            if (bellClip == null) bellClip = GenerateBellClip();
            bellAudio.PlayOneShot(bellClip);
        }

        private AudioClip GenerateBellClip()
        {
            int rate = 44100;
            int len = rate * 2;
            var clip = AudioClip.Create("BellPlaceholder", len, 1, rate, false);
            var data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Exp(-3f * t);
                data[i] = (Mathf.Sin(2f * Mathf.PI * 880f * t)
                    + 0.5f * Mathf.Sin(2f * Mathf.PI * 1320f * t)
                    + 0.3f * Mathf.Sin(2f * Mathf.PI * 1760f * t)) * env * 0.5f;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
