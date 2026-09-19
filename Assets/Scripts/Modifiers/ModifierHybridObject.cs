using UnityEngine;

public class ModifierHybridObject : AbstractPhysicsModifierObject
{
    private bool wasInfluenced;

    public override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.freezeRotation = false;
    }

    private void FixedUpdate()
    {
        bool isInfluenced = CheckIfInfluenced();

        SanitizeVelocities();

        // Transition: physics -> kinematic when a modifier first touches the object
        if (isInfluenced && !wasInfluenced && !rb.isKinematic)
        {
            momentumVelocity = rb.linearVelocity;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Transition: kinematic -> physics when all modifiers stop
        if (!isInfluenced && wasInfluenced)
        {
            currentHorizontalVelocity = Vector3.zero;
            currentVerticalVelocity = Vector3.zero;
            momentumVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = true;
            return;
        }

        // Not influenced and already in physics — nothing to do
        if (!isInfluenced) return;

        momentumVelocity = Vector3.Lerp(momentumVelocity, Vector3.zero, kinematicDrag * Time.fixedDeltaTime);

        CollectForces(isInfluenced, out Vector3 windForce, out Vector3 gravityForce, out bool hasWind, out bool hasGravity);

        ApplyHorizontalVelocity(windForce);
        ApplyVerticalVelocity(gravityForce, hasWind, hasGravity);

        Vector3 totalVelocity = momentumVelocity + currentHorizontalVelocity + currentVerticalVelocity;
        MoveKinematicWithCollision(totalVelocity * Time.fixedDeltaTime);

        wasInfluenced = isInfluenced;
    }

    public override void ApplyVerticalVelocity(Vector3 gravityForce, bool hasWind, bool hasGravity)
    {
        if (hasGravity)
        {
            Vector3 targetVertical = gravityForce * speed;
            currentVerticalVelocity = Vector3.Lerp(
                currentVerticalVelocity,
                targetVertical,
                acceleration * Time.fixedDeltaTime
            );
        }
        else if (hasWind)
        {
            if (IsGrounded())
                currentVerticalVelocity = Vector3.zero;
            else
                currentVerticalVelocity += Vector3.down * passiveGravityStrength * Time.fixedDeltaTime;
        }
        else
        {
            currentVerticalVelocity = Vector3.zero;
        }
    }
}
