using UnityEngine;
public abstract class SteeringBase : MonoBehaviour
{
    [SerializeField] protected float maxSpeed = 3f;
    [SerializeField] protected float maxForce = 8f;
    [SerializeField] protected float mass = 1f;

    protected Vector3 velocity;
    protected Vector3 facing;
    protected Rigidbody rb;

    /// <summary>
    /// Cuando inicia el programa y crea el objeto 
    /// obtengo el rg del objecto 
    /// desactivo la gravedad
    /// establezco la direccion inicial
    /// y la velocidades en cero 
    /// </summary>
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        facing = Vector3.forward;
        velocity = Vector3.zero;
    }

    /// <summary>
    /// Método preparado para calcular una fuerza de evasión, por si solo no hace nada
    /// Devuelve un vector cero 
    /// las clases hijas pueden sobrescribir este método para devolver una fuerza que permita esquivar obstáculos.
    /// </summary>
    protected virtual Vector3 GetAvoidance()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// Obtiene la velocidad actual del rg eliminando cualquier movimiento en Y.
    /// Limita la fuerza de steering al valor máximo permitido para evitar exageraciones de aceleramiento 
    /// Añade una fuerza adicional para evitar obstáculos.
    /// Divide la fuerza entre la masa aplicando Aceleración = Fuerza / Masa.
    /// Actualiza la velocidad usando la aceleración calculada.
    /// Limita la velocidad para que no supere maxSpeed.
    /// Aplica la nueva velocidad al Rigidbody.
    /// Elimina cualquier rotación física no deseada.
    /// Si se está moviendo, calcula la dirección actual.
    /// Finalmente rota el personaje para que mire hacia donde avanza.
    /// </summary>
    protected void Move(Vector3 steering)
    {
        velocity = Flat(rb.linearVelocity);

        steering = Vector3.ClampMagnitude(steering, maxForce);
       
        steering += GetAvoidance();

        steering /= mass;

        velocity += steering * Time.fixedDeltaTime;

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        rb.linearVelocity = velocity;
        rb.angularVelocity = Vector3.zero;

        if (velocity.sqrMagnitude > 0.04f)
        {
            facing = velocity.normalized;
            rb.MoveRotation(Quaternion.LookRotation(facing));
        }
    }

    /// <summary>
    /// Genera una fuerza de frenado.
    /// Devuelve una fuerza opuesta a la velocidad actual.
    /// </summary>
    protected Vector3 Brake()
    {
        return -velocity;
    }

    /// <summary>
    /// Calcula la distancia entre el agente y un punto objetivo.
    /// Obtiene el vector que va desde el agente al destino.
    /// Elimina el eje Y para trabajar únicamente en el plano XZ.
    /// Devuelve la magnitud del vector resultante.
    /// </summary>
    protected float DistTo(Vector3 point)
    {
        return Flat(point - transform.position).magnitude;
    }

    /// <summary>
    /// Evita movimiento en el eje Y.
    /// Establece el valor Y en cero.
    /// Devuelve el nuevo vector plano.
    /// </summary>
    protected static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }
}