using StarterAssets;
using UnityEngine;

public class PhysicsChecker
{
    private PlayerContext ctx;

    public PhysicsChecker(PlayerContext ctx)
    {
        this.ctx = ctx;
    }

    public void Tick(float dt)
    {
        CheckGround();
        CheckLedge();
        TickTimers(dt);
    }

    private void CheckGround()
    {
        float radius = ctx.capsuleCollider.radius;
        Vector3 capsuleCenter = ctx.rb.position + Vector3.up * ctx.capsuleCollider.center.y;

        // Sphere bottom starts at feet + stepHeight, so taller ledges are ignored
        Vector3 castOrigin = ctx.rb.position + Vector3.up * (ctx.stepHeight + radius);

        float castDistance = ctx.stepHeight + ctx.groundedCastDistance;

        if (Physics.SphereCast(castOrigin, radius, Vector3.down,
            out RaycastHit hit, castDistance,
            ctx.groundLayers, QueryTriggerInteraction.Ignore))
        {
            ctx.isGrounded = true;
            ctx.onLedge = false;
            ctx.groundHit = hit;
            ctx.distanceToGround = capsuleCenter.y - hit.point.y; // keep hover math unchanged
            ctx.rideable = hit.collider.GetComponent<Rideable>();
        }
        else
        {
            ctx.isGrounded = false;
            if (!(ctx.onLedge && ctx.rideable != null))
                ctx.rideable = null;
        }

        if (ctx.animator)
            ctx.animator.SetBool(ctx.animIDGrounded, ctx.isGrounded);
    }

    private void CheckLedge()
    {
        if (ctx.isGrounded)
        {
            ctx.onLedge = false;
            if (ctx.animator) ctx.animator.SetBool(ctx.animIDOnLedge, false);
            return;
        }

        if (ctx.verticalVelocity > 1.0f || ctx.onLedge)
            return;

        Vector3 origin = ctx.rb.position
            + ctx.rb.transform.forward * ctx.capsuleCollider.radius
            + Vector3.up * 1.75f;

        if (Physics.SphereCast(origin, ctx.capsuleCollider.radius, Vector3.down,
            out RaycastHit hit, ctx.ledgeCheckDistance,
            ctx.groundLayers, QueryTriggerInteraction.Ignore))
        {
            ctx.onLedge = true;
            ctx.ledgeHit = hit;
            Debug.Log($"Ledge detected at {hit.point}, normal: {hit.normal}");
            Debug.DrawRay(hit.point, hit.normal, Color.red, 1.0f);
        }

        if (ctx.animator)
            ctx.animator.SetBool(ctx.animIDOnLedge, ctx.onLedge);
    }

    private void TickTimers(float dt)
    {
        if (ctx.isGrounded && !ctx.onLedge)
        {
            ctx.coyoteTimeDelta = ctx.coyoteTime;
            ctx.fallTimeoutDelta = ctx.fallTimeout;
            ctx.jumpTimeoutDelta -= dt;
            if (ctx.jumpTimeoutDelta < 0) ctx.jumpTimeoutDelta = 0;
        }
        else if (!ctx.onLedge)
        {
            ctx.coyoteTimeDelta -= dt;
            ctx.fallTimeoutDelta -= dt;
            ctx.jumpTimeoutDelta = ctx.jumpTimeout;

            if (ctx.jumpBufferDelta >= 0)
                ctx.jumpBufferDelta -= dt;
            else
                ctx.bufferedJump = false;
        }
    }
}