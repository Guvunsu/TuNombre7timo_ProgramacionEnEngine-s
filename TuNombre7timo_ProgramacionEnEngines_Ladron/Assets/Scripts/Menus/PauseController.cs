using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class PauseController : MonoBehaviour
{
    [Header("Panel de pausa")]
    [SerializeField] private GameObject pausePanel;

    [Header("Botones")]
    [SerializeField] private Button continueButton;

    private InputAction pauseAction;
    private bool paused;

    private void Awake()
    {
        // Input System: Esc en el teclado y Start en el gamepad.
        // "start" es el botón Menu de Xbox y el botón Options de PlayStation.
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPausePressed;
        pauseAction.Disable();
    }

    private void OnDestroy()
    {
        pauseAction.Dispose();
    }

    private void Start()
    {
        // Empezar el juego sin pausa y con el panel escondido.
        Time.timeScale = 1f;
        pausePanel.SetActive(false);

        // Conectar los botones.
        continueButton.onClick.AddListener(Continue);
    }

    // Se llama al pulsar Esc o Start.
    private void OnPausePressed(InputAction.CallbackContext context)
    {
        if (paused) Continue();
        else Pause();
    }

    // Pausar el juego.
    public void Pause()
    {
        paused = true;
        pausePanel.SetActive(true);

        // Con timeScale en 0, Time.deltaTime vale 0 y todo se detiene.
        Time.timeScale = 0f;

        // Para que el gamepad pueda moverse por los botones del panel.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    // Continuar el juego.
    public void Continue()
    {
        paused = false;
        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}