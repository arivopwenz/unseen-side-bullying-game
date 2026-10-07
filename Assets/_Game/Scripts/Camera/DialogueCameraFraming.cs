using UnityEngine;
using Unity.Cinemachine;
using BullyingGame.NPC;
using BullyingGame.Core;

namespace BullyingGame.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class DialogueCameraFraming : MonoBehaviour
    {
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
            transform.position = centre + side * 3.5f - forward * 1.4f + Vector3.up * .35f;
            transform.rotation = Quaternion.LookRotation(centre - transform.position);
            var cameraRig = GetComponent<CinemachineCamera>();
            var lens = cameraRig.Lens; lens.FieldOfView = 45; cameraRig.Lens = lens;
        }
    }
}
