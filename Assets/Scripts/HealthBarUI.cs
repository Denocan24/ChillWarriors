using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    private Canvas healthCanvas;
    private Image borderImage;
    private Image bgImage;
    private Image ghostFill;
    private Image mainFill;
    private TextMeshProUGUI healthText;
    private Image heartIconImage;

    private PlayerHealth playerHealth;

    private float currentHealthPercent = 1f;
    private float targetHealthPercent = 1f;
    private float ghostHealthPercent = 1f;

    private float mainFillSpeed = 5f;
    private float ghostFillSpeed = 1.5f;

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        CreateUI();
        
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.maxHealth);
        }
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        DontDestroyOnLoad(canvasObj);
        
        healthCanvas = canvasObj.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        healthCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Border Container
        GameObject borderObj = new GameObject("HealthBarContainer");
        borderObj.transform.SetParent(canvasObj.transform, false);
        borderImage = borderObj.AddComponent<Image>();
        borderImage.color = new Color(0.4f, 0.4f, 0.5f, 0.9f);
        RectTransform borderRT = borderImage.rectTransform;
        borderRT.anchorMin = new Vector2(0.5f, 1f);
        borderRT.anchorMax = new Vector2(0.5f, 1f);
        borderRT.pivot = new Vector2(0.5f, 1f);
        borderRT.sizeDelta = new Vector2(404, 39);
        borderRT.anchoredPosition = new Vector2(0, -20);
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(borderObj.transform, false);
        bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.85f);
        RectTransform bgRT = bgImage.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = new Vector2(2, 2);
        bgRT.offsetMax = new Vector2(-2, -2);
        
        // Ghost Fill
        GameObject ghostObj = new GameObject("GhostFill");
        ghostObj.transform.SetParent(bgObj.transform, false);
        ghostFill = ghostObj.AddComponent<Image>();
        ghostFill.color = new Color(1f, 1f, 1f, 0.5f);
        ghostFill.type = Image.Type.Filled;
        ghostFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform ghostRT = ghostFill.rectTransform;
        ghostRT.anchorMin = Vector2.zero;
        ghostRT.anchorMax = Vector2.one;
        ghostRT.offsetMin = Vector2.zero;
        ghostRT.offsetMax = Vector2.zero;
        
        // Main Fill
        GameObject mainObj = new GameObject("MainFill");
        mainObj.transform.SetParent(bgObj.transform, false);
        mainFill = mainObj.AddComponent<Image>();
        mainFill.color = new Color(0.2f, 0.9f, 0.3f);
        mainFill.type = Image.Type.Filled;
        mainFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform mainRT = mainFill.rectTransform;
        mainRT.anchorMin = Vector2.zero;
        mainRT.anchorMax = Vector2.one;
        mainRT.offsetMin = Vector2.zero;
        mainRT.offsetMax = Vector2.zero;
        
        // Text
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(borderObj.transform, false);
        healthText = textObj.AddComponent<TextMeshProUGUI>();
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.fontStyle = FontStyles.Bold;
        healthText.color = Color.white;
        healthText.fontSize = 20;
        RectTransform textRT = healthText.rectTransform;
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        textObj.AddComponent<Shadow>();
        
        // Heart Icon (Sprite)
        GameObject heartObj = new GameObject("HeartIcon");
        heartObj.transform.SetParent(borderObj.transform, false);
        RectTransform heartRT = heartObj.AddComponent<RectTransform>();
        heartRT.anchorMin = new Vector2(0, 0.5f);
        heartRT.anchorMax = new Vector2(0, 0.5f);
        heartRT.pivot = new Vector2(1, 0.5f);
        heartRT.sizeDelta = new Vector2(40, 40);
        heartRT.anchoredPosition = new Vector2(-10, 0);

        heartIconImage = heartObj.AddComponent<Image>();
        heartIconImage.raycastTarget = false;
        heartIconImage.preserveAspect = true;

        // Sprite'ı Resources'tan yükle ve arka planı kaldır
        Texture2D heartTex = Resources.Load<Texture2D>("UI/HeartIcon");
        if (heartTex != null)
        {
            // Siyah arka planı transparan yap
            Texture2D cleanTex = RemoveBlackBackground(heartTex);
            Sprite heartSprite = Sprite.Create(cleanTex, new Rect(0, 0, cleanTex.width, cleanTex.height), new Vector2(0.5f, 0.5f));
            heartIconImage.sprite = heartSprite;
            heartIconImage.color = Color.white;
        }
        else
        {
            heartIconImage.color = new Color(0.9f, 0.15f, 0.15f);
        }
    }

    void UpdateHealthBar(float current, float max)
    {
        targetHealthPercent = Mathf.Clamp01(current / max);
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    void Update()
    {
        if (currentHealthPercent != targetHealthPercent)
        {
            // Snap ghost immediately if healing
            if (targetHealthPercent > currentHealthPercent && ghostHealthPercent < targetHealthPercent)
            {
                ghostHealthPercent = targetHealthPercent;
                ghostFill.fillAmount = ghostHealthPercent;
            }

            currentHealthPercent = Mathf.Lerp(currentHealthPercent, targetHealthPercent, Time.deltaTime * mainFillSpeed);
            mainFill.fillAmount = currentHealthPercent;
            
            if (currentHealthPercent > 0.5f)
            {
                mainFill.color = Color.Lerp(new Color(1f, 0.7f, 0f), new Color(0.2f, 0.9f, 0.3f), (currentHealthPercent - 0.5f) * 2f);
            }
            else
            {
                mainFill.color = Color.Lerp(new Color(0.9f, 0.15f, 0.15f), new Color(1f, 0.7f, 0f), currentHealthPercent * 2f);
            }
        }
        
        if (ghostHealthPercent != targetHealthPercent)
        {
            ghostHealthPercent = Mathf.Lerp(ghostHealthPercent, targetHealthPercent, Time.deltaTime * ghostFillSpeed);
            ghostFill.fillAmount = ghostHealthPercent;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
        
        if (healthCanvas != null)
        {
            Destroy(healthCanvas.gameObject);
        }
    }

    /// <summary>
    /// Siyah arka planı transparan yapan yardımcı metod.
    /// </summary>
    private Texture2D RemoveBlackBackground(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        readable.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        Color[] pixels = readable.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            float brightness = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
            if (brightness < 0.15f)
            {
                pixels[i] = new Color(0, 0, 0, 0);
            }
            else if (brightness < 0.4f)
            {
                float alpha = Mathf.InverseLerp(0.15f, 0.4f, brightness);
                pixels[i].a = alpha;
            }
        }

        readable.SetPixels(pixels);
        readable.Apply();
        return readable;
    }
}
