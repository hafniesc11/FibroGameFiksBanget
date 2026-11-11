using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PCPlayerController : MonoBehaviour
{
    public static bool isMovementLocked = false;

    [Header("Pergerakan")]
    public float moveSpeed = 4f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake()
    {
        isMovementLocked = false;

        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
        }

        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        isMovementLocked = false;
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;

        // 1. Jangan gerakkan karakter jika pergerakan dikunci
        if (isMovementLocked) return;

        // 2. Cegah karakter jalan saat sedang mengetik teks di kotak Nama/Kelas
        if (PCInteractionController.isTypingText) return;

        // Input WASD / Arrow Keys normal
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        // Cek apakah menyentuh lantai untuk reset gaya gravitasi
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Terapkan gravitasi
        velocity.y += gravity * Time.deltaTime;

        // Gerakkan karakter
        Vector3 finalMove = (move * moveSpeed) + velocity;
        controller.Move(finalMove * Time.deltaTime);
    }
}