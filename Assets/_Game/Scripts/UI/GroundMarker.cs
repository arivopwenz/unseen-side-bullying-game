using UnityEngine;

namespace BullyingGame.UI
{
    /// <summary>Marker objective tanah ala Bully (lingkaran kuning BERSINAR di titik tujuan).
    /// ATURAN BARU (2026-10-26, perintah Ari): marker SELALU di TEMPAT endpoint misi
    /// (ruang kelas, kantin, meja jauh, titik observasi) — TIDAK PERNAH di karakter
    /// (Bima, bullies, dll). Karena itu API ini hanya menerima Vector3 (tempat), bukan Transform.
    /// VFX maksimal: 4 cakram konsentris emissive (inti terang s.d. halo redup) + PointLight
    /// kuning di atasnya + denyut skala & intensitas cahaya, persis cahaya marker di Bully.</summary>
    public class GroundMarker : MonoBehaviour
    {
        private static GroundMarker instance;
        private UnityEngine.Light glowLight;

        [Header("Denyut cahaya (bisa diedit manual di Inspector)")]
        public float baseIntensity = 2.5f;
        public float pulseIntensity = 0.7f;
        public float pulseScale = 0.1f;

        [Header("Atur manual (biar tidak ditimpa kode)")]
        [Tooltip("Geseran permanen dari titik objective (meter). Mis. (1.5, 0, 4.5) menggeser marker sedikit dari titik aslinya.")]
        public Vector3 manualOffset = Vector3.zero;
        [Tooltip("Pengali ukuran permanen. 1 = normal, 0.5 = setengah, 2 = dua kali lipat.")]
        public float manualScale = 1f;
        [Tooltip("Bila AKTIF: marker DIAM di posisi & skala Transform scene (yang kamu drag di Editor), tidak mengikuti objective dan tidak didenyutkan skalanya. Matikan lagi bila ingin marker mengikuti objective.")]
        public bool lockToSceneTransform = false;

        private static readonly string[] charRoots = new string[]
        {
            "Bima_Guide", "Nakal", "Nakal1", "Player", "GuruBK_Placeholder"
        };

        private void Awake()
        {
            instance = this;
            if (!TryBindScene()) BuildProcedural();
            gameObject.SetActive(false);
        }

        /// <summary>Pakai disc + lampu scene bila sudah ada (dibuat manual di Editor).
        /// Bila tidak ada sama sekali, kembalikan false agar Awake membangunnya via kode.</summary>
        private bool TryBindScene()
        {
            bool hasDisc = false;
            foreach (Transform c in transform)
            {
                if (c.name == "MarkerDisc") hasDisc = true;
                if (c.name == "GlowLight") glowLight = c.GetComponent<UnityEngine.Light>();
            }
            return hasDisc || glowLight != null;
        }

        private void BuildProcedural()
        {
            // Inti paling terang di tengah, meredup ke luar (radial gradient palsu via cincin).
            MakeDisc(0.55f, 0.04f, new Color(1f, 0.95f, 0.1f, 1f), 1.6f);
            MakeDisc(0.9f, 0.03f, new Color(1f, 0.78f, 0f, 1f), 1.1f);
            MakeDisc(1.3f, 0.02f, new Color(0.95f, 0.6f, 0f, 1f), 0.6f);
            MakeDisc(1.8f, 0.015f, new Color(0.6f, 0.35f, 0f, 1f), 0f);

            var lightObj = new GameObject("GlowLight");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            glowLight = lightObj.AddComponent<UnityEngine.Light>();
            glowLight.type = LightType.Point;
            glowLight.color = new Color(1f, 0.85f, 0.2f, 1f);
            glowLight.intensity = baseIntensity;
            glowLight.range = 5f;
            glowLight.shadows = LightShadows.None;
        }

        private void MakeDisc(float radius, float y, Color color, float emission)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "MarkerDisc";
            disc.transform.SetParent(transform, false);
            disc.transform.localPosition = new Vector3(0f, y, 0f);
            disc.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
            var col = disc.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var rend = disc.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = color;
                if (emission > 0f)
                {
                    rend.material.EnableKeyword("_EMISSION");
                    rend.material.SetColor("_EmissionColor", color * emission);
                }
            }
        }

        private static void Ensure()
        {
            if (instance != null) return;
            var arr = FindObjectsByType<GroundMarker>(FindObjectsInactive.Include);
            if (arr.Length > 0) instance = arr[0];
        }

        /// <summary>Tempatkan marker di sebuah TEMPAT (endpoint misi). Bukan karakter.
        /// Bila lockToSceneTransform aktif, posisi TIDAK dipindah (tetap di Transform scene manual).</summary>
        public static void SetTarget(Vector3 place)
        {
            Ensure();
            if (instance == null) return;
            if (!instance.lockToSceneTransform)
                instance.transform.position = ProjectGround(place) + instance.manualOffset;
            instance.gameObject.SetActive(true);
        }

        public static void ClearTarget()
        {
            Ensure();
            if (instance == null) return;
            instance.gameObject.SetActive(false);
        }

        private static Vector3 ProjectGround(Vector3 pos)
        {
            float ground = pos.y;
            float bestDist = float.MaxValue;
            var hits = Physics.RaycastAll(new Vector3(pos.x, pos.y + 0.5f, pos.z), Vector3.down, 5f);
            foreach (var h in hits)
            {
                string root = h.collider.transform.root.gameObject.name;
                bool isChar = false;
                foreach (var c in charRoots) if (root == c) { isChar = true; break; }
                if (isChar) continue;
                if (h.distance < bestDist) { bestDist = h.distance; ground = h.point.y; }
            }
            return new Vector3(pos.x, ground, pos.z);
        }

        private void Update()
        {
            float pulse = Mathf.Sin(Time.time * 4f);
            if (!lockToSceneTransform)
            {
                float s = manualScale * (1f + pulseScale * pulse);
                transform.localScale = new Vector3(s, 1f, s);
            }
            if (glowLight != null) glowLight.intensity = baseIntensity + pulseIntensity * pulse;
        }
    }
}
