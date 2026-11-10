using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Objeto")]
    [SerializeField] private string itemId = "objeto";       
    [SerializeField] private string displayName = "objeto"; 

    public string DisplayName
    {
        get { return displayName; }
    }

    // Lo llama el jugador al recogerlo
    public void Collect(PlayerPickupBehaviour player)
    {
        player.AddItem(itemId);
        Destroy(gameObject);
    }
}