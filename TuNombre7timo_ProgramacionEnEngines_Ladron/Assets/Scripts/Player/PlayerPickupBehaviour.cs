using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//(clase padre de PlayerController).
// Se encarga solo de recoger los PickupItem que le pasas por referencia.
public abstract class PlayerPickupBehaviour : MonoBehaviour
{
    [Header("Objetos recogidos")]
    [SerializeField] private PickupItem[] pickups;
    [SerializeField] private float pickupRange = 1.5f;

    private InputAction interactAction;
    private readonly List<string> items = new List<string>();

    protected virtual void Awake()
    {
        // Recoger: teclado, gamepad y mouse 
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonSouth");
        interactAction.AddBinding("<Mouse>/leftButton");
    }

    protected virtual void OnEnable()
    {
        interactAction.Enable();
    }

    protected virtual void OnDisable()
    {
        interactAction.Disable();
    }

    protected virtual void OnDestroy()
    {
        interactAction.Dispose();
    }

    //se pulso el botón de recoger en este frame?
    protected bool WantsPickup()
    {
        return interactAction.WasPressedThisFrame();
    }

    // Recoge el PickupItem más cercano si está dentro del alcance. Devuelve true si recogió algo.
    protected bool TryPickup()
    {
        PickupItem nearest = null;
        float best = pickupRange * pickupRange;

        foreach (PickupItem p in pickups)
        {
            if (p == null) continue;// ya recogido

            Vector3 offset = p.transform.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude <= best)
            {
                best = offset.sqrMagnitude;
                nearest = p;
            }
        }

        if (nearest == null) return false;

        nearest.Collect(this);
        return true;
    }

    //llama PickupItem al ser recogido
    public void AddItem(string itemId)
    {
        items.Add(itemId);
        Debug.Log($"Jugador: obtuvo '{itemId}'", this);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}