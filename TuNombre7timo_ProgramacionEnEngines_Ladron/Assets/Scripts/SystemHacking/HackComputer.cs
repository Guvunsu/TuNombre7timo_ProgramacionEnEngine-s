using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class HackComputer : MonoBehaviour
{
    private enum HackEffect
    {
        RemoveNode,
        DuplicateNode,
        MoveNodeEarlier,
        MoveNodeLater,
        ReversePath
    }

    [Header("Referencias")]
    [SerializeField] private PlayerController player;
    [SerializeField] private HackPanelUI panel;
    [SerializeField] private PathFollowingBehaviour target;
    [SerializeField] private GameObject promptObject;

    [Header("Interaccion")]
    [SerializeField] private float interactRange = 2f;

    [Header("Pregunta de hackeo")]
    [SerializeField, TextArea]
    private string question = "Escribir pregunta";

    [SerializeField]
    private string correctAnswer = "respuesta";

    [Header("Efecto al hackear")]
    [SerializeField]
    private HackEffect effect = HackEffect.RemoveNode;

    private InputAction hackAction;
    private bool hacked;
    private bool promptShown;

    private void Awake()
    {
        hackAction = new InputAction("Hack", InputActionType.Button);

        hackAction.AddBinding("<Keyboard>/f");
        hackAction.AddBinding("<Mouse>/rightButton");
        hackAction.AddBinding("<Gamepad>/buttonWest");
    }

    private void OnEnable()
    {
        hackAction?.Enable();
    }

    private void OnDisable()
    {
        hackAction?.Disable();

        if (panel != null)
        {
            panel.Hacked -= OnHacked;
            panel.Closed -= OnClosed;
        }

        if (panel != null && panel.IsOpen)
            panel.Close();

        SetPrompt(false);
    }

    private void OnDestroy()
    {
        hackAction?.Dispose();
    }

    private void Update()
    {
        if (player == null || panel == null || target == null)
            return;

        Vector3 offset = player.transform.position - transform.position;
        offset.y = 0f;

        bool inRange =
            offset.sqrMagnitude <= interactRange * interactRange;

        bool canInteract = inRange && !panel.IsOpen;

        SetPrompt(canInteract);

        if (canInteract && hackAction.WasPressedThisFrame())
            OpenPanel();
    }

    private void OpenPanel()
    {
        if (hacked && panel.IsOpen)
            return;

        LockPlayer(true);

        panel.Hacked -= OnHacked;
        panel.Closed -= OnClosed;

        panel.Hacked += OnHacked;
        panel.Closed += OnClosed;

        panel.Open(question, correctAnswer, hacked);
    }

    private void OnClosed()
    {
        panel.Hacked -= OnHacked;
        panel.Closed -= OnClosed;

        LockPlayer(false);
    }

    private void OnHacked()
    {
        if (hacked)
            return;

        hacked = true;
        ApplyEffect();
    }

    private void ApplyEffect()
    {
        if (target == null || target.Path == null)
            return;

        LinkedListNode<Transform> node = target.CurrentNode;

        switch (effect)
        {
            case HackEffect.RemoveNode:
                if (node != null)
                    target.RemoveNode(node);
                break;

            case HackEffect.DuplicateNode:
                if (node != null)
                    target.DuplicateNode(node);
                break;

            case HackEffect.MoveNodeEarlier:
                if (node != null)
                    target.MoveNodeEarlier(node);
                break;

            case HackEffect.MoveNodeLater:
                if (node != null)
                    target.MoveNodeLater(node);
                break;

            case HackEffect.ReversePath:
                target.ReversePath();
                break;
        }
    }

    private void LockPlayer(bool locked)
    {
        if (player == null)
            return;

        player.enabled = !locked;

        Rigidbody body = player.GetComponent<Rigidbody>();

        if (locked && body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    private void SetPrompt(bool show)
    {
        if (promptObject == null || show == promptShown)
            return;

        promptShown = show;
        promptObject.SetActive(show);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        if (target != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }
}