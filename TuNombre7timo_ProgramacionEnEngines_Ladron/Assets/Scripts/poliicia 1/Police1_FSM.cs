
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

    [SerializeField] private State state = State.PATROL;

    [Header("Puntos de patrulla")]
    [SerializeField] private Transform node1;
    [SerializeField] private Transform node2;
    [SerializeField] private Transform node3;
    [SerializeField] private Transform node4;
    [SerializeField] private Transform node5;
    [SerializeField] private Transform node6;

    [Header("Jugador")]
    [SerializeField] private float idleTime = 2f;
    [SerializeField] private Transform player;
    [SerializeField] private float sightRange = 6f;
    [SerializeField] private float fov = 90f;
    [SerializeField] private float catchDistance = 1.4f;
    [SerializeField] private float loseTime = 2f;
    [SerializeField] private LayerMask sightBlockMask;

    [Header("Derrota")]
    [SerializeField] private SceneController script_sceneController;
    [SerializeField] private float caughtTime = 3f;

    [Header("Tiemppo")]
    [SerializeField] private float timer;
    [SerializeField] private float caughtTimer;
    private Vector3 lastSeenPos;

    /// <summary>
    /// Inicializa el agente.
    /// Ejecuta el Awake de la clase base(syeering behaviour).
    /// Agrega los puntos de patrulla a la ruta(PathFollowingBehaviour.AddNode()).
    /// Guarda la posición inicial.
    /// Inicializa el temporizador de idle para esperar.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        AddNode(node1);
        AddNode(node2);
        AddNode(node3);
        AddNode(node4);
        AddNode(node5);
        AddNode(node6);
        lastSeenPos = transform.position;
        timer = idleTime;
    }
    /// <summary>
    /// actualiza la fsm
    /// </summary>
    private void FixedUpdate()
    {
        //si no hay ruta o jugador se sale
        //compruebo si el jugador es visible para el policia
        //guardo la ultima pos donde el poli viu al jugador
        //e inicio una fuerza de frenado por el efecto de steering de SteeringBehaviour
        if (!HasPath || player == null) return;
        bool seesPlayer = ICanSeeThePlayerAndWalls();
        if (seesPlayer) lastSeenPos = player.position;
        Vector3 steering = Brake();


        switch (state)
        {
            //me muevo al punto actual, si veo al jugador cambio el estado de persecucion,
            //y si llego al punto de patrullaje se detiene
            case State.PATROL:
                steering = Arrive(CurrentNodePos);
                if (seesPlayer) ChangeState(State.CHASE);
                else if (ReachedNode()) ChangeState(State.IDLE);
                break;
            //no aplico movimiento, voy restando el tiempo acumulativamente de espera
            //si vio al jugador cambio al estado de persecucion,
            //y si el tiempo es menor o igual cero avanza al siguiente nodo y pasa al estado de patrullaje
            case State.IDLE:
                steering = Vector3.zero;
                timer -= Time.fixedDeltaTime;
                if (seesPlayer) ChangeState(State.CHASE);
                else if (timer <= 0)
                {
                    AdvanceNode();
                    ChangeState(State.PATROL);
                }
                break;
            //si vio el jugador reinicia el tiempo para perder la vista al jugador,
            //lo persigue al jugador si esta cerca su pos

            //si la distancia del jugador objetivo es menor o igual al alcance de capturarlo pasa al estado de captura

            //si no, el tiempo del timer se va acumulando hacia atras tomando en consideracion que va hacia la ultima posicion conocida suavemente
            //si el tiempo se acaba vuelve a patrullar
            case State.CHASE:

                if (seesPlayer)
                {
                    timer = loseTime;
                    steering = Seek(player.position);

                    if (DistTo(player.position) <= catchDistance)
                        ChangeState(State.CATCH);
                }
                else
                {
                    timer -= Time.fixedDeltaTime;
                    steering = Arrive(lastSeenPos);
                    if (timer <= 0) ChangeState(State.COME_BACK);
                }
                break;
            //si la direccion del policia esta junto al jugador, cuenta regresiva para capturarlo,
            //si es cero el tiempo cargo la escena de derrota
            //si mi jugador se aleja de mi distancai de captura, vuelve al estado de persecusion
            case State.CATCH:
                steering = Arrive(player.position);

                caughtTimer -= Time.fixedDeltaTime;
                Debug.Log("Tiempo restante: " + caughtTimer);

                if (caughtTimer <= 0)
                {
                    script_sceneController.LoadLoose();
                    return;
                }
                if (DistTo(player.position) > catchDistance) ChangeState(State.CHASE);
                break;
            //regreso a mi ruta de patrullaje calculando la direccion del objetivo
            //si vi al jugador paso al estado de persecucion
            //y si llegue al nodo, me paso al idle
            case State.COME_BACK:
                steering = Arrive(CurrentNodePos);

                if (seesPlayer) ChangeState(State.CHASE);
                else if (ReachedNode()) ChangeState(State.IDLE);
                break;
        }
        // Aplica el movimiento calculado segun su estado 
        Move(steering);
    }

    /// <summary>
    /// Cambia el estado del policía.
    /// reinicio los temporizadores necesarios según el nuevo necesitado 
    /// </summary>
    private void ChangeState(State newState)
    {
        //evito cambiar al mismo estado
        if (state == newState) return;

        state = newState;
        if (state == State.IDLE) timer = idleTime;
        if (state == State.CHASE) timer = loseTime;
        if (state == State.CATCH) caughtTimer = caughtTime;
        Debug.Log("Nuevo estado Police1: " + state);
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
