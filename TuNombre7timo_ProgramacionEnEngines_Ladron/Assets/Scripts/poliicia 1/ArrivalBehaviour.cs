
using UnityEngine;

public abstract class ArrivalBehaviour : SeekBehaviour
{
    [Header("Arrival")]
    [SerializeField] protected float slowingRadius = 2f;

    protected Vector3 Arrive(Vector3 target)
    {
        Vector3 offset = Flat(target - transform.position);
        float distance = offset.magnitude;

        if (distance < 0.001f)
            return -velocity;

        float speed = maxSpeed;

        if (distance < slowingRadius)
        {
            speed = maxSpeed * distance
                    / Mathf.Max(slowingRadius, 0.01f);
        }

        Vector3 desired = offset.normalized * speed;

        return desired - velocity;
    }
}
