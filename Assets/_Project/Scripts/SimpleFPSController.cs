using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Main Level")]
    public MainLevelManager mainLevelManager;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 5.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float maxLookAngle = 85f;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();

            if (cam != null)
                cameraTransform = cam.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Game Over alatt ne lehessen mozogni vagy körbenézni.
        if (mainLevelManager != null && mainLevelManager.IsGameOver())
            return;

        HandleLook();
        HandleMovement();

        // ESC csak kurzor feloldás debughoz, ha nincs külön pause menu.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleLook()
    {
        Vector2 mouseDelta = Vector2.zero;

        if (Mouse.current != null)
            mouseDelta = Mouse.current.delta.ReadValue();

        float yaw = mouseDelta.x * mouseSensitivity;
        float lookY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * yaw);

        pitch -= lookY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 move = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) move.y += 1;
            if (Keyboard.current.sKey.isPressed) move.y -= 1;
            if (Keyboard.current.aKey.isPressed) move.x -= 1;
            if (Keyboard.current.dKey.isPressed) move.x += 1;
        }

        Vector3 moveDir = (transform.right * move.x + transform.forward * move.y).normalized;

        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool sprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        float speed = sprint ? sprintSpeed : moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (controller.isGrounded && jumpPressed)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * speed + Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}