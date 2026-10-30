using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using BullyingGame.Camera;
using BullyingGame.NPC;
using BullyingGame.Player;
using BullyingGame.UI;

namespace BullyingGame.Events
{
    /// <summary>
    /// Percakapan Aji-Bima + Misi 2 (jalan ke kantin), di Level01 (tanpa pindah scene).
    /// Alur: setelah MasukKelas kembali ke Level01 (Mission1Complete), tunggu 2 detik ->
    /// fade out (hitam) -> setup percakapan (Bima di depan Aji, kamera dialog) -> fade in ->
    /// dialog 6 baris (maju pakai E, atau auto 2 detik bila autoAdvance=true untuk test) ->
    /// TANPA fade out, langsung Misi 2: Bima jalan ke kantin (BimaMark_Kantin), Aji bebas
    /// mengikuti (objective + marker + minimap kantin). Sampai (Bima + Aji di kantin) ->
    /// "MISI 2 Selesai". Revert: nonaktifkan/hapus GO BimaConversationDriver (Misi 1 tetap jalan).
    /// Catatan: "gendut ngajak Bima" dibaca sebagai "Bima (si gendut) ngajak Aji" (Bima = korban
    /// gendut, Misi 2 = Aji jalan ke kantin) — koreksi bila maksudnya beda.
    /// </summary>
    public class BimaConversationDriver : MonoBehaviour
    {
        public static bool ConversationDone = false;
        public static bool CanteenWalkDone = false;
        [Header("Test: dialog maju otomatis tiap 2 detik (default false = maju pakai E)")]
        public bool autoAdvance = false;
        [Header("Polish jalan Bima: putaran halus (Slerp) + kecepatan jalan")]
        public float turnSpeed = 6f;
        public float walkSpeed = 1.8f;

    /// <summary>Entry point publik untuk trigger cutscene (mis. Bekal) agar bisa memulai Misi 2 tanpa refleksi. Idempotent (aman dipanggil ulang).</summary>
    public void StartCanteenWalk() { StartMission2(); }
        private bool hasTriggered = false;
        private bool walkingToCanteen = false;
        private Vector3 canteenPos = new Vector3(-39.50f, -0.07f, -1.00f);
        private Transform playerTf;
        private Transform bimaTf;
        private NavMeshAgent bimaAgent;
        private Animator bimaAnimator;
        private PlayerMovement playerMovement;
        private GuideController guideController;
        private GameObject dialogueCamGO;
        private GameObject gameplayCamGO;
        private DialogueCameraFraming framing;
        private GameObject subtitleCanvas;
        private Text lineText;
        private class Line { public string speaker; public string text; public Line(string s, string t) { speaker = s; text = t; } }
        private readonly Line[] dialogueLines = new Line[]
        {
            new Line("Aji", "Bima, boleh aku tanya? Tadi kenapa kamu dipukul?"),
            new Line("Bima", "Aku... harus ngasih uang ke mereka. Buat jajan mereka."),
            new Line("Bima", "Kalau aku nggak ngasih, mereka mukul aku."),
            new Line("Aji", "Itu nggak adil, Bim. Kamu nggak harus ngasih uang ke mereka."),
            new Line("Bima", "Makasih, Ji. Kamu mau ikut aku jalan ke kantin? Aku mau ke sana."),
            new Line("Aji", "Ayo, aku temenin."),
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetForNewPlaySession()
        {
            ConversationDone = false;
            CanteenWalkDone = false;
        }

        private void Awake()
        {
            BuildSubtitlePanel();
        }

        private void Start()
        {
            if (YellowCircleTrigger.Mission1Complete && !ConversationDone && !hasTriggered)
            {
                hasTriggered = true;
                StartCoroutine(ConversationSequence());
            }
        }

        private void Update()
        {
            if (!walkingToCanteen || bimaAgent == null || playerTf == null) return;
            if (bimaAnimator != null) bimaAnimator.SetFloat("Speed", bimaAgent.velocity.magnitude); // RAW m/s — Gendut_Guide blend: 0=Idle, 3.5=Running normal, 4.0=Running funny (JANGAN dinormalisasi 0-1, akan selip)
            if (bimaTf != null && bimaAgent.velocity.magnitude > 0.1f)
            {
                Vector3 vel = bimaAgent.velocity; vel.y = 0f;
                Quaternion targetRot = Quaternion.LookRotation(vel, Vector3.up);
                bimaTf.rotation = Quaternion.Slerp(bimaTf.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
            bool bimaArrived = !bimaAgent.pathPending && bimaAgent.hasPath && bimaAgent.remainingDistance <= 1.5f;
            if (!bimaArrived) return;
            Vector3 p = playerTf.position; p.y = 0f;
            Vector3 c = canteenPos; c.y = 0f;
            if (Vector3.Distance(p, c) > 5f) return;
            walkingToCanteen = false;
            if (bimaAgent != null) bimaAgent.updateRotation = true;
            if (bimaAnimator != null) bimaAnimator.SetFloat("Speed", 0f);
            CanteenWalkDone = true;
            MissionTitleCard.ShowTitle("MISI 2", "Selesai");
            MissionBanner.SetObjective("Sampai di kantin");
            GroundMarker.ClearTarget();
            Minimap.ClearMissionTarget();
        }

        private IEnumerator ConversationSequence()
        {
            yield return new WaitForSeconds(2f);
            FadeController.FadeOutIn(0.5f, 0.1f, 0.5f, SetupConversation);
            yield return new WaitForSeconds(1.1f);
            if (subtitleCanvas != null) subtitleCanvas.SetActive(true);
            for (int i = 0; i < dialogueLines.Length; i++)
            {
                if (lineText != null) lineText.text = "<b>" + dialogueLines[i].speaker + ":</b> " + dialogueLines[i].text;
                SetCameraForLine(dialogueLines[i].speaker);
                if (autoAdvance) yield return new WaitForSeconds(2f);
                else { while (!IsEPressed()) yield return null; }
            }
            if (subtitleCanvas != null) subtitleCanvas.SetActive(false);
            ConversationDone = true;
            StartMission2();
        }

        private void SetupConversation()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) playerTf = playerGO.transform;
            var bimaGO = GameObject.Find("Bima_Guide");
            if (bimaGO != null) bimaTf = bimaGO.transform;
            if (playerGO != null) playerMovement = playerGO.GetComponent<PlayerMovement>();
            if (bimaGO != null)
            {
                guideController = bimaGO.GetComponent<GuideController>();
                bimaAgent = bimaGO.GetComponent<NavMeshAgent>();
                bimaAnimator = bimaGO.GetComponent<Animator>();
            }
            if (playerMovement != null) playerMovement.enabled = false;
            if (guideController != null) guideController.enabled = false;
            if (bimaAgent != null) bimaAgent.isStopped = true;
            if (playerTf != null && bimaTf != null)
            {
                Vector3 pp = playerTf.position;
                Vector3 bp = new Vector3(pp.x + playerTf.forward.x * 1.8f, pp.y, pp.z + playerTf.forward.z * 1.8f);
                bimaTf.position = bp;
                if (bimaAgent != null)
                {
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(bimaTf.position, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        bimaAgent.Warp(hit.position);
                        bimaTf.position = hit.position;
                        bp = hit.position;
                    }
                }
                playerTf.LookAt(new Vector3(bp.x, pp.y, bp.z));
                bimaTf.LookAt(new Vector3(pp.x, bp.y, pp.z));
            }
            gameplayCamGO = GameObject.Find("CinemachineCamera");
            dialogueCamGO = FindGOInclusive("DialogueCamera");
            if (gameplayCamGO != null) gameplayCamGO.SetActive(false);
            if (dialogueCamGO != null)
            {
                dialogueCamGO.SetActive(true);
                framing = dialogueCamGO.GetComponent<DialogueCameraFraming>();
                if (framing != null) framing.enabled = false;
            }
            if (dialogueLines.Length > 0) SetCameraForLine(dialogueLines[0].speaker);
        }

        private void SetCameraForLine(string speaker)
        {
            if (dialogueCamGO == null || playerTf == null || bimaTf == null) return;
            Transform speakerTf = (speaker == "Aji") ? playerTf : bimaTf;
            Transform listenerTf = (speaker == "Aji") ? bimaTf : playerTf;
            Vector3 camPos = listenerTf.position + (listenerTf.forward * -1.0f) + new Vector3(0f, 1.4f, 0f);
            dialogueCamGO.transform.position = camPos;
            Vector3 lookPos = speakerTf.position + new Vector3(0f, 1.0f, 0f);
            dialogueCamGO.transform.LookAt(lookPos);
        }

        private void StartMission2()
        {
            if (dialogueCamGO != null) dialogueCamGO.SetActive(false);
            if (framing != null) framing.enabled = true;
            if (gameplayCamGO != null) gameplayCamGO.SetActive(true);
            if (playerMovement != null) playerMovement.enabled = true;
            if (bimaAgent != null) bimaAgent.isStopped = false;
            var markGO = FindGOInclusive("BimaMark_Kantin");
            if (markGO != null) canteenPos = markGO.transform.position;
            MissionBanner.SetObjective("Ikuti Bima ke kantin");
            Minimap.SetMissionTarget(canteenPos);
            var markerTf = FindMarker();
            if (markerTf != null)
            {
                markerTf.position = canteenPos;
                if (!markerTf.gameObject.activeSelf) markerTf.gameObject.SetActive(true);
            }
            if (bimaAgent != null && bimaTf != null)
            {
                bimaAgent.updateRotation = false;
                bimaAgent.speed = walkSpeed;
                bimaAgent.stoppingDistance = 0.5f;
                UnityEngine.AI.NavMeshHit hit2;
                if (UnityEngine.AI.NavMesh.SamplePosition(bimaTf.position, out hit2, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    bimaAgent.Warp(hit2.position);
                }
                if (bimaAgent.isOnNavMesh)
                {
                    bimaAgent.SetDestination(canteenPos);
                    walkingToCanteen = true;
                }
            }
        }

        private bool IsEPressed()
        {
            var kb = Keyboard.current;
            return kb != null && kb.eKey.wasPressedThisFrame;
        }

        private GameObject FindGOInclusive(string name)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                {
                    if (tr.gameObject.name == name) return tr.gameObject;
                }
            }
            return null;
        }

        private Transform FindMarker()
        {
            var go = FindGOInclusive("GroundMarker");
            return go != null ? go.transform : null;
        }

        private void BuildSubtitlePanel()
        {
            var canvasGO = new GameObject("BimaSubtitleCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 102;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            var panelGO = new GameObject("SubtitlePanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelRT = panelGO.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f); panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = new Vector2(0f, -300f); panelRT.sizeDelta = new Vector2(1100f, 140f);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.7f);
            var lineGO = new GameObject("SubtitleLine");
            lineGO.transform.SetParent(panelGO.transform, false);
            var lineRT = lineGO.AddComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0.5f, 0.5f); lineRT.anchorMax = new Vector2(0.5f, 0.5f);
            lineRT.anchoredPosition = new Vector2(0f, 20f); lineRT.sizeDelta = new Vector2(1060f, 80f);
            lineText = lineGO.AddComponent<Text>();
            lineText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lineText.fontSize = 28; lineText.color = Color.white;
            lineText.alignment = TextAnchor.MiddleCenter;
            var hintGO = new GameObject("SubtitleHint");
            hintGO.transform.SetParent(panelGO.transform, false);
            var hintRT = hintGO.AddComponent<RectTransform>();
            hintRT.anchorMin = new Vector2(0.5f, 0.5f); hintRT.anchorMax = new Vector2(0.5f, 0.5f);
            hintRT.anchoredPosition = new Vector2(0f, -45f); hintRT.sizeDelta = new Vector2(1060f, 30f);
            var hintTxt = hintGO.AddComponent<Text>();
            hintTxt.text = "Tekan E untuk lanjut";
            hintTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintTxt.fontSize = 18; hintTxt.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            hintTxt.alignment = TextAnchor.MiddleCenter;
            subtitleCanvas = canvasGO;
            subtitleCanvas.SetActive(false);
        }
    }
}
