using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sahnedeki bina objelerine NavMeshObstacle ekleyerek
/// NPC'lerin binalardan geçmesini engeller.
/// Runtime'da NavMesh'i dinamik olarak günceller (carving).
/// Boş bir GameObject'e veya bir sahne yöneticisine eklenir.
/// </summary>
public class NavMeshValidator : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Binalara otomatik NavMeshObstacle eklensin mi?")]
    public bool autoSetupObstacles = true;
    
    [Tooltip("Obstacle boyutuna eklenecek buffer (metre)")]
    public float sizeBuffer = 0.5f;
    
    [Tooltip("Eklenen obstacle sayısını logla")]
    public bool debugLog = true;

    void Start()
    {
        if (autoSetupObstacles)
        {
            int count = SetupBuildingObstacles();
            if (debugLog)
                Debug.Log($"[NavMeshValidator] {count} binaya NavMeshObstacle eklendi.");
        }
    }

    /// <summary>
    /// Sahnedeki tüm bina objelerini tarar ve NavMeshObstacle ekler.
    /// </summary>
    public int SetupBuildingObstacles()
    {
        int addedCount = 0;

        // 1. HouseCollisionSetup olan objeleri bul
        HouseCollisionSetup[] houses = FindObjectsByType<HouseCollisionSetup>(FindObjectsSortMode.None);
        foreach (var house in houses)
        {
            if (AddNavMeshObstacle(house.gameObject))
                addedCount++;
        }

        // 2. İsimlerinde "House", "Building", "home" geçen objeleri de tara
        // (HouseCollisionSetup olmayabilir ama bina olabilir)
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            string nameLower = obj.name.ToLower();
            if ((nameLower.Contains("house") || nameLower.Contains("building") || nameLower.Contains("home")) 
                && obj.GetComponent<NavMeshObstacle>() == null 
                && obj.GetComponentInChildren<Renderer>() != null)
            {
                // Sadece root objeler (parent'ı başka bir bina değilse)
                if (obj.transform.parent == null || 
                    (!obj.transform.parent.name.ToLower().Contains("house") && 
                     !obj.transform.parent.name.ToLower().Contains("building")))
                {
                    if (AddNavMeshObstacle(obj))
                        addedCount++;
                }
            }
        }

        return addedCount;
    }

    /// <summary>
    /// Bir objeye NavMeshObstacle ekler. Renderer bounds kullanarak boyut hesaplar.
    /// </summary>
    private bool AddNavMeshObstacle(GameObject obj)
    {
        // Zaten varsa atla
        if (obj.GetComponent<NavMeshObstacle>() != null)
            return false;

        // Renderer bounds hesapla
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return false;

        // Combined bounds hesapla
        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        // NavMeshObstacle ekle
        NavMeshObstacle obstacle = obj.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true;
        obstacle.shape = NavMeshObstacleShape.Box;

        // World bounds'u local space'e çevir
        Vector3 localCenter = obj.transform.InverseTransformPoint(combinedBounds.center);
        
        // Size'ı local scale'e göre ayarla
        Vector3 localSize = combinedBounds.size;
        Vector3 lossyScale = obj.transform.lossyScale;
        localSize.x = lossyScale.x != 0 ? localSize.x / Mathf.Abs(lossyScale.x) : localSize.x;
        localSize.y = lossyScale.y != 0 ? localSize.y / Mathf.Abs(lossyScale.y) : localSize.y;
        localSize.z = lossyScale.z != 0 ? localSize.z / Mathf.Abs(lossyScale.z) : localSize.z;

        obstacle.center = localCenter;
        obstacle.size = localSize + Vector3.one * sizeBuffer;

        return true;
    }
}
