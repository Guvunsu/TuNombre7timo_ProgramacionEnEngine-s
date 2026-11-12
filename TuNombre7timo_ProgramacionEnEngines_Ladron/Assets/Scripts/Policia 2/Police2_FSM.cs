
using UnityEngine;

public class Police2_FSM : WanderSteeringBehaviour
{
    // Estados del policía.
    private enum State
    {
        WANDER,
        CHASE,
        CATCH
    }

    [SerializeField] private State state = State.WANDER;

    [Header("Zona de patrullaje")]
    [SerializeField] private Transform wanderCenter;
    [SerializeField] private float wanderAreaRadius = 8f;

    [Header("Jugador")]
    [SerializeField] private Transform player;
    [SerializeField] private float sightRange = 6f;
    [SerializeField] private float fov = 90f;
    [SerializeField] private float catchDistance = 1.4f;
    [SerializeField] private float loseTime = 2f;
    [SerializeField] private LayerMask sightBlockMask;

    [Header("Derrota")]
    [SerializeField] private SceneController script_sceneController;
    [SerializeField] private float caughtTime = 3f;

    private float timer;
    private float caughtTimer;
    private Vector3 lastSeenPos;

    protected override void Awake()
    {
        base.Awake();

        lastSeenPos = transform.position;
        timer = loseTime;
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        bool seesPlayer = CanSeePlayer();

        if (seesPlayer)
            lastSeenPos = player.position;

        Vector3 steering = Vector3.zero;

        switch (state)
        {
            case State.WANDER:

                // Regresar al centro si sale de su zona.
                if (wanderCenter != null && DistTo(wanderCenter.position) > wanderAreaRadius)
                    steering = Arrive(wanderCenter.position);
                else
                    steering = Wander();

                if (seesPlayer)
                    ChangeState(State.CHASE);

                break;

            case State.CHASE:

                if (seesPlayer)
                {
                    timer = loseTime;
                    steering = Seek(player.position);

                    if (DistTo(player.position) <= catchDistance)
                        ChangeState(State.CATCH);
                } else
                {
                    timer -= Time.fixedDeltaTime;
                    steering = Arrive(lastSeenPos);

                    if (timer <= 0)
                        ChangeState(State.WANDER);
                }

                break;

            case State.CATCH:

                Debug.Log("Estado CATCH");

                steering = Arrive(player.position);

                caughtTimer -= Time.fixedDeltaTime;

                Debug.Log("Tiempo restante: " + caughtTimer);

                if (caughtTimer <= 0)
                {
                    Debug.Log("Intentando cargar derrota");

                    if (script_sceneController == null)
                    {
                        Debug.LogError("SceneController no asignado");
                        return;
                    }

                    script_sceneController.LoadLoose();
                    return;
                }

                if (DistTo(player.position) > catchDistance * 1.5f)
                {
                    Debug.Log("El jugador escapó");
                    ChangeState(State.CHASE);
                }

                break;
        }

        Move(steering);
    }

    // Cambiar de estado.
    private void ChangeState(State newState)
    {
        if (state == newState)
            return;

        state = newState;

        if (state == State.CHASE)
            timer = loseTime;

        if (state == State.CATCH)
            caughtTimer = caughtTime;

        Debug.Log("Policia 2 nuevo estado: " + state);
    }

    // Comprobar si el policía puede ver al jugador.
    private bool CanSeePlayer()
    {
        Vector3 direction = Flat(player.position - transform.position);

        float distance = direction.magnitude;

        if (distance > sightRange || distance < 0.001f)
            return false;

        float angle = Vector3.Angle(transform.forward, direction.normalized);

        if (angle > fov / 2f)
            return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;

        if (Physics.Linecast(origin, target, sightBlockMask))
            return false;

        return true;
    }
}
