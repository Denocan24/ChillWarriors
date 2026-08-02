using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class PlayerShooting : MonoBehaviour
{
    [Header("Silah Ayarları")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.15f; // İki mermi arası bekleme
    public int maxAmmo = 30;
    public float reloadTime = 2.5f; // Animasyona göre tahmini süre

    [Header("Referanslar")]
    public Animator weaponAnimator;
    public Transform raycastOrigin; // Main Camera
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI reloadingText;

    [Header("Ses")]
    public AudioSource audioSource;
    public AudioClip gunshotClip;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;
    private bool isFiring = false;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateUI();
        
        if (reloadingText != null)
        {
            reloadingText.gameObject.SetActive(false);
        }

        if (raycastOrigin == null)
        {
            raycastOrigin = Camera.main.transform;
        }
    }

    void Update()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (isFiring && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    /// <summary>
    /// UI Fire Butonu OnPointerDown (Basılı tutma başlar)
    /// </summary>
    public void StartFiring()
    {
        isFiring = true;
    }

    /// <summary>
    /// UI Fire Butonu OnPointerUp (Basılı tutma biter)
    /// </summary>
    public void StopFiring()
    {
        isFiring = false;
    }

    private void Shoot()
    {
        currentAmmo--;
        UpdateUI();

        if (audioSource != null && gunshotClip != null)
        {
            audioSource.PlayOneShot(gunshotClip);
        }

        // Ateş Animasyonu
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Fire");
        }

        // Raycast Atış
        Vector3 rayStart = raycastOrigin.position;
        Vector3 rayDir = raycastOrigin.forward;

        if (Physics.Raycast(rayStart, rayDir, out RaycastHit hit, range))
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        if (currentAmmo <= 0)
        {
            isFiring = false;
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        isFiring = false;

        if (reloadingText != null)
        {
            reloadingText.gameObject.SetActive(true);
            reloadingText.text = "Reloading...";
        }

        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        UpdateUI();

        if (reloadingText != null)
        {
            reloadingText.gameObject.SetActive(false);
        }

        isReloading = false;
    }

    private void UpdateUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
        }
    }
}
