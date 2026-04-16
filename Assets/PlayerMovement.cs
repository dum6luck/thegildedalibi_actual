using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;

    [Tooltip("Maximum vertical look angle in degrees (90 = straight up/down).")]
    public float lookXLimit = 90f;

    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;

    // True when the cursor is locked and camera look is active.
    private bool isCursorLocked = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        ApplyCursorState();
    }

    void Update()
    {
        HandleCursorToggle();

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Block movement input while cursor is unlocked so the player
        // doesn't drift when clicking UI elements.
        float curSpeedX = (canMove && isCursorLocked) ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = (canMove && isCursorLocked) ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && isCursorLocked && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.R) && canMove && isCursorLocked)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Only rotate the camera when the cursor is locked.
        if (canMove && isCursorLocked)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    /// <summary>Toggles cursor lock state when the player presses Tab.</summary>
    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isCursorLocked = !isCursorLocked;
            ApplyCursorState();
        }
    }

    /// <summary>Applies the current isCursorLocked value to the Unity cursor API.</summary>
    private void ApplyCursorState()
    {
        Cursor.lockState = isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isCursorLocked;
    }
}