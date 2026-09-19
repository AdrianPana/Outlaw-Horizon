using UnityEngine;

public class ModifierAmbientObject : AbstractPhysicsModifierObject
{
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

        if (!isInfluenced) return;

        foreach (var provider in providers)
        {
            rb.AddForce(provider.GetForceContribution(transform.position), ForceMode.Acceleration);
        }
    }

    public override void ApplyVerticalVelocity(Vector3 gravityForce, bool hasWind, bool hasGravity)
    {
        throw new System.NotImplementedException();
    }
}
