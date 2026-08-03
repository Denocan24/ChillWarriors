using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ekranın ortasına crosshair ikonu yerleştirir.
/// Resources/UI/CrosshairIcon texture'ını yükler ve siyah arka planı kaldırır.
/// </summary>
public class CrosshairUI : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Crosshair boyutu (piksel)")]
    public float crosshairSize = 50f;

    private Image crosshairImage;

    void Start()
    {
        crosshairImage = GetComponent<Image>();
        if (crosshairImage == null) return;

        // Crosshair ikonunu yükle
        Texture2D crossTex = Resources.Load<Texture2D>("UI/CrosshairIcon");
        if (crossTex != null)
        {
            // Siyah arka planı kaldır
            Texture2D cleanTex = RemoveBlackBackground(crossTex);
            Sprite crossSprite = Sprite.Create(cleanTex,
                new Rect(0, 0, cleanTex.width, cleanTex.height),
                new Vector2(0.5f, 0.5f));

            crosshairImage.sprite = crossSprite;
            crosshairImage.preserveAspect = true;
            crosshairImage.raycastTarget = false;
            crosshairImage.color = Color.white;
        }

        // Boyutu ayarla
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(crosshairSize, crosshairSize);
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
