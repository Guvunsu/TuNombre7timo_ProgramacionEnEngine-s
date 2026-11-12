using UnityEngine;

public abstract class WanderSteeringBehaviour : CollisionAvoidanceBehaviour
{
    [Header("Wander")]
    [SerializeField] private float circleDistance = 2f;
    [SerializeField] private float circleRadius = 1.5f;
    [SerializeField] private float angleChange = 300f;

    private float wanderAngle;
    private float stuckTimer;

    // Caminar sin rumbo fijo.
    protected Vector3 Wander()
    {
        // Si lleva mucho tiempo casi parado, está atorado: da media vuelta.
        if (velocity.magnitude < 0.25f)
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        if (stuckTimer > 1f)
        {
            facing = -facing;
            stuckTimer = 0f;
        }

        // El círculo está enfrente del policía.
        Vector3 center = transform.position + facing * circleDistance;

        // Cambiar un poco el ángulo al azar.
        wanderAngle += Random.Range(-1f, 1f) * angleChange * Mathf.Deg2Rad * Time.fixedDeltaTime;

        // Punto sobre el círculo.
        Vector3 point = center + new Vector3(Mathf.Cos(wanderAngle), 0f, Mathf.Sin(wanderAngle)) * circleRadius;

        // Ir hacia ese punto.
        return Seek(point);
    }
}