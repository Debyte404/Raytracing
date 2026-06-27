using UnityEngine;
using UnityEngine.InputSystem;

public class cameracontroll : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 3f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public bool requireRightMouseButton = true;

    private float pitch;
    private float yaw;

    void OnEnable()
    {
        Vector3 angles = transform.eulerAngles;
        pitch = NormalizeAngle(angles.x);
        yaw = angles.y;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (requireRightMouseButton && !mouse.rightButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector2 mouseDelta = mouse.delta.ReadValue();
        yaw += mouseDelta.x * mouseSensitivity * Time.deltaTime;
        pitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        Vector3 moveDirection = Vector3.zero;

        if (keyboard.wKey.isPressed)
        {
            moveDirection += transform.forward;
        }

        if (keyboard.sKey.isPressed)
        {
            moveDirection -= transform.forward;
        }

        if (keyboard.dKey.isPressed)
        {
            moveDirection += transform.right;
        }

        if (keyboard.aKey.isPressed)
        {
            moveDirection -= transform.right;
        }

        if (keyboard.spaceKey.isPressed)
        {
            moveDirection += Vector3.up;
        }

        if (keyboard.leftCtrlKey.isPressed)
        {
            moveDirection -= Vector3.up;
        }

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        float currentSpeed = moveSpeed;
        if (keyboard.leftShiftKey.isPressed)
        {
            currentSpeed *= sprintMultiplier;
        }

        transform.position += moveDirection * currentSpeed * Time.deltaTime;
    }

    float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
