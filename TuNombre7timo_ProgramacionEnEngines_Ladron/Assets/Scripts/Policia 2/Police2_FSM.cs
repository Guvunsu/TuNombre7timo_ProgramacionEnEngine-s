using UnityEngine;

public class Police2_FSM : WanderSteeringBehaviour
{
    private enum State
    {
        WANDER,
        CHASE,
        CATCH
    }

    [Header("FSM")]
    [SerializeField] private State state = State.WANDER;
    [SerializeField] private bool showStateLogs = true;

    [Header("Zona Wander")]
    [SerializeField] private Transform wanderCenter;
    [SerializeField] private float wanderAreaRadius = 8f;

    [Header("Jugador")]
    [SerializeField] private Transform player;

    [SerializeField] private float sightRange = 6f;
    [SerializeField] private float fov = 90f;
    [SerializeField] private float eyeHeight = 0.5f;
    [SerializeField] private LayerMask sightBlockMask;
    [SerializeField] private float catchDistance = 1.4f;
    [SerializeField] private float loseTime = 2f;

    private float lostTimer;
    private Vector3 lastSeenPos;
    private bool returningToZone;

    protected override void Awake()
    {
        base.Awake();
        lostTimer = loseTime;
        lastSeenPos = transform.position;

        if (player == null)
        {
            Debug.LogWarning("Police2_FSM: falta asignar Player",this);
        }

        if (wanderCenter == null)
        {
            Debug.LogWarning("Police2_FSM: falta Wander Center",this);
        }
    }

    private void ChangeState(State newState)
    {
        if (state == newState) return;

        if (showStateLogs)
        {
            Debug.Log($"Police2: {state} -> {newState}",this);
        }
        state = newState;
        if (state == State.CHASE)
        {
            lostTimer = loseTime;
        }
    }

    private void FixedUpdate()
    {
        bool seesPlayer = CanSeePlayer();
        if (seesPlayer)
        {
            lastSeenPos = player.position;
        }

        Vector3 steering = Vector3.zero;

        switch (state)
        {
            case State.WANDER:

                if (seesPlayer)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                steering = WanderInsideZone();
                break;
            case State.CHASE:
                if (seesPlayer)
                {
                    lostTimer = loseTime;

                    steering =Seek(player.position);

                    if (DistTo(player.position)<= catchDistance)
                    {
                        ChangeState(State.CATCH);
                    }
                } else
                {
                    lostTimer -= Time.fixedDeltaTime;

                    steering = Arrive(lastSeenPos);

                    if (lostTimer <= 0f)
                    {
                        ChangeState(State.WANDER);
                    }
                }
                break;
            case State.CATCH:

                steering = Arrive(player.position);

                if (DistTo(player.position) > catchDistance * 1.5f)
                {
                    ChangeState(State.CHASE);
                }

                break;
        }
        Move(steering);
    }

    private Vector3 WanderInsideZone()
    {
        if (wanderCenter == null)
            return Wander();

        float distance =DistTo(wanderCenter.position);

        if (distance > wanderAreaRadius)
        {
            returningToZone = true;
        } else if (distance < wanderAreaRadius * 0.6f)
        {
            returningToZone = false;
        }

        if (returningToZone)
        {
            return Arrive(wanderCenter.position);
        }
        return Wander();
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer =Flat(player.position-transform.position);
        float distance = toPlayer.magnitude;

        if (distance > sightRange) return false;
        if (distance < 0.01f) return false;

        float angle =Vector3.Angle( facing,toPlayer.normalized);

        if (angle > fov * 0.5f) return false;

        Vector3 origin =transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * eyeHeight;

        if (Physics.Linecast(origin,target,sightBlockMask,QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return true;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (wanderCenter != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wanderCenter.position,wanderAreaRadius);
        }

        if (!Application.isPlaying) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Gizmos.color = Color.white;
        Vector3 left = Quaternion.Euler( 0f,-fov*0.5f,0f)*facing;
        Vector3 right = Quaternion.Euler(0f,fov * 0.5f,0f)*facing;
        Gizmos.DrawLine(origin,origin + left * sightRange);

        Gizmos.DrawLine(origin,origin + right * sightRange);

        Gizmos.DrawLine(origin,origin + facing * sightRange);

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position,catchDistance);
        }
    }
}