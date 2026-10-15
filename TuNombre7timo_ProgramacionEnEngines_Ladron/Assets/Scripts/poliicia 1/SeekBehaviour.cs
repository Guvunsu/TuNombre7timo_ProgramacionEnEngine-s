
using UnityEngine;

public abstract class SeekBehaviour : SteeringBase
{
    protected Vector3 Seek(Vector3 target)
    {
        Vector3 offset = Flat(target - transform.position);

        if (offset.sqrMagnitude < 0.0001f)
            return -velocity;

        Vector3 desired = offset.normalized * maxSpeed;

        return desired - velocity;
    }
}
