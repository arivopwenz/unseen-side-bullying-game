using UnityEngine;
using Unity.Cinemachine;
using BullyingGame.NPC;
using BullyingGame.Core;

namespace BullyingGame.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class DialogueCameraFraming : MonoBehaviour
    {
        [Header("Conversation Centre / Optional Deoccluder Target")]
        [SerializeField] private Transform compositionTarget;
        private void OnEnable() => BaseNPC.OnConversationFramed += Frame;
        private void OnDisable() => BaseNPC.OnConversationFramed -= Frame;
        private void Frame(Transform player, Transform npc)
        {
            if (player == null || npc == null || (CutsceneManager.Instance != null && CutsceneManager.Instance.CurrentDirector != null)) return;
            Vector3 forward = npc.position - player.position;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.01f) forward = npc.forward;
            forward.Normalize();
            Vector3 centre = (player.position + npc.position) * .5f + Vector3.up * 1.15f;
            Vector3 side = Vector3.Cross(Vector3.up, forward);
            var anchor = npc.Find("DialogueCameraAnchor");
            Vector3 candidate = centre + side * 4.1f - forward * 1.8f + Vector3.up * .4f;
            if (anchor != null) candidate = anchor.position;
            else if (!Clear(centre, candidate, player, npc))
            {
                var opposite = centre - side * 4.1f - forward * 1.8f + Vector3.up * .4f;
                candidate = Clear(centre, opposite, player, npc) ? opposite : centre - forward * 4.6f + Vector3.up * .4f;
            }
            transform.position = candidate;
            transform.rotation = Quaternion.LookRotation(centre - transform.position);
            var cameraRig = GetComponent<CinemachineCamera>();
            if (compositionTarget != null)
            {
                compositionTarget.position = centre;
                cameraRig.LookAt = compositionTarget;
            }
            var lens = cameraRig.Lens; lens.FieldOfView = 50; cameraRig.Lens = lens;
        }
        private static bool Clear(Vector3 focus, Vector3 camera, Transform player, Transform npc)
        {
            var direction = camera - focus;
            foreach (var hit in Physics.SphereCastAll(focus, .2f, direction.normalized, direction.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player) || hit.transform == npc || hit.transform.IsChildOf(npc)) continue;
                return false;
            }
            return true;
        }
    }
}
