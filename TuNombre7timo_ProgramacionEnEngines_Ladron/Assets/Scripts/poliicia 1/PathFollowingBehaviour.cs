
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
    /// si es el primer nodo, lo establece como actual (primero)
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
    /// si hay ruta, persigo el nodo actual usando seek con su pos actual y del objetivo,
    /// si no hay ruta, no hagas nada"
    /// </summary>
    protected Vector3 FollowPath()
    {
        if (HasPath) return Seek(CurrentNodePos);
        else return Vector3.zero;
    }

    /// <summary>
    /// Comprueba si llegó al nodo actual.
    /// regresa y verifica que si hay ruta y si la distancia es menor o igual que al radio permitido ( para detectarlo)
    /// </summary>
    protected bool ReachedNode()
    {
        return HasPath && DistTo(CurrentNodePos) <= nodeRadius;
    }

    /// <summary>
    /// Pasar al siguiente nodo.
    /// se sale si no existe un nodo actual
    /// si hay un nodo existente adelante del actual, si es verdadero avanza a ese siguiente
    /// si no hay siguiente,vuelve al primero 
    /// </summary>
    protected void AdvanceNode()
    {
        if (currentNode == null) return;

        if (currentNode.Next != null)
        {
            currentNode = currentNode.Next;
        }  
        else  currentNode = path.First;
    }
}