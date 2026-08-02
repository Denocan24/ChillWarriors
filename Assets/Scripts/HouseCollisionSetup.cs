using UnityEngine;

/// <summary>
/// House objelerine mesh tabanlı collision ekleyen component.
/// MeshFilter bulunan tüm child objelere otomatik olarak MeshCollider ekler.
/// Böylece evin duvarları geçilmez olurken, kapı ve pencere gibi boşluklar açık kalır.
/// LOD sistemi olan evlerde sadece en detaylı LOD seviyesine (lod_0) collider ekler.
/// </summary>
public class HouseCollisionSetup : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Sadece en detaylı LOD seviyesine (lod_0) collider eklensin mi?")]
    public bool onlyHighestLOD = true;

    [Tooltip("Eğer child objelerde zaten collider varsa atla")]
    public bool skipExistingColliders = true;

    void Awake()
    {
        SetupColliders();
    }

    /// <summary>
    /// Tüm child MeshFilter'ları tarar ve uygun olanlara MeshCollider ekler.
    /// </summary>
    public void SetupColliders()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);

        foreach (MeshFilter meshFilter in meshFilters)
        {
            // Mesh yoksa atla
            if (meshFilter.sharedMesh == null)
                continue;

            // Sadece en yüksek LOD seviyesini kullan (lod_0)
            if (onlyHighestLOD)
            {
                string objName = meshFilter.gameObject.name.ToLower();
                // LOD ismi içerip lod_0 olmayan objeleri atla
                if (objName.Contains("lod_") && !objName.Contains("lod_0"))
                    continue;
            }

            // Zaten collider varsa atla
            if (skipExistingColliders && meshFilter.GetComponent<Collider>() != null)
                continue;

            // MeshCollider ekle - mesh'in gerçek şekline göre collision oluşturur
            // Bu sayede duvarlar geçilmez olur, boşluklar (kapı, pencere) açık kalır
            MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = false; // Concave mesh = iç boşluklar korunur
        }
    }

    /// <summary>
    /// Bu component tarafından eklenen tüm MeshCollider'ları kaldırır.
    /// </summary>
    public void RemoveColliders()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);

        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshCollider existingCollider = meshFilter.GetComponent<MeshCollider>();
            if (existingCollider != null)
            {
                if (Application.isPlaying)
                    Destroy(existingCollider);
                else
                    DestroyImmediate(existingCollider);
            }
        }
    }
}
