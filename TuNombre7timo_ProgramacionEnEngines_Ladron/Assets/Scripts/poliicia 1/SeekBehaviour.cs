using UnityEngine;
//es abstracto porque solamente necesito heredar y no crear directamente uno nuevo
public abstract class SeekBehaviour : SteeringBase
{
    /// <summary>
    /// Es protected para que la puedan heredar las clases hijas 
    /// Calcula la diferencia entre pos objetivo y la pos actual
    /// Comprueba si el agente está muy cerca del objetivo usando sqrMagnitude, que devuelve la longitud al cuadrado del vector offset
    /// si llegamos devuelve una fuerza oppuesta a la velocidad actual
    /// normaliza el vector para conservar únicamente la dirección y después lo multiplica por maxSpeed para obtener la velocidad deseada
    /// Devuelve la diferencia entre la velocidad deseada y la velocidad actual, obteniendo la fuerza de steering necesaria para corregir el movimiento.
    /// </summary>
    protected Vector3 Seek(Vector3 target)
    {
        Vector3 offset = Flat(target - transform.position);

        if (offset.sqrMagnitude < 0.0001f)
            return -velocity;

        Vector3 desired = offset.normalized * maxSpeed;

        return desired - velocity;
    }
}