using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BullyingGame.Quest
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(QuestItem))]
    public class QuestItemRehide : MonoBehaviour
    {
        [Header("Lokasi Sembunyi (titik tengah item)")]
        [SerializeField] private Transform[] hidePoints = new Transform[0];
        [SerializeField, Min(0.1f)] private float minimumMoveDistance = 2f;
        [Header("Validasi Jalan di NavMesh")]
        [SerializeField] private bool requireReachablePath = true;
        [SerializeField] private int agentTypeId;
        [SerializeField, Min(0.05f)] private float sampleRadius = 0.75f;

        private readonly List<Vector3> candidates = new List<Vector3>();
        private NavMeshPath path;
        public Transform LastHidePoint { get; private set; }

        private void Awake() => path = new NavMeshPath();

        public bool TryRehide()
        {
            candidates.Clear();
            LastHidePoint = null;
            var validPoints = new List<Transform>();
            var filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = NavMesh.AllAreas };
            Vector3 origin = transform.position;
            Vector3 start = origin;
            if (requireReachablePath)
            {
                if (!NavMesh.SamplePosition(origin, out var hit, sampleRadius, filter))
                    return WarnNoLocation();
                start = hit.position;
            }

            if (hidePoints != null)
            {
                foreach (var point in hidePoints)
                {
                    // Anchors must stay fixed when the item moves; never put them under the item.
                    if (point == null || !point.gameObject.activeInHierarchy || point.IsChildOf(transform)) continue;
                    Vector3 destination = point.position;
                    if (!IsFinite(destination) || HorizontalDistance(origin, destination) < minimumMoveDistance) continue;
                    bool duplicate = false;
                    foreach (var candidate in candidates)
                        if ((candidate - destination).sqrMagnitude < 0.01f) { duplicate = true; break; }
                    if (duplicate) continue;
                    if (requireReachablePath &&
                        (!NavMesh.SamplePosition(destination, out var hit, sampleRadius, filter) ||
                         !NavMesh.CalculatePath(start, hit.position, filter, path) ||
                         path.status != NavMeshPathStatus.PathComplete)) continue;
                    // Keep the authored height: a navigation surface describes feet, not the item's centre.
                    candidates.Add(destination);
                    validPoints.Add(point);
                }
            }
            if (candidates.Count == 0) return WarnNoLocation();
            int index = Random.Range(0, candidates.Count);
            LastHidePoint = validPoints[index];
            transform.position = candidates[index];
            Physics.SyncTransforms();
            return true;
        }

        private bool WarnNoLocation()
        {
            Debug.LogWarning("Buku belum bisa dipindahkan: isi Hide Points dengan lokasi berbeda yang terhubung NavMesh. Item dikembalikan agar quest tetap bisa dilanjutkan.", this);
            return false;
        }

        public bool IsAuthoredLocation(Vector3 position)
        {
            if (!IsFinite(position) || hidePoints == null) return false;
            foreach (var point in hidePoints)
                if (point != null && point.gameObject.activeInHierarchy && !point.IsChildOf(transform) &&
                    (point.position - position).sqrMagnitude < 0.01f) return true;
            return false;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b) =>
            Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

        private static bool IsFinite(Vector3 value) =>
            !float.IsNaN(value.x) && !float.IsNaN(value.y) && !float.IsNaN(value.z) &&
            !float.IsInfinity(value.x) && !float.IsInfinity(value.y) && !float.IsInfinity(value.z);

        private void OnDrawGizmosSelected()
        {
            if (hidePoints == null) return;
            Gizmos.color = Color.cyan;
            foreach (var point in hidePoints)
            {
                if (point == null) continue;
                Gizmos.DrawWireSphere(point.position, 0.2f);
                Gizmos.DrawLine(transform.position, point.position);
            }
        }
    }
}
