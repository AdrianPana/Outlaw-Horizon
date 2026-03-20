using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class CustomPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 10f;
    public float gravity = -25f;

    private Rigidbody rb;
    private Vector3 velocity;
    private bool grounded;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private PlayerInput playerInput;
    private Vector2 moveInput;
    private bool jumpInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Jump"].performed += ctx => jumpInput = true;
    }

    void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
        playerInput.actions["Jump"].performed -= ctx => jumpInput = true;
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        GroundCheck();
        HandleMovement();
        ApplyGravity();
        Move();
        jumpInput = false; // reset jump after FixedUpdate
    }

    void GroundCheck()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (grounded && velocity.y < 0)
            velocity.y = 0f;
    }

    void HandleMovement()
    {
        // Convert input to world-space direction
        Vector3 inputDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        inputDir.Normalize();

        Vector3 horizontalVel = new Vector3(inputDir.x, 0, inputDir.z) * moveSpeed;
        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        if (grounded && jumpInput)
        {
            velocity.y = jumpForce;
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.fixedDeltaTime;
    }

    void Move()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
}
