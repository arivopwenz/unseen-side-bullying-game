using UnityEngine;
using UnityEngine.InputSystem;

namespace BullyingGame.Prologue
{
    public sealed class PrologueCamera : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float distance = 4.2f;
        [SerializeField] private float sensitivity = .12f;
        private float yaw;
        private float pitch = 15f;

        private void Start() => yaw = player.eulerAngles.y;

        private void LateUpdate()
        {
            if (player == null) return;
            if (Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
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
            transform.SetPositionAndRotation(focus + backwards * clearance, rotation);
        }
    }
}
