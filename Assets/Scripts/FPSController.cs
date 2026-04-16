using UnityEngine;

/// <summary>
/// First-person player controller.
/// - WASD to move, Shift to sprint, Space to jump
/// - Mouse always looks around; cursor is locked and hidden
/// - Set canMove / canLook to false externally (e.g. ObjectInspector) to suppress input
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera that is a child of this GameObject.")]
    public Transform cameraTransform;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    [Tooltip("Maximum degrees the camera can tilt up or down.")]
    public float verticalLookLimit = 90f;

    /// <summary>Set to false by ObjectInspector during inspection to disable WASD movement.</summary>
    [HideInInspector] public bool canMove = true;

    /// <summary>Set to false by ObjectInspector during inspection to disable mouse look.</summary>
    [HideInInspector] public bool canLook = true;

    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalAngle;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleGravityAndJump();
        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>Rotates the player body horizontally and the camera vertically using the mouse.</summary>
    private void HandleLook()
    {
        if (!canLook) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalAngle -= mouseY;
        verticalAngle = Mathf.Clamp(verticalAngle, -verticalLookLimit, verticalLookLimit);
        cameraTransform.localRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
    }

    /// <summary>Reads WASD input and moves the character in world space.</summary>
    private void HandleMovement()
    {
        if (!canMove)
        {
            velocity.x = 0f;
            velocity.z = 0f;
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.magnitude > 1f) move.Normalize();

        velocity.x = move.x * speed;
        velocity.z = move.z * speed;
    }

    /// <summary>Applies gravity and handles jumping.</summary>
    private void HandleGravityAndJump()
    {
        bool grounded = characterController.isGrounded;

        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (grounded && canMove && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
        }

        velocity.y += gravity * Time.deltaTime;
    }
}
