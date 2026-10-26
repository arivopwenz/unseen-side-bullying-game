using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using BullyingGame.Core;

namespace BullyingGame.NPC
{
    /// <summary>
    /// Generic escort/guide controller for the opening sequence.
    /// The guide walks a list of waypoints with a NavMeshAgent, waits when the
    /// player falls behind, resumes when the player comes back, and feeds its
    /// real speed into the Animator "Speed" parameter (Student controller).
    /// Attach to the guide NPC root (same object as its NavMeshAgent).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class GuideController : MonoBehaviour
    {
        public enum GuideState { Idle, Leading, Waiting, Arrived, Paused }

        [Header("Route")]
        [Tooltip("Waypoints in walking order. The last one is the final destination.")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2f;
        [Tooltip("Guide stops and waits when the player is farther than this (meters).")]
        [SerializeField] private float waitDistance = 7f;
        [Tooltip("Guide resumes walking when the player comes back within this distance (meters).")]
        [SerializeField] private float resumeDistance = 4f;
        [Tooltip("How close to a waypoint counts as reaching it (meters).")]
        [SerializeField] private float waypointReachDistance = 0.8f;

        [Header("Behaviour")]
        [SerializeField] private bool beginOnStart = false;
        [SerializeField] private bool facePlayerWhileWaiting = true;
        [Tooltip("Only walk while GameState is Playing (same rule as SchoolCompanion).")]
        [SerializeField] private bool runOnlyWhilePlaying = true;
        [SerializeField] private string speedParameterName = "Speed";
        [SerializeField] private float animationDampTime = 0.15f;

        [Header("Events")]
        public UnityEvent onGuideStarted;
        public UnityEvent onArrived;

        private NavMeshAgent agent;
        private Animator animator;
        private Transform player;
        private int waypointIndex;
        private bool hasSpeedParam;
        private GuideState stateBeforePause;

        public GuideState CurrentState { get; private set; } = GuideState.Idle;
        public int CurrentWaypointIndex => waypointIndex;
        public bool IsGuiding => CurrentState == GuideState.Leading || CurrentState == GuideState.Waiting;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            CacheAnimatorParams();
        }

        private void Start()
        {
            FindPlayer();
            if (beginOnStart)
            {
                BeginGuide();
            }
        }

        private void CacheAnimatorParams()
        {
            hasSpeedParam = false;
            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == speedParameterName && parameter.type == AnimatorControllerParameterType.Float)
                {
                    hasSpeedParam = true;
                    break;
                }
            }
        }

        private void FindPlayer()
        {
            if (player != null)
            {
                return;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        /// <summary>Start (or restart) guiding from the first waypoint.</summary>
        public void BeginGuide()
        {
            FindPlayer();
            waypointIndex = 0;

            if (waypoints == null || waypoints.Count == 0)
            {
                Debug.LogWarning("[GuideController] No waypoints assigned on " + gameObject.name, this);
                CurrentState = GuideState.Idle;
                return;
            }

            if (agent != null)
            {
                agent.enabled = true;
                agent.speed = walkSpeed;
                agent.updateRotation = true;

                if (!agent.isOnNavMesh)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                    }
                    else
                    {
                        Debug.LogWarning("[GuideController] " + gameObject.name + " is not near any NavMesh.", this);
                    }
                }

                agent.isStopped = false;
                agent.SetDestination(waypoints[0].position);
            }

            CurrentState = GuideState.Leading;
            if (onGuideStarted != null)
            {
                onGuideStarted.Invoke();
            }
        }

        /// <summary>Start guiding from a later waypoint (e.g. the canteen leg), without restarting the tour.</summary>
        public void BeginLeg(int startIndex)
        {
            FindPlayer();
            if (waypoints == null || waypoints.Count == 0)
            {
                CurrentState = GuideState.Idle;
                return;
            }
            if (startIndex < 0) startIndex = 0;
            if (startIndex >= waypoints.Count) startIndex = waypoints.Count - 1;
            waypointIndex = startIndex;
            if (agent != null)
            {
                agent.enabled = true;
                agent.speed = walkSpeed;
                agent.updateRotation = true;
                if (!agent.isOnNavMesh)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                    }
                }
                agent.isStopped = false;
                agent.SetDestination(waypoints[waypointIndex].position);
            }
            CurrentState = GuideState.Leading;
        }

        /// <summary>Freeze the guide in place (used by later sequence stages).</summary>
        public void PauseGuide()
        {
            if (CurrentState == GuideState.Leading || CurrentState == GuideState.Waiting)
            {
                stateBeforePause = CurrentState;
                CurrentState = GuideState.Paused;
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
            }
        }

        /// <summary>Continue guiding after PauseGuide.</summary>
        public void ResumeGuide()
        {
            if (CurrentState != GuideState.Paused)
            {
                return;
            }

            CurrentState = stateBeforePause;
            if (agent != null && agent.isOnNavMesh && CurrentState == GuideState.Leading)
            {
                agent.isStopped = false;
                if (waypoints != null && waypointIndex < waypoints.Count)
                {
                    agent.SetDestination(waypoints[waypointIndex].position);
                }
            }
        }

        /// <summary>Stop guiding completely and go back to Idle.</summary>
        public void StopGuide()
        {
            CurrentState = GuideState.Idle;
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
        }

        private void Update()
        {
            UpdateAnimationSpeed();

            if (!IsGuiding)
            {
                return;
            }

            if (agent == null || player == null)
            {
                return;
            }

            if (runOnlyWhilePlaying && GameStateManager.Instance != null
                && GameStateManager.Instance.CurrentState != GameState.Playing)
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                return;
            }

            float playerDistance = Vector3.Distance(transform.position, player.position);

            if (CurrentState == GuideState.Leading && playerDistance > waitDistance)
            {
                CurrentState = GuideState.Waiting;
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                agent.updateRotation = false;
            }
            else if (CurrentState == GuideState.Waiting)
            {
                if (facePlayerWhileWaiting)
                {
                    FacePlayer();
                }

                if (playerDistance <= resumeDistance)
                {
                    CurrentState = GuideState.Leading;
                    agent.updateRotation = true;
                    if (agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(waypoints[waypointIndex].position);
                    }
                }
            }

            if (CurrentState == GuideState.Leading)
            {
                CheckWaypointReached();
            }
        }

        private void CheckWaypointReached()
        {
            if (agent.pathPending || !agent.isOnNavMesh)
            {
                return;
            }

            if (agent.remainingDistance > Mathf.Max(waypointReachDistance, agent.stoppingDistance))
            {
                return;
            }

            waypointIndex++;

            if (waypointIndex >= waypoints.Count)
            {
                CurrentState = GuideState.Arrived;
                agent.isStopped = true;
                FacePlayer();
                if (onArrived != null)
                {
                    onArrived.Invoke();
                }

                return;
            }

            agent.SetDestination(waypoints[waypointIndex].position);
        }

        private void FacePlayer()
        {
            if (player == null)
            {
                return;
            }

            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        private void UpdateAnimationSpeed()
        {
            if (animator == null || !hasSpeedParam)
            {
                return;
            }

            float speed = 0f;
            if (agent != null && agent.enabled && !agent.isStopped)
            {
                speed = agent.velocity.magnitude;
            }

            animator.SetFloat(speedParameterName, speed, animationDampTime, Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Vector3 previous = transform.position;
            foreach (Transform waypoint in waypoints)
            {
                if (waypoint == null)
                {
                    continue;
                }

                Gizmos.DrawWireSphere(waypoint.position, 0.35f);
                Gizmos.DrawLine(previous, waypoint.position);
                previous = waypoint.position;
            }
        }
    }
}
