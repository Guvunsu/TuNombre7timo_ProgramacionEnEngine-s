using UnityEngine;
public class Police2_FSM : WanderSteeringBehaviour
{
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

    [Header("Tiempo")]
    [SerializeField] private float timer;
    [SerializeField] private float caughtTimer;

    private Vector3 lastSeenPos;

    /// <summary>
    /// Inicializa el agente.
    /// Ejecuta el Awake de la clase base (WanderSteeringBehaviour)
    /// Guarda la posición inicial del policía
    /// Inicializa el temporizador para perder de vista al jugador
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        lastSeenPos = transform.position;
        timer = loseTime;
    }

    /// <summary>
    /// actualiza la fsm
    /// </summary>
    private void FixedUpdate()
    {
        //si no hay jugador asignado, no realiza ningún comportamiento.
        //compruebo si el jugador es visible para el policia
        //guardo la ultima pos donde el poli vio al jugador
        if (player == null) return;
        bool seesPlayer = ICanSeeThePlayerAndWalls();
        if (seesPlayer) lastSeenPos = player.position;

        // Inicializa la fuerza de dirección que se aplicará al policía.
        Vector3 steering = Vector3.zero;

        switch (state)
        {
            // comprueba que exista un centro de patrullaje asignado y comprueba la distancia entre el poli y el centro es mayor al radio permitido
            // Si sale de su area zona, utiliza Arrive para regresar al centro de su area establecido
            // si no esta afuera, deambula
            // Si ve al jugador, cambia al estado de persecución.
            case State.WANDER:
                if (wanderCenter != null && DistTo(wanderCenter.position) > wanderAreaRadius)
                    steering = Arrive(wanderCenter.position);
                else steering = Wander();
                if (seesPlayer) ChangeState(State.CHASE);
                break;

            //si puedo ver al jugador
            //reinicia el temporizador 
            //si está suficientemente cerca, calculo mi pos(poli) con el pos (jugador) para perseguirlo
            //si mi calculo entre la distancia del poli y el jugador es menor o igual a la distancia minima que requiero para pasar al estado de captura
            //si el poli ya no lo puede ver, el temporizador se termina por tratar de dirigirse para capturarlo, vuelve a deambular.
            case State.CHASE:

                if (seesPlayer)
                {
                    timer = loseTime;
                    steering = Seek(player.position);
                    if (DistTo(player.position) <= catchDistance) ChangeState(State.CATCH);
                }
                else
                {
                    timer -= Time.fixedDeltaTime;
                    steering = Arrive(lastSeenPos);
                    if (timer <= 0) ChangeState(State.WANDER);
                }
                break;

            //si el policía está cerca del jugador, intenta capturarlo al dirigirse a el por su pos
            //cuenta regresivamente el tiempo establecido para capturarlo y que pierda el jugador
            //si el tiempo llega a cero, carga la escena de derrota
            //si el jugador se aleja de la distancia de captura,vuelve al estado de persecución.
            case State.CATCH:

                steering = Arrive(player.position);
                caughtTimer -= Time.fixedDeltaTime;
                Debug.Log("Tiempo restante: " + caughtTimer);

                if (caughtTimer <= 0)
                {
                    script_sceneController.LoadLoose();
                    return;
                }

                if (DistTo(player.position) > catchDistance * 1.5f)
                {
                    ChangeState(State.CHASE);
                }
                break;
        }
        // Aplica el movimiento calculado segun su estado 
        Move(steering);
    }

    /// <summary>
    /// Cambia el estado del policía.
    /// Reinicia los temporizadores necesarios dependiendo del nuevo estado.
    /// </summary>
    private void ChangeState(State newState)
    {    //evita cambiar al mismo estado.
        if (state == newState) return;
        state = newState;
        if (state == State.CHASE) timer = loseTime;
        if (state == State.CATCH) caughtTimer = caughtTime;
        Debug.Log("Nuevo estado Police2: " + state);
    }

    /// <summary>
    /// Comprueba si el policía puede ver al jugador
    /// verifica distancia,angulo de vision y para los obstaculos
    /// 
    /// si no hay jugador,no hay nada que detectar
    /// obtengo la direccion hacia al jugador 
    /// calculo la distancia de el
    /// compruebo si esta fuera del rango de su vision
    /// 
    /// compruebo lanzando un rayo laser (linecast)para si una pared bloquea su vision
    /// </summary>
    private bool ICanSeeThePlayerAndWalls()
    {
        if (player == null) return false;
        Vector3 direction = Flat(player.position - transform.position);
        float distance = direction.magnitude;
        if (distance > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, direction.normalized);

        if (angle > fov / 2f) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;

        if (Physics.Linecast(origin, target, sightBlockMask)) return false;
        return true;
    }
}
