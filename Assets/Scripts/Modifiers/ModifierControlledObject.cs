using UnityEngine;

[RequireComponent(typeof(Rideable))]
public class ModifierControlledObject : AbstractPhysicsModifierObject
{
    public override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        bool isInfluenced = CheckIfInfluenced();

        SanitizeVelocities();

        momentumVelocity = Vector3.Lerp(momentumVelocity, Vector3.zero, kinematicDrag * Time.fixedDeltaTime);

        CollectForces(isInfluenced, out Vector3 windForce, out Vector3 gravityForce, out bool hasWind, out bool hasGravity);

        ApplyHorizontalVelocity(windForce);
        ApplyVerticalVelocity(gravityForce, hasWind, hasGravity);

        Vector3 totalVelocity = momentumVelocity + currentHorizontalVelocity + currentVerticalVelocity;
        MoveKinematicWithCollision(totalVelocity * Time.fixedDeltaTime);
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
        else
        {
            if (currentVerticalVelocity.y > 0)
            {
                currentVerticalVelocity = Vector3.zero;
            }

            if (IsGrounded())
                if (currentVerticalVelocity.y < 0f)
                    currentVerticalVelocity = Vector3.zero;
            else
                currentVerticalVelocity += passiveGravityStrength * Time.fixedDeltaTime * Vector3.down;
        }
    }
}
