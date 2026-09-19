using Game.Modifiers;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class AbstractPhysicsModifierObject : AbstractModifierObject
{
    [Header("Settings")]
    public float speed = 3f;
    public float acceleration = 6f;
    public float kinematicDrag = 2f;
    public float passiveGravityStrength = 9.81f;

    protected Rigidbody rb;
    protected Collider physicalCollider;
    private const float GroundCheckSkin = 0.05f;
    protected LayerMask obstacleMask = ~0;

    public Vector3 CurrentVelocity { get; private set; }
    protected Vector3 momentumVelocity;
    protected Vector3 currentHorizontalVelocity;
    protected Vector3 currentVerticalVelocity;

    public override void Awake()
    {
        base.Awake();
        foreach (var col in GetComponents<Collider>())
        {
            if (!col.isTrigger)
            {
                physicalCollider = col;
                break;
            }
        }
    }

    protected void CollectForces(bool isInfluenced, out Vector3 windForce, out Vector3 gravityForce, out bool hasWind, out bool hasGravity)
    {
        windForce = Vector3.zero;
        gravityForce = Vector3.zero;
        hasWind = false;
        hasGravity = false;

        if (!isInfluenced) return;

        foreach (var provider in providers)
        {
            if (!provider.IsActiveOnObject(transform.position)) continue;

            Vector3 contribution = provider.GetForceContribution(transform.position);
            if (HasNaN(contribution))
            {
                Debug.LogError($"[NaN] provider {provider} returned NaN force on {name}", this);
                contribution = Vector3.zero;
            }

            if (provider is WindProvider)
            {
                windForce += new Vector3(contribution.x, 0f, contribution.z);
                hasWind = true;
            }
            else if (provider is GravityProvider)
            {
                gravityForce += new Vector3(0f, contribution.y, 0f);
                hasGravity = true;
            }
        }
    }

    protected void ApplyHorizontalVelocity(Vector3 windForce)
    {
        Vector3 targetHorizontal = windForce * speed;
        currentHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            targetHorizontal,
            acceleration * Time.fixedDeltaTime
        );
    }

    public abstract void ApplyVerticalVelocity(Vector3 gravityForce, bool hasWind, bool hasGravity);

    protected bool IsGrounded()
    {
        if (physicalCollider == null) return false;

        GetGroundCheckParams(out Vector3 center, out Vector3 halfExtents);

        return Physics.BoxCast(
            center,
            halfExtents,
            Vector3.down,
            out _,
            Quaternion.identity,
            GroundCheckSkin,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );
    }

    protected void GetGroundCheckParams(out Vector3 center, out Vector3 halfExtents)
    {
        Bounds bounds = physicalCollider.bounds;

        float inset = 0.001f;
        center = new Vector3(
            bounds.center.x,
            bounds.min.y + inset,
            bounds.center.z
        );

        halfExtents = new Vector3(
            bounds.extents.x,
            inset,
            bounds.extents.z
        );
    }

    protected void MoveKinematicWithCollision(Vector3 delta)
    {
        if (delta.sqrMagnitude < 0.00001f || physicalCollider == null)
        {
            if (delta.sqrMagnitude >= 0.00001f) rb.MovePosition(rb.position + delta);
            CurrentVelocity = delta / Time.fixedDeltaTime;
            return;
        }

        if (rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude, QueryTriggerInteraction.Ignore))
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0)
            {
                float safeDist = Mathf.Max(0, hit.distance - 0.01f);
                Vector3 safeDelta = delta.normalized * safeDist;
                rb.MovePosition(rb.position + safeDelta);
                CurrentVelocity = safeDelta / Time.fixedDeltaTime;

                Vector3 normal = hit.normal;
                currentHorizontalVelocity = Vector3.ProjectOnPlane(currentHorizontalVelocity, normal);
                currentVerticalVelocity = Vector3.ProjectOnPlane(currentVerticalVelocity, normal);
                momentumVelocity = Vector3.ProjectOnPlane(momentumVelocity, normal);
                return;
            }

            Rigidbody hitRb = hit.collider.attachedRigidbody;
            if (hitRb != null && !hitRb.isKinematic)
            {
                Vector3 pushVelocity = Vector3.Project(delta / Time.fixedDeltaTime, delta.normalized);
                hitRb.linearVelocity = Vector3.Lerp(hitRb.linearVelocity, pushVelocity, acceleration * Time.fixedDeltaTime);
            }
        }

        rb.MovePosition(rb.position + delta);
        CurrentVelocity = delta / Time.fixedDeltaTime;
    }

    protected void SanitizeVelocities()
    {
        if (HasNaN(momentumVelocity))
        {
            Debug.LogError($"[NaN] momentumVelocity on {name}", this);
            momentumVelocity = Vector3.zero;
        }
        if (HasNaN(currentHorizontalVelocity))
        {
            Debug.LogError($"[NaN] currentHorizontalVelocity on {name}", this);
            currentHorizontalVelocity = Vector3.zero;
        }
        if (HasNaN(currentVerticalVelocity))
        {
            Debug.LogError($"[NaN] currentVerticalVelocity on {name}", this);
            currentVerticalVelocity = Vector3.zero;
        }
    }

    private static bool HasNaN(Vector3 v) =>
        float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
}
