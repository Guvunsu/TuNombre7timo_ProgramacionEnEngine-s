using System.Collections.Generic;
using UnityEngine;
public abstract class PathFollowingBehaviour: CollisionAvoidanceBehaviour
{
    [Header("Path Following")]
    [SerializeField] protected float nodeRadius = 1f;

    //ruta del policía.
    protected LinkedList<Transform> path =  new LinkedList<Transform>();
    // Nodo al que se dirige actualmente
    protected LinkedListNode<Transform> currentNode;
    //recorre la ruta hacia delante o hacia atrás.
    private bool forward = true;

    //acceso a la lista para
    // Permite HackComputer.cs consulte la ruta
    public LinkedList<Transform> Path
    {
        get { return path; }
    }
    //Permite consultar el nodo actual.
    public LinkedListNode<Transform> CurrentNode
    {
        get { return currentNode; }
    }
    // Comprueba si un nodo es el nodo actual.
    public bool IsCurrentNode(LinkedListNode<Transform> node)
    {
        return node != null && node == currentNode;
    }

    //agregar nodos
    protected void AddNode(Transform node)
    {
        if (node == null)
            return;

        path.AddLast(node);

        if (currentNode == null)
            currentNode = path.First;
    }

    //seguimiento original
    protected bool HasPath
    {
        get
        {
            return currentNode != null && currentNode.Value != null;
        }
    }

    protected Vector3 CurrentNodePos
    {
        get
        {
            return HasPath ? currentNode.Value.position : transform.position;
        }
    }

    protected Vector3 FollowPath()
    {
        return HasPath ? Seek(CurrentNodePos): Vector3.zero;
    }

    protected bool ReachedNode()
    {
        return HasPath && DistTo(CurrentNodePos) <= nodeRadius;
    }

    protected void AdvanceNode()
    {
        if (path.Count < 2 || currentNode == null)
            return;

        if (forward)
        {
            if (currentNode.Next != null)
            {
                currentNode = currentNode.Next;
            } else
            {
                forward = false;
                currentNode = currentNode.Previous;
            }
        } else
        {
            if (currentNode.Previous != null)
            {
                currentNode = currentNode.Previous;
            } else
            {
                forward = true;
                currentNode = currentNode.Next;
            }
        }
    }

    //manipulacion de la lista
    // Elimina el nodo indicado.
    public void RemoveNode(LinkedListNode<Transform> node)
    {
        if (!ContainsNode(node)) return;

        // Si eliminamos el nodo actual, elegimos el siguiente
        // según la dirección de recorrido del policía.
        if (node == currentNode)
        {
            LinkedListNode<Transform> nextNode = forward ? node.Next ?? node.Previous : node.Previous ?? node.Next;
            currentNode = nextNode;
        }
        path.Remove(node);
        if (path.Count == 0)
            currentNode = null;
    }

    // Duplica el valor del nodo después de él.
    public void DuplicateNode(LinkedListNode<Transform> node)
    {
        if (!ContainsNode(node)) return;
        path.AddAfter(node, node.Value);
    }

    // mover el nodo al principio de la lista
    public void MoveNodeEarlier(LinkedListNode<Transform> node)
    {
        if (!ContainsNode(node) || node.Previous == null)
            return;

        LinkedListNode<Transform> previous = node.Previous;

        path.Remove(node);
        path.AddBefore(previous, node);
    }

    // mover el nodo al final de la lista
    public void MoveNodeLater(LinkedListNode<Transform> node)
    {
        if (!ContainsNode(node) || node.Next == null)
            return;

        LinkedListNode<Transform> next = node.Next;

        path.Remove(node);
        path.AddAfter(next, node);
    }

    //invertir los nodos de la lista
    public void ReversePath()
    {
        if (path.Count < 2)
            return;

        // guardar la posiciona ctual del nodo y su valores
        List<Transform> nodes = new List<Transform>(path);
        int currentIndex = 0;
        int index = 0;

        for (
            LinkedListNode<Transform> node = path.First;
            node != null;
            node = node.Next)
        {
            if (node == currentNode)
            {
                currentIndex = index;
                break;
            }
            index++;
        }

        nodes.Reverse();

        path.Clear();

        foreach (Transform node in nodes)
            path.AddLast(node);

        // Conservamos la posición equivalente en la ruta invertida.
        currentNode = path.First;

        int reversedIndex = nodes.Count - 1 - currentIndex;

        for (int i = 0; i < reversedIndex; i++)
            currentNode = currentNode.Next;
    }

    // Comprueba que el nodo pertenezca a esta lista.
    private bool ContainsNode(LinkedListNode<Transform> node)
    {
        return node != null && node.List == path;
    }

    //Gizmos 

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!Application.isPlaying || path == null)
            return;

        for (
            LinkedListNode<Transform> n = path.First;
            n != null;
            n = n.Next)
        {
            if (n.Value == null) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(n.Value.position, nodeRadius);

            if (n.Next != null && n.Next.Value != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine( n.Value.position,n.Next.Value.position);
            }
        }

        if (HasPath)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(CurrentNodePos, 0.15f);
        }
    }
}
