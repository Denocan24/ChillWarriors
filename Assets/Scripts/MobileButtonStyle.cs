using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MobileButtonStyle : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ApplyStylesDelayed());
    }

    private IEnumerator ApplyStylesDelayed()
    {
        // Wait 2 frames to ensure UI is fully initialized
        yield return null;
        yield return null;

        Color fireTop, fireBot, jumpTop, jumpBot, runTop, runBot;
        ColorUtility.TryParseHtmlString("#E6443E", out fireTop);
        ColorUtility.TryParseHtmlString("#991A1A", out fireBot);
        ColorUtility.TryParseHtmlString("#33B5E5", out jumpTop);
        ColorUtility.TryParseHtmlString("#1A5C99", out jumpBot);
        ColorUtility.TryParseHtmlString("#4DD95A", out runTop);
        ColorUtility.TryParseHtmlString("#267A33", out runBot);

        // Apply Button Styles
        StyleButton("FireButton", CreateGradientTexture(128, 128, fireTop, fireBot), "UI/FireIcon");
        StyleButton("JumpButton", CreateGradientTexture(128, 128, jumpTop, jumpBot), "UI/JumpIcon");
        StyleButton("RunButton", CreateGradientTexture(128, 128, runTop, runBot), "UI/RunIcon");

        // Apply Joystick Styles
        StyleJoystick();
    }

    private void StyleButton(string buttonName, Texture2D gradientTex, string iconResourcePath)
    {
        Transform buttonTrans = FindChildRecursive(transform, buttonName);
        if (buttonTrans == null) return;

        // Buton arkaplanını tamamen transparan yap
        Image img = buttonTrans.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(1f, 1f, 1f, 0f); // Tamamen görünmez
        }

        // Minimum size constraint
        RectTransform rect = buttonTrans.GetComponent<RectTransform>();
        if (rect != null)
        {
            float newWidth = Mathf.Max(rect.sizeDelta.x, 100f);
            float newHeight = Mathf.Max(rect.sizeDelta.y, 100f);
            rect.sizeDelta = new Vector2(newWidth, newHeight);
        }

        // İkon texture'ını yükle ve arka planı kaldır
        Texture2D iconTex = Resources.Load<Texture2D>(iconResourcePath);
        Sprite iconSprite = null;

        if (iconTex != null)
        {
            // Readable kopya oluştur ve siyah arka planı kaldır
            Texture2D cleanTex = RemoveBlackBackground(iconTex);
            iconSprite = Sprite.Create(cleanTex,
                new Rect(0, 0, cleanTex.width, cleanTex.height),
                new Vector2(0.5f, 0.5f));
        }

        if (iconSprite != null)
        {
            // İkon için child Image oluştur
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buttonTrans, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f, 0.1f);
            iconRT.anchorMax = new Vector2(0.9f, 0.9f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"[MobileButtonStyle] '{iconResourcePath}' sprite yüklenemedi!");
        }

        // Press animation component (sadece scale efekti, renk değişimi yok)
        ButtonPressEffect pressEffect = buttonTrans.gameObject.GetComponent<ButtonPressEffect>();
        if (pressEffect == null) pressEffect = buttonTrans.gameObject.AddComponent<ButtonPressEffect>();
        pressEffect.targetImage = null; // Arkaplan transparan olduğu için renk animasyonu yok
    }

    private void StyleJoystick()
    {
        VirtualJoystick joystick = GetComponentInChildren<VirtualJoystick>();
        if (joystick == null) return;

        Texture2D bgTex = CreateRadialGradientTexture(256, 256, new Color(1f, 1f, 1f, 0.5f), new Color(1f, 1f, 1f, 0f));
        Texture2D handleTex = CreateRadialGradientTexture(128, 128, new Color(1f, 1f, 1f, 0.9f), new Color(1f, 1f, 1f, 0.3f));

        if (joystick.background != null)
        {
            Image bgImg = joystick.background.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.sprite = Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f));
                bgImg.color = Color.white;
                
                Outline outline = joystick.background.gameObject.GetComponent<Outline>();
                if (outline == null) outline = joystick.background.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 1f, 1f, 0.3f);
                outline.effectDistance = new Vector2(2, 2);
            }
        }

        if (joystick.handle != null)
        {
            Image handleImg = joystick.handle.GetComponent<Image>();
            if (handleImg != null)
            {
                handleImg.sprite = Sprite.Create(handleTex, new Rect(0, 0, handleTex.width, handleTex.height), new Vector2(0.5f, 0.5f));
                handleImg.color = Color.white;
                
                Outline outline = joystick.handle.gameObject.GetComponent<Outline>();
                if (outline == null) outline = joystick.handle.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 1f, 1f, 0.3f);
                outline.effectDistance = new Vector2(2, 2);
            }
        }
    }

    private Texture2D CreateGradientTexture(int width, int height, Color top, Color bottom)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color rowColor = Color.Lerp(bottom, top, t);
            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = rowColor;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private Texture2D CreateRadialGradientTexture(int width, int height, Color center, Color edge)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        Vector2 centerPos = new Vector2(width / 2f, height / 2f);
        float maxRadius = Mathf.Min(width, height) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), centerPos);
                float t = Mathf.Clamp01(dist / maxRadius);
                pixels[y * width + x] = Color.Lerp(center, edge, t);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Siyah arka planı transparan yapan yardımcı metod.
    /// Koyu piksellerin alpha değerini parlaklığa göre ayarlar.
    /// </summary>
    private Texture2D RemoveBlackBackground(Texture2D source)
    {
        // Readable kopya oluştur
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
            // Pikselin parlaklığını hesapla
            float brightness = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;

            if (brightness < 0.15f)
            {
                // Çok koyu piksel → tamamen transparan
                pixels[i] = new Color(0, 0, 0, 0);
            }
            else if (brightness < 0.4f)
            {
                // Geçiş bölgesi → kısmen transparan (yumuşak kenar)
                float alpha = Mathf.InverseLerp(0.15f, 0.4f, brightness);
                pixels[i].a = alpha;
            }
            // Parlak pikseller olduğu gibi kalır (alpha = 1)
        }

        readable.SetPixels(pixels);
        readable.Apply();
        return readable;
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}

public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float pressScale = 0.9f;
    public float scaleSpeed = 12f;
    public Color normalColor = Color.white;
    public Color pressColor = new Color(0.8f, 0.8f, 0.8f, 0.4f);
    public Image targetImage;

    private bool isPressed = false;
    private Vector3 initialScale;

    private void Start()
    {
        initialScale = transform.localScale;
        
        if (targetImage == null)
            targetImage = GetComponent<Image>();
            
        if (targetImage != null)
            normalColor = targetImage.color;
    }

    private void Update()
    {
        Vector3 targetScale = isPressed ? initialScale * pressScale : initialScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

        if (targetImage != null)
        {
            Color targetCol = isPressed ? pressColor : normalColor;
            targetImage.color = Color.Lerp(targetImage.color, targetCol, Time.deltaTime * scaleSpeed);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}
