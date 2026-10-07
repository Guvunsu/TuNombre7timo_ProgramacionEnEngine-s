using UnityEngine;

public class WanderBehaviour : MonoBehaviour
{
    [Header("Agente conf")]
    [SerializeField] float max_Speed = 3f;
    [SerializeField] float max_force = 0.1f;
    [SerializeField] float mass = 1f;

    [Header("Wander conf")]
    [SerializeField] float circleDistance = 2f;   // qué tan lejos está el círculo del agente
    [SerializeField] float circleRadius = 1f;     // radio del círculo (más grande = giros más amplios)
    [SerializeField] float angleChange = 120f;    // grados por segundo de variación máxima del ángulo

    private Vector3 velocity;
    private float wanderAngle;                    // ángulo (radianes) del punto sobre el círculo
    private Vector3 wanderTarget;                 // solo para dibujar gizmos
    private Vector3 circleCenter;

    void Start()
    {
        // Velocidad inicial aleatoria: si fuera (0,0,0), no habría "frente" para colocar el círculo
        Vector2 dir = Random.insideUnitCircle.normalized;
        velocity = new Vector3(dir.x, dir.y, 0f) * max_Speed;

        wanderAngle = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // 1) Dirección actual del agente (vector unitario).
        //    Si la velocidad es casi cero usamos transform.up para no normalizar un vector nulo.
        Vector3 heading = velocity.sqrMagnitude > 0.0001f ? velocity.normalized : transform.up;

        // 2) Centro del círculo = posición + heading * circleDistance
        //    (nos movemos circleDistance unidades hacia adelante).
        circleCenter = transform.position + heading * circleDistance;

        // 3) Perturbación pequeña del ángulo: valor en [-1, 1] * angleChange (en radianes) * dt.
        //    Multiplicar por deltaTime hace que el giro no dependa de los FPS.
        wanderAngle += Random.Range(-1f, 1f) * angleChange * Mathf.Deg2Rad * Time.deltaTime;

        // 4) Vector de desplazamiento sobre el círculo:
        //    (cos θ, sin θ) es un vector unitario con ángulo θ; lo escalamos por el radio.
        Vector3 displacement = new Vector3(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle), 0f) * circleRadius;

        // 5) Punto objetivo = centro del círculo + desplazamiento.
        wanderTarget = circleCenter + displacement;

        // 6) Velocidad deseada: apunta hacia el wanderTarget a máxima velocidad.
        Vector3 desiredVelocity = (wanderTarget - transform.position).normalized * max_Speed;

        // 7) Steering = lo que me falta para llegar a la velocidad deseada.
        Vector3 steering = desiredVelocity - velocity;

        // 8) Limitar la fuerza y aplicar la masa (a más masa, giros más lentos).
        steering = Vector3.ClampMagnitude(steering, max_force);
        steering /= mass;

        // 9) Nueva velocidad = velocidad + steering, limitada a max_Speed.
        velocity = Vector3.ClampMagnitude(velocity + steering, max_Speed);

        // 10) Mover: posición += velocidad * dt.
        transform.position += velocity * Time.deltaTime;

        // 11) (Opcional) Rotar el sprite para que mire hacia donde se mueve (sprite apuntando a +Y).
        if (velocity.sqrMagnitude > 0.0001f)
            transform.up = velocity;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(circleCenter, circleRadius);   // el círculo
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(wanderTarget, 0.1f);               // el punto objetivo
        Gizmos.DrawLine(transform.position, wanderTarget);
    }
}