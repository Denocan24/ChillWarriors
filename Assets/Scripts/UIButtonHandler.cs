using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Canvas üzerindeki joystick ve aksiyon butonlarını
/// PlayerController'a bağlayan UI yönetici scripti.
/// Joystick ile hareket, butonlarla zıplama ve koşma desteği sağlar.
/// </summary>
public class UIButtonHandler : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Player Controller referansı. Boş bırakılırsa otomatik bulunur.")]
    public PlayerController playerController;

    [Tooltip("Sanal joystick referansı. Boş bırakılırsa Canvas içinde otomatik bulunur.")]
    public VirtualJoystick joystick;

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

        // Joystick referansı yoksa otomatik bul
        if (joystick == null)
        {
            joystick = GetComponentInChildren<VirtualJoystick>();
        }

        if (joystick == null)
        {
            Debug.LogError("[UIButtonHandler] VirtualJoystick bulunamadı! Canvas içine joystick ekleyin.");
        }
        else
        {
            Debug.Log("[UIButtonHandler] VirtualJoystick başarıyla bağlandı.");
        }

        // JumpButton: Sadece basıldığında tetiklenir
        SetupActionButton("JumpButton",
            () => playerController.RequestJump());

        // RunButton: Basılı tutulduğunda koşma, bırakılınca normal yürüme
        SetupDirectionButton("RunButton",
            () => playerController.SetRunning(true),
            () => playerController.SetRunning(false));
    }

    void Update()
    {
        if (playerController == null || joystick == null) return;

        // Joystick girişini al ve PlayerController'a gönder
        Vector2 joystickInput = joystick.InputDirection;
        Vector3 moveInput = new Vector3(joystickInput.x, 0f, joystickInput.y);

        // PlayerController'a hareket girişini gönder
        playerController.SetMoveInput(moveInput);
    }

    /// <summary>
    /// Basılı tutma desteği olan buton kurulumu (RunButton için).
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
        if (playerController != null)
        {
            playerController.SetRunning(false);
            playerController.SetMoveInput(Vector3.zero);
        }
    }
}
