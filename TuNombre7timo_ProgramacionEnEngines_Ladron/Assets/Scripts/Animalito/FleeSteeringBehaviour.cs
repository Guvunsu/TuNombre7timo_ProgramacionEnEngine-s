using UnityEngine;

// FLEE: huir de una amenaza (lo contrario de Seek).
//
// (Se llama FleeSteeringBehaviour para no chocar con tu script suelto FleeBehaviour.)
//
// Cadena del NPC:
//   SteeringBase > SeekBehaviour > ArrivalBehaviour > Police2CollisionAvoidance
//     > FleeSteeringBehaviour > NPC_FSM
// Reutiliza Police2CollisionAvoidance (la evasión ya corregida): NO hace falta otra para el ratón.
// Plano XZ (Y congelado), igual que tu SteeringBase.
public abstract class FleeSteeringBehaviour : Police2CollisionAvoidance
{
    [Header("Flee anti-atoramiento (esquinas)")]
    [SerializeField] private float stuckSpeed = 0.25f;       // por debajo de esta velocidad cuenta como "parado"
    [SerializeField] private float stuckTime = 0.5f;         // segundos parado antes de salir de la esquina
    [SerializeField] private float escapeDuration = 0.6f;    // cuánto corre de lado para salir

    private float stuckTimer;
    private float escapeTimer;
    private Vector3 escapeDir;

    // Devuelve la fuerza de steering (NO mueve nada). Se llama desde FixedUpdate.
    protected Vector3 Flee(Vector3 threat)
    {
        float dt = Time.fixedDeltaTime;

        // 1) Velocidad deseada: DESDE la amenaza HACIA el agente (al revés que Seek)
        Vector3 away = Flat(transform.position - threat);

        if (away.sqrMagnitude < 0.0001f)       // exactamente encima de la amenaza: elige hacia donde mira
            away = Flat(facing);

        if (away.sqrMagnitude < 0.0001f)
            away = Vector3.forward;

        away.Normalize();

        // 2) Modo escape: corre perpendicular a la huida para salir de la esquina
        if (escapeTimer > 0f)
        {
            escapeTimer -= dt;
            return SteerToward(escapeDir);
        }

        // 3) Detectar atoramiento (acorralado contra una pared o esquina)
        if (velocity.magnitude < stuckSpeed) stuckTimer += dt;
        else stuckTimer = 0f;

        if (stuckTimer >= stuckTime)
        {
            stuckTimer = 0f;
            escapeTimer = escapeDuration;

            // Perpendicular a la dirección de huida, con el lado al azar
            escapeDir = Vector3.Cross(Vector3.up, away).normalized * (Random.value < 0.5f ? -1f : 1f);
            return SteerToward(escapeDir);
        }

        // 4) Flee normal
        return SteerToward(away);
    }

    // steering = velocidad deseada - velocidad actual
    private Vector3 SteerToward(Vector3 direction)
    {
        Vector3 desired = Flat(direction).normalized * maxSpeed;
        return desired - velocity;
    }

    // Reinicia el anti-atoramiento (llámalo al cambiar de estado)
    protected void ResetFlee()
    {
        stuckTimer = 0f;
        escapeTimer = 0f;
    }
}