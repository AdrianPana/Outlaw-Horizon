using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(Rigidbody))]
    public class AIController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 2.0f;
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.50f;
        public float JumpBufferTime = 0.1f;
        public float CoyoteTime = 0.1f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public float GroundedCastDistance = 0.8f;
        public LayerMask GroundLayers;

        [Header("Ledge Detection")]
        public bool OnLedge = false;
        public float LedgeCheckDistance = 0.5f;
        public float HangOffset = 1.0f;

        [Header("Slope Handling")]
        public float maxSlopeAngle;
        public float stepHeight;
        public float lowerDist = 0.1f;
        public float upperDist = 0.2f;

        public Rideable _ridable;

        // AI input — set these from your Agent script
        [HideInInspector] public Vector2 aiMoveInput;
        [HideInInspector] public bool aiJump;

        private RaycastHit _groundHit;
        private CapsuleCollider _capsuleCollider;

        private float _speed;
        private float _animationBlend;
        private float _rotationVelocity;
        public float _verticalVelocity;
        private float _terminalVelocity = 10.0f;
        private bool _bufferedJump;
        private bool _isSteppingUp;

        private float _jumpBufferingDelta;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _coyoteTimeDelta;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDOnLedge;

        private Rigidbody _rb;
        private Animator _animator;
        private bool _hasAnimator;

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _rb = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void FixedUpdate()
        {
            _hasAnimator = TryGetComponent(out _animator);

            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            GroundedCheck();
            LedgeCheck();
            JumpAndGravity();
            Move();
            SnapToGround();
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
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);

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

            if (_hasAnimator)
                _animator.SetBool(_animIDGrounded, Grounded);
        }

        private void Move()
        {
            float targetSpeed = MoveSpeed;

            if (aiMoveInput == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_rb.linearVelocity.x, 0.0f, _rb.linearVelocity.z).magnitude;
            float speedOffset = 0.1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * aiMoveInput.magnitude,
                    Time.fixedDeltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.fixedDeltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // Move and rotate in world space directly — no camera influence
            if (aiMoveInput != Vector2.zero && !OnLedge)
            {
                Vector3 worldDirection = new Vector3(aiMoveInput.x, 0f, aiMoveInput.y).normalized;

                float targetRotation = Mathf.Atan2(worldDirection.x, worldDirection.z) * Mathf.Rad2Deg;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation,
                    ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }

            Vector3 inputMove = Vector3.zero;

            if (aiMoveInput != Vector2.zero && !OnLedge)
            {
                Vector3 moveDirection = transform.forward * _speed;

                if (Grounded)
                {
                    if (Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), transform.forward, out RaycastHit hitLower, lowerDist))
                    {
                        float hitAngle = Vector3.Angle(Vector3.up, hitLower.normal);
                        float groundAngle = Vector3.Angle(Vector3.up, _groundHit.normal);

                        bool isStep = hitAngle > 85f;
                        bool isOnFlat = groundAngle < 5f;

                        if (isOnFlat || isStep)
                        {
                            if (!Physics.Raycast(transform.position + new Vector3(0, stepHeight + 0.1f, 0), transform.forward, out RaycastHit hitUpper, upperDist))
                            {
                                Vector3 aboveObstacle = hitLower.point + new Vector3(0, stepHeight, 0);
                                if (Physics.Raycast(aboveObstacle, Vector3.down, out RaycastHit topHit, stepHeight, GroundLayers))
                                {
                                    _rb.position = new Vector3(_rb.position.x, topHit.point.y, _rb.position.z);
                                    _isSteppingUp = true;
                                }
                            }
                        }
                    }

                    moveDirection = Vector3.ProjectOnPlane(moveDirection, _groundHit.normal);
                }

                inputMove = moveDirection;
            }

            Vector3 platformVelocity = Vector3.zero;
            if (_ridable != null)
                platformVelocity = _ridable.Velocity;

            Vector3 finalVelocity = inputMove + new Vector3(0, _verticalVelocity, 0) + platformVelocity;
            _rb.MovePosition(_rb.position + finalVelocity * Time.fixedDeltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, aiMoveInput.magnitude);
            }
        }

        private void LedgeCheck()
        {
            if (Grounded)
            {
                if (_hasAnimator) _animator.SetBool(_animIDOnLedge, false);
                return;
            }

            if (_verticalVelocity > 1.0f)
                return;

            Vector3 forwardCastOrigin = transform.position + transform.forward * _capsuleCollider.radius + Vector3.up * 1.75f;
            if (Physics.SphereCast(forwardCastOrigin, _capsuleCollider.radius, Vector3.down, out RaycastHit forwardHit,
                LedgeCheckDistance, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                OnLedge = true;
                _ridable = forwardHit.collider.GetComponent<Rideable>();

                _verticalVelocity = 0;
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                    _animator.SetBool(_animIDOnLedge, false);
                }

                Vector3 hangPosition = new Vector3(
                    transform.position.x,
                    forwardHit.point.y - 1.0f - HangOffset,
                    transform.position.z);

                if (_ridable != null)
                    hangPosition += _ridable.Velocity * Time.fixedDeltaTime;

                transform.position = hangPosition;

                Vector3 ledgeFacing = forwardHit.point - transform.position;
                ledgeFacing.y = 0;
                if (ledgeFacing != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(ledgeFacing.normalized);
            }

            if (_hasAnimator)
                _animator.SetBool(_animIDOnLedge, OnLedge);
        }

        private void JumpAndGravity()
        {
            if (Grounded || _coyoteTimeDelta >= 0 || OnLedge)
            {
                if ((aiJump || _bufferedJump) && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * (OnLedge ? 2f : 1f) * -2f * Gravity);
                    OnLedge = false;
                    _ridable = null;
                    aiJump = false;

                    if (_hasAnimator)
                        _animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }

            if (Grounded && !OnLedge)
            {
                _fallTimeoutDelta = FallTimeout;
                _coyoteTimeDelta = CoyoteTime;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                    _verticalVelocity = -0.5f;
            }
            else if (!OnLedge)
            {
                if (aiJump)
                {
                    _bufferedJump = true;
                    _jumpBufferingDelta = JumpBufferTime;
                }
                if (_jumpBufferingDelta >= 0.0f)
                    _jumpBufferingDelta -= Time.deltaTime;
                else
                    _bufferedJump = false;

                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                    _fallTimeoutDelta -= Time.deltaTime;
                else if (_hasAnimator)
                    _animator.SetBool(_animIDFreeFall, true);

                if (_coyoteTimeDelta >= 0.0f)
                    _coyoteTimeDelta -= Time.deltaTime;

                aiJump = false;
            }

            if (OnLedge)
                _verticalVelocity = 0f;
            else if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        private void SnapToGround()
        {
            if (!Grounded || OnLedge) return;
            if (_isSteppingUp) { _isSteppingUp = false; return; }
            if (_verticalVelocity > 0.0f) return;
            if (_groundHit.distance > 0.3f) return;

            Vector3 targetPosition = _rb.position;
            targetPosition.y = _groundHit.point.y;
            _rb.MovePosition(targetPosition);
        }
    }
}