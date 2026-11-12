using UnityEngine;
public class NPC_FSM : FleeSteeringBehaviour
{
    private enum State
    {
        IDLE,
        FLEE
    }

    [SerializeField] private State state = State.IDLE;

    [Header("Amenaza")]
    [SerializeField] private Transform threat;
    [SerializeField] private float fleeDistance = 5f;
    [SerializeField] private float safeDistance = 8f;

    private void FixedUpdate()
    {
        if (threat == null) return;
        float distance = DistTo(threat.position);
        Vector3 steering = Vector3.zero;

        switch (state)
        {
            case State.IDLE:
                steering = Brake();
                // Si la amenaza se acerca, empieza a huir.
                if (distance < fleeDistance)
                {
                    ChangeState(State.FLEE);
                }
                break;

            case State.FLEE:
                steering = Flee(threat.position);
                // Si ya está lejos, se calma.
                if (distance > safeDistance)
                {
                    ChangeState(State.IDLE);
                }
                break;
        }
        Move(steering);
    }

    // Cambiar de estado.
    private void ChangeState(State newState)
    {
        if (state == newState) return;
        state = newState;
        Debug.Log("nuevo estado NPC: " + state);
    }
}