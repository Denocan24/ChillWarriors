using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Ekranın boş alanına dokunup sürükleyerek kamerayı döndüren script.
/// Yeni Input System ile çalışır.
/// Editor'de mouse, mobilde dokunmatik ekran destekler.
/// UI elemanlarının üzerindeki dokunuşları yok sayar.
/// </summary>
public class TouchCameraController : MonoBehaviour
{
    [Header("Hassasiyet Ayarları")]
    [Tooltip("Yatay (sağ-sol) döndürme hassasiyeti")]
    public float horizontalSensitivity = 0.2f;

    [Tooltip("Dikey (yukarı-aşağı) döndürme hassasiyeti")]
    public float verticalSensitivity = 0.15f;

    [Header("Dikey Sınırlar")]
    [Tooltip("Yukarı bakma sınırı (derece)")]
    public float minVerticalAngle = -30f;

    [Tooltip("Aşağı bakma sınırı (derece)")]
    public float maxVerticalAngle = 60f;

    [Header("Referanslar")]
    [Tooltip("Döndürülecek kamera Transform'u. Boş bırakılırsa child'daki kamera bulunur.")]
    public Transform cameraTransform;

    // Dahili değişkenler
    private float yaw;
    private float pitch;
    private bool isDragging;
    private Vector2 lastPosition;
    private int activeTouchId = -1;

    void Start()
    {
        if (cameraTransform == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraTransform = cam.transform;
        }

        if (cameraTransform == null)
        {
            Debug.LogError("[TouchCameraController] Kamera bulunamadı!");
            enabled = false;
            return;
        }

        yaw = transform.eulerAngles.y;
        pitch = cameraTransform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    void Update()
    {
        // Dokunmatik ekran varsa dokunmatik kullan, yoksa mouse kullan
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            HandleTouch();
        }
        else if (Mouse.current != null)
        {
            HandleMouse();
        }
    }

    /// <summary>
    /// Mouse ile kamera kontrolü.
    /// Sol tık + sürükleme ile çalışır. UI üzerindeki tıklamaları filtreler.
    /// </summary>
    private void HandleMouse()
    {
        var mouse = Mouse.current;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = mouse.position.ReadValue();
            if (!IsPositionOverUI(pos))
            {
                isDragging = true;
                lastPosition = pos;
            }
        }

        if (mouse.leftButton.isPressed && isDragging)
        {
            Vector2 currentPos = mouse.position.ReadValue();
            Vector2 delta = currentPos - lastPosition;
            ApplyRotation(delta);
            lastPosition = currentPos;
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    /// <summary>
    /// Dokunmatik ekran ile kamera kontrolü.
    /// UI üzerindeki dokunuşları filtreler.
    /// </summary>
    private void HandleTouch()
    {
        var touchscreen = Touchscreen.current;
        var touches = touchscreen.touches;

        // Aktif dokunuş hâlâ var mı kontrol et
        if (activeTouchId != -1)
        {
            bool found = false;
            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];
                if (t.touchId.ReadValue() == activeTouchId &&
                    t.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.None &&
                    t.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Ended &&
                    t.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    found = true;
                    break;
                }
            }
            if (!found) activeTouchId = -1;
        }

        for (int i = 0; i < touches.Count; i++)
        {
            var touch = touches[i];
            var phase = touch.phase.ReadValue();
            int touchId = touch.touchId.ReadValue();
            Vector2 pos = touch.position.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (activeTouchId == -1 && !IsPositionOverUI(pos))
                {
                    activeTouchId = touchId;
                    lastPosition = pos;
                }
            }

            if (touchId == activeTouchId)
            {
                if (phase == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    Vector2 delta = pos - lastPosition;
                    ApplyRotation(delta);
                    lastPosition = pos;
                }

                if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                    phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    activeTouchId = -1;
                }
            }
        }
    }

    /// <summary>
    /// Delta değerine göre yaw ve pitch rotasyonunu uygular.
    /// </summary>
    private void ApplyRotation(Vector2 delta)
    {
        yaw += delta.x * horizontalSensitivity;
        pitch -= delta.y * verticalSensitivity;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    /// <summary>
    /// Ekran pozisyonunun UI elemanı üzerinde olup olmadığını kontrol eder.
    /// </summary>
    private bool IsPositionOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}
