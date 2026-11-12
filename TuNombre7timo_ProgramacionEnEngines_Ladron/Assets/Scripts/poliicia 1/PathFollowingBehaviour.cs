
using System.Collections.Generic;
using UnityEngine;

public class PathFollowingBehaviour : CollisionAvoidanceBehaviour
{
    [SerializeField] private float nodeRadius = 1f;

    [Header("lista del camino que seguir el policia")]
    private LinkedList<Transform> path = new LinkedList<Transform>();

    [Header("nodo actual de la lista")]
    private LinkedListNode<Transform> currentNode;

    /// <summary>
    /// Agregar un punto a la ruta.
    /// evita agregar nodos nulos (es una seguridad)
    /// añade el nodo al final de la ruta
    /// si es el primer nodo, lo establece como actual
    /// </summary>
    public void AddNode(Transform node)
    {
        if (node == null) return;
        path.AddLast(node);
        if (currentNode == null) currentNode = path.First;
    }

    /// <summary>
    /// Comprueba si existe un nodo valido para seguir
    /// comrpueba si existe el nodo y su transform (value)
    /// </summary>
    protected bool HasPath
    {
        get { return currentNode != null && currentNode.Value != null; }
    }

    /// <summary>
    /// si existe ruta, devuelve la pos del nodo actual
    /// si no devuelve la pos actual del agente
    /// </summary>
    protected Vector3 CurrentNodePos
    {
        get
        {
            if (HasPath) return currentNode.Value.position;
            else return transform.position;
        }
    }

    /// <summary>
    /// Para seguir la ruta 
    /// Persigo el nodo actual usando seek si hay ruta, síguela, si no hay ruta, no hagas nada"
    /// </summary>
    protected Vector3 FollowPath()
    {
        if (HasPath) return Seek(CurrentNodePos);
        else return Vector3.zero;
    }

    /// <summary>
    /// Comprueba si llegó al nodo actual.
    /// verifica si la distancia es menor o igual al radio permitido
    /// </summary>
    protected bool ReachedNode()
    {
        return HasPath && DistTo(CurrentNodePos) <= nodeRadius;
    }

    /// <summary>
    /// Pasar al siguiente nodo.
    /// se sale si no existe un nodo actual
    /// avanza al siguiente nodo o vuelve al primero 
    /// </summary>
    protected void AdvanceNode()
    {
        if (currentNode == null) return;
        currentNode = currentNode.Next != null ? currentNode.Next : path.First;
    }
}