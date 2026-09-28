using UnityEngine;

namespace BullyingGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float sprintSpeed = 6f;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private Transform cameraTransform;

        [Header("Gravity")]
        [SerializeField] private float gravity = -20f;

        private CharacterController characterController;
        private Vector2 moveInput;
        private bool isSprinting;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            FindCamera();
        }

        private void FindCamera()
        {
            if (cameraTransform != null) return;

            if (UnityEngine.Camera.main != null)
            {
                cameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                var cam = FindFirstObjectByType<UnityEngine.Camera>();
                if (cam != null)
                {
                    cameraTransform = cam.transform;
                }
            }
        }

        public void SetMoveInput(Vector2 input)
        {
            moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void SetSprintInput(bool sprinting)
        {
            isSprinting = sprinting;
        }

        private void Update()
        {
            Vector3 movement = CalculateCameraRelativeDirection(moveInput);

            float speed = isSprinting ? sprintSpeed : moveSpeed;

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = movement * speed + Vector3.up * verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);

            RotateTowardsMovement(movement);
        }

        /// <summary>
        /// Calculates the movement direction relative to where the camera is facing on the horizontal plane.
        /// </summary>
        private Vector3 CalculateCameraRelativeDirection(Vector2 input)
        {
            if (cameraTransform == null)
            {
                FindCamera();
                if (cameraTransform == null)
                {
                    return new Vector3(input.x, 0f, input.y);
                }
            }

            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
            Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up);

            if (cameraForward.sqrMagnitude > 0.001f)
                cameraForward.Normalize();

            if (cameraRight.sqrMagnitude > 0.001f)
                cameraRight.Normalize();

            Vector3 direction = cameraForward * input.y + cameraRight * input.x;
            return Vector3.ClampMagnitude(direction, 1f);
        }

        /// <summary>
        /// Smoothly rotates the character to face the direction of movement.
        /// </summary>
        private void RotateTowardsMovement(Vector3 movementDirection)
        {
            if (movementDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}