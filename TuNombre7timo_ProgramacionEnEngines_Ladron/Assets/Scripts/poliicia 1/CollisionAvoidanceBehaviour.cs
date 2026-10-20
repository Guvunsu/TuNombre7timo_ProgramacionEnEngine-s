
using UnityEngine;

public abstract class CollisionAvoidanceBehaviour : ArrivalBehaviour
{
    [Header("Collision Avoidance")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float lookAhead = 1.5f;
    [SerializeField] private float agentRadius = 0.45f;
    [SerializeField] private float sideSensorAngle = 35f;
    [SerializeField] private float maxAvoidForce = 10f;

    protected override Vector3 GetAvoidance()
    {
        Vector3 heading = velocity.sqrMagnitude > 0.01f ? velocity.normalized : facing;

        if (heading.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 force = Vector3.zero;

        force += CastSensor(heading, lookAhead, true);
        force += CastSensor(Quaternion.Euler(0f, -sideSensorAngle, 0f) * heading, lookAhead * 0.6f, false);

        force += CastSensor(Quaternion.Euler(0f, sideSensorAngle, 0f) * heading, lookAhead * 0.6f, false);
        return Vector3.ClampMagnitude(force, maxAvoidForce);
    }

    private Vector3 CastSensor(Vector3 direction,float length,bool addLateral)
    {
        direction = Flat(direction).normalized;

        if (direction.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        if (!Physics.SphereCast(transform.position,agentRadius,direction,out RaycastHit hit,length,obstacleMask,QueryTriggerInteraction.Ignore))
        {
            return Vector3.zero;
        }

        float strength = 1f - Mathf.Clamp01(hit.distance / length);

        Vector3 normal = Flat(hit.normal).normalized;

        if (normal.sqrMagnitude < 0.0001f)
            normal = -direction;

        if (addLateral)
        {
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;

            if (Vector3.Dot(side, normal) < 0f)
                side = -side;
            normal = (normal + side).normalized;
        }
        return normal * maxAvoidForce * strength;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (!Application.isPlaying) return;

        Vector3 heading = velocity.sqrMagnitude > 0.01f ? velocity.normalized : facing;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position,transform.position + heading * lookAhead);

        Vector3 left = Quaternion.Euler(0f, -sideSensorAngle, 0f) * heading;
        Vector3 right = Quaternion.Euler(0f, sideSensorAngle, 0f) * heading;

        Gizmos.DrawLine(transform.position,transform.position + left * lookAhead * 0.6f);
        Gizmos.DrawLine(transform.position,transform.position + right * lookAhead * 0.6f);
    }
}
