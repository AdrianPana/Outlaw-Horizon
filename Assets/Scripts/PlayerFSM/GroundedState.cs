using UnityEngine;

public class GroundedState : BaseState
{
    private float _animationBlend;
    private float _rotationVelocity;

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
    }

    public override void Tick(float dt)
    {
        // Check transitions first — if we're no longer grounded, leave immediately
        //if (ctx.onLedge)
        //{
        //    sm.TransitionTo(new LedgeGrabState(ctx, sm));
        //    return;
        //}

        if (!ctx.isGrounded)
        {
            sm.TransitionTo(new AirborneState(ctx, sm));
            return;
        }

        HandleJump();

        // If jump was triggered this frame, HandleJump already
        // set verticalVelocity and transitioned — don't move
        if (!ctx.isGrounded) return;

        HandleMovement(dt);
        SnapToGround();
        UpdateAnimator(dt);
    }

    public override void Exit()
    {
        // coyoteTimeDelta was already set to CoyoteTime by PhysicsChecker
        // while grounded — it will now count down in AirborneState,
        // giving the player the coyote time window
    }

    // -------------------------------------------------------------------------

    private void HandleJump()
    {
        bool jumpPressed = ctx.inputActions.Player.Jump.IsPressed();

        if (jumpPressed || ctx.bufferedJump)
        {
            if (ctx.jumpTimeoutDelta <= 0f)
            {
                ctx.verticalVelocity = Mathf.Sqrt(ctx.jumpHeight * -2f * ctx.gravity);
                ctx.bufferedJump = false;

                if (ctx.animator)
                    ctx.animator.SetBool(ctx.animIDJump, true);

                // Transition immediately — we are now airborne
                sm.TransitionTo(new AirborneState(ctx, sm));
            }
        }
    }

    private void HandleMovement(float dt)
    {
        Vector2 moveInput = ctx.inputActions.Player.Move.ReadValue<Vector2>();
        float targetSpeed = moveInput == Vector2.zero ? 0f : ctx.moveSpeed;

        // Smooth speed
        float speedOffset = 0.1f;
        float inputMagnitude = moveInput.magnitude; // analogMovement assumed true
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

        if (moveInput == Vector2.zero) return;

        RotateTowardInput(moveInput);
        ApplyHorizontalMove(dt);
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

    private void ApplyHorizontalMove(float dt)
    {
        Vector3 moveDir = ctx.rb.transform.forward * ctx.horizontalSpeed;

        float groundAngle = Vector3.Angle(Vector3.up, ctx.groundHit.normal);
        Debug.Log($"groundAngle: {groundAngle} | normal: {ctx.groundHit.normal} | grounded: {ctx.isGrounded}");


        if (groundAngle <= ctx.maxSlopeAngle)
        {
            // Project onto the slope — this tilts the movement vector
            // to follow the surface rather than cut through it
            moveDir = Vector3.ProjectOnPlane(moveDir, ctx.groundHit.normal).normalized
                      * ctx.horizontalSpeed;
            //Debug.Log($"Ground angle: {groundAngle}, Ground normal: {ctx.groundHit.normal}, Move dir: {moveDir}");
            // Kill vertical velocity on slopes so we don't bounce or fight the ground
            if (ctx.verticalVelocity < 0f)
                ctx.verticalVelocity = -0.1f;
        }
        else if (groundAngle > ctx.maxSlopeAngle)
        {
            // Too steep — slide down instead of climbing
            moveDir += new Vector3(ctx.groundHit.normal.x, -ctx.groundHit.normal.y, ctx.groundHit.normal.z)
                       * ctx.horizontalSpeed;
        }

        //TryStepUp();

        Vector3 velocity = moveDir + new Vector3(0f, ctx.verticalVelocity, 0f);
        ctx.rb.MovePosition(ctx.rb.position + velocity * dt);
    }

    private void TryStepUp()
    {
        Vector3 origin = ctx.rb.position + new Vector3(0f, 0.001f, 0f);

        // Is there an obstacle directly ahead at foot level?
        if (!Physics.Raycast(origin, ctx.rb.transform.forward,
            out RaycastHit hitLower, ctx.lowerDist))
            return;

        float hitAngle = Vector3.Angle(Vector3.up, hitLower.normal);
        float groundAngle = Vector3.Angle(Vector3.up, ctx.groundHit.normal);
        //bool isStep = hitAngle > 85f;
        //bool isOnFlat = groundAngle < 5f;
        //Debug.Log($"Hit angle: {hitAngle}, Ground angle: {groundAngle}, Is step: {isStep}, Is on flat: {isOnFlat}");
        //if (!isStep && !isOnFlat) return;

        // Is the space above the step clear?
        Vector3 upperOrigin = ctx.rb.position + new Vector3(0f, ctx.stepHeight + 0.1f, 0f);
        if (Physics.Raycast(upperOrigin, ctx.rb.transform.forward, ctx.upperDist))
            return;

        // Is there ground on top of the step?
        Vector3 aboveObstacle = hitLower.point + new Vector3(0f, ctx.stepHeight, 0f);
        if (Physics.Raycast(aboveObstacle, Vector3.down, out RaycastHit topHit,
            ctx.stepHeight, ctx.groundLayers))
        {
            ctx.rb.position = new Vector3(ctx.rb.position.x, topHit.point.y, ctx.rb.position.z);
            ctx.isSteppingUp = true;
        }
    }

    private void SnapToGround()
    {
        if (ctx.isSteppingUp)
        {
            ctx.isSteppingUp = false;
            return;
        }

        // Don't snap while jumping
        if (ctx.verticalVelocity > 0f) return;

        // Only snap if very close to ground (avoids snapping on ledge edges)
        if (ctx.groundHit.distance > ctx.stepHeight) return;

        float capsuleBottom = ctx.capsuleCollider.center.y
                              - ctx.capsuleCollider.height / 2f
                              + ctx.capsuleCollider.radius;

        Vector3 target = ctx.rb.position;
        target.y = ctx.groundHit.point.y;
        ctx.rb.MovePosition(target);
    }

    private void UpdateAnimator(float dt)
    {
        if (!ctx.animator) return;
        float inputMagnitude = ctx.inputActions.Player.Move.ReadValue<Vector2>().magnitude;
        ctx.animator.SetFloat(ctx.animIDSpeed, _animationBlend);
        ctx.animator.SetFloat(ctx.animIDMotionSpeed, inputMagnitude);
    }
}