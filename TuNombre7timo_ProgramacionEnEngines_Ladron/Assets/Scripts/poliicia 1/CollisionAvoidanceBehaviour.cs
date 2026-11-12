using UnityEngine;
public abstract class CollisionAvoidanceBehaviour : ArrivalBehaviour
{
    [Header("Collision Avoidance")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float lookAhead = 1.5f;
    [SerializeField] private float agentRadius = 0.45f;
    [SerializeField] private float maxAvoidForce = 10f;

    /// <summary>
    /// Calcula la fuerza de evasion
    /// si esta quieto no evita nada
    /// obtiene la direccion del movimiento del policia
    /// lanza un spherecast para detectar si hay un obstáculo en la direccion en la que se mueve el agente.
    /// si encuentra un obstaculo se aleja del obstáculo.
    /// se desliza lateralmente por la pared para no quedarse pegado a esta
    /// devuelve la fuerza de evasion
    /// Si no hay obstáculos, devuelve cero su fuerza de evasion
    /// </summary>
    protected override Vector3 GetAvoidance()
    {
        if (velocity.sqrMagnitude < 0.01f) return Vector3.zero;
        Vector3 direction = velocity.normalized;

        RaycastHit hit;
        if (Physics.SphereCast(
            transform.position,
            agentRadius,
            direction,
            out hit,
            lookAhead,
            obstacleMask))
        {
            Vector3 away = Flat(hit.normal).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, direction);
            //Comprueba si la dirección lateral apunta hacia el lado correcto para esquivar el obstáculo.
            //Si apunta al lado contrario, la invierte.
            if (Vector3.Dot(side, away) < 0f) side = -side;
            return (away + side).normalized * maxAvoidForce;
        }
        return Vector3.zero;
    }
}