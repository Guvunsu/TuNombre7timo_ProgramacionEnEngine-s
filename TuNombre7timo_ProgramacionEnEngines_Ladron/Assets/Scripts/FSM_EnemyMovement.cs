using UnityEngine;
using System.Collections.Generic;

public class FSM_EnemyMovement : MonoBehaviour
{
    public enum FSM_Enemy
    {
        PATROL, IDLE, CATCH_HIM, COME_BACK, ALERT
    }

    [Header("Referencias")]
    public FSM_Enemy fsm_Enemy;
    public Rigidbody body;
    public Transform player;

    [Header("Puntos del camino")]
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;
    public Transform point5;

    [Header("Listas/Listas ligadas")]
    LinkedList<Transform> pointsToVisit = new LinkedList<Transform>();
    LinkedListNode<Transform> currentNode;

    [Header("Variables")]
    [SerializeField] float patrolSpeed = 3.33f;
    [SerializeField] float chaseSpeed = 3.33f;
    [SerializeField] float arriveDistance = 0.2f;
    [SerializeField] float idleTimer = 3.33f;
    [SerializeField] float alertTimer = 1.5f;
    [SerializeField] float viewRange = 5f;
    [SerializeField] float catchRange = 1f;
    [SerializeField] float timer;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        LinkedListPointsToVisit();
        currentNode = pointsToVisit.First;
        fsm_Enemy = FSM_Enemy.PATROL;

    }
    void Update()
    {
        switch (fsm_Enemy)
        {
            case FSM_Enemy.PATROL:
                PatrolState();
                break;
            case FSM_Enemy.IDLE:
                IdleState();
                break;
            case FSM_Enemy.CATCH_HIM:
                CatchHimState();
                break;
            case FSM_Enemy.COME_BACK:
                ComeBackState();
                break;
            case FSM_Enemy.ALERT:
                AlertState();
                break;
        }
    }
    public void AlertState()
    {
        timer += Time.deltaTime;
        if (timer >= alertTimer)
        {
            if (CanSeePlayer())
            {
                fsm_Enemy = FSM_Enemy.CATCH_HIM;
            } else GoComeBack();
        }
    }
    public void ComeBackState()
    {
        if (CanSeePlayer())
        {
            fsm_Enemy = FSM_Enemy.ALERT;
            return;
        }

        MoveTo(currentNode.Value.position, patrolSpeed);

        if (Reached(currentNode.Value.position))
        {
            fsm_Enemy = FSM_Enemy.PATROL;
        }
    }
    public void CatchHimState()
    {
        if (!CanSeePlayer())
        {
            GoComeBack();
            return;
        }

        MoveTo(player.position, chaseSpeed);

        if (Vector3.Distance(transform.position, player.position) <= catchRange)
        {
            Debug.Log("Te atrape :3");
            //hacer referencia del script de gameOver
        }
    }
    public void IdleState()
    {
        if (CanSeePlayer())
        {
            timer = 0;
            fsm_Enemy = FSM_Enemy.ALERT;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= idleTimer)
        {
            currentNode = NextNode(currentNode);
            fsm_Enemy = FSM_Enemy.PATROL;
        }
    }
    public void PatrolState()
    {
        if (CanSeePlayer())
        {
            timer = 0;
            fsm_Enemy = FSM_Enemy.ALERT;
            return;
        }

        MoveTo(currentNode.Value.position, patrolSpeed);

        if (Reached(currentNode.Value.position))
        {
            timer = 0;
            fsm_Enemy = FSM_Enemy.IDLE;
        }
    }


    #region Auxiliares
    public void GoComeBack()
    {
        currentNode = NextNode(currentNode);
        fsm_Enemy = FSM_Enemy.COME_BACK;
    }
    public void MoveTo(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
    public bool CanSeePlayer()
    {
        if (player == null) return false;
        else return Vector3.Distance(transform.position, player.position) <= viewRange;
    }
    public bool Reached(Vector3 target)
    {
        return Vector3.Distance(transform.position, target) <= arriveDistance;
    }
    #endregion Auxiliares


    public void LinkedListPointsToVisit()
    {
        pointsToVisit.AddFirst(point1);
        pointsToVisit.AddLast(point2);
        pointsToVisit.AddLast(point3);
        pointsToVisit.AddLast(point4);
        pointsToVisit.AddLast(point5);
    }
    LinkedListNode<Transform> NextNode(LinkedListNode<Transform> node)
    {
        if (node.Next != null)
        {
            return node.Next;
        } else
        {
            return pointsToVisit.First;
        }
    }
}
