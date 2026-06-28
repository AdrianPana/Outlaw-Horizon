using UnityEngine;

public class AirborneState : BaseState
{
    private float _animationBlend;
    private float _rotationVelocity;
    private int _jumpsRemaining;

    public AirborneState(PlayerContext ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        // If we walked off an edge (not from a jump), jumpsRemaining
        // starts at max so the player can still double jump
        _jumpsRemaining = 1;

        if (ctx.animator)
            ctx.animator.SetBool(ctx.animIDJump, ctx.verticalVelocity > 0f);
    }

    public override void Tick(float dt)
    {
        // Transitions first
        if (ctx.isGrounded)
        {
            sm.TransitionTo(new GroundedState(ctx, sm));
            return;
        }

        //if (ctx.onLedge)
        //{
        //    sm.TransitionTo(new LedgeGrabState(ctx, sm));
        //    return;
        //}

        HandleJump();
        ApplyGravity(dt);
        HandleMovement(dt);
        UpdateAnimator(dt);
    }

    // -------------------------------------------------------------------------

    private void HandleJump()
    {
        bool jumpPressed = ctx.inputActions.Player.Jump.WasPressedThisFrame();

        if (jumpPressed)
        {
            // Coyote jump — just left the ground, still counts as a ground jump
            if (ctx.coyoteTimeDelta > 0f)
            {
                PerformJump(ctx.jumpHeight);
                return;
            }

            // Double jump
            if (_jumpsRemaining > 0)
            {
                _jumpsRemaining--;
                PerformJump(ctx.jumpHeight * 0.8f); // slightly lower than ground jump
                return;
            }

            // No jump available — buffer it in case we land soon
            ctx.bufferedJump = true;
            ctx.jumpBufferDelta = ctx.jumpBufferTime;
        }
    }

    private void PerformJump(float height)
    {
        ctx.verticalVelocity = Mathf.Sqrt(height * -2f * ctx.gravity);
        ctx.bufferedJump = false;

        if (ctx.animator)
            ctx.animator.SetBool(ctx.animIDJump, true);
    }

    private void ApplyGravity(float dt)
    {
        if (ctx.verticalVelocity < ctx.terminalVelocity)
            ctx.verticalVelocity += ctx.gravity * dt;

        // Tick fall animation
        if (ctx.fallTimeoutDelta > 0f)
        {
            ctx.fallTimeoutDelta -= dt;
        }
        else
        {
            if (ctx.animator)
                ctx.animator.SetBool(ctx.animIDFreeFall, true);
        }

        // Move vertically
        ctx.rb.MovePosition(ctx.rb.position + new Vector3(0f, ctx.verticalVelocity * dt, 0f));
    }

    private void HandleMovement(float dt)
    {
        Vector2 moveInput = ctx.inputActions.Player.Move.ReadValue<Vector2>();

        if (moveInput == Vector2.zero)
        {
            // Bleed off horizontal speed while in air
            ctx.horizontalSpeed = Mathf.Lerp(ctx.horizontalSpeed, 0f, dt * ctx.speedChangeRate * 0.3f);
            return;
        }

        // Same rotation logic as grounded
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                               + ctx.mainCamera.eulerAngles.y;

        float rotation = Mathf.SmoothDampAngle(
            ctx.rb.transform.eulerAngles.y,
            targetRotation,
            ref _rotationVelocity,
            ctx.rotationSmoothTime);

        ctx.rb.transform.rotation = Quaternion.Euler(0f, rotation, 0f);

        // Accelerate toward target speed, but slower than grounded
        float inputMagnitude = moveInput.magnitude;
        ctx.horizontalSpeed = Mathf.Lerp(
            ctx.horizontalSpeed,
            ctx.moveSpeed * inputMagnitude,
            dt * ctx.speedChangeRate * 0.5f); // 0.5 = less responsive in air

        // No slope projection, no step detection
        Vector3 move = ctx.rb.transform.forward * ctx.horizontalSpeed;
        ctx.rb.MovePosition(ctx.rb.position + new Vector3(move.x, 0f, move.z) * dt);
    }

    private void UpdateAnimator(float dt)
    {
        if (!ctx.animator) return;
        _animationBlend = Mathf.Lerp(_animationBlend, ctx.horizontalSpeed, dt * ctx.speedChangeRate);
        ctx.animator.SetFloat(ctx.animIDSpeed, _animationBlend);
    }
}