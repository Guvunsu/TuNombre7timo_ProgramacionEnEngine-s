using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class AgentBehaviour : MonoBehaviour
{
    [Header("Agente conf")]
    [SerializeField] Transform target; //posicion dle objeto que estamos buscando 
    [SerializeField] float max_Speed = 5f;
    [SerializeField] float max_force = 0.1f;
    [SerializeField] float mass = 1f;
    [SerializeField] float slowingRadius = 3f;

    private Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        Vector3 targetOffset = target.position - transform.position;

        float distance = targetOffset.magnitude;

        Vector3 desiredVelocity;

        if (distance < slowingRadius)
        {
            desiredVelocity = targetOffset.normalized * max_Speed * (distance / slowingRadius);
        } else
        {
            desiredVelocity = targetOffset.normalized * max_Speed;
        }

        Vector3 steering = desiredVelocity - velocity;
        //limitar la fuerza y aplicar la masa
        steering = Vector3.ClampMagnitude(steering, max_force);
        steering /= mass;
        //
        velocity = Vector3.ClampMagnitude(velocity + steering, max_Speed);

        transform.position += velocity * Time.deltaTime;
    }

    //hacer un pursuit 
    private void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = Color.softRed;
            Gizmos.DrawWireSphere(target.position, slowingRadius);
        }
    }
}
