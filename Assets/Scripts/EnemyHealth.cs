using UnityEngine;
using System.Collections;

/// <summary>
/// NPC'nin can sistemini yönetir.
/// Ölüm halinde karakteri gizler ve 60 saniye sonra diriltir.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    [Header("Ses")]
    public AudioSource audioSource;
    public AudioClip hurtClip;
    
    private bool isDead = false;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    /// <summary>
    /// Karaktere hasar verir.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (audioSource != null && hurtClip != null && amount > 0)
        {
            audioSource.PlayOneShot(hurtClip);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Ölüm animasyonu yerine geriye doğru devrilmesini sağlıyoruz
        StartCoroutine(FallBackRoutine());
    }

    private IEnumerator FallBackRoutine()
    {
        // Hareket yeteneklerini durdur
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        var anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        // Geriye doğru düşme animasyonu (Rotasyon)
        float duration = 0.5f;
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(-90f, 0f, 0f); // X ekseninde -90 derece yat

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;

        // Yerde biraz bekleyip kaybol
        yield return new WaitForSeconds(2f);

        // Gizle ve dirilmeyi bekle
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // 60 saniye bekle
        yield return new WaitForSeconds(60f);

        // Pozisyonu ve değerleri sıfırla
        transform.position = startPosition;
        transform.rotation = startRotation;
        currentHealth = maxHealth;
        isDead = false;

        // Görselliği ve bileşenleri tekrar aç
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = true;

        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = true;

        var anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = true;
    }
}
