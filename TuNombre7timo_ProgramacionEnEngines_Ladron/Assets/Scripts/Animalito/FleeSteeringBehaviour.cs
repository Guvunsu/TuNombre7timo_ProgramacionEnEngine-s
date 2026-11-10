using UnityEngine;
public abstract class FleeSteeringBehaviour : Police2CollisionAvoidance
{
    [Header("Flee anti-atoramiento (esquinas)")]
    [SerializeField] private float stuckSpeed = 0.25f;   //Velocidad mínima para considerar que está avanzando
    [SerializeField] private float stuckTime = 0.5f;     //Tiempo que debe permanecer lento para detectar atoramiento.
    [SerializeField] private float escapeDuration = 0.6f;//Duración del movimiento de escape lateral.

    private float stuckTimer;       //Cuenta cuánto tiempo lleva atorado
    private float escapeTimer;      //Controla cuánto dura el escape.
    private Vector3 escapeDir;      //Guarda la dirección lateral de escape.

    //calcula la direccion contraria a la amenaza para que el npc pueda escapar
    protected Vector3 Flee(Vector3 threat)
    {
        float dt = Time.fixedDeltaTime;

        // calcula el vector que apunta desde la amenaza hacia el npc 
        Vector3 away = Flat(transform.position - threat);

        if (away.sqrMagnitude < 0.0001f)      
            away = Flat(facing);

        if (away.sqrMagnitude < 0.0001f)
            away = Vector3.forward;

        away.Normalize();

        //corre perpendicular a la huida para salir de la esquina
        if (escapeTimer > 0f)
        {
            escapeTimer -= dt;
            return SteerToward(escapeDir);
        }

        // seguridad por si me atoro
        if (velocity.magnitude < stuckSpeed) stuckTimer += dt;
        else stuckTimer = 0f;

        if (stuckTimer >= stuckTime)
        {
            stuckTimer = 0f;
            escapeTimer = escapeDuration;

            //escape, si detecto que esta atorado, elige random un lado y corre perpendicular
            escapeDir = Vector3.Cross(Vector3.up, away).normalized * (Random.value < 0.5f ? -1f : 1f);
            return SteerToward(escapeDir);
        }

        //flee normal
        return SteerToward(away);
    }

    //calcula la fuerza de mov. restando la velocidad actual a la velocidad deseada (desired-velocit)
    private Vector3 SteerToward(Vector3 direction)
    {
        Vector3 desired = Flat(direction).normalized * maxSpeed;
        return desired - velocity;
    }

    // Reinicia temporizadores cuando el npc cambia de estado
    protected void ResetFlee()
    {
        stuckTimer = 0f;
        escapeTimer = 0f;
    }
}