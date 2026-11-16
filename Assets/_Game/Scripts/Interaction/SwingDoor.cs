using System.Collections.Generic;
using UnityEngine;

namespace BullyingGame.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public sealed class SwingDoor : MonoBehaviour, IInteractable
    {
        internal static readonly HashSet<SwingDoor> ActiveDoors = new HashSet<SwingDoor>();
        [SerializeField] private bool reverseHinge;
        [SerializeField] private bool reverseSwing;
        [SerializeField, Min(1f)] private float degreesPerSecond = 240f;

        private MeshCollider doorCollider;
        private Vector3 hinge;
        private Vector3 hingeAxis;
        private Vector3 closedPosition;
        private Vector3 interactionAnchor;
        private Quaternion closedRotation;
        private float angle;
        private float targetAngle;

        public Vector3 InteractionPosition => doorCollider.bounds.center;
        internal Vector3 DoorwayPosition => transform.parent.TransformPoint(interactionAnchor);
        private Vector3 closedNormal;

        private void Awake()
        {
            doorCollider = GetComponent<MeshCollider>();
            var worldBounds = doorCollider.bounds;
            interactionAnchor = transform.parent.InverseTransformPoint(worldBounds.center);
            closedNormal = transform.parent.InverseTransformDirection(transform.up);
            var bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
            var localHinge = bounds.center;
            localHinge.x = reverseHinge ? bounds.max.x : bounds.min.x;
            hinge = transform.parent.InverseTransformPoint(transform.TransformPoint(localHinge));
            hingeAxis = transform.parent.InverseTransformDirection(Vector3.up);
            closedPosition = transform.localPosition;
            closedRotation = transform.localRotation;
        }

        private void OnEnable() => ActiveDoors.Add(this);
        private void OnDisable() => ActiveDoors.Remove(this);

        public bool CanInteract() => isActiveAndEnabled;

        public string GetPromptText() => targetAngle == 0f ? "Buka" : "Tutup";

        public Vector3 GetPromptPosition(Vector3 viewer)
        {
            var center = DoorwayPosition;
            var normal = transform.parent.TransformDirection(closedNormal);
            float side = Vector3.Dot(viewer - center, normal) < 0f ? -1f : 1f;
            return center + normal * (side * .38f);
        }

        public void Interact(GameObject interactor)
        {
            if (targetAngle != 0f)
            {
                targetAngle = 0f;
                return;
            }

            Open();
        }

        public void Open()
        {
            if (targetAngle != 0f) return;

            if (angle != 0f)
            {
                targetAngle = Mathf.Sign(angle) * 90f;
                return;
            }

            var swingDirection = Vector3.Cross(hingeAxis, interactionAnchor - hinge);
            float inwardAngle = Vector3.Dot(swingDirection, closedNormal) < 0f ? -90f : 90f;
            targetAngle = reverseSwing ? -inwardAngle : inwardAngle;
        }

        private void Update()
        {
            if (angle == targetAngle) return;

            angle = Mathf.MoveTowards(angle, targetAngle, degreesPerSecond * Time.deltaTime);
            var swing = Quaternion.AngleAxis(angle, hingeAxis);
            transform.localPosition = hinge + swing * (closedPosition - hinge);
            transform.localRotation = swing * closedRotation;
        }
    }
}
