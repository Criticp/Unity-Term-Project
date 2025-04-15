using UnityEngine;
using CMF;

public class BridgeAnimationController : MonoBehaviour
{
    AdvancedWalkerController controller;
    Animator animator;
    public Transform characterMeshTransform;
    public Transform cameraTransform;

    void Awake()
    {
        controller = GetComponent<AdvancedWalkerController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Handle grounded state
        bool _isGrounded = controller.IsGrounded();
        animator.SetBool("IsGrounded", _isGrounded);

        // Handle movement velocity
        Vector3 _movementVelocity = controller.GetMovementVelocity();
        animator.SetFloat("totalMovementSpeed", _movementVelocity.magnitude);

        float _forwardSpeed = VectorMath.GetDotProduct(_movementVelocity, characterMeshTransform.forward);
        float _sidewardSpeed = VectorMath.GetDotProduct(_movementVelocity, characterMeshTransform.right);

        animator.SetFloat("speedForward", _forwardSpeed);
        animator.SetFloat("speedSideward", _sidewardSpeed);

        // Handle vertical speed
        Vector3 _momentum = controller.GetMomentum();
        float _verticalSpeed = VectorMath.GetDotProduct(_momentum, characterMeshTransform.up);
        animator.SetFloat("VerticalSpeed", _verticalSpeed);

        // Rotate the mesh only if there's meaningful movement
        Vector3 flatVelocity = _movementVelocity;
        if (flatVelocity.magnitude > 0.01f)
        {
            // Use character's up vector to project the camera forward onto the local ground plane
            Vector3 projectedCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, characterMeshTransform.up).normalized;

            if (projectedCameraForward.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(projectedCameraForward, characterMeshTransform.up);
                characterMeshTransform.rotation = Quaternion.Slerp(characterMeshTransform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
}
