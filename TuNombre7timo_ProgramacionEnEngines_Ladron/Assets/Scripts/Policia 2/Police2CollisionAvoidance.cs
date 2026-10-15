using UnityEngine;

// COLLISION AVOIDANCE del Policía 2 (plano XZ).
// Cambios para que NO se quede trabado en las paredes:
//  - El lado de escape ya no depende de 'facing' (que parpadea en un choque de frente): se elige
//    con la normal de la pared y, si es casi de frente, se CONSERVA el lado elegido antes.
//  - La fuerza de escape = normal de la pared + lateral (se aleja Y se desliza).
//  - El SphereCast arranca un poco ATRÁS del agente: si el cubo está pegado a la pared,
//    una esfera que empieza tocándola no la detecta; así sí.
//  - Expone IsAvoiding y LastAvoidance para que el Wander apunte hacia donde se escapa
//    en lugar de empujar contra la pared.
public abstract class Police2CollisionAvoidance : ArrivalBehaviour
{
    [Header("Police 2 Avoidance")]
    [SerializeField] private LayerMask obstacleMask;         // capa de las paredes (NO la de los policías)

    [SerializeField] private float lookAhead = 2f;
    [SerializeField] private float agentRadius = 0.45f;      // cubo de lado 1 => ~0.45
    [SerializeField] private float sideSensorAngle = 35f;
    [SerializeField] private float maxAvoidForce = 10f;      // que sea >= maxForce para que gane al seek/wander
    [SerializeField] private float backOffset = 0.3f;        // cuánto retrocede el origen del SphereCast

    // Los usa el Wander
    protected bool IsAvoiding { get; private set; }
    protected Vector3 LastAvoidance { get; private set; }

    private int avoidSign = 1;                               // lado de escape elegido (+1 / -1)

    protected override void Awake()
    {
        base.Awake();
        avoidSign = Random.value < 0.5f ? -1 : 1;
    }

    protected override Vector3 GetAvoidance()
    {
        Vector3 heading =
            velocity.sqrMagnitude > 0.01f
            ? velocity.normalized
            : facing;

        heading = Flat(heading).normalized;

        if (heading.sqrMagnitude < 0.0001f)
        {
            IsAvoiding = false;
            LastAvoidance = Vector3.zero;
            return Vector3.zero;
        }

        Vector3 avoidance = Vector3.zero;

        avoidance += CastSensor(heading, lookAhead, true);

        avoidance += CastSensor(
            Quaternion.Euler(0f, -sideSensorAngle, 0f) * heading,
            lookAhead * 0.6f,
            false);

        avoidance += CastSensor(
            Quaternion.Euler(0f, sideSensorAngle, 0f) * heading,
            lookAhead * 0.6f,
            false);

        avoidance = Vector3.ClampMagnitude(avoidance, maxAvoidForce);

        LastAvoidance = avoidance;
        IsAvoiding = avoidance.sqrMagnitude > 0.01f;

        return avoidance;
    }

    private Vector3 CastSensor(
        Vector3 direction,
        float length,
        bool isFrontSensor)
    {
        direction = Flat(direction).normalized;

        if (direction.sqrMagnitude < 0.001f)
            return Vector3.zero;

        // El origen retrocede un poco para detectar paredes que ya casi tocamos
        Vector3 origin = transform.position - direction * backOffset;

        if (!Physics.SphereCast(
            origin,
            agentRadius,
            direction,
            out RaycastHit hit,
            length + backOffset,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            return Vector3.zero;
        }

        // 0 si la pared está lejos, 1 si estamos pegados
        float distance = Mathf.Max(0f, hit.distance - backOffset);
        float strength = 1f - Mathf.Clamp01(distance / length);

        Vector3 normal = Flat(hit.normal).normalized;

        if (normal.sqrMagnitude < 0.001f)
            normal = -direction;

        // Sensores laterales: solo empujan lejos de la pared
        if (!isFrontSensor)
            return normal * (maxAvoidForce * 0.5f) * strength;

        // Sensor frontal: lateral perpendicular a la dirección de avance
        Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;

        // Si la pared está inclinada, escapa hacia el lado al que apunta su normal.
        // Si es casi de frente (d ~ 0), conserva el lado que ya había elegido (evita el parpadeo).
        float d = Vector3.Dot(side, normal);

        if (Mathf.Abs(d) > 0.2f)
            avoidSign = d > 0f ? 1 : -1;

        Vector3 away = (normal + side * avoidSign).normalized;

        return away * maxAvoidForce * strength;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!Application.isPlaying)
            return;

        Vector3 heading =
            velocity.sqrMagnitude > 0.01f
            ? velocity.normalized
            : facing;

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            transform.position,
            transform.position + heading * lookAhead);

        Vector3 left =
            Quaternion.Euler(0f, -sideSensorAngle, 0f) * heading;

        Vector3 right =
            Quaternion.Euler(0f, sideSensorAngle, 0f) * heading;

        Gizmos.DrawLine(
            transform.position,
            transform.position + left * lookAhead * 0.6f);

        Gizmos.DrawLine(
            transform.position,
            transform.position + right * lookAhead * 0.6f);

        // Fuerza de evasión actual
        if (IsAvoiding)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawLine(
                transform.position,
                transform.position + LastAvoidance.normalized * 1.5f);
        }
    }
}