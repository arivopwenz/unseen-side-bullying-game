using Unity.Cinemachine;
using UnityEngine;

namespace BullyingGame.Prologue
{
    [DisallowMultipleComponent]
    public sealed class PrologueDialogueShot : MonoBehaviour
    {
        [Header("Cinemachine khusus karakter ini")]
        [SerializeField] private CinemachineCamera dialogueCamera;
        [SerializeField] private PrologueCamera gameplay;
        [SerializeField] private Transform player;

        [Header("Komposisi otomatis")]
        [Tooltip("Matikan untuk memakai posisi dan rotasi Cinemachine Camera yang kamu atur manual.")]
        [SerializeField] private bool autoFrame = true;
        [SerializeField, Min(1f)] private float distance = 3.4f;
        [SerializeField, Range(-180f, 180f)] private float angle = 65f;
        [SerializeField] private float focusHeight = 1.3f;
        [SerializeField] private float cameraHeight = .3f;
        [SerializeField] private bool faceEachOther = true;

        private bool presenting;
        private Quaternion npcRotation, playerRotation;

        internal void Enter()
        {
            if (dialogueCamera == null || gameplay == null || player == null || presenting) return;
            presenting = true;
            npcRotation = transform.rotation;
            playerRotation = player.rotation;

            Vector3 heading = Vector3.ProjectOnPlane(transform.position - player.position, Vector3.up);
            if (heading.sqrMagnitude < .01f) heading = player.forward;
            heading.Normalize();
            if (faceEachOther)
            {
                player.rotation = Quaternion.LookRotation(heading);
                transform.rotation = Quaternion.LookRotation(-heading);
            }
            if (autoFrame) Frame(heading);
            dialogueCamera.PreviousStateIsValid = false;
            gameplay.BeginDialogue(dialogueCamera);
        }

        internal void Exit()
        {
            if (!presenting) return;
            presenting = false;
            if (gameplay != null) gameplay.EndDialogue(dialogueCamera);
            if (faceEachOther)
            {
                transform.rotation = npcRotation;
                if (player != null) player.rotation = playerRotation;
            }
        }

        private void OnDisable() => Exit();

        private void Frame(Vector3 heading)
        {
            Vector3 centre = (player.position + transform.position) * .5f + Vector3.up * focusHeight;
            float separation = Vector3.ProjectOnPlane(transform.position - player.position, Vector3.up).magnitude;
            float shotDistance = Mathf.Max(distance, separation * .8f + .9f);
            Vector3 best = centre - heading * shotDistance + Vector3.up * cameraHeight;
            float bestScore = float.NegativeInfinity;

            // Try nearby angles first, then the opposite side when furniture or walls block the shot.
            for (int i = 0; i < 12; i++)
            {
                float offset = i % 2 == 0 ? (i / 2) * 30f : -((i + 1) / 2) * 30f;
                Vector3 direction = Quaternion.AngleAxis(angle + offset, Vector3.up) * -heading;
                Vector3 desired = direction * shotDistance + Vector3.up * cameraHeight;
                float clearance = ClearDistance(centre, desired.normalized, desired.magnitude);
                Vector3 candidate = centre + desired.normalized * Mathf.Max(.5f, clearance);
                float score = Visible(candidate, player.position + Vector3.up * 1.5f) +
                    Visible(candidate, transform.position + Vector3.up * 1.5f);
                score = score * 100f + clearance - i * .12f;
                if (score <= bestScore) continue;
                bestScore = score;
                best = candidate;
            }
            dialogueCamera.transform.SetPositionAndRotation(best, Quaternion.LookRotation(centre - best));
        }

        private float ClearDistance(Vector3 origin, Vector3 direction, float length)
        {
            float clearance = length;
            foreach (var hit in Physics.SphereCastAll(origin, .2f, direction, length, ~0, QueryTriggerInteraction.Ignore))
            {
                if (IsCharacter(hit.transform)) continue;
                clearance = Mathf.Min(clearance, Mathf.Max(0, hit.distance - .15f));
            }
            return clearance;
        }

        private float Visible(Vector3 cameraPosition, Vector3 eyes)
        {
            Vector3 ray = eyes - cameraPosition;
            foreach (var hit in Physics.RaycastAll(cameraPosition, ray.normalized, ray.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!IsCharacter(hit.transform)) return 0;
            return 1;
        }

        private bool IsCharacter(Transform target) => target.IsChildOf(transform) || target.IsChildOf(player);
    }
}
