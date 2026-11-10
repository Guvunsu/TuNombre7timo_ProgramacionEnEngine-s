using UnityEngine;
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
    [SerializeField] private Transform threat;  // de quién huye 
    [SerializeField] private float fleeDistance = 5f;        
    [SerializeField] private float safeDistance = 8f;      

    protected override void Awake()
    {
        base.Awake();

        if (threat == null)
        {
            Debug.LogWarning("NPC_FSM: falta asignar la amenaza (threat).",this);
        }
    }

    private void ChangeState(State newState)
    {
        if (state == newState) return;
        if (showStateLogs)
        {
            Debug.Log($"NPC: {state} -> {newState}",this);
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

        Move(steering); // Move() ya suma la evasión
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        // Distancia para....
        Gizmos.color = Color.red;//comenzar a huir 
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        Gizmos.color = Color.green;//sentirse seguro
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        if (!Application.isPlaying || threat == null)
            return;

        Gizmos.color = Color.yellow;//entre el npc y la amenaza
        Gizmos.DrawLine(transform.position, threat.position);
    }
}