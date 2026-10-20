
using System.Collections.Generic;
using UnityEngine;

public abstract class PathFollowingBehaviour
    : CollisionAvoidanceBehaviour
{
    [Header("Path Following")]
    [SerializeField] protected float nodeRadius = 1f;

    protected LinkedList<Transform> path = new LinkedList<Transform>();
    protected LinkedListNode<Transform> currentNode;
    private bool forward = true;

    protected void AddNode(Transform node)
    {
        if (node == null) return;
        path.AddLast(node);
        if (currentNode == null)
            currentNode = path.First;
    }

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
        return HasPath ? Seek(CurrentNodePos) : Vector3.zero;
    }

    protected bool ReachedNode()
    {
        return HasPath && DistTo(CurrentNodePos) <= nodeRadius;
    }

    protected void AdvanceNode()
    {
        if (path.Count < 2 || currentNode == null) return;

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

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!Application.isPlaying || path == null)
            return;

        for (LinkedListNode<Transform> n = path.First;
             n != null;
             n = n.Next)
        {
            if (n.Value == null) continue;

            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(n.Value.position, nodeRadius);

            if (n.Next != null && n.Next.Value != null)
            {
                Gizmos.color = Color.magenta;

                Gizmos.DrawLine(n.Value.position,n.Next.Value.position);
            }
        }

        if (HasPath)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawSphere(CurrentNodePos, 0.15f);
        }
    }
}
