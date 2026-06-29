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

        // Snap to hang position
        Vector3 hangPosition = new Vector3(
            ctx.rb.position.x,
            _ledgeHit.point.y,// - ctx.hangOffset,
            ctx.rb.position.z);
        ctx.rb.position = hangPosition;

        // Face the ledge
        Vector3 ledgeFacing = _ledgeHit.point - ctx.rb.position;
        ledgeFacing.y = 0f;
        if (ledgeFacing != Vector3.zero)
            ctx.rb.transform.rotation = Quaternion.LookRotation(ledgeFacing.normalized);

        // Set platform if ledge is rideable
        ctx.rideable = _ledgeHit.collider.GetComponent<Rideable>();
        SetPlatform(_ledgeHit.transform);

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

        // Re-snap in case platform is moving
        if (ctx.rideable != null)
        {
            Vector3 hangPosition = new Vector3(
                ctx.rb.position.x,
                _ledgeHit.point.y,
                ctx.rb.position.z);
            ctx.rb.position = hangPosition;
        }

        HandleJump();
        HandleDrop();
    }

    public override void Exit()
    {
        SetPlatform(null);
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
            // Jump off ledge with extra height
            ctx.verticalVelocity = Mathf.Sqrt(ctx.jumpHeight * 2f * -2f * ctx.gravity);
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

    private void SetPlatform(Transform platform)
    {
        ctx.currentPlatform = platform;
        ctx.rb.transform.SetParent(platform, true);
    }
}