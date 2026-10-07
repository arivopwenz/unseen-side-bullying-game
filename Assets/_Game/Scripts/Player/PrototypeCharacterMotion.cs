using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using BullyingGame.Core;
using BullyingGame.Audio;

namespace BullyingGame.Player
{
    [RequireComponent(typeof(Animator))]
    public class PrototypeCharacterMotion : MonoBehaviour
    {
        [Header("Rig")]
        [SerializeField] private MultiAimConstraint headAim;
        [SerializeField] private TwoBoneIKConstraint handIK;
        [Header("Footsteps")]
        [SerializeField] private bool emitFootsteps;
        [SerializeField] private AudioClip footstepClip;
        private Animator animator;
        private NavMeshAgent agent;
        private CharacterController controller;
        private float stepAt;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private void Awake()
        {
            animator = GetComponent<Animator>();
            agent = GetComponentInParent<NavMeshAgent>();
            controller = GetComponentInParent<CharacterController>();
        }
        private void Update()
        {
            float speed = agent != null && agent.enabled && agent.isOnNavMesh ? agent.velocity.magnitude :
                controller != null && controller.enabled ? Vector3.ProjectOnPlane(controller.velocity, Vector3.up).magnitude : 0;
            animator.SetFloat(Speed, Mathf.Clamp01(speed / 4f), .12f, Time.deltaTime);
            var state = GameStateManager.Instance != null ? GameStateManager.Instance.CurrentState : GameState.Playing;
            float look = state == GameState.Dialogue || state == GameState.Cinematic || state == GameState.QTE ? .65f : .12f;
            if (headAim != null) headAim.weight = Mathf.Lerp(headAim.weight, look, 1f - Mathf.Exp(-5 * Time.deltaTime));
            if (handIK != null) handIK.weight = Mathf.Lerp(handIK.weight, state == GameState.QTE ? .6f : 0, 1f - Mathf.Exp(-5 * Time.deltaTime));
            if (emitFootsteps && speed > .3f && Time.time > stepAt)
            {
                stepAt = Time.time + .42f;
                AudioManager.Instance?.PlaySFX(footstepClip, .1f);
            }
        }
    }
}
