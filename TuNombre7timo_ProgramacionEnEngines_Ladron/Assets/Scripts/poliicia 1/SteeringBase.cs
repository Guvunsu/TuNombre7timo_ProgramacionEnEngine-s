
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class SteeringBase : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] protected float maxSpeed = 3f;
    [SerializeField] protected float maxForce = 8f;
    [SerializeField] protected float mass = 1f;

    [Header("Orientacion")]
    [SerializeField] protected Vector3 startFacing = Vector3.forward;
    [SerializeField] protected float turnRate = 360f;
    [SerializeField] protected bool rotateBody = true;

    protected Vector3 velocity;
    protected Vector3 facing;
    protected Rigidbody rb;
    protected bool avoidanceEnabled = true;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints |= RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

        facing = Flat(startFacing);

        if (facing.sqrMagnitude < 0.0001f)
            facing = Vector3.forward;

        facing.Normalize();
        velocity = Flat(rb.linearVelocity);
    }

    protected virtual Vector3 GetAvoidance()
    {
        return Vector3.zero;
    }

    protected void Move(Vector3 steering)
    {
        if (rb == null) return;

        velocity = Flat(rb.linearVelocity);

        steering = Vector3.ClampMagnitude(
            Flat(steering), maxForce);

        if (avoidanceEnabled)
            steering += GetAvoidance();

        steering = Flat(steering) / Mathf.Max(mass, 0.01f);

        velocity = Vector3.ClampMagnitude(
            velocity + steering * Time.fixedDeltaTime,
            maxSpeed);

        rb.linearVelocity = velocity;
        rb.angularVelocity = Vector3.zero;

        UpdateFacing();
    }

    protected Vector3 Brake()
    {
        return -velocity;
    }

    private void UpdateFacing()
    {
        Vector3 desiredDirection =
            velocity.sqrMagnitude > 0.01f
            ? velocity.normalized
            : facing;

        facing = Vector3.RotateTowards(
            facing,
            desiredDirection,
            turnRate * Mathf.Deg2Rad * Time.fixedDeltaTime,
            0f
        ).normalized;

        if (rotateBody && facing.sqrMagnitude > 0.0001f)
        {
            rb.MoveRotation(
                Quaternion.LookRotation(
                    facing,
                    Vector3.up));
        }
    }

    protected float DistTo(Vector3 point)
    {
        return Flat(point - transform.position).magnitude;
    }

    protected static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    protected virtual void OnDrawGizmos() { }
}
