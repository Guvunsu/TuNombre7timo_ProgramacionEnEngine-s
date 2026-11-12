using UnityEngine;

public abstract class FleeSteeringBehaviour : CollisionAvoidanceBehaviour
{
    // Huir de una amenaza (es lo contrario de Seek).
    protected Vector3 Flee(Vector3 threat)
    {
        // Dirección que va desde la amenaza hacia el NPC.
        Vector3 away = Flat(transform.position - threat);

        // Si está justo encima de la amenaza, huye hacia donde mira.
        if (away.sqrMagnitude < 0.0001f)
            away = facing;

        // Velocidad que quiere llevar.
        Vector3 desired = away.normalized * maxSpeed;

        // Fuerza necesaria para llegar a esa velocidad.
        return desired - velocity;
    }
}