using UnityEngine;

public class PathFollowings : MonoBehaviour
{
    [Header("Configuracion de Agente")]
    [SerializeField] private Transform target;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxForce = 8f;   // límite de la fuerza de arrival (aceleración)
    [SerializeField] private float mass = 1f;

    [Header("configuracion de Arrival")]
    [SerializeField] private float slowingRadius = 3f;

    [Header("Configurtacion de la evacion")]
    [SerializeField] Transform[] obstacles;
    [SerializeField] float obstaculeRadius = 1f;
    [SerializeField] float maxSeeAhead = 2f;
    [SerializeField] float maxAvoidForce = 3f;

    private Vector2 velocity;                        // Vector2 porque todo el cálculo es 2D
    private Vector2 aheadGizmo, ahead2Gizmo;         // solo para dibujar los Gizmos

    void Update()
    {
        if (target == null) return;

        Vector2 currentPos = transform.position;
        Vector2 targetOffset = (Vector2)target.position - currentPos;
        float distance = targetOffset.magnitude;

        // ARRIVAL: velocidad deseada, reducida dentro del slowingRadiuss
        Vector2 desiredVelocity;
        if (distance < slowingRadius)
        {
            desiredVelocity = targetOffset.normalized * maxSpeed * (distance / slowingRadius);
        } else
        {
            desiredVelocity = targetOffset.normalized * maxSpeed;
        }

        // Fuerza de arrival = deseada - actual, limitada por separado
        Vector2 arrivalForce = desiredVelocity - velocity;
        arrivalForce = Vector2.ClampMagnitude(arrivalForce, maxForce);

        // Fuerza de evasión (vector cero si no hay obstáculo bloqueando)
        Vector2 avoidForce = GetCollisionAvoidance();

        // Sumar las fuerzas, limitar el total y aplicar la masa
        Vector2 steering = arrivalForce + avoidForce;
        steering = Vector2.ClampMagnitude(steering, maxForce + maxAvoidForce);
        steering /= mass;

        // Actualizar la velocidad (la fuerza es una aceleración, por eso * deltaTime)
        velocity = Vector2.ClampMagnitude(velocity + steering * Time.deltaTime, maxSpeed);

        // Mover al agente
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    private Vector2 GetCollisionAvoidance()
    {
        // Longitud dinámica (0 a 1): si estamos detenidos, no proyectamos nada hacia adelante
        float dynamicLength = (maxSpeed > 0) ? velocity.magnitude / maxSpeed : 0f;

        Vector2 currentPos = transform.position;
        Vector2 ahead = currentPos + velocity.normalized * maxSeeAhead * dynamicLength;
        Vector2 ahead2 = currentPos + velocity.normalized * maxSeeAhead * dynamicLength * 0.5f;

        // Guardamos los puntos para los Gizmos
        aheadGizmo = ahead;
        ahead2Gizmo = ahead2;

        Transform mostThreatening = FindMostThreatening(ahead, ahead2);
        Vector2 avoidanceForce = Vector2.zero;

        if (mostThreatening != null)
        {
            // Vector desde el centro del obstáculo hacia la punta de ahead, normalizado y escalado
            avoidanceForce = ahead - (Vector2)mostThreatening.position;
            avoidanceForce = avoidanceForce.normalized * maxAvoidForce;
        }
        return avoidanceForce;
    }

    private Transform FindMostThreatening(Vector2 ahead, Vector2 ahead2)
    {
        Transform mostThreatening = null;
        float distanceToMostThreatening = float.MaxValue;

        // Recorremos todos los obstáculos
        foreach (Transform t in obstacles)
        {
            if (t == null) continue;

            bool collision = LineIntersectsCircle(ahead, ahead2, (Vector2)t.position, obstaculeRadius);
            if (collision)
            {
                // De los que bloquean, nos quedamos con el más cercano al agente
                float distance = Vector2.Distance(transform.position, t.position);
                if (mostThreatening == null || distance < distanceToMostThreatening)
                {
                    mostThreatening = t;
                    distanceToMostThreatening = distance;
                }
            }
        }
        return mostThreatening;
    }

    private bool LineIntersectsCircle(Vector2 ahead, Vector2 ahead2, Vector2 obstacleCenter, float radius)
    {
        // Hay colisión si ahead, ahead2 o la posición del agente están dentro del círculo
        return Vector2.Distance(obstacleCenter, ahead) <= radius
            || Vector2.Distance(obstacleCenter, ahead2) <= radius
            || Vector2.Distance(obstacleCenter, transform.position) <= radius;
    }

    void OnDrawGizmos()
    {
        // Círculos de los obstáculos
        if (obstacles != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform o in obstacles)
                if (o != null) Gizmos.DrawWireSphere(o.position, obstaculeRadius);
        }

        // Línea de visión del agente (solo en Play)
        if (!Application.isPlaying) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, aheadGizmo);
        Gizmos.DrawSphere(aheadGizmo, 0.08f);
        Gizmos.DrawSphere(ahead2Gizmo, 0.08f);
    }
}