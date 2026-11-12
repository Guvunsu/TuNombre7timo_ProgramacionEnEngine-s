using UnityEngine;
using UnityEngine.InputSystem;
public class HackComputer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerController player;
    [SerializeField] private HackPanelUI panel;
    [SerializeField] private GameObject promptObject;

    [Header("Interaccion")]
    [SerializeField] private float interactRange = 2f;

    [Header("Pregunta")]
    [SerializeField] private string question = "Escribe la respuesta";
    [SerializeField] private string correctAnswer = "respuesta";

    private InputAction hackAction;
    private bool hacked;

    private void Awake()
    {
        // F o X para abrir.
        hackAction = new InputAction("Hack", InputActionType.Button);
        hackAction.AddBinding("<Keyboard>/f");
        hackAction.AddBinding("<Gamepad>/buttonWest");
    }

    private void OnEnable()
    {
        hackAction.Enable();
    }

    private void OnDisable()
    {
        hackAction.Disable();

        if (panel != null)
        {
            panel.Hacked -= OnHacked;
            panel.Closed -= OnClosed;

            if (panel.IsOpen)
                panel.Close();
        }

        LockPlayer(false);

        if (promptObject != null)
            promptObject.SetActive(false);
    }

    private void OnDestroy()
    {
        hackAction.Dispose();
    }

    private void Update()
    {
        if (player == null || panel == null)
            return;

        if (panel.IsOpen)
            return;

        // Comprobar distancia con el jugador.
        float distance = Vector3.Distance(transform.position, player.transform.position);

        bool canInteract = distance <= interactRange && !hacked;

        if (promptObject != null)
            promptObject.SetActive(canInteract);

        // Abrir la computadora.
        if (canInteract && hackAction.WasPressedThisFrame())
            OpenPanel();
    }

    // Abrir el panel.
    private void OpenPanel()
    {
        LockPlayer(true);

        if (promptObject != null)
            promptObject.SetActive(false);

        panel.Hacked -= OnHacked;
        panel.Closed -= OnClosed;

        panel.Hacked += OnHacked;
        panel.Closed += OnClosed;

        panel.Open(question, correctAnswer, false);
    }

    // Respuesta correcta.
    private void OnHacked()
    {
        hacked = true;
    }

    // Cuando se cierra el panel.
    private void OnClosed()
    {
        LockPlayer(false);

        panel.Hacked -= OnHacked;
        panel.Closed -= OnClosed;
    }

    // Bloquear o permitir el movimiento.
    private void LockPlayer(bool locked)
    {
        if (player == null)
            return;

        player.enabled = !locked;

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (locked && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
