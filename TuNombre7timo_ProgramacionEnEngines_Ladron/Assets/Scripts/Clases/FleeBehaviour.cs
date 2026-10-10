using UnityEngine;

public class FleeBehaviour : MonoBehaviour
{
    [Header("Agente conf")]
    [SerializeField] Transform threat;      // de quién huimos
    [SerializeField] float max_Speed = 5f;
    [SerializeField] float max_force = 0.1f;
    [SerializeField] float mass = 1f;

    private Vector3 velocity;

    void Update()
    {
        if (threat == null) return;

        // 1) Velocidad deseada: desired_velocity = normalize(position - threat) * max_speed
        //    (el vector va DESDE la amenaza HACIA el agente, es decir, al revés que seek)
        Vector3 desiredVelocity = (transform.position - threat.position).normalized * max_Speed;
        desiredVelocity.z = 0f; // top view en XY

        // 2) steering = desired_velocity - velocitys
        Vector3 steering = desiredVelocity - velocity;

        // 3) steering = truncate(steering, max_force)
        steering = Vector3.ClampMagnitude(steering, max_force);

        // 4) steering = steering / masss
        steering /= mass;

        // 5) velocitys = truncate(velocitys + steering, max_speed)
        velocity = Vector3.ClampMagnitude(velocity + steering, max_Speed);

        // 6) position = position + velocitys
        transform.position += velocity * Time.deltaTime;
    }

    void OnDrawGizmos()
    {
        if (threat == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + velocity); // velocidad actual
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, threat.position);               // línea hacia la amenaza
    }
}