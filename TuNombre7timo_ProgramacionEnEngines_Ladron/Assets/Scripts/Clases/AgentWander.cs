using UnityEngine;

// Wander (Craig Reynolds) + evasión de paredes con SphereCast (colliders 3D).
// El movimiento sigue siendo en el plano XY (z = 0), como tu versión original.
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

    [Header("Evasión de obstáculos")]
    [SerializeField] LayerMask obstacleMask;      // capa de paredes/obstáculos (NO incluyas la capa del agente)
    [SerializeField] float lookAhead = 1.5f;      // qué tan lejos "ve" el agente hacia adelante
    [SerializeField] float agentRadius = 0.3f;    // grosor del agente para que no roce esquinas
    [SerializeField] float avoidForce = 0.3f;     // fuerza máxima de evasión (mayor que max_force para que gane al wander)

    private Vector3 velocity;
    private float wanderAngle;                    // ángulo (radianes) del punto sobre el círculo
    private Vector3 wanderTarget;                 // solo para dibujar gizmos
    private Vector3 circleCenter;                 // solo para dibujar gizmos
    private bool hitSomething;                    // ¿el último cast pegó con algo? (para gizmos)

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
        circleCenter = transform.position + heading * circleDistance;

        // 3) Perturbación pequeña del ángulo, independiente de los FPS gracias a deltaTime.
        wanderAngle += Random.Range(-1f, 1f) * angleChange * Mathf.Deg2Rad * Time.deltaTime;

        // 4) Desplazamiento sobre el círculo: (cos θ, sin θ) es un vector unitario; lo escalamos por el radio.
        Vector3 displacement = new Vector3(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle), 0f) * circleRadius;

        // 5) Punto objetivo = centro del círculo + desplazamiento.
        wanderTarget = circleCenter + displacement;

        // 6) Velocidad deseada: apunta hacia el wanderTarget a máxima velocidad.
        Vector3 desiredVelocity = (wanderTarget - transform.position).normalized * max_Speed;

        // 7) Steering del wander = lo que me falta para llegar a la velocidad deseada,
        //    limitado a max_force.
        Vector3 steering = Vector3.ClampMagnitude(desiredVelocity - velocity, max_force);

        // 8) Evasión: sumamos la fuerza que nos aleja de las paredes.
        //    Se suma DESPUÉS del clamp para que el wander no la "ahogue".
        steering += ComputeAvoidance(heading);

        // 9) Aplicar la masa (a más masa, giros más lentos).
        steering /= mass;

        // 10) Nueva velocidad = velocidad + steering, limitada a max_Speed.
        velocity = Vector3.ClampMagnitude(velocity + steering, max_Speed);

        // 11) Mover: posición += velocidad * dt.
        transform.position += velocity * Time.deltaTime;

        // 12) Rotar el sprite/modelo para que mire hacia donde se mueve (apuntando a +Y).
        if (velocity.sqrMagnitude > 0.0001f)
            transform.up = velocity;
    }

    // Lanza un SphereCast hacia adelante. Si pega con una pared devuelve una fuerza
    // que aleja al agente; si no, devuelve (0,0,0).
    Vector3 ComputeAvoidance(Vector3 heading)
    {
        // SphereCast = raycast con grosor (agentRadius). Evita que el agente se atore en esquinas.
        // QueryTriggerInteraction.Ignore: ignora los colliders marcados como Trigger (zonas, sensores, etc.).
        hitSomething = Physics.SphereCast(
            transform.position, agentRadius, heading, out RaycastHit hit,
            lookAhead, obstacleMask, QueryTriggerInteraction.Ignore);

        // Nada enfrente: no hay evasión.
        if (!hitSomething) return Vector3.zero;

        // Intensidad: 0 si la pared está lejos (distance = lookAhead), 1 si estamos pegados (distance = 0).
        float strength = 1f - (hit.distance / lookAhead);

        // En 3D la normal puede traer componente en Z (ej. al pegar con una esquina o un borde).
        // Como nos movemos en XY, la aplanamos para que no nos empuje fuera del plano.
        Vector3 normal = hit.normal;
        normal.z = 0f;
        normal.Normalize();

        // La normal aleja de la pared, pero en un choque frontal es opuesta a heading
        // y solo frenaría al agente sin hacerlo girar.
        // Por eso añadimos un componente lateral (perpendicular a heading en el plano XY).
        Vector3 side = new Vector3(-heading.y, heading.x, 0f);

        // Elegimos el lado que se aleja de la pared (producto punto con la normal).
        if (Vector3.Dot(side, normal) < 0f) side = -side;

        // Dirección de escape = normal + lateral, normalizada y escalada por fuerza e intensidad.
        Vector3 away = (normal + side).normalized;
        return away * avoidForce * strength;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Círculo y punto objetivo del wander
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(circleCenter, circleRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(wanderTarget, 0.1f);
        Gizmos.DrawLine(transform.position, wanderTarget);

        // Cast de evasión: verde si está libre, magenta si detecta pared
        Gizmos.color = hitSomething ? Color.magenta : Color.green;
        Vector3 end = transform.position + transform.up * lookAhead;
        Gizmos.DrawLine(transform.position, end);
        Gizmos.DrawWireSphere(end, agentRadius);
    }
}

/*Pon las paredes en una capa (por ejemplo Obstacles) y selecciónala en obstacleMask. Si el agente tiene su propio collider y está en esa capa, el cast lo detectará a él mismo.
Si tus colliders son 3D (BoxCollider en vez de BoxCollider2D), cambia a Physics.SphereCast y RaycastHit.
Si el agente se atora de frente, sube avoidForce o lookAhead. Si tiembla en pasillos angostos, baja agentRadius.
Para esquinas más finas, agrega dos “bigotes” (casts a ±30° de heading) y suma sus resultados.
Esto solo evita que el wander se meta a las paredes. Para que no las atraviese del todo (por ejemplo a velocidades altas), conviene que el agente tenga un Rigidbody2D y un collider.*/