using UnityEngine;

// NPC GENÉRICO (ratón, paloma, perrito...): Flee + Collision Avoidance
//
// Hereda la cadena: SteeringBase > Seek > Arrival > Police2CollisionAvoidance > FleeSteering.
// Es el ÚNICO script que se agrega al cubo.
//
// Estado | Behaviour usado
// -------|----------------------------------------------------------
// IDLE   | quieto (frena)
// FLEE   | Flee (huye de la amenaza)
// (Collision Avoidance se suma en Move() en TODOS los estados)
//
// Histéresis: empieza a huir a fleeDistance y se calma hasta safeDistance (mayor),
// para que no titubee entre estados justo en el borde.
public class NPC_FSM : FleeSteeringBehaviour
{
    private enum State
    {
        IDLE,
        FLEE
    }

    [Header("FSM")]
    [SerializeField] private State state = State.IDLE;
    [SerializeField] private bool showStateLogs = true;

    [Header("Amenaza")]
    [SerializeField] private Transform threat;               // de quién huye (el jugador, un policía...)
    [SerializeField] private float fleeDistance = 5f;        // si la amenaza se acerca a menos de esto, huye
    [SerializeField] private float safeDistance = 8f;        // se calma cuando está más lejos que esto

    protected override void Awake()
    {
        base.Awake();

        if (threat == null)
        {
            Debug.LogWarning(
                "NPC_FSM: falta asignar la amenaza (threat).",
                this);
        }
    }

    private void ChangeState(State newState)
    {
        if (state == newState)
            return;

        if (showStateLogs)
        {
            Debug.Log(
                $"NPC: {state} -> {newState}",
                this);
        }

        state = newState;
        ResetFlee();
    }

    private void FixedUpdate()
    {
        if (threat == null)
        {
            Move(Brake());
            return;
        }

        float distance = DistTo(threat.position);

        Vector3 steering = Vector3.zero;

        switch (state)
        {
            case State.IDLE:
                steering = Brake();

                if (distance < fleeDistance)
                    ChangeState(State.FLEE);
                break;

            case State.FLEE:
                steering = Flee(threat.position);

                if (distance > safeDistance)
                    ChangeState(State.IDLE);
                break;
        }

        Move(steering);                                      // Move() ya suma la evasión
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Distancias (se ven también sin dar Play)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        if (!Application.isPlaying || threat == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, threat.position);
    }
}