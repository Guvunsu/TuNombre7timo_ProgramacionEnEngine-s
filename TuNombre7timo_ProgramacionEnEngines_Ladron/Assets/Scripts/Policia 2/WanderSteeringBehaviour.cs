using UnityEngine;

// WANDER del Policía 2 (plano XZ), hereda de Police2CollisionAvoidance.
// Cambios para que el wander funcione bien junto a las paredes:
//  1) Si la evasión está activa, el punto del círculo se ORIENTA hacia donde escapa la evasión;
//     así el wander ayuda a salir de la pared en vez de empujar contra ella.
//  2) Si se queda casi parado (atorado) por un rato, entra en modo ESCAPE: se va hacia atrás
//     con un ángulo al azar durante un momento y luego retoma el wander.
public abstract class WanderSteeringBehaviour : Police2CollisionAvoidance
{
    [Header("Wander")]
    [SerializeField] private float circleDistance = 2f;      
    [SerializeField] private float circleRadius = 1.5f;      
    [SerializeField] private float angleChange = 300f;       
    [SerializeField] private float wanderWeight = 0.8f;

    [Header("Anti-atoramiento")]
    [SerializeField] private float stuckSpeed = 0.25f;
    [SerializeField] private float stuckTime = 0.6f;
    [SerializeField] private float escapeDuration = 0.8f;

    private float wanderAngle;
    private Vector3 circleCenter;
    private Vector3 wanderTarget;

    private float stuckTimer;
    private float escapeTimer;
    private Vector3 escapeDir;

    protected override void Awake()
    {
        base.Awake();

        wanderAngle = Random.Range(0f, Mathf.PI * 2f);
    }
    protected Vector3 Wander()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 heading = Flat(facing).normalized;

        if (escapeTimer > 0f)
        {
            escapeTimer -= dt;

            circleCenter = transform.position;
            wanderTarget = transform.position + escapeDir * circleDistance * 1.5f;

            return Seek(wanderTarget);
        }

        //detecto que me atoro
        if (velocity.magnitude < stuckSpeed) stuckTimer += dt;
        else stuckTimer = 0f;

        if (stuckTimer >= stuckTime)
        {
            stuckTimer = 0f;
            escapeTimer = escapeDuration;

            // Hacia atrás (la cara frontal apuntaba a la pared) con un ángulo al azar
            escapeDir = (Quaternion.Euler(0f, Random.Range(-60f, 60f), 0f) * -heading).normalized;

            // El wander continúa desde esa dirección cuando termine el escape
            wanderAngle = Mathf.Atan2(escapeDir.z, escapeDir.x);

            return Seek(transform.position + escapeDir * circleDistance * 1.5f);
        }
        circleCenter = transform.position + heading * circleDistance;

        if (IsAvoiding)
        {
            // Hay pared adelante: el punto apunta hacia donde escapa la evasión
            wanderAngle = Mathf.Atan2(LastAvoidance.z, LastAvoidance.x);
        } else
        {
            wanderAngle += Random.Range(-1f, 1f) * angleChange * Mathf.Deg2Rad * dt;
        }

        // Punto sobre el círculo: (cos θ, 0, sin θ) es un vector unitario en el plano XZ
        Vector3 displacement = new Vector3(Mathf.Cos(wanderAngle), 0f, Mathf.Sin(wanderAngle)) * circleRadius;

        wanderTarget = circleCenter + displacement;

        // Wander = Seek a un punto que cambia
        return Seek(wanderTarget) * wanderWeight;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(circleCenter, circleRadius);
        Gizmos.color = escapeTimer > 0f ? Color.magenta : Color.red;
        Gizmos.DrawSphere(wanderTarget, 0.15f);
        Gizmos.DrawLine(transform.position, wanderTarget);
    }
}