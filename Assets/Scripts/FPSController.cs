using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the 'Cameras' folder here.")]
    public Transform cameraTransform;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Look")]
    public float mouseSensitivity = 2f;

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canLook = true;

    private CharacterController characterController;
    private Vector3 velocity;

    // This variable tracks the up/down rotation and prevents the "swinging" effect
    private float verticalAngle = 0f;

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

        // Final movement execution
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (!canLook) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 1. Rotate the Body horizontally (left and right)
        transform.Rotate(Vector3.up * mouseX);

        // 2. Calculate the Vertical Tilt (up and down)
        verticalAngle -= mouseY;

        // Note: If you want to limit looking straight up/down to stop clipping, 
        // uncomment the line below:
        // verticalAngle = Mathf.Clamp(verticalAngle, -89f, 89f);

        // 3. Apply rotation to the cameraTransform pivot
        // By using localRotation, the folder stays at (0,0,0) and just spins
        cameraTransform.localRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
    }

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

        // Prevents faster diagonal movement
        if (move.magnitude > 1f) move.Normalize();

        velocity.x = move.x * speed;
        velocity.z = move.z * speed;
    }

    private void HandleGravityAndJump()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            // Keeps the player snapped to the floor
            velocity.y = -2f;
        }

        if (canMove && characterController.isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
        }

        velocity.y += gravity * Time.deltaTime;
    }
}