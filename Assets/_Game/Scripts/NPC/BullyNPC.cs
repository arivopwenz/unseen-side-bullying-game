using UnityEngine;
using UnityEngine.AI;
using BullyingGame.Core;

namespace BullyingGame.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class BullyNPC : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField, Min(0.05f)] private float navMeshSampleDistance = 0.6f;
        [SerializeField, Min(0.01f)] private float arrivalTolerance = 0.1f;
        [Header("Facing")]
        [SerializeField, Min(1f)] private float turnSpeed = 360f;

        private NavMeshAgent agent;
        private Vector3 destination, homePosition;
        private Quaternion homeRotation;
        private Transform faceTarget;
        private bool owned, moving, failed, returning;
        private bool previousStopped, previousUpdateRotation;

        public bool IsMoving => moving;
        public bool HasArrived => owned && !moving && !failed;
        public bool HasPathFailed => failed || (owned && !IsOnNavMesh);
        public float Radius => Agent.radius;
        private NavMeshAgent Agent => agent != null ? agent : agent = GetComponent<NavMeshAgent>();
        private bool IsOnNavMesh => isActiveAndEnabled && Agent.isActiveAndEnabled && Agent.isOnNavMesh;

        private void Awake() => agent = GetComponent<NavMeshAgent>();

        internal bool TryPrepare(Vector3 target, out NavMeshPath path, out string reason)
        {
            path = null;
            reason = "";
            if (!IsOnNavMesh)
            {
                reason = $"{name}: NPC/agent tidak aktif atau belum berada di NavMesh. Periksa Bake dan posisi kaki.";
                return false;
            }
            if (!owned && Agent.hasPath)
            {
                reason = $"{name}: agent sedang dikendalikan sistem lain.";
                return false;
            }
            var filter = new NavMeshQueryFilter { agentTypeID = Agent.agentTypeID, areaMask = Agent.areaMask };
            if (!NavMesh.SamplePosition(target, out var hit, navMeshSampleDistance, filter))
            {
                reason = $"{name}: titik tujuan berada di luar NavMesh.";
                return false;
            }
            path = new NavMeshPath();
            if (!Agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                reason = $"{name}: tidak ada jalur lengkap menuju titik tujuan.";
                return false;
            }
            return true;
        }

        internal bool BeginApproach(NavMeshPath path, Transform target)
        {
            if (owned || !IsOnNavMesh || path == null || path.corners.Length == 0) return false;
            homePosition = Agent.nextPosition;
            homeRotation = transform.rotation;
            previousStopped = Agent.isStopped;
            previousUpdateRotation = Agent.updateRotation;
            owned = true;
            faceTarget = target;
            returning = false;
            return FollowPath(path);
        }

        private bool FollowPath(NavMeshPath path)
        {
            destination = path.corners[path.corners.Length - 1];
            Agent.updateRotation = true;
            Agent.isStopped = false;
            failed = !Agent.SetPath(path);
            moving = !failed;
            return !failed;
        }

        internal bool ReturnHome()
        {
            if (!owned) return true;
            faceTarget = null;
            returning = true;
            if (!TryPrepare(homePosition, out var path, out _) || path.corners.Length == 0)
            {
                Release();
                return false;
            }
            if (FollowPath(path)) return true;
            Release();
            return false;
        }

        private void Update()
        {
            if (!owned || !IsOnNavMesh) return;
            bool paused = GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Paused;
            Agent.isStopped = paused || !moving;
            if (paused) return;
            if (moving)
            {
                if (Agent.pathPending) return;
                Vector3 delta = Agent.nextPosition - destination;
                delta.y = 0;
                float tolerance = Mathf.Max(arrivalTolerance, Agent.stoppingDistance + 0.05f);
                if (delta.sqrMagnitude <= tolerance * tolerance)
                {
                    moving = false;
                    Agent.isStopped = true;
                    Agent.ResetPath();
                    Agent.updateRotation = false;
                    if (returning)
                    {
                        transform.rotation = homeRotation;
                        Release();
                    }
                }
                else if (!Agent.hasPath || Agent.pathStatus != NavMeshPathStatus.PathComplete)
                {
                    failed = true;
                    moving = false;
                    Agent.isStopped = true;
                    Agent.ResetPath();
                }
            }
            if (!moving && !failed && faceTarget != null)
            {
                Vector3 direction = faceTarget.position - transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
            }
        }

        internal void Release()
        {
            if (!owned) return;
            if (Agent.isActiveAndEnabled && Agent.isOnNavMesh)
            {
                Agent.ResetPath();
                Agent.isStopped = previousStopped;
            }
            Agent.updateRotation = previousUpdateRotation;
            owned = moving = failed = returning = false;
            faceTarget = null;
        }

        private void OnDisable() => Release();
    }
}
