using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(Rigidbody))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        //[Tooltip("Sprint speed of the character in m/s")]
        //public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time allowed to pre press jump before hitting the ground to jump again")]
        public float JumpBufferTime = 0.1f;

        [Tooltip("Time allowed to jump after leaving a platform. Set to 0f to not allow jumping after leaving a platform")]
        public float CoyoteTime = 0.1f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("The distance the sphere cast from the origin to check if grounded")]
        public float GroundedCastDistance = 0.8f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Ledge Detection")]
        [Tooltip("Whether or not the player is currently on a ledge")]
        public bool OnLedge = false;

        [Tooltip("The distance the sphere cast from the origin to check for ledges when not grounded")]
        public float LedgeCheckDistance = 0.5f;

        [Tooltip("The offset so that the hands allign with the ledge")]
        public float HangOffset = 1.0f;

        [Header("Slope Handling")]
        [Tooltip("The maximum slope angle the character can walk up")]
        public float maxSlopeAngle;

        [Tooltip("Step height")]
        public float stepHeight;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Tooltip("Added when player sits on moving platform")]
        public Vector3 PlatformMovement = Vector3.zero;
        public Rideable _ridable;

        private RaycastHit _groundHit;
        private CapsuleCollider _capsuleCollider;


        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        public float _verticalVelocity;
        private float _terminalVelocity = 10.0f;
        private bool bufferedJump;
        private bool _onRamp = false;

        // timeout deltatime
        private float _jumpBufferingDelta;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _coyoteTimeDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDOnLedge;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Rigidbody rb;
        private Animator _animator;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        RaycastHit forwardHit;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _input = GetComponent<StarterAssetsInputs>();
            rb = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void FixedUpdate()
        {
            _hasAnimator = TryGetComponent(out _animator);

            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;

            GroundedCheck();
            LedgeCheck();
            JumpAndGravity();
            Move();
            SnapToGround();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDOnLedge = Animator.StringToHash("OnLedge");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            //Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            //    QueryTriggerInteraction.Ignore);

            if (Physics.SphereCast(spherePosition, GroundedRadius, Vector3.down, out _groundHit,
                GroundedOffset + GroundedCastDistance, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                Grounded = true;
                _ridable = _groundHit.collider.GetComponent<Rideable>();
            }
            else
            {
                Grounded = false;
                _ridable = null;
            }

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // (REMOVED) set target speed based on move speed, sprint speed and if sprint is pressed
            //float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            float targetSpeed = MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            //float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float currentHorizontalSpeed = new Vector3(rb.linearVelocity.x, 0.0f, rb.linearVelocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.fixedDeltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.fixedDeltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            Debug.Log("OnLedge: " + OnLedge);

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero && !OnLedge)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            // Input movement
            Vector3 inputMove = Vector3.zero;

            if (_input.move != Vector2.zero && !OnLedge)
            {
                Vector3 moveDirection = transform.forward * _speed;

                if (Grounded)
                {
                    _onRamp = false;
                    // CHECK FOR RAMP AHEAD
                    Vector3 forwardCastOrigin = transform.position + transform.forward * _capsuleCollider.radius * 2.0f + Vector3.up;
                    if (Physics.SphereCast(forwardCastOrigin, _capsuleCollider.radius, Vector3.down, out RaycastHit forwardHit,
                        1 - stepHeight, GroundLayers, QueryTriggerInteraction.Ignore))
                    {

                        float forwardSlopeAngle = Vector3.Angle(Vector3.up, forwardHit.normal);
                        if (forwardSlopeAngle < maxSlopeAngle)
                        {
                            // lift player to slope height before moving forward
                            float targetY = forwardHit.point.y;
                            float currentY = rb.position.y;
                            if (targetY > currentY)
                            {
                                Vector3 lifted = rb.position;
                                lifted.y = Mathf.MoveTowards(currentY, targetY, _speed * Time.fixedDeltaTime);
                                rb.MovePosition(lifted);
                            }

                            moveDirection = Vector3.ProjectOnPlane(moveDirection, forwardHit.normal);
                            _onRamp = true;
                        }
                    }
                    else
                    {
                        // no surface ahead, fall back to current ground normal
                        moveDirection = Vector3.ProjectOnPlane(moveDirection, _groundHit.normal);
                        _onRamp = false;
                    }
                }

                // inside Move(), right before inputMove = moveDirection
                Debug.DrawRay(transform.position, moveDirection, Color.green, 0.1f);
                inputMove = moveDirection;
            }

            Vector3 playerVelocity =
                inputMove +
                new Vector3(0, _verticalVelocity, 0);

            // Movement of the platform the player is riding, if any
            Vector3 platformVelocity = Vector3.zero;

            if (_ridable != null)
            {
                platformVelocity = _ridable.Velocity;
                Debug.Log("Platform velocity: " + platformVelocity);
            }

            Vector3 finalVelocity = playerVelocity + platformVelocity;

            rb.MovePosition(rb.position + finalVelocity * Time.fixedDeltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void LedgeCheck()
        {
            if (Grounded)
            {
                _animator.SetBool(_animIDOnLedge, false);
                return;
            }

            if (_verticalVelocity > 1.0f)
                return;

            // If falling, check for a collision in front of the player at the height of a ledge.
            Vector3 forwardCastOrigin = transform.position + transform.forward * _capsuleCollider.radius + Vector3.up * 1.75f;
            if (Physics.SphereCast(forwardCastOrigin, _capsuleCollider.radius, Vector3.down, out RaycastHit forwardHit,
                LedgeCheckDistance, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                OnLedge = true;
                _ridable = forwardHit.collider.GetComponent<Rideable>();

                _verticalVelocity = 0;
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
                _animator.SetBool(_animIDOnLedge, false);

                Vector3 hangPosition = new Vector3(
                    transform.position.x,
                    forwardHit.point.y - 1.0f - HangOffset,
                    transform.position.z);

                // add platform movement if hanging on a moving platform
                if (_ridable != null)
                    hangPosition += _ridable.Velocity * Time.fixedDeltaTime;

                transform.position = hangPosition;

                // rotate to face the ledge
                Vector3 ledgeFacing = forwardHit.point - transform.position;
                ledgeFacing.y = 0;
                if (ledgeFacing != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(ledgeFacing.normalized);
            }

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDOnLedge, OnLedge);
            }
        }

        private void JumpAndGravity()
        {
            // If on the ground or recently left it, perform jump
            if (Grounded || _coyoteTimeDelta >= 0 || OnLedge)
            {
                // Jump
                if ((_input.jump || bufferedJump) && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * (OnLedge ? 2f : 1f) * -2f * Gravity);

                    OnLedge = false;
                    _ridable = null;

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }

            // Basic grounded behaviour
            if (Grounded && !OnLedge)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;
                _coyoteTimeDelta = CoyoteTime;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -0.5f;
                }
            }
            //
            else if (!OnLedge)
            {
                // If we press jump while not on the ground, start a timer.
                // If we hit the ground again while the timer is running, jump.
                if (_input.jump)
                {
                    bufferedJump = true;
                    _jumpBufferingDelta = JumpBufferTime;
                }
                if (_jumpBufferingDelta >= 0.0f)
                {
                    _jumpBufferingDelta -= Time.deltaTime;
                }
                else
                {
                    bufferedJump = false;
                }

                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                if (_coyoteTimeDelta >= 0.0f)
                {
                    _coyoteTimeDelta -= Time.deltaTime;
                }
                // if we are not grounded and not in coyote time, do not jump      
                _input.jump = false;
            }

            if (OnLedge)
            {
                _verticalVelocity = 0f; // hard lock
            }
            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            else if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void SnapToGround()
        {
            if (!Grounded || OnLedge)
                return;

            if (_verticalVelocity > 0.0f)
                return;

            if (_groundHit.distance > 0.3f)
                return;

            float capsuleBottomn = _capsuleCollider.center.y - _capsuleCollider.height / 2f + _capsuleCollider.radius;
            Vector3 targetPosition = rb.position;
            targetPosition.y = _groundHit.point.y;

            rb.MovePosition(targetPosition);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
            
            Gizmos.DrawSphere(
                transform.position + transform.forward * GroundedRadius + Vector3.up * 2.0f,
                GroundedRadius);

            Gizmos.DrawSphere(
                transform.position + transform.forward * GroundedRadius + Vector3.up * (2.0f - LedgeCheckDistance),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    //AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                //AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}