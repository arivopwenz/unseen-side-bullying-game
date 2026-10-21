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
                [Header("Animation")]
        [SerializeField] private string speedParameterName = "Speed";
        [SerializeField] private float animationDampTime = 0.12f;

        private Animator animator;
private float verticalVelocity;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private string jumpTriggerName = "Jump";
        [SerializeField] private string groundedBoolName = "Grounded";

        private bool hasSpeedParam;
        private bool hasJumpTriggerParam;
        private bool hasGroundedParam;

        private bool jumpRequested;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            CacheAnimatorParams();
        }

        private void OnDisable()
        {
            // When movement is disabled (dialogue, cinematic, pause), zero the
            // locomotion Speed immediately; otherwise the blend tree keeps
            // looping with the last damped Speed value.
            if (animator != null && hasSpeedParam)
            {
                animator.SetFloat(speedParameterName, 0f);
            }

            moveInput = Vector2.zero;
            isSprinting = false;
            jumpRequested = false;
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
                var cam = FindAnyObjectByType<UnityEngine.Camera>();
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

        public void RequestJump()
        {
            jumpRequested = true;
        }


        private void Update()
        {
            Vector3 movement = CalculateCameraRelativeDirection(moveInput);

            float speed = isSprinting ? sprintSpeed : moveSpeed;

            bool isGrounded = characterController != null && characterController.enabled && characterController.isGrounded;

            HandleJump(isGrounded);

            if (isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = movement * speed + Vector3.up * verticalVelocity;

            if (characterController != null && characterController.enabled)
            {
                characterController.Move(velocity * Time.deltaTime);
            }

            if (animator != null && characterController != null && hasGroundedParam)
            {
                animator.SetBool(groundedBoolName, characterController.isGrounded);
            }

            UpdateAnimation(velocity);
            RotateTowardsMovement(movement);
        }

        private void UpdateAnimation(Vector3 velocity)
        {
            if (animator == null || !hasSpeedParam)
            {
                return;
            }

            float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            animator.SetFloat(speedParameterName, horizontalSpeed, animationDampTime, Time.deltaTime);
        }

        private void HandleJump(bool isGrounded)
        {
            if (!jumpRequested)
            {
                return;
            }

            jumpRequested = false;

            if (isGrounded)
            {
                verticalVelocity = jumpForce;
                if (animator != null)
                {
                    if (hasJumpTriggerParam)
                    {
                        animator.SetTrigger(jumpTriggerName);
                    }

                    if (hasGroundedParam)
                    {
                        animator.SetBool(groundedBoolName, false);
                    }
                }
            }
        }

        private void CacheAnimatorParams()
        {
            hasSpeedParam = HasAnimatorParam(speedParameterName);
            hasJumpTriggerParam = HasAnimatorParam(jumpTriggerName);
            hasGroundedParam = HasAnimatorParam(groundedBoolName);
        }

        private bool HasAnimatorParam(string paramName)
        {
            if (animator == null || string.IsNullOrEmpty(paramName))
            {
                return false;
            }

            foreach (var p in animator.parameters)
            {
                if (p.name == paramName)
                {
                    return true;
                }
            }

            return false;
        }




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