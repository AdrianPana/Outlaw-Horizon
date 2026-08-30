using UnityEngine;

public class GroundedState : BaseState
{
    private float _animationBlend;
    private float _rotationVelocity;
    private bool _steppedUpThisContact;

    public GroundedState(PlayerContext ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        _animationBlend = 0f;

        if (ctx.animator)
        {
            ctx.animator.SetBool(ctx.animIDJump, false);
            ctx.animator.SetBool(ctx.animIDFreeFall, false);
        }

        // Clamp downward velocity so we don't accumulate fall speed
        // while grounded (e.g. walking down a slope)
        if (ctx.verticalVelocity < 0f)
            ctx.verticalVelocity = -0.5f;

        // Synchronize Rigidbody velocity
        ctx.rb.linearVelocity = new Vector3(ctx.rb.linearVelocity.x, ctx.verticalVelocity, ctx.rb.linearVelocity.z);
        ctx.externalVelocity = Vector3.zero;
    }

    public override void Tick(float dt)
    {
        //Check transitions first   if we're no longer grounded, leave immediately
        if (ctx.onLedge)
        {
            sm.TransitionTo(new LedgeGrabState(ctx, sm));
            return;
        }

        if (!ctx.isGrounded)
        {
            sm.TransitionTo(new AirborneState(ctx, sm));
            return;
        }

        if (HandleJump())
        {
            sm.TransitionTo(new AirborneState(ctx, sm));
            return;
        }


        // Cancel any residual horizontal velocity from collisions/impulses.
        // Horizontal motion is fully driven via MovePosition, so leftover
        // physics-injected velocity here is never intentional.
        ctx.rb.linearVelocity = new Vector3(0f, ctx.rb.linearVelocity.y, 0f);

        //if (HandleJump()) return;

        // If jump was triggered this frame, HandleJump already
        // set verticalVelocity and transitioned   don't move
        if (!ctx.isGrounded) return;

        ApplyMovement(dt);
        ApplyHoverForce(dt);
        UpdateAnimator(dt);

        // Keep verticalVelocity in sync for transitions
        ctx.verticalVelocity = ctx.rb.linearVelocity.y;
    }

    public override void Exit()
    {
        // coyoteTimeDelta was already set to CoyoteTime by PhysicsChecker
        // while grounded   it will now count down in AirborneState,
        // giving the player the coyote time window
    }

    // -------------------------------------------------------------------------

    private bool HandleJump()
    {
        if (ctx.input.jumpPressed || ctx.bufferedJump)
        {
            if (ctx.rideable != null)
            {
                ctx.externalVelocity = new Vector3(
                    ctx.rideable.Velocity.x,
                    0,
                    ctx.rideable.Velocity.z);
            }

            if (ctx.jumpTimeoutDelta <= 0f)
            {
                ctx.verticalVelocity = Mathf.Sqrt(ctx.jumpHeight * -2f * ctx.gravity);
                ctx.bufferedJump = false;
                ctx.rb.linearVelocity = new Vector3(ctx.rb.linearVelocity.x, ctx.verticalVelocity, ctx.rb.linearVelocity.z);

                if (ctx.animator)
                    ctx.animator.SetBool(ctx.animIDJump, true);

                ctx.isJumping = true;
                return true;
            }
        }

        return false;
    }

    private void ApplyMovement(float dt)
    {
        Vector3 totalDelta = Vector3.zero;
        totalDelta += GetPlatformMovement(dt);
        totalDelta += GetPlayerMovement(dt);

        Vector3 nextPosition = ctx.rb.position + totalDelta;
        nextPosition.y = ctx.rb.position.y + ctx.rb.linearVelocity.y * dt; // vertical stays physics-driven
        ctx.rb.MovePosition(nextPosition);
    }

    private Vector3 GetPlayerMovement(float dt)
    {
        Vector2 moveInput = ctx.input.move;
        float targetSpeed = moveInput == Vector2.zero ? 0f : ctx.moveSpeed;

        float speedOffset = 0.1f;
        float inputMagnitude = moveInput.magnitude;
        if (Mathf.Abs(ctx.horizontalSpeed - targetSpeed) > speedOffset)
        {
            ctx.horizontalSpeed = Mathf.Round(
                Mathf.Lerp(ctx.horizontalSpeed, targetSpeed * inputMagnitude, dt * ctx.speedChangeRate)
                * 1000f) / 1000f;
        }
        else
        {
            ctx.horizontalSpeed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, dt * ctx.speedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        if (moveInput == Vector2.zero)
        {
            ApplyPlatformFacing();
            return Vector3.zero;
        }

        RotateTowardInput(moveInput);
        return GetHorizontalMoveDelta(dt);
    }
    
    private void ApplyPlatformFacing()
    {
        if (ctx.rideable == null) return;
        Quaternion rotDelta = ctx.rideable.RotationDelta;
        if (rotDelta != Quaternion.identity)
            ctx.rb.transform.rotation = rotDelta * ctx.rb.transform.rotation;
    }

    private void RotateTowardInput(Vector2 moveInput)
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                               + ctx.mainCamera.eulerAngles.y;

        float rotation = Mathf.SmoothDampAngle(
            ctx.rb.transform.eulerAngles.y,
            targetRotation,
            ref _rotationVelocity,
            ctx.rotationSmoothTime);

        ctx.rb.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
    }

    private Vector3 GetHorizontalMoveDelta(float dt)
    {
        Vector3 moveDir = ctx.rb.transform.forward * ctx.horizontalSpeed;

        float groundAngle = Vector3.Angle(Vector3.up, ctx.groundHit.normal);

        if (groundAngle <= ctx.maxSlopeAngle)
        {
            // Project onto the slope   this tilts the movement vector
            // to follow the surface rather than cut through it
            moveDir = Vector3.ProjectOnPlane(moveDir, ctx.groundHit.normal).normalized
                      * ctx.horizontalSpeed;
            //Debug.Log($"Ground angle: {groundAngle}, Ground normal: {ctx.groundHit.normal}, Move dir: {moveDir}");
            // Kill vertical velocity on slopes so we don't bounce or fight the ground
            if (ctx.verticalVelocity < 0f)
                ctx.verticalVelocity = -0.1f;
        }
        else
        {
            // Too steep   slide down instead of climbing
            moveDir += new Vector3(ctx.groundHit.normal.x, -ctx.groundHit.normal.y, ctx.groundHit.normal.z)
                       * ctx.horizontalSpeed;
        }

        //TryStepUp();

        return new Vector3(moveDir.x, 0f, moveDir.z) * dt;

        //// Move horizontally via MovePosition, but vertically we let physics simulate.
        //// This is done by integrating the Rigidbody's velocity.y.
        //Vector3 horizontalMove = new Vector3(moveDir.x, 0f, moveDir.z) * dt;
        //Vector3 nextPosition = ctx.rb.position + horizontalMove;
        //nextPosition.y = ctx.rb.position.y + ctx.rb.linearVelocity.y * dt;

        //ctx.rb.MovePosition(nextPosition);
    }

    private Vector3 GetPlatformMovement(float dt)
    {
        if (ctx.rideable == null) return Vector3.zero;

        Vector3 pivot = ctx.rideable.transform.position;
        Quaternion rotDelta = ctx.rideable.RotationDelta;
        Vector3 linearDelta = ctx.rideable.Velocity * dt;

        Vector3 offsetFromPivot = ctx.rb.position - pivot;
        Vector3 rotatedOffset = rotDelta * offsetFromPivot;
        Vector3 rotationDelta = rotatedOffset - offsetFromPivot;

        ctx.platformMovement = rotationDelta + linearDelta;
        return ctx.platformMovement;
    }

    private void TryStepUp()
    {
        Vector3 origin = ctx.rb.position + new Vector3(0f, 0.001f, 0f);

        if (!Physics.Raycast(origin, ctx.rb.transform.forward,
            out RaycastHit hitLower, ctx.lowerStepCastDist, ctx.groundLayers))
        {
            _steppedUpThisContact = false; // no obstacle, reset debounce
            return;
        }

        float hitAngle = Vector3.Angle(Vector3.up, hitLower.normal);
        float groundAngle = Vector3.Angle(Vector3.up, ctx.groundHit.normal);
        bool isStep = hitAngle > 85f;
        bool isOnFlat = groundAngle < 5f;

        // Require BOTH: a vertical-ish face AND currently on stable ground
        if (!isStep || !isOnFlat) return;

        // Don't re-trigger every frame against the same obstacle
        if (_steppedUpThisContact) return;

        // Is the space above the step clear?
        Vector3 upperOrigin = origin + new Vector3(0f, ctx.stepHeight, 0f);
        if (Physics.Raycast(upperOrigin, ctx.rb.transform.forward,
            ctx.upperStepCastDist, ctx.groundLayers))
            return;

        // Look for a top surface directly ahead of the CHARACTER (not the wall face),
        // so we don't pick up ground behind/beside the obstacle by accident.
        Vector3 probeXZ = ctx.rb.position + ctx.rb.transform.forward * (ctx.lowerStepCastDist + 0.05f);
        Vector3 aboveObstacle = new Vector3(probeXZ.x, ctx.rb.position.y + ctx.stepHeight, probeXZ.z);

        if (!Physics.Raycast(aboveObstacle, Vector3.down, out RaycastHit topHit,
            ctx.stepHeight, ctx.groundLayers))
            return;

        float stepUpAmount = topHit.point.y - ctx.rb.position.y;

        // Explicit bound check: reject anything that isn't a genuine small step.
        // (min threshold avoids reacting to floor noise/seams)
        if (stepUpAmount <= 0.02f || stepUpAmount > ctx.stepHeight)
            return;

        ctx.rb.position = new Vector3(ctx.rb.position.x, topHit.point.y, ctx.rb.position.z);

        // Prevent stale hover-force data from launching us on the next tick
        ctx.rb.linearVelocity = new Vector3(ctx.rb.linearVelocity.x, 0f, ctx.rb.linearVelocity.z);
        ctx.verticalVelocity = 0f;
        ctx.distanceToGround = ctx.capsuleCollider.center.y; // matches hover target, no spike

        ctx.isSteppingUp = true;
        _steppedUpThisContact = true;
    }

    private void ApplyHoverForce(float dt)
    {
        float targetDistance = ctx.capsuleCollider.center.y; // cast origin is center.y above rb.position, ground should be center.y below cast origin
        float currentDistance = ctx.distanceToGround;

        float displacement = targetDistance - currentDistance;
        float springForce = displacement * ctx.springStiffness;
        float dampingForce = ctx.rb.linearVelocity.y * ctx.springDamping;
        float hoverForceY = springForce - dampingForce;
        float gravityForceY = ctx.gravity * ctx.rb.mass;

        ctx.rb.AddForce(Vector3.up * (hoverForceY + gravityForceY), ForceMode.Acceleration);
    }

    private void UpdateAnimator(float dt)
    {
        if (!ctx.animator) return;
        float inputMagnitude = ctx.inputActions.Player.Move.ReadValue<Vector2>().magnitude;
        ctx.animator.SetFloat(ctx.animIDSpeed, _animationBlend);
        ctx.animator.SetFloat(ctx.animIDMotionSpeed, inputMagnitude);
    }
}