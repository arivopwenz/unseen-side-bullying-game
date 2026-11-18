using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BullyingGame.Prologue
{
    [DefaultExecutionOrder(-100)]
    public sealed class PrologueCamera : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float distance = 4.2f;
        [SerializeField] private float sensitivity = .12f;
        [SerializeField] private CinemachineCamera gameplayCamera;
        private CinemachineCamera dialogueCamera;
        private float yaw;
        private float pitch = 15f;

        private void Start() => yaw = player.eulerAngles.y;

        internal void BeginDialogue(CinemachineCamera shot)
        {
            if (gameplayCamera == null || shot == null) return;
            if (dialogueCamera != null) dialogueCamera.Priority = -10;
            dialogueCamera = shot;
            shot.Priority = 100;
        }

        internal void EndDialogue(CinemachineCamera shot)
        {
            if (shot == null) return;
            shot.Priority = -10;
            if (dialogueCamera == shot) dialogueCamera = null;
        }

        private void LateUpdate()
        {
            if (player == null) return;
            if (dialogueCamera == null && Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
            {
                Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity;
                yaw += delta.x;
                pitch = Mathf.Clamp(pitch - delta.y, -15f, 48f);
            }
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 focus = player.position + Vector3.up * 1.55f;
            Vector3 backwards = rotation * Vector3.back;
            float clearance = distance;
            foreach (var hit in Physics.SphereCastAll(focus, .17f, backwards, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.distance <= 0 || hit.transform.IsChildOf(player)) continue;
                clearance = Mathf.Min(clearance, Mathf.Max(.25f, hit.distance - .15f));
            }
            Transform rig = gameplayCamera != null ? gameplayCamera.transform : transform;
            rig.SetPositionAndRotation(focus + backwards * clearance, rotation);
        }
    }
}
