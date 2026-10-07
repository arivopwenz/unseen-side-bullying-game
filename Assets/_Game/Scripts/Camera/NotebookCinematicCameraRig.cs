using UnityEngine;
using Unity.Cinemachine;
using BullyingGame.Events;

namespace BullyingGame.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class NotebookCinematicCameraRig : MonoBehaviour
    {
        [Header("Encounter")]
        [SerializeField] private BullyingEventDirector director;
        [SerializeField] private BullyingEventManager events;
        [SerializeField] private BullyingEventData eventData;
        [SerializeField] private Transform player;
        [SerializeField] private Transform[] actors;
        [Header("Framing")]
        [SerializeField, Min(0.1f)] private float blendSpeed = 5f;
        [SerializeField, Range(30f, 65f)] private float wideFieldOfView = 50f;
        [SerializeField, Range(30f, 65f)] private float closeFieldOfView = 43f;
        [SerializeField, Range(-1f, 0f)] private float compositionYOffset = -.55f;

        private CinemachineCamera cameraRig;
        private bool owned;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float originalFieldOfView;

        private void Awake() => cameraRig = GetComponent<CinemachineCamera>();

        private void LateUpdate()
        {
            bool active = director != null && director.IsRunning && events != null && events.CurrentEvent == eventData;
            if (!active)
            {
                Restore();
                return;
            }
            if (player == null || cameraRig == null) return;
            if (!owned)
            {
                owned = true;
                originalPosition = transform.position;
                originalRotation = transform.rotation;
                originalFieldOfView = cameraRig.Lens.FieldOfView;
            }
            Vector3 centre = player.position + Vector3.up;
            int count = 1;
            if (actors != null)
                foreach (var actor in actors)
                    if (actor != null) { centre += actor.position + Vector3.up; count++; }
            centre /= count;
            Vector3 forward = actors != null && actors.Length > 0 && actors[0] != null
                ? actors[0].position - player.position : player.forward;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.01f) forward = player.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            bool close = events.CurrentState == BullyingEventState.Confrontation ||
                events.CurrentState == BullyingEventState.WaitingResponse;
            float fov = close ? closeFieldOfView : wideFieldOfView;
            float radius = Vector3.Distance(centre, player.position + Vector3.up) + 1.2f;
            if (actors != null)
                foreach (var actor in actors)
                    if (actor != null) radius = Mathf.Max(radius, Vector3.Distance(centre, actor.position + Vector3.up) + 1.2f);
            // Fit the entire group instead of assuming that the player picked up from one exact direction.
            float distance = Mathf.Max(5.5f, radius / Mathf.Sin(fov * Mathf.Deg2Rad * 0.5f));
            Vector3 viewDirection = (right * 0.42f - forward * 0.91f + Vector3.up * 0.25f).normalized;
            Vector3 position = centre + viewDirection * distance;
            float t = 1f - Mathf.Exp(-blendSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, position, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(centre + Vector3.up * compositionYOffset - position), t);
            var lens = cameraRig.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, fov, t);
            cameraRig.Lens = lens;
        }

        private void Restore()
        {
            if (!owned) return;
            transform.SetPositionAndRotation(originalPosition, originalRotation);
            if (cameraRig != null)
            {
                var lens = cameraRig.Lens;
                lens.FieldOfView = originalFieldOfView;
                cameraRig.Lens = lens;
            }
            owned = false;
        }

        private void OnDisable() => Restore();
    }
}
