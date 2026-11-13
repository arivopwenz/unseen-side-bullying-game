using UnityEngine;
using UnityEngine.AI;
using BullyingGame.Core;

namespace BullyingGame.LevelOne
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class LevelOneCompanion : MonoBehaviour
    {
        [Header("Companion and movable route")]
        [SerializeField] private LevelOneDirector director;
        [SerializeField] private Transform[] route;
        [SerializeField] private bool witness;
        [SerializeField] private Transform home;
        NavMeshAgent agent;
        Animator animator;
        int waypoint;
        bool hasSpeed;
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>(); animator = GetComponentInChildren<Animator>();
            if (animator != null) foreach (var p in animator.parameters) if (p.name == "Speed") hasSpeed = true;
        }
        void Update()
        {
            if (director == null || director.Player == null || !agent.isOnNavMesh) return;
            bool active = GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Playing;
            float distance = Vector3.Distance(transform.position, director.Player.transform.position);
            bool follow = witness ? director.Searching : director.StepIndex == 1;
            agent.isStopped = !active || !follow || distance > 12;
            if (!agent.isStopped)
            {
                Vector3 goal = director.Player.transform.position + director.Player.transform.right * (witness ? 1.7f : -1.7f);
                if (!witness && director.StepIndex == 1 && route != null && waypoint < route.Length)
                {
                    goal = route[waypoint].position;
                    if (Vector3.Distance(transform.position, goal) < 1.8f) waypoint++;
                }
                if (NavMesh.SamplePosition(goal, out var hit, 3, NavMesh.AllAreas)) agent.SetDestination(hit.position);
            }
            if (hasSpeed) animator.SetFloat("Speed", agent.isStopped ? 0 : agent.velocity.magnitude, .15f, Time.deltaTime);
            // Recover on the navigation mesh if the player leaves the companion behind.
            if (active && follow && distance > 25 && NavMesh.SamplePosition(director.Player.transform.position + Vector3.back * 2, out var recovery, 3, NavMesh.AllAreas)) agent.Warp(recovery.position);
        }
        void Start()
        {
            if (home != null && NavMesh.SamplePosition(home.position, out var hit, 3, NavMesh.AllAreas)) agent.Warp(hit.position);
        }
    }
}
