using StarterAssets;
using UnityEngine;

public class LedgeGrabState : BaseState
{
    private RaycastHit _ledgeHit;

    public LedgeGrabState(PlayerContext ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        _ledgeHit = ctx.ledgeHit;

        // Zero out all velocity — we're hanging
        ctx.verticalVelocity = 0f;
        ctx.horizontalSpeed = 0f;
        ctx.rb.linearVelocity = Vector3.zero;
        ctx.externalVelocity = Vector3.zero;

        // Snap to hang position
        Vector3 hangPosition = getCurrentHangPosition();
        ctx.rb.position = hangPosition;

        // Face the ledge
        Vector3 ledgeFacing = _ledgeHit.point - ctx.rb.position;
        ledgeFacing.y = 0f;
        if (ledgeFacing != Vector3.zero)
            ctx.rb.transform.rotation = Quaternion.LookRotation(ledgeFacing.normalized);

        // Set platform if ledge is rideable
        ctx.rideable = _ledgeHit.collider.GetComponent<Rideable>();
        ctx.rideable?.RegisterPassenger(ctx.capsuleCollider);

        if (ctx.animator)
        {
            ctx.animator.SetBool(ctx.animIDJump, false);
            ctx.animator.SetBool(ctx.animIDFreeFall, false);
            ctx.animator.SetBool(ctx.animIDOnLedge, true);
        }
    }

    public override void Tick(float dt)
    {
        // Hard lock position every frame — no physics drift
        //TODO CHECK IF REMOVAL IS BETTER
        ctx.rb.linearVelocity = Vector3.zero;
        ctx.verticalVelocity = 0f;

        Debug.Log("FAAAH " + ctx.rideable + " " + ctx.onLedge);
        // Re-snap in case platform is moving
        if (ctx.rideable != null)
        {
            Debug.Log("RIDEABLE2: " + ctx.rideable.name);
            Vector3 hangPosition = getCurrentHangPosition();
            Vector3 platformMovement = GetPlatformMovement(dt);
            Debug.Log("Platform Movement: " + platformMovement);
            ctx.rb.position = hangPosition + platformMovement;
        }

        HandleJump();
        HandleDrop();
    }

    public override void Exit()
    {
        ctx.rideable?.UnregisterPassenger(ctx.capsuleCollider);
        ctx.rideable = null;
        ctx.onLedge = false;

        if (ctx.animator)
            ctx.animator.SetBool(ctx.animIDOnLedge, false);
    }

    // -------------------------------------------------------------------------

    private void HandleJump()
    {
        if (ctx.input.jumpPressed)
        {
            if (ctx.rideable != null)
            {
                ctx.externalVelocity = new Vector3(
                    ctx.rideable.Velocity.x,
                    0,
                    ctx.rideable.Velocity.z);
            }

            // Jump off ledge with extra height
            ctx.verticalVelocity = Mathf.Sqrt(ctx.jumpHeight * ctx.ledgeJumpModifier * -2f * ctx.gravity);
            ctx.rb.linearVelocity = new Vector3(
                ctx.rb.linearVelocity.x,
                ctx.verticalVelocity,
                ctx.rb.linearVelocity.z);

            if (ctx.animator)
                ctx.animator.SetBool(ctx.animIDJump, true);

            sm.TransitionTo(new AirborneState(ctx, sm));
        }
    }

    private void HandleDrop()
    {
        //// Drop if player pushes down or just let go with no input
        //bool dropPressed = ctx.input.move.y < -0.1f;

        //if (dropPressed)
        //{
        //    // Small downward nudge so we clear the ledge collider
        //    ctx.verticalVelocity = -10f;
        //    ctx.rb.linearVelocity = new Vector3(0f, ctx.verticalVelocity, 0f);
        //    sm.TransitionTo(new AirborneState(ctx, sm));
        //}
    }

    private Vector3 getCurrentHangPosition()
    {
        return new Vector3(
            ctx.rb.position.x,
            _ledgeHit.point.y - ctx.hangOffset,
            ctx.rb.position.z);
    }

    private Vector3 GetPlatformMovement(float dt)
    {
        Debug.Log("Platform" + ctx.rideable);
        if (ctx.rideable == null) return Vector3.zero;
        return ctx.rideable.Velocity * dt;
    }
}