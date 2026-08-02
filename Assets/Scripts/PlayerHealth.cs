using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Oyuncunun can sistemini yönetir.
/// Düşme hasarı, ölüm ekranı, respawn ve ana menüye dönüş işlevlerini içerir.
/// Player objesine eklenir.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    [Tooltip("Maksimum can değeri")]
    public float maxHealth = 100f;

    [Header("Düşme Hasarı Ayarları")]
    [Tooltip("Bu yüksekliğin altındaki düşüşler hasar vermez (metre)")]
    public float safeFallHeight = 3f;

    [Tooltip("Her ekstra metre için verilen hasar")]
    public float damagePerMeter = 10f;

    [Tooltip("Ölüm yüksekliği (bu yükseklikten düşerse direkt ölür)")]
    public float lethalFallHeight = 15f;

    [Header("UI Referansları")]
    [Tooltip("Can barı Image (Filled)")]
    public Image healthBarFill;

    [Tooltip("Can barı üzerindeki text")]
    public TextMeshProUGUI healthText;

    [Tooltip("Ölüm ekranı paneli")]
    public GameObject deathScreenPanel;

    [Header("Ses")]
    public AudioSource audioSource;
    public AudioClip hurtClip;

    [Header("Sahne Ayarları")]
    [Tooltip("Ana menü sahne adı")]
    public string mainMenuSceneName = "MainMenu";

    // Dahili değişkenler
    private float currentHealth;
    private CharacterController controller;
    private bool isDead;
    private bool wasFalling;
    private float fallStartY;
    private float highestY;

    // Can değişikliği event'i (diğer scriptler dinleyebilir)
    public System.Action<float, float> OnHealthChanged;
    public System.Action OnPlayerDied;

    /// <summary>
    /// Mevcut can değeri (dışarıdan erişim için).
    /// </summary>
    public float CurrentHealth => currentHealth;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        isDead = false;

        // Başlangıçta ölüm ekranını gizle
        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(false);

        // Yükseklik takibi başlat
        highestY = transform.position.y;
        fallStartY = transform.position.y;

        UpdateHealthBar();
    }

    void Update()
    {
        if (isDead) return;

        TrackFalling();
    }

    /// <summary>
    /// Düşme durumunu takip eder ve yere indiğinde hasar hesaplar.
    /// </summary>
    private void TrackFalling()
    {
        if (controller == null) return;

        bool isGrounded = controller.isGrounded;

        if (!isGrounded)
        {
            // Havadayken - en yüksek noktayı takip et
            if (!wasFalling)
            {
                // Düşmeye başladı
                wasFalling = true;
                fallStartY = transform.position.y;
                highestY = transform.position.y;
            }
            else
            {
                // Yükseklik artıyorsa (zıplama), en yüksek noktayı güncelle
                if (transform.position.y > highestY)
                {
                    highestY = transform.position.y;
                }
            }
        }
        else
        {
            // Yere indi
            if (wasFalling)
            {
                wasFalling = false;
                float fallDistance = highestY - transform.position.y;

                if (fallDistance > safeFallHeight)
                {
                    ApplyFallDamage(fallDistance);
                }

                // Sıfırla
                highestY = transform.position.y;
                fallStartY = transform.position.y;
            }
            else
            {
                highestY = transform.position.y;
            }
        }
    }

    /// <summary>
    /// Düşme mesafesine göre hasar uygular.
    /// </summary>
    private void ApplyFallDamage(float fallDistance)
    {
        float damage;

        if (fallDistance >= lethalFallHeight)
        {
            // Ölümcül düşüş
            damage = currentHealth;
        }
        else
        {
            // Fazla düşüş mesafesine göre hasar
            float excessHeight = fallDistance - safeFallHeight;
            damage = excessHeight * damagePerMeter;
        }

        TakeDamage(damage);
    }

    /// <summary>
    /// Karaktere hasar verir.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (audioSource != null && hurtClip != null && damage > 0)
        {
            audioSource.PlayOneShot(hurtClip);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Karakterin canını iyileştirir.
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Can barını günceller.
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;

            // Canın durumuna göre renk değiştir
            float healthPercent = currentHealth / maxHealth;
            if (healthPercent > 0.6f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthBarFill.color = new Color(1f, 0.65f, 0f); // Turuncu
            else
                healthBarFill.color = Color.red;
        }

        // Can text'ini güncelle
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    /// <summary>
    /// Karakter öldüğünde çağrılır.
    /// </summary>
    private void Die()
    {
        isDead = true;
        OnPlayerDied?.Invoke();

        // Hareketi durdur
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetMoveInput(Vector3.zero);
            playerController.enabled = false;
        }

        // Kamera kontrolünü durdur
        TouchCameraController cameraController = GetComponent<TouchCameraController>();
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }

        // Ölüm ekranını göster
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Oyunu yeniden başlatır (Respawn).
    /// </summary>
    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Ana menüye döner.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
