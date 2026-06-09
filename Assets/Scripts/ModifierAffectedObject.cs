using UnityEngine;
using System.Collections.Generic;
using Game.Modifiers;

[RequireComponent(typeof(Rigidbody))]
public class ModifierAffectedObject : MonoBehaviour
{
    public enum BehaviorType { Controlled, Hybrid, Ambient }

    [Header("Settings")]
    public BehaviorType behaviorType = BehaviorType.Controlled;
    public LayerMask obstacleMask;
    public float speed = 3f;
    public float acceleration = 6f;
    public float kinematicDrag = 2f;
    public float passiveGravityStrength = 9.81f;

    // TODO: If non-BoxCollider shapes are needed (capsule, sphere), extend GetGroundCheckParams()
    private const float GroundCheckSkin = 0.05f;

    protected Rigidbody rb;
    private Collider physicalCollider;
    private List<IModifierProvider> providers = new List<IModifierProvider>();

    private Vector3 momentumVelocity;
    private Vector3 currentHorizontalVelocity;
    private Vector3 currentVerticalVelocity;

    private bool wasInfluenced;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        foreach (var col in GetComponents<Collider>())
        {
            if (!col.isTrigger)
            {
                physicalCollider = col;
                break;
            }
        }

        providers.AddRange(GetComponents<IModifierProvider>());

        switch (behaviorType)
        {
            case BehaviorType.Controlled:
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.freezeRotation = true;
                obstacleMask = ~LayerMask.GetMask("Modifiable");
                break;

            case BehaviorType.Hybrid:
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.freezeRotation = false;
                obstacleMask = ~LayerMask.GetMask("Modifiable");
                break;

            case BehaviorType.Ambient:
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.freezeRotation = false;
                obstacleMask = ~0;
                break;
        }
    }

    private void FixedUpdate()
    {
        bool isInfluenced = CheckIfInfluenced();

        switch (behaviorType)
        {
            case BehaviorType.Controlled:
                HandleControlledMovement(isInfluenced);
                break;
            case BehaviorType.Hybrid:
                HandleHybridMovement(isInfluenced);
                break;
            case BehaviorType.Ambient:
                HandleAmbientMovement(isInfluenced);
                break;
        }

        wasInfluenced = isInfluenced;
    }

    private bool CheckIfInfluenced()
    {
        foreach (var provider in providers)
        {
            if (provider.IsActiveOnObject(transform.position)) return true;
        }
        return false;
    }

    // Always kinematic — modifiers influence forces, passive gravity + ground check apply when no modifier is active
    private void HandleControlledMovement(bool isInfluenced)
    {
        SanitizeVelocities();

        momentumVelocity = Vector3.Lerp(momentumVelocity, Vector3.zero, kinematicDrag * Time.fixedDeltaTime);

        CollectForces(isInfluenced, out Vector3 windForce, out Vector3 gravityForce, out bool hasWind, out bool hasGravity);

        ApplyHorizontalVelocity(windForce);
        ApplyVerticalVelocity(gravityForce, hasWind, hasGravity);

        Vector3 totalVelocity = momentumVelocity + currentHorizontalVelocity + currentVerticalVelocity;
        MoveKinematicWithCollision(totalVelocity * Time.fixedDeltaTime);
    }

    // Physics by default — goes kinematic when a modifier is active, hands back to physics when modifiers stop
    private void HandleHybridMovement(bool isInfluenced)
    {
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
    }

    private void HandleAmbientMovement(bool isInfluenced)
    {
        if (!isInfluenced) return;

        foreach (var provider in providers)
        {
            rb.AddForce(provider.GetForceContribution(transform.position), ForceMode.Acceleration);
        }
    }

    private void CollectForces(bool isInfluenced, out Vector3 windForce, out Vector3 gravityForce, out bool hasWind, out bool hasGravity)
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

    private void ApplyHorizontalVelocity(Vector3 windForce)
    {
        Vector3 targetHorizontal = windForce * speed;
        currentHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            targetHorizontal,
            acceleration * Time.fixedDeltaTime
        );
    }

    private void ApplyVerticalVelocity(Vector3 gravityForce, bool hasWind, bool hasGravity)
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
        else if (hasWind || behaviorType == BehaviorType.Controlled)
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

    /// <summary>
    /// Performs a downward BoxCast from the bottom of the collider's bounds.
    /// Returns true when a solid obstacle is within GroundCheckSkin distance below.
    /// </summary>
    private bool IsGrounded()
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

    /// <summary>
    /// Derives the BoxCast origin and half-extents from the physical collider's world bounds.
    /// TODO: Extend this for CapsuleCollider and SphereCollider if needed in the future.
    /// </summary>
    private void GetGroundCheckParams(out Vector3 center, out Vector3 halfExtents)
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

    private void MoveKinematicWithCollision(Vector3 delta)
    {
        if (delta.sqrMagnitude < 0.00001f || physicalCollider == null)
        {
            if (delta.sqrMagnitude >= 0.00001f) rb.MovePosition(rb.position + delta);
            return;
        }

        if (rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude, QueryTriggerInteraction.Ignore))
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0)
            {
                float safeDist = Mathf.Max(0, hit.distance - 0.01f);
                rb.MovePosition(rb.position + delta.normalized * safeDist);

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
    }

    private void SanitizeVelocities()
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