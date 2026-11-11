using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    public float mouseSensitivity = 150f;
    public Transform playerBody;

    [Header("Sudut Awal Kamera (Pitch)")]
    public float defaultPitch = 0f;

    [Header("Batas Pandangan Vertikal")]
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Mode & Controls")]
    public bool enableLook = true;
    public bool lockCursorOnStart = true;
    public KeyCode toggleCursorKey = KeyCode.None;

    private float xRotation = 0f;

    void Start()
    {
        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }

        ResetView(defaultPitch);
        SetGameplayMode(lockCursorOnStart);
    }

    void Update()
    {
        // 1. Kunci perputaran kamera HANYA saat sedang aktif mengetik teks (agar tidak goyang saat mengetik)
        if (PCInteractionController.isTypingText)
        {
            return;
        }

        // Fitur opsional toggle kursor
        if (toggleCursorKey != KeyCode.None && Input.GetKeyDown(toggleCursorKey))
        {
            SetGameplayMode(!enableLook);
        }

        if (!enableLook || playerBody == null)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    public void ResetView(float pitch = 0f)
    {
        xRotation = pitch;
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    public void SetGameplayMode(bool gameplay)
    {
        enableLook = gameplay;
        UpdateCursor();
    }

    void UpdateCursor()
    {
        if (enableLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}