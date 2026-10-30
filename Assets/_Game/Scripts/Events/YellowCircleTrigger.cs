using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BullyingGame.UI;

namespace BullyingGame.Events
{
    /// <summary>
    /// MISI 1 ONLY (atas permintaan Ari 2026-10-29): sentuh lingkaran kuning -> misi selesai +
    /// CUTSCENE DIMAINKAN DI SCENE ASLINYA ("Cutscene perkenalan kelas", Timeline MasukKelas
    /// diputar CutsceneAutoReturn di sana), lalu kembali ke Level01 (free play).
    /// v6 (ganti v5): TANPA DontDestroyOnLoad / sceneLoaded (rawan ambigu) — driver cutscene
    /// tinggal di scene cutscene (CutsceneAutoReturn di "Cinemachine").
    /// Flag statis Mission1Complete bertahan melewati LoadScene (tanpa domain reload),
    /// sehingga saat kembali ke Level01 pemicu TIDAK mengulang (anti-loop).
    /// Return placement DELAY 0.5 detik (coroutine) supaya tidak ditimpa logika spawn Player
    /// (StoryChapterSequence.Start) yang jalan di Start saat Level01 dimuat ulang.
    /// Revert: kembalikan YellowCircleTrigger v4 (Slot 1) + hapus CutsceneAutoReturn + hapus
    /// scene cutscene dari Build Settings.
    /// </summary>
    public class YellowCircleTrigger : MonoBehaviour
    {
        public static bool Mission1Complete = false;
        public static Vector3 ReturnPosition;
        public static bool HasReturnPosition = false;
        private const string CutsceneSceneName = "Cutscene perkenalan kelas";

        /// <summary>Reset flag misi tiap Play session baru (sebelum scene pertama dimuat).
        /// Tanpa ini, static Mission1Complete nyangkut True dari Play sebelumnya bila domain
        /// tidak di-reload (Enter Play Mode Options), sehingga Play berikutnya langsung
        /// menganggap misi selesai dan lingkaran tidak pernah memicu cutscene ("gabisa").
        /// Method ini jalan tiap Play start (bukan tiap LoadScene), jadi return dari cutscene
        /// dalam satu Play session yang sama TIDAK ke-reset (tetap True, anti-loop).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetForNewPlaySession()
        {
            Mission1Complete = false;
            HasReturnPosition = false;
        }
        [Header("Pemicu")]
        public float touchDistance = 2.0f;
        private bool hasTriggered = false;
        private Transform playerTf;
        private Transform markerTf;

        private void Start()
        {
            if (Mission1Complete)
            {
                hasTriggered = true;
                MissionBanner.SetObjective("Misi 1 selesai");
                GroundMarker.ClearTarget();
                Minimap.ClearMissionTarget();
                ClockHUD.SetSchedule("Istirahat");
                CompanionBar.SetCompanion("", 0f, false);
                var retPlayer = GameObject.FindGameObjectWithTag("Player");
                if (retPlayer != null && HasReturnPosition) retPlayer.transform.position = ReturnPosition;
                StartCoroutine(PlacePlayerDelayed());
                StartCoroutine(CorrectSpawnReset());
                return;
            }
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTf = p.transform;
            markerTf = FindMarker();
            if (markerTf != null && !markerTf.gameObject.activeSelf) markerTf.gameObject.SetActive(true);
            MissionBanner.SetObjective("Ikuti Bima ke kelas");
            if (markerTf != null) Minimap.SetMissionTarget(markerTf.position);
        }

        private IEnumerator PlacePlayerDelayed()
        {
            yield return new WaitForSeconds(2f);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && HasReturnPosition) player.transform.position = ReturnPosition;
        }

        /// <summary>Koreksi spawn-reset: selama 5 detik setelah kembali ke Level01,
        /// bila Player ketemu di dekat titik spawn (PRDPlayerSpawn), kembalikan ke ReturnPosition.
        /// Ini mengakhiri race dengan logika spawn StoryChapterSequence (POV Apply) yang waktunya
        /// bervariasi (kadang sebelum, kadang sesudah placement 2 detik) sehingga hasil return
        /// sempat flaky (kadang ReturnPosition, kadang spawn). Hanya mengoreksi bila Player di
        /// dekat spawn, jadi tidak mengganggu jalan bebas user setelah kembali.</summary>
        private IEnumerator CorrectSpawnReset()
        {
            var spawnGO = GameObject.Find("PRDPlayerSpawn");
            Vector2 spawnXZ = spawnGO != null ? new Vector2(spawnGO.transform.position.x, spawnGO.transform.position.z) : new Vector2(-7.10f, -50.09f);
            float elapsed = 0f;
            while (elapsed < 5f)
            {
                yield return new WaitForSeconds(0.25f);
                elapsed += 0.25f;
                if (!HasReturnPosition) yield break;
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null) continue;
                Vector2 pXZ = new Vector2(player.transform.position.x, player.transform.position.z);
                if (Vector2.Distance(pXZ, spawnXZ) <= 2f) player.transform.position = ReturnPosition;
            }
        }

        private Transform FindMarker()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                {
                    if (tr.gameObject.name == "GroundMarker") return tr;
                }
            }
            return null;
        }

        private void Update()
        {
            if (Mission1Complete || hasTriggered) return;
            if (playerTf == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTf = player.transform;
            }
            if (markerTf == null)
            {
                markerTf = FindMarker();
                if (markerTf != null && !markerTf.gameObject.activeSelf) markerTf.gameObject.SetActive(true);
            }
            if (playerTf == null || markerTf == null) return;
            Vector3 a = playerTf.position; a.y = 0f;
            Vector3 b = markerTf.position; b.y = 0f;
            if (Vector3.Distance(a, b) <= touchDistance)
            {
                hasTriggered = true;
                StartCoroutine(RunSequenceSceneLoad());
            }
        }

        private IEnumerator RunSequenceSceneLoad()
        {
            MissionTitleCard.ShowTitle("MISI 1", "Selesai");
            MissionBanner.SetObjective("Misi 1 selesai");
            GroundMarker.ClearTarget();
            Minimap.ClearMissionTarget();
            ClockHUD.SetSchedule("Istirahat");
            CompanionBar.SetCompanion("", 0f, false);
            Mission1Complete = true;
            if (playerTf != null) { ReturnPosition = playerTf.position; HasReturnPosition = true; }
            yield return new WaitForSeconds(0.5f);
            LoadingScreen.LoadSceneWithLoading(CutsceneSceneName);
        }
    }
}
