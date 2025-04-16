using UnityEngine;
using CMF;

public class BridgeAnimationController : MonoBehaviour
{
    AdvancedWalkerController controller;
    Animator animator;
    public Transform characterMeshTransform;
    public Transform cameraTransform;

    [Header("Double Jump Settings")]
    // The impulse added for the second jump.
    public float secondJumpImpulse = 100f;
    private bool pendingDoubleJump = false;
    private bool doubleJumpUsed = false;

    void Awake()
    {
        controller = GetComponent<AdvancedWalkerController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // --- DOUBLE JUMP INPUT ---
        // First jump is handled by CMF's built-in system.
        // Here, if the character is airborne, jump key is pressed, and we haven't used a double jump yet,
        // mark a pending double jump.
        if (!controller.IsGrounded() && Input.GetKeyDown(KeyCode.Space) && !doubleJumpUsed)
        {
            pendingDoubleJump = true;
        }
        // Reset double jump when grounded.
        if (controller.IsGrounded())
        {
            doubleJumpUsed = false;
        }

        // --- ANIMATOR UPDATES ---
        // Handle grounded state.
        bool _isGrounded = controller.IsGrounded();
        animator.SetBool("IsGrounded", _isGrounded);

        // Handle movement velocity.
        Vector3 _movementVelocity = controller.GetMovementVelocity();
        animator.SetFloat("totalMovementSpeed", _movementVelocity.magnitude);

        float _forwardSpeed = VectorMath.GetDotProduct(_movementVelocity, characterMeshTransform.forward);
        float _sidewardSpeed = VectorMath.GetDotProduct(_movementVelocity, characterMeshTransform.right);

        animator.SetFloat("speedForward", _forwardSpeed);
        animator.SetFloat("speedSideward", _sidewardSpeed);

        // Handle vertical speed.
        Vector3 _momentum = controller.GetMomentum();
        float _verticalSpeed = VectorMath.GetDotProduct(_momentum, characterMeshTransform.up);
        animator.SetFloat("VerticalSpeed", _verticalSpeed);

        // --- ROTATION LOGIC USING INPUT ---
        // Retrieve input axes.
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");
        Vector2 inputVector = new Vector2(inputHorizontal, inputVertical);

        // Process rotation only if input is non-negligible.
        if (inputVector.sqrMagnitude > 0.01f)
        {
            inputVector.Normalize();

            // Get camera's forward and right projected onto the local ground plane.
            Vector3 projectedCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, characterMeshTransform.up).normalized;
            Vector3 projectedCameraRight = Vector3.ProjectOnPlane(cameraTransform.right, characterMeshTransform.up).normalized;

            Vector3 desiredDirection = projectedCameraForward; // default base direction
            float threshold = 0.1f; // threshold to detect pure horizontal input

            if (Mathf.Abs(inputVertical) < threshold)
            {
                // Pure horizontal: face exactly left or right.
                if (inputHorizontal > 0)
                {
                    desiredDirection = projectedCameraRight;
                }
                else if (inputHorizontal < 0)
                {
                    desiredDirection = -projectedCameraRight;
                }
            }
            else
            {
                if (inputVertical > 0)
                {
                    // Forward movement: base direction is projected camera forward.
                    desiredDirection = projectedCameraForward;
                    // Compute angle offset from arctan2; clamp to ±60°.
                    float angleOffset = Mathf.Atan2(inputHorizontal, inputVertical) * Mathf.Rad2Deg;
                    angleOffset = Mathf.Clamp(angleOffset, -60f, 60f);
                    Quaternion rotationOffset = Quaternion.AngleAxis(angleOffset, characterMeshTransform.up);
                    desiredDirection = rotationOffset * desiredDirection;
                }
                else // inputVertical < 0
                {
                    // Backward movement: base direction is opposite of camera forward.
                    desiredDirection = -projectedCameraForward;
                    // Compute angle offset using arctan2 with absolute vertical; clamp to ±60°.
                    float angleOffset = Mathf.Atan2(inputHorizontal, Mathf.Abs(inputVertical)) * Mathf.Rad2Deg;
                    angleOffset = Mathf.Clamp(angleOffset, -60f, 60f);
                    // Invert the angle offset for backward movement so that strafing yields the correct direction.
                    angleOffset = -angleOffset;
                    Quaternion rotationOffset = Quaternion.AngleAxis(angleOffset, characterMeshTransform.up);
                    desiredDirection = rotationOffset * desiredDirection;
                }
            }

            // Build the target rotation using character's up vector.
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, characterMeshTransform.up);
            characterMeshTransform.rotation = Quaternion.Slerp(characterMeshTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Note: Gravity-shift behavior is preserved since all rotations use characterMeshTransform.up.
    }

    void FixedUpdate()
    {
        if (pendingDoubleJump)
        {
            // Get the current momentum so that horizontal movement is preserved.
            Vector3 currentMomentum = controller.GetMomentum();
            // Add an upward impulse along the current up vector (which follows the gravity shift).
            Vector3 newMomentum = currentMomentum + (characterMeshTransform.up * secondJumpImpulse);
            controller.SetMomentum(newMomentum);
            pendingDoubleJump = false;
            doubleJumpUsed = true;
        }
    }
}