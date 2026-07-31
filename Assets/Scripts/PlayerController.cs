using UnityEngine;

/// <summary>
/// Player karakterinin hareketini kontrol eden script.
/// CharacterController bileşeni ile çalışır.
/// Hareket girişleri UIButtonHandler tarafından sağlanır.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [Tooltip("Normal yürüme hızı")]
    public float moveSpeed = 5f;

    [Tooltip("Koşma hız çarpanı")]
    public float runSpeedMultiplier = 2f;

    [Header("Zıplama Ayarları")]
    [Tooltip("Zıplama kuvveti")]
    public float jumpForce = 2f;

    [Tooltip("Yerçekimi değeri")]
    public float gravity = -9.81f;

    [Header("Yer Kontrolü")]
    [Tooltip("Yerde olup olmadığını kontrol etmek için ek mesafe")]
    public float groundCheckOffset = 0.1f;

    // Dahili değişkenler
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveInput;
    private bool isRunning;
    private bool jumpRequested;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Yer kontrolü
        bool isGrounded = controller.isGrounded;

        // Yerdeyken düşme hızını sıfırla
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; // Küçük bir değer bırakarak yere yapışmayı sağla
        }

        // Hareket hesaplaması - kameranın baktığı yöne göre
        float currentSpeed = isRunning ? moveSpeed * runSpeedMultiplier : moveSpeed;

        // Player'ın yönüne (kameranın yatay yönü) göre hareket vektörünü hesapla
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Dikey bileşeni sıfırla (sadece yatay düzlemde hareket)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * moveInput.z + right * moveInput.x) * currentSpeed;

        // Yatay hareket uygula
        controller.Move(move * Time.deltaTime);

        // Zıplama
        if (jumpRequested && isGrounded)
        {
            // v = sqrt(2 * jumpForce * |gravity|) formülü ile zıplama hızı hesapla
            velocity.y = Mathf.Sqrt(jumpForce * 2f * Mathf.Abs(gravity));
            jumpRequested = false;
        }

        // Yerçekimi uygula
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Hareket girişini ayarlar. UIButtonHandler tarafından her frame çağrılır.
    /// </summary>
    /// <param name="input">Normalize edilmiş hareket yönü (X: sağ-sol, Z: ileri-geri)</param>
    public void SetMoveInput(Vector3 input)
    {
        moveInput = input;
    }

    /// <summary>
    /// Koşma modunu aktif/pasif yapar.
    /// </summary>
    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    /// <summary>
    /// Zıplama isteği gönderir. Yerdeyken bir sonraki frame'de uygulanır.
    /// </summary>
    public void RequestJump()
    {
        jumpRequested = true;
    }

    /// <summary>
    /// Player şu an yerde mi kontrolü (dışarıdan erişim için).
    /// </summary>
    public bool IsGrounded()
    {
        return controller != null && controller.isGrounded;
    }
}
