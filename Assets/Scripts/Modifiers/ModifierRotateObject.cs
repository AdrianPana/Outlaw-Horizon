using UnityEngine;

public class ModifierRotateObject : AbstractModifierObject
{
    public override void Awake()
    {
        base.Awake();
    }

    private void FixedUpdate()
    {
        bool isInfluenced = CheckIfInfluenced();

        if (!isInfluenced) return;
        foreach (var provider in providers)
        {
            var forceContribution = provider.GetForceContribution(transform.position);
            Debug.Log($"Applying torque from provider {provider}: {forceContribution}");
            transform.Rotate(new Vector3(0, forceContribution.magnitude * Time.fixedDeltaTime * 20, 0), Space.World);
        }
    }
}
