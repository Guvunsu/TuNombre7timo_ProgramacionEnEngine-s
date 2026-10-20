using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : PlayerPickupBehaviour
{
    private enum State { IDLE, MOVE, PICKUP }

    [Header("FSM")]
    [SerializeField] private State state = State.IDLE;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnRate = 720f;        
    [SerializeField] private float pickupTime = 0.3f;      

    private Rigidbody rb;
    private InputAction moveAction;

    private Vector2 moveInput;
    private float pickupTimer;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints |= RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

        // Mover: teclado (WASD y flechas) y gamepad
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        moveAction.AddBinding("<Gamepad>/leftStick");
        moveAction.AddBinding("<Gamepad>/dpad");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        moveAction.Enable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        moveAction.Disable();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        moveAction.Dispose();
    }

    private void Update()
    {
        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        bool hasInput = moveInput.sqrMagnitude > 0.01f;
        bool wantsPickup = WantsPickup();                     // viene de la clase padre

        switch (state)
        {
            case State.IDLE:
                if (wantsPickup && StartPickup()) break;
                if (hasInput) state = State.MOVE;
                break;

            case State.MOVE:
                if (wantsPickup && StartPickup()) break;
                if (!hasInput) state = State.IDLE;
                break;

            case State.PICKUP:
                pickupTimer -= Time.deltaTime;
                if (pickupTimer <= 0f) state = State.IDLE;
                break;
        }
    }

    private void FixedUpdate()
    {
        // Solo se mueve en el estado MOVE
        Vector3 direction = state == State.MOVE ? new Vector3(moveInput.x, 0f, moveInput.y): Vector3.zero;

        rb.linearVelocity = direction * moveSpeed;
        rb.angularVelocity = Vector3.zero;

        // Girar hacia donde se mueve
        if (direction.sqrMagnitude > 0.01f)
        {
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation,Quaternion.LookRotation(direction, Vector3.up),turnRate * Time.fixedDeltaTime));
        }
    }

    // Usar TryPickup() de la clase padre y, si recogió algo, entra al estado PICKUP
    private bool StartPickup()
    {
        if (!TryPickup()) return false;

        state = State.PICKUP;
        pickupTimer = pickupTime;
        return true;
    }
}