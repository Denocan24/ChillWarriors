using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Mobil cihazlar için sanal joystick bileşeni.
/// Canvas üzerinde sürüklenebilir bir joystick oluşturur ve
/// hareket girişini PlayerController'a iletir.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Joystick Ayarları")]
    [Tooltip("Joystick kolunun hareket edebileceği maksimum mesafe (piksel)")]
    public float handleRange = 60f;

    [Tooltip("Joystick'in giriş olarak algılanması için gereken minimum eşik (0-1)")]
    public float deadZone = 0.1f;

    [Header("Referanslar")]
    [Tooltip("Joystick arka plan Image'ı (otomatik bulunur)")]
    public RectTransform background;

    [Tooltip("Joystick kolu Image'ı (otomatik bulunur)")]
    public RectTransform handle;

    // Dahili değişkenler
    private Canvas canvas;
    private Camera cam;
    private Vector2 input = Vector2.zero;

    /// <summary>
    /// Joystick'in normalize edilmiş giriş değeri (-1 ile 1 arası).
    /// X: sağ-sol, Y: ileri-geri
    /// </summary>
    public Vector2 InputDirection => input;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            cam = canvas.worldCamera;
        }

        // Handle'ı merkeze sıfırla
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        // Dokunma noktasını background'un lokal koordinatına çevir
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, cam, out localPoint);

        // Background boyutuna göre normalize et (-1 ile 1 arası)
        Vector2 bgSize = background.sizeDelta;
        localPoint.x /= (bgSize.x * 0.5f);
        localPoint.y /= (bgSize.y * 0.5f);

        // Büyüklüğü 1 ile sınırla
        input = localPoint.magnitude > 1f ? localPoint.normalized : localPoint;

        // Dead zone uygula
        if (input.magnitude < deadZone)
        {
            input = Vector2.zero;
        }

        // Handle'ı görsel olarak hareket ettir
        handle.anchoredPosition = new Vector2(
            input.x * handleRange,
            input.y * handleRange);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Joystick bırakıldığında sıfırla
        input = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }
}
