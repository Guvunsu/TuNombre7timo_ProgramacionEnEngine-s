using UnityEngine;

public class PathFollowing : MonoBehaviour
{
    [Header("Configuracion de Agente")]
    [SerializeField] private float maxSpeeds = 5f;
    [SerializeField] private float maxForces = 8f;   // límite de la fuerza de seguimiento (aceleración)
    [SerializeField] private float masss = 1f;

    [Header("Configuracion de la ruta (Path Following)")]
    [SerializeField] private Transform[] pathNodes; // nodos de la ruta, en orden
    [SerializeField] private float pathRadius = 0.5f; // "ancho" de la ruta: a esta distancia el nodo cuenta como alcanzado
    [SerializeField] private bool backAndForth = true; // true = patrulla de ida y vuelta

    [Header("configuracion de Arrival")]
    [SerializeField] private float slowingRadiuss = 3f; // solo se usa en el último nodo si backAndForth está desactivado

    [Header("Configurtacion de la evacion")]
    [SerializeField] Transform[] obstacless;
    [SerializeField] float obstaculeRadiuss = 1f;
    [SerializeField] float maxSeeAheads = 2f;
    [SerializeField] float maxAvoidForces = 3f;

    private Vector2 velocitys;
    private int currentNode = 0;   // nodo que estamos buscando
    private int pathDir = 1;       // 1 = hacia el final, -1 = hacia el inicio
    private Vector2 aheadGizmos, ahead2Gizmos;

    void Update()
    {
        if (pathNodes == null || pathNodes.Length == 0) return;

        Vector2 currentPos = transform.position;

        // 1) El target es el nodo actual de la ruta
        Vector2 targetPos = pathNodes[currentNode].position;
        Vector2 targetOffset = targetPos - currentPos;
        float distance = targetOffset.magnitude;

        // 2) ¿Estamos en el último nodo y no hay ida y vuelta? Entonces nos detenemos ahí (arrival)
        bool stopAtEnd = !backAndForth && currentNode == pathNodes.Length - 1;

        // 3) Si llegamos al nodo (distancia <= radio de la ruta), pasamos al siguiente
        if (!stopAtEnd && distance <= pathRadius)
        {
            currentNode += pathDir;

            // ¿Nos salimos de la lista de nodos?
            if (currentNode >= pathNodes.Length || currentNode < 0)
            {
                if (backAndForth)
                {
                    pathDir *= -1;          // invertimos la dirección
                    currentNode += pathDir; // y volvemos a un índice válido
                } else
                {
                    currentNode = pathNodes.Length - 1; // nos quedamos en el último nodo
                }
            }

            // El nodo cambió: recalculamos el target
            targetPos = pathNodes[currentNode].position;
            targetOffset = targetPos - currentPos;
            distance = targetOffset.magnitude;
        }

        // 4) SEEK al nodo actual: velocidad deseada apuntando al target
        Vector2 desiredVelocity;
        if (stopAtEnd && distance < slowingRadiuss)
        {
            // ARRIVAL: frenamos al acercarnos al último nodo
            desiredVelocity = targetOffset.normalized * maxSpeeds * (distance / slowingRadiuss);
        } else
        {
            desiredVelocity = targetOffset.normalized * maxSpeeds;
        }

        // 5) Fuerza de seguimiento = deseada - actual, limitada por separado
        Vector2 pathForce = desiredVelocity - velocitys;
        pathForce = Vector2.ClampMagnitude(pathForce, maxForces);

        // 6) Fuerza de evasión (vector cero si no hay obstáculo bloqueando)
        Vector2 avoidForce = GetCollisionAvoidances();

        // 7) Sumar fuerzas, limitar el total y aplicar la masa
        Vector2 steering = pathForce + avoidForce;
        steering = Vector2.ClampMagnitude(steering, maxForces + maxAvoidForces);
        steering /= masss;

        // 8) Actualizar velocidad y mover (la fuerza es una aceleración, por eso * deltaTime)
        velocitys = Vector2.ClampMagnitude(velocitys + steering * Time.deltaTime, maxSpeeds);
        transform.position += (Vector3)(velocitys * Time.deltaTime);
    }

    private Vector2 GetCollisionAvoidances()
    {
        // Longitud dinámica (0 a 1): si estamos detenidos, no proyectamos nada hacia adelante
        float dynamicLength = (maxSpeeds > 0) ? velocitys.magnitude / maxSpeeds : 0f;

        Vector2 currentPos = transform.position;
        Vector2 ahead = currentPos + velocitys.normalized * maxSeeAheads * dynamicLength;
        Vector2 ahead2 = currentPos + velocitys.normalized * maxSeeAheads * dynamicLength * 0.5f;

        aheadGizmos = ahead;
        ahead2Gizmos = ahead2;

        Transform mostThreatening = FindMostThreatenings(ahead, ahead2);
        Vector2 avoidanceForce = Vector2.zero;

        if (mostThreatening != null)
        {
            // Del centro del obstáculo hacia la punta de ahead, normalizado y escalado
            avoidanceForce = ahead - (Vector2)mostThreatening.position;
            avoidanceForce = avoidanceForce.normalized * maxAvoidForces;
        }
        return avoidanceForce;
    }

    private Transform FindMostThreatenings(Vector2 ahead, Vector2 ahead2)
    {
        Transform mostThreatening = null;
        float distanceToMostThreatening = float.MaxValue;

        if (obstacless == null) return null;

        foreach (Transform t in obstacless)
        {
            if (t == null) continue;

            if (LineIntersectsCircle(ahead, ahead2, (Vector2)t.position, obstaculeRadiuss))
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
        // Colisión si ahead, ahead2 o la posición del agente están dentro del círculo
        return Vector2.Distance(obstacleCenter, ahead) <= radius
            || Vector2.Distance(obstacleCenter, ahead2) <= radius
            || Vector2.Distance(obstacleCenter, transform.position) <= radius;
    }

    void OnDrawGizmos()
    {
        // La ruta: nodos con su radio y líneas entre ellos
        if (pathNodes != null)
        {
            for (int i = 0; i < pathNodes.Length; i++)
            {
                if (pathNodes[i] == null) continue;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(pathNodes[i].position, pathRadius);

                if (i + 1 < pathNodes.Length && pathNodes[i + 1] != null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(pathNodes[i].position, pathNodes[i + 1].position);
                }
            }
        }

        // Obstáculos
        if (obstacless != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform o in obstacless)
                if (o != null) Gizmos.DrawWireSphere(o.position, obstaculeRadiuss);
        }

        // Línea de visión del agente (solo en Play)
        if (!Application.isPlaying) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, aheadGizmos);
        Gizmos.DrawSphere(aheadGizmos, 0.08f);
        Gizmos.DrawSphere(ahead2Gizmos, 0.08f);
    }
}