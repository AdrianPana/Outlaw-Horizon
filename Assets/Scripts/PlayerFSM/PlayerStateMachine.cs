using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10f;
    public float JumpHeight = 1.2f;
    public float Gravity = -15f;
    public float JumpTimeout = 0.5f;
    public float JumpBufferTime = 0.1f;
    public float CoyoteTime = 0.1f;
    public float FallTimeout = 0.15f;
    public float TerminalVelocity = 10f;
    public float ExternalMomentumDecayRate = 2f;

    [Header("Hover Spring")]
    public float SpringStiffness = 200f;
    public float SpringDamping = 20f;

    [Header("Ground Check")]
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public float GroundedCastDistance = 0.8f;
    public LayerMask GroundLayers;

    [Header("Ledge")]
    public float LedgeCheckDistance = 0.5f;
    public float HangOffset = 1.0f;
    public float LedgeJumpModifier = 1.5f;

    [Header("Slope / Step")]
    public float MaxSlopeAngle = 45f;
    public float StepHeight = 0.3f;
    public float LowerDist = 0.1f;
    public float UpperDist = 0.2f;

    [Header("Camera")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 70f;
    public float BottomClamp = -30f;
    public float CameraAngleOverride = 0f;
    public bool LockCameraPosition = false;
    public float LookSensitivity = 0.5f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;


    private PlayerContext ctx;
    private PhysicsChecker physicsChecker;
    private BaseState currentState;

    private void Awake()
    {
        ctx = new PlayerContext();

        // Copy inspector values into context
        ctx.jumpHeight = JumpHeight;
        ctx.gravity = Gravity;
        ctx.moveSpeed = MoveSpeed;
        ctx.rotationSmoothTime = RotationSmoothTime;
        ctx.speedChangeRate = SpeedChangeRate;
        ctx.jumpTimeout = JumpTimeout;
        ctx.jumpBufferTime = JumpBufferTime;
        ctx.coyoteTime = CoyoteTime;
        ctx.fallTimeout = FallTimeout;
        ctx.terminalVelocity = TerminalVelocity;
        ctx.externalVelocityDecayRate = ExternalMomentumDecayRate;
        ctx.springStiffness = SpringStiffness;
        ctx.springDamping = SpringDamping;
        ctx.groundedOffset = GroundedOffset;
        ctx.groundedRadius = GroundedRadius;
        ctx.groundedCastDistance = GroundedCastDistance;
        ctx.groundLayers = GroundLayers;
        ctx.ledgeCheckDistance = LedgeCheckDistance;
        ctx.hangOffset = HangOffset;
        ctx.ledgeJumpModifier = LedgeJumpModifier;
        ctx.maxSlopeAngle = MaxSlopeAngle;
        ctx.stepHeight = StepHeight;
        ctx.lowerStepCastDist = LowerDist;
        ctx.upperStepCastDist = UpperDist;

        // Grab components
        ctx.rb = GetComponent<Rigidbody>();
        //ctx.rb.isKinematic = true;
        ctx.capsuleCollider = GetComponent<CapsuleCollider>();
        ctx.animator = GetComponent<Animator>();
        ctx.inputActions = new InputSystem_Actions();
        ctx.inputActions.Enable();
        ctx.mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        ctx.cinemachineCameraTarget = CinemachineCameraTarget.transform;

        // Animation IDs
        ctx.animIDSpeed = Animator.StringToHash("Speed");
        ctx.animIDGrounded = Animator.StringToHash("Grounded");
        ctx.animIDJump = Animator.StringToHash("Jump");
        ctx.animIDFreeFall = Animator.StringToHash("FreeFall");
        ctx.animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        ctx.animIDOnLedge = Animator.StringToHash("OnLedge");

        ctx.jumpTimeoutDelta = JumpTimeout;
        ctx.fallTimeoutDelta = FallTimeout;

        _cinemachineTargetYaw = ctx.cinemachineCameraTarget.rotation.eulerAngles.y;

        physicsChecker = new PhysicsChecker(ctx);

        TransitionTo(new GroundedState(ctx, this));
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        if (physicsChecker == null || currentState == null) return;
        ctx.input = GatherInput();
        physicsChecker.Tick(Time.fixedDeltaTime);
        currentState.Tick(Time.fixedDeltaTime);
    }

    private InputSnapshot GatherInput()
    {
        var player = ctx.inputActions.Player;
        return new InputSnapshot
        {
            move = player.Move.ReadValue<Vector2>(),
            jumpPressed = player.Jump.IsPressed(),
        };
    }

    public void TransitionTo(BaseState next)
    {
        currentState?.Exit();
        Debug.Log($"Transitioning from {currentState?.GetType().Name ?? "null"} to {next.GetType().Name}");
        currentState = next;
        currentState.Enter();
    }

    private void LateUpdate()
    {
        if (ctx == null || ctx.cinemachineCameraTarget == null) return;
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        Vector2 lookInput = ctx.inputActions.Player.Look.ReadValue<Vector2>();

        if (lookInput.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            _cinemachineTargetYaw += lookInput.x * LookSensitivity;
            _cinemachineTargetPitch += -lookInput.y * LookSensitivity;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        ctx.cinemachineCameraTarget.rotation = Quaternion.Euler(
            _cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw,
            0f);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}