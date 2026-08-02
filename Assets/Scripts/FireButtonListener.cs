using UnityEngine;
using UnityEngine.EventSystems;

public class FireButtonListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private PlayerShooting playerShooting;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerShooting = player.GetComponent<PlayerShooting>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playerShooting != null)
        {
            playerShooting.StartFiring();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (playerShooting != null)
        {
            playerShooting.StopFiring();
        }
    }
}
