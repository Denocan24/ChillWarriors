using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public Transform target;
    public float attackRange = 10f;
    public float shootingInterval = 3f;
    public float damage = 25f;

    [Header("Efektler ve Ses")]
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip gunshotClip;

    private NavMeshAgent agent;
    private Animator animator;
    private float lastShootTime;
    private bool isPlayerDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (target == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
                var playerHealth = playerObj.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.OnPlayerDied += () => isPlayerDead = true;
                }
            }
        }
    }

    void Update()
    {
        if (target == null || isPlayerDead)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsShooting", false);
            }
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange)
        {
            // Ateş etme menzilinde
            if (agent.isOnNavMesh) agent.isStopped = true;
            
            // Oyuncuya dön
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsShooting", true);
            }

            if (Time.time >= lastShootTime + shootingInterval)
            {
                Shoot();
            }
        }
        else
        {
            // Takip et
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
                animator.SetBool("IsShooting", false);
            }
        }
    }

    private void Shoot()
    {
        lastShootTime = Time.time;

        if (audioSource != null && gunshotClip != null)
        {
            audioSource.PlayOneShot(gunshotClip);
        }

        // Muzzle Flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        else
        {
            // Basit bir ışık flaşı oluştur (eğer partikül yoksa)
            StartCoroutine(SimpleMuzzleFlash());
        }

        // Oyuncuya hasar ver (Raycast ile düz atış simulasyonu veya direkt hasar)
        // Burada basitlik adına menzildeyse ve arada duvar yoksa hasar veriyoruz
        Vector3 rayStart = transform.position + Vector3.up * 1.5f;
        Vector3 dirToTarget = (target.position + Vector3.up * 1.5f) - rayStart;

        if (Physics.Raycast(rayStart, dirToTarget, out RaycastHit hit, attackRange))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.transform.root.CompareTag("Player"))
            {
                var ph = target.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
            }
        }
    }

    private IEnumerator SimpleMuzzleFlash()
    {
        GameObject lightObj = new GameObject("MuzzleFlashLight");
        lightObj.transform.position = transform.position + transform.forward * 1f + Vector3.up * 1.2f;
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.yellow;
        light.range = 5f;
        light.intensity = 3f;

        yield return new WaitForSeconds(0.1f);
        Destroy(lightObj);
    }
}
