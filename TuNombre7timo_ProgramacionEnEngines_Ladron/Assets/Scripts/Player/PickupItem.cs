using UnityEngine;

// OBJETO RECOGIBLE (llave, moneda, documento...).
// Agrégalo a un objeto con Collider (puede ser Trigger). El PlayerController lo detecta
// y lo recoge cuando el jugador pulsa Interactuar.
public class PickupItem : MonoBehaviour
{
    [Header("Objeto")]
    [SerializeField] private string itemId = "objeto";         // identificador
    [SerializeField] private string displayName = "objeto";    // nombre que se muestra en pantalla

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