using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    #region Scripts
    public PlayerComprobation script_playerComprobation;
    public PlayerShoot script_playerShoot;
    #endregion

    #region Enum
    public enum PlayerMoveState { MOVE, IDLE }
    public PlayerMoveState enumPlayer;
    #endregion

    #region Variables
    [SerializeField] float speedMovementPlayer;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    #endregion

    #region Input System
    private PlayerInput playerInput;
    private InputAction moveAction;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
    }

    void OnEnable()
    {
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
    }

    void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
    }

    void Start()
    {
        enumPlayer = PlayerMoveState.IDLE;
    }

    void Update()
    {
        // Si está muerto, no hay mov.
        if (!script_playerComprobation.isAlive) return;

        // Cambiar de estado según si hay input o no
        enumPlayer = moveInput.sqrMagnitude > 0.01f ? PlayerMoveState.MOVE: PlayerMoveState.IDLE;
 
        switch (enumPlayer)
        {
            case PlayerMoveState.MOVE:
                PlayerMoveAction();
                break;
            case PlayerMoveState.IDLE:
                PlayerIdle();
                break;
        }
    }

    #region Input Callbacks
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    #endregion

    #region Idle/Move
    public void PlayerMoveAction()
    {
        rb.linearVelocity = moveInput * speedMovementPlayer;
    }

    public void PlayerIdle()
    {
        rb.linearVelocity = Vector2.zero;
    }
    #endregion
}