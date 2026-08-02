using UnityEngine;

/// <summary>
/// FPS silah görünümünü yöneten component.
/// Silahı kameraya monte eder ve hafif sway/bobbing efekti ekler.
/// Player/Main Camera altına yerleştirilmiş silah objesine eklenir.
/// </summary>
public class FPSWeaponSetup : MonoBehaviour
{
    [Header("Sway Ayarları")]
    [Tooltip("Kamera hareket ettiğinde silahın sallantı miktarı")]
    public float swayAmount = 0.02f;

    [Tooltip("Sway düzgünleştirme hızı")]
    public float swaySmoothness = 6f;

    [Tooltip("Maksimum sway açısı")]
    public float maxSwayAmount = 0.06f;

    [Header("Bobbing Ayarları (Yürürken sallanma)")]
    [Tooltip("Bobbing etkinleştirilsin mi?")]
    public bool enableBobbing = true;

    [Tooltip("Bobbing hızı")]
    public float bobSpeed = 10f;

    [Tooltip("Bobbing dikey miktarı")]
    public float bobAmountVertical = 0.01f;

    [Tooltip("Bobbing yatay miktarı")]
    public float bobAmountHorizontal = 0.005f;

    // Dahili değişkenler
    private Vector3 initialLocalPosition;
    private CharacterController characterController;
    private float bobTimer;

    void Start()
    {
        initialLocalPosition = transform.localPosition;

        // Player'daki CharacterController'ı bul (yürüme tespiti için)
        characterController = GetComponentInParent<CharacterController>();
    }

    void LateUpdate()
    {
        ApplySway();

        if (enableBobbing)
        {
            ApplyBobbing();
        }
    }

    /// <summary>
    /// Kamera hareket ettiğinde silahın hafif sallantı yapmasını sağlar.
    /// </summary>
    private void ApplySway()
    {
        // Mouse/dokunmatik delta değerlerini al
        float mouseX = 0f;
        float mouseY = 0f;

        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            var delta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
            mouseX = delta.x;
            mouseY = delta.y;
        }

        // Sway hesapla
        float swayX = Mathf.Clamp(-mouseX * swayAmount * 0.01f, -maxSwayAmount, maxSwayAmount);
        float swayY = Mathf.Clamp(-mouseY * swayAmount * 0.01f, -maxSwayAmount, maxSwayAmount);

        Vector3 targetPosition = new Vector3(
            initialLocalPosition.x + swayX,
            initialLocalPosition.y + swayY,
            initialLocalPosition.z
        );

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySmoothness);
    }

    /// <summary>
    /// Karakter hareket ederken silahın hafif sallanmasını sağlar.
    /// </summary>
    private void ApplyBobbing()
    {
        if (characterController == null) return;

        // Karakter hareket ediyor mu ve yerde mi?
        float speed = new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude;

        if (speed > 0.1f && characterController.isGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;

            float bobX = Mathf.Sin(bobTimer) * bobAmountHorizontal;
            float bobY = Mathf.Sin(bobTimer * 2f) * bobAmountVertical;

            // Sway ile çakışmaması için mevcut local position'a ekle
            Vector3 currentPos = transform.localPosition;
            transform.localPosition = new Vector3(
                currentPos.x + bobX,
                currentPos.y + bobY,
                currentPos.z
            );
        }
        else
        {
            bobTimer = 0f;
        }
    }
}
