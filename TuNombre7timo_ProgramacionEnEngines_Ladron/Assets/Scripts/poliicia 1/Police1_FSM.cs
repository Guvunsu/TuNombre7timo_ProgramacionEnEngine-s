using UnityEngine;
public class Police1_FSM : PathFollowingBehaviour
{
    private enum State
    {
        PATROL,
        IDLE,
        CHASE,
        CATCH,
        COME_BACK
    }

    [Header("Estado FSM")]
    [SerializeField] private State state = State.PATROL;
    [SerializeField] private bool showStateLogs = true;

    [Header("Nodos de patrulla")]
    [SerializeField] private Transform node1;
    [SerializeField] private Transform node2;
    [SerializeField] private Transform node3;
    [SerializeField] private Transform node4;
    [SerializeField] private Transform node5;
    [SerializeField] private Transform node6;
    [SerializeField] private float idleTime = 2f;

    [Header("Jugador y vision")]
    [SerializeField] private Transform player;
    [SerializeField] private float sightRange = 6f;
    [SerializeField] private float fov = 90f;
    [SerializeField] private LayerMask sightBlockMask;
    [SerializeField] private float catchDistance = 1.4f;
    [SerializeField] private float loseTime = 2f;

    private float idleTimer;
    private float lostTimer;
    private Vector3 lastSeenPos;

    protected override void Awake()
    {
        base.Awake();
        FillPath();
        lastSeenPos = transform.position;
        idleTimer = idleTime;
        lostTimer = loseTime;

        if (!HasPath)
        {
            Debug.LogError("Police1_FSM: asigna al menos un nodo.",this);
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("Police1_FSM: falta asignar el jugador.",this);
        }
    }

    private void FillPath()
    {
        AddNode(node1);
        AddNode(node2);
        AddNode(node3);
        AddNode(node4);
        AddNode(node5);
        AddNode(node6);
    }

    private void ChangeState(State newState)
    {
        if (state == newState)
            return;

        if (showStateLogs)
        {
            Debug.Log($"Policia: {state} -> {newState}",this);
        }

        state = newState;

        if (newState == State.IDLE)
            idleTimer = idleTime;
        if (newState == State.CHASE)
            lostTimer = loseTime;
    }

    private void FixedUpdate()
    {
        if (!HasPath)
        {
            Debug.LogWarning("Police1_FSM: el nodo actual ya no existe", this);
            return;
        }

        bool sees = CanSeePlayer();

        if (sees)
            lastSeenPos = player.position;

        Vector3 steering = Vector3.zero;

        switch (state)
        {
            case State.PATROL:
                avoidanceEnabled = true;

                if (sees)
                {
                    ChangeState(State.CHASE);
                    break;
                }

                steering = Arrive(CurrentNodePos);

                if (ReachedNode())
                {
                    ChangeState(State.IDLE);
                    steering = Vector3.zero;
                }
                break;

            case State.IDLE:
                if (sees)
                {
                    avoidanceEnabled = true;
                    ChangeState(State.CHASE);
                    break;
                }

                avoidanceEnabled = false;
                steering = Arrive(CurrentNodePos);

                idleTimer -= Time.fixedDeltaTime;

                if (idleTimer <= 0f)
                {
                    AdvanceNode();
                    avoidanceEnabled = true;
                    ChangeState(State.PATROL);
                    steering = Vector3.zero;
                }
                break;

            case State.CHASE:
                avoidanceEnabled = true;

                if (sees)
                {
                    lostTimer = loseTime;
                    steering = Seek(player.position);

                    if (DistTo(player.position) <= catchDistance)
                    {
                        ChangeState(State.CATCH);
                        steering = Vector3.zero;
                    }
                } else
                {
                    lostTimer -= Time.fixedDeltaTime;
                    steering = Arrive(lastSeenPos);

                    if (lostTimer <= 0f)
                    {
                        ChangeState(State.COME_BACK);
                        steering = Vector3.zero;
                    }
                }
                break;

            case State.CATCH:
                avoidanceEnabled = true;

                steering = Arrive(player.position);

                if (DistTo(player.position) > catchDistance * 1.5f)
                {
                    ChangeState(State.CHASE);
                }
                // Aqui! sistema de Game Over.
                break;

            case State.COME_BACK:
                avoidanceEnabled = true;

                if (sees)
                {
                    ChangeState(State.CHASE);
                    break;
                }

                steering = Arrive(CurrentNodePos);

                if (ReachedNode())
                {
                    // Se reincorpora en el nodo actual.
                    // El siguiente avance ocurrira despues de IDLE.
                    ChangeState(State.IDLE);
                    steering = Vector3.zero;
                }
                break;
        }
        Move(steering);
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer = Flat(player.position - transform.position);

        float distance = toPlayer.magnitude;

        if (distance > sightRange || distance < 0.001f) return false;

        float angle = Vector3.Angle(facing, toPlayer.normalized);

        if (angle > fov * 0.5f) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;

        if (Physics.Linecast(origin,target,sightBlockMask,QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return true;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!Application.isPlaying)
            return;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Vector3 left = Quaternion.Euler(0f, -fov * 0.5f, 0f) * facing;
        Vector3 right = Quaternion.Euler(0f, fov * 0.5f, 0f) * facing;

        Gizmos.DrawLine(origin, origin + left * sightRange);
        Gizmos.DrawLine(origin, origin + right * sightRange);
        Gizmos.DrawLine(origin, origin + facing * sightRange);

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, catchDistance);
        }
    }
}
