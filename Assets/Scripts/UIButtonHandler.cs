using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Canvas üzerindeki butonları isimlerine göre otomatik bulan ve
/// PlayerController'a bağlayan UI yönetici scripti.
/// EventTrigger kullanarak basılı tutma (press & hold) desteği sağlar.
/// </summary>
public class UIButtonHandler : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Player Controller referansı. Boş bırakılırsa otomatik bulunur.")]
    public PlayerController playerController;

    // Yön butonları durum bayrakları
    private bool isForwardPressed;
    private bool isBackwardPressed;
    private bool isLeftPressed;
    private bool isRightPressed;

    void Start()
    {
        // PlayerController referansı yoksa otomatik bul
        if (playerController == null)
        {
            // Önce tag ile bul
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();

                // Tag doğru objeyi bulduysa ama PlayerController yoksa, parent'ta ara
                if (playerController == null && player.transform.parent != null)
                {
                    playerController = player.transform.parent.GetComponent<PlayerController>();
                }
            }

            // Tag ile bulunamadıysa isimle dene
            if (playerController == null)
            {
                GameObject playerByName = GameObject.Find("Player");
                if (playerByName != null)
                {
                    playerController = playerByName.GetComponent<PlayerController>();
                }
            }

            // Hiçbir şekilde bulunamadıysa, sahnedeki herhangi bir PlayerController'ı bul
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            if (playerController == null)
            {
                Debug.LogError("[UIButtonHandler] PlayerController bulunamadı! " +
                    "Player objesine PlayerController scripti eklendiğinden emin olun.");
                enabled = false;
                return;
            }

            Debug.Log("[UIButtonHandler] PlayerController başarıyla bulundu: " + playerController.gameObject.name);
        }

        // Butonları isimlerine göre bul ve event'leri bağla
        SetupDirectionButton("ForwardButton",
            () => isForwardPressed = true,
            () => isForwardPressed = false);

        SetupDirectionButton("BackwardButton",
            () => isBackwardPressed = true,
            () => isBackwardPressed = false);

        SetupDirectionButton("LeftButton",
            () => isLeftPressed = true,
            () => isLeftPressed = false);

        SetupDirectionButton("RightButton",
            () => isRightPressed = true,
            () => isRightPressed = false);

        // JumpButton: Sadece basıldığında tetiklenir (basılı tutma gereksiz)
        SetupActionButton("JumpButton",
            () => playerController.RequestJump());

        // RunButton: Basılı tutulduğunda koşma, bırakılınca normal yürüme
        SetupDirectionButton("RunButton",
            () => playerController.SetRunning(true),
            () => playerController.SetRunning(false));
    }

    void Update()
    {
        if (playerController == null) return;

        // Yön girişini hesapla
        Vector3 moveInput = Vector3.zero;

        if (isForwardPressed) moveInput.z += 1f;
        if (isBackwardPressed) moveInput.z -= 1f;
        if (isRightPressed) moveInput.x += 1f;
        if (isLeftPressed) moveInput.x -= 1f;

        // Çapraz harekette hızın artmaması için normalize et
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        // PlayerController'a hareket girişini gönder
        playerController.SetMoveInput(moveInput);
    }

    /// <summary>
    /// Basılı tutma desteği olan buton kurulumu (yön butonları ve RunButton için).
    /// PointerDown ve PointerUp eventlerini bağlar.
    /// </summary>
    private void SetupDirectionButton(string buttonName, System.Action onPointerDown, System.Action onPointerUp)
    {
        Transform buttonTransform = FindChildRecursive(transform, buttonName);
        if (buttonTransform == null)
        {
            Debug.LogWarning($"[UIButtonHandler] '{buttonName}' isimli buton Canvas içinde bulunamadı!");
            return;
        }

        // EventTrigger bileşenini al veya ekle
        EventTrigger trigger = buttonTransform.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = buttonTransform.gameObject.AddComponent<EventTrigger>();
        }

        // PointerDown event'i ekle
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((eventData) => onPointerDown?.Invoke());
        trigger.triggers.Add(pointerDownEntry);

        // PointerUp event'i ekle
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((eventData) => onPointerUp?.Invoke());
        trigger.triggers.Add(pointerUpEntry);

        Debug.Log($"[UIButtonHandler] '{buttonName}' başarıyla bağlandı.");
    }

    /// <summary>
    /// Tek seferlik aksiyon buton kurulumu (JumpButton için).
    /// Sadece PointerDown event'ini bağlar.
    /// </summary>
    private void SetupActionButton(string buttonName, System.Action onPointerDown)
    {
        Transform buttonTransform = FindChildRecursive(transform, buttonName);
        if (buttonTransform == null)
        {
            Debug.LogWarning($"[UIButtonHandler] '{buttonName}' isimli buton Canvas içinde bulunamadı!");
            return;
        }

        // EventTrigger bileşenini al veya ekle
        EventTrigger trigger = buttonTransform.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = buttonTransform.gameObject.AddComponent<EventTrigger>();
        }

        // PointerDown event'i ekle
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((eventData) => onPointerDown?.Invoke());
        trigger.triggers.Add(pointerDownEntry);

        Debug.Log($"[UIButtonHandler] '{buttonName}' başarıyla bağlandı.");
    }

    /// <summary>
    /// Transform hiyerarşisinde isimle recursive arama yapar.
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Uygulama odağını kaybettiğinde tüm buton durumlarını sıfırla.
    /// Bu, buton basılıyken alt-tab yapılması gibi durumları önler.
    /// </summary>
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ResetAllInputs();
        }
    }

    /// <summary>
    /// Tüm giriş durumlarını sıfırlar.
    /// </summary>
    private void ResetAllInputs()
    {
        isForwardPressed = false;
        isBackwardPressed = false;
        isLeftPressed = false;
        isRightPressed = false;

        if (playerController != null)
        {
            playerController.SetRunning(false);
            playerController.SetMoveInput(Vector3.zero);
        }
    }
}
