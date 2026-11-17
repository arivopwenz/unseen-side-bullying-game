using UnityEngine;
using UnityEngine.AI;

namespace BullyingGame.Prologue
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CharacterController), typeof(Animator))]
    public sealed class PrologueGuideMotion : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");

        private NavMeshAgent agent;
        private CharacterController body;
        private Animator animator;
        private float verticalSpeed;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            body = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            agent.updatePosition = false;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0 || !body.enabled || !agent.isOnNavMesh) return;

            // Navigation chooses the step, and the character controller resolves contact with scene colliders.
            Vector3 start = transform.position;
            Vector3 advance = Vector3.ProjectOnPlane(agent.nextPosition - start, Vector3.up);
            advance = Vector3.ClampMagnitude(advance, agent.speed * dt);
            if (agent.isStopped) advance = Vector3.zero;

            verticalSpeed = body.isGrounded ? -2f : Mathf.Max(verticalSpeed - 22f * dt, -12f);
            body.Move(advance + Vector3.up * (verticalSpeed * dt));
            agent.nextPosition = transform.position;

            float speed = Vector3.ProjectOnPlane(transform.position - start, Vector3.up).magnitude / dt;
            animator.SetFloat(Speed, speed, .15f, dt);
        }
    }
}
