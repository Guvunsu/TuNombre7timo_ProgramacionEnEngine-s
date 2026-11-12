
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private enum State
    {
        IDLE,
        MOVE
    }

    [SerializeField] private State state = State.IDLE;
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction collectAction;

    private Vector2 moveInput;

    // Objeto que el jugador está tocando.
    private Collectable currentCollectable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Movimiento con WASD.
        moveAction = new InputAction("Move", InputActionType.Value);

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Movimiento con flechas.
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        // Movimiento con Gamepad.
        moveAction.AddBinding("<Gamepad>/leftStick");
        moveAction.AddBinding("<Gamepad>/dpad");

        // Recoger objetos con E.
        collectAction = new InputAction("Collect", InputActionType.Button);
        collectAction.AddBinding("<Keyboard>/e");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        collectAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        collectAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        collectAction.Dispose();
    }

    private void Update()
    {
        // Leer el movimiento.
        moveInput = Vector2.ClampMagnitude(
            moveAction.ReadValue<Vector2>(), 1f
        );

        state = moveInput.sqrMagnitude > 0.01f
            ? State.MOVE
            : State.IDLE;

        // Recoger el objeto al presionar E.
        if (collectAction.WasPressedThisFrame() && currentCollectable != null)
        {
            currentCollectable.Collect();
            currentCollectable = null;
        }
    }

    private void FixedUpdate()
    {
        Vector3 direction = Vector3.zero;

        // Mover sobre los ejes X y Z.
        if (state == State.MOVE)
            direction = new Vector3(moveInput.x, 0f, moveInput.y);

        rb.linearVelocity = direction * moveSpeed;
        rb.angularVelocity = Vector3.zero;

        // Girar hacia donde se mueve.
        if (direction != Vector3.zero)
            rb.MoveRotation(Quaternion.LookRotation(direction));
    }

    // Detectar cuando toca un objeto.
    private void OnCollisionEnter(Collision collision)
    {
        currentCollectable = collision.collider.GetComponentInParent<Collectable>();
    }

    // Dejar de detectar el objeto cuando se aleja.
    private void OnCollisionExit(Collision collision)
    {
        Collectable collectable = collision.collider.GetComponentInParent<Collectable>();

        if (collectable == currentCollectable)
            currentCollectable = null;
    }
}
