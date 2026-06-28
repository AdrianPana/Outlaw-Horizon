using StarterAssets;
using UnityEngine;

public class PlayerContext
{
    // Ground
    public bool isGrounded;
    public RaycastHit groundHit;
    public Vector3 groundNormal => groundHit.normal;
    public Rideable rideable;
    public Transform currentPlatform;

    // Ledge
    public bool onLedge;
    public RaycastHit ledgeHit;

    // Movement
    public float verticalVelocity;
    public float horizontalSpeed;
    public bool isSteppingUp;
    public Vector3 platformMovement = Vector3.zero;

    // Timers (ticked by PhysicsChecker)
    public float jumpTimeoutDelta;
    public float fallTimeoutDelta;
    public float coyoteTimeDelta;
    public float jumpBufferDelta;
    public bool bufferedJump;

    // Tuning — set from inspector via PlayerStateMachine
    public float jumpHeight;
    public float gravity;
    public float moveSpeed;
    public float rotationSmoothTime;
    public float speedChangeRate;
    public float jumpTimeout;
    public float jumpBufferTime;
    public float coyoteTime;
    public float fallTimeout;
    public float terminalVelocity;
    public float groundedOffset;
    public float groundedRadius;
    public float groundedCastDistance;
    public float ledgeCheckDistance;
    public float hangOffset;
    public float maxSlopeAngle;
    public float stepHeight;
    public float lowerDist;
    public float upperDist;
    public LayerMask groundLayers;

    // Component references
    public Rigidbody rb;
    public CapsuleCollider capsuleCollider;
    public Animator animator;
    public InputSystem_Actions inputActions;
    public Transform mainCamera;
    public Transform cinemachineCameraTarget;

    // Animation IDs (assigned once at start)
    public int animIDSpeed;
    public int animIDGrounded;
    public int animIDJump;
    public int animIDFreeFall;
    public int animIDMotionSpeed;
    public int animIDOnLedge;
}