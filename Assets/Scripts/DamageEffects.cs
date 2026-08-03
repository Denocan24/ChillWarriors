using UnityEngine;
using UnityEngine.UI;

public class DamageEffects : MonoBehaviour
{
    [Header("Flash Settings")]
    public float flashDuration = 0.3f;
    public float maxFlashAlpha = 0.4f;
    public Color flashColor = new Color(0.8f, 0f, 0f);

    [Header("Shake Settings")]
    public float shakeIntensity = 0.15f;
    public float shakeDuration = 0.2f;

    private Canvas damageCanvas;
    private Image vignetteImage;
    
    private float currentFlashTime;
    private bool isFlashing;
    private float currentFlashMaxAlpha;

    private Camera targetCamera;
    private Vector3 originalCameraLocalPos;
    private float currentShakeTime;
    private bool isShaking;
    private float currentShakeIntensity;

    void Start()
    {
        targetCamera = GetComponentInChildren<Camera>();
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            originalCameraLocalPos = targetCamera.transform.localPosition;

        CreateDamageCanvas();
    }

    void CreateDamageCanvas()
    {
        GameObject canvasObj = new GameObject("DamageCanvas");
        DontDestroyOnLoad(canvasObj);
        
        damageCanvas = canvasObj.AddComponent<Canvas>();
        damageCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        damageCanvas.sortingOrder = 999;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        GameObject imageObj = new GameObject("Vignette");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        vignetteImage = imageObj.AddComponent<Image>();
        vignetteImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        vignetteImage.raycastTarget = false;
        
        RectTransform rt = vignetteImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        vignetteImage.sprite = CreateVignetteSprite();
    }

    Sprite CreateVignetteSprite()
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((dist - radius * 0.4f) / (radius * 0.6f));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void PlayDamageEffect(float damagePercent)
    {
        damagePercent = Mathf.Clamp01(damagePercent);
        
        currentFlashMaxAlpha = Mathf.Lerp(0.1f, maxFlashAlpha, damagePercent);
        currentFlashTime = 0f;
        isFlashing = true;

        currentShakeIntensity = Mathf.Lerp(shakeIntensity * 0.2f, shakeIntensity, damagePercent);
        currentShakeTime = 0f;
        isShaking = true;
    }

    void Update()
    {
        if (isFlashing)
        {
            currentFlashTime += Time.deltaTime;
            float flashProgress = currentFlashTime / flashDuration;
            
            float alpha = 0f;
            if (flashProgress <= 0.2f)
            {
                alpha = Mathf.Lerp(0f, currentFlashMaxAlpha, flashProgress / 0.2f);
            }
            else if (flashProgress <= 1.0f)
            {
                alpha = Mathf.Lerp(currentFlashMaxAlpha, 0f, (flashProgress - 0.2f) / 0.8f);
            }
            else
            {
                alpha = 0f;
                isFlashing = false;
            }
            
            vignetteImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
        }

        if (isShaking && targetCamera != null)
        {
            currentShakeTime += Time.deltaTime;
            float shakeProgress = currentShakeTime / shakeDuration;
            
            if (shakeProgress < 1f)
            {
                float dampen = 1f - shakeProgress;
                Vector3 randomOffset = Random.insideUnitSphere * currentShakeIntensity * dampen;
                targetCamera.transform.localPosition = originalCameraLocalPos + randomOffset;
            }
            else
            {
                targetCamera.transform.localPosition = originalCameraLocalPos;
                isShaking = false;
            }
        }
    }

    void OnDestroy()
    {
        if (damageCanvas != null)
        {
            Destroy(damageCanvas.gameObject);
        }
    }
}
