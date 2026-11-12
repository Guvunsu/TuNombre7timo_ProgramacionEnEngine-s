using UnityEngine;
public abstract class ArrivalBehaviour : SeekBehaviour
{
    [SerializeField] protected float slowingRadius = 2f;
    /// <summary>
    /// Calcula la dirección hacia el objetivo
    /// Obtiene la distancia al objetivo
    /// Si ya llego, frena
    /// Usa la velocidad máxima por defecto
    /// Reduce la velocidad suavemente cuando está cerca
    /// Calcula la velocidad deseada
    /// Devuelve la fuerza de steering
    /// </summary>
    protected Vector3 Arrive(Vector3 target)
    {
        Vector3 direction = Flat(target - transform.position);
        float distance = direction.magnitude;

        if (distance < 0.001f) return -velocity;

        float speed = maxSpeed;

        if (distance < slowingRadius)
        {
            speed = maxSpeed * distance / slowingRadius;
        }
        Vector3 desired = direction.normalized * speed;
        return desired - velocity;
    }
}
